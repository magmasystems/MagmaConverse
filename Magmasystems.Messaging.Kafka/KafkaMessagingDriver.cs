using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Confluent.Kafka;
using log4net;
using Newtonsoft.Json;

namespace Magmasystems.Messaging.Kafka
{
    public interface IKafkaMessagingDriver : IDisposable
    {
        bool IsBrokerAlive();
        KafkaConsumer<T> CreateAndStartConsumer<T>(string topic, Dictionary<string, object> config = null, Action<KafkaConsumer<T>> onBeforeStart = null);
        KafkaConsumer<T> CreateConsumer<T>(string topic, Dictionary<string, object> config = null);
        KafkaProducer<TValue> CreateProducer<TValue>(Dictionary<string, object> config = null);
    }

    public class KafkaMessagingDriver : IKafkaMessagingDriver
    {
        #region Variables
        // ReSharper disable once UnusedMember.Local
        private readonly ILog Logger = LogManager.GetLogger(typeof(KafkaMessagingDriver));

        protected List<Task> ConsumerTasks { get; } = new List<Task>();
        protected List<IDisposable> TheProducers { get; set; } = new List<IDisposable>();
        #endregion

        #region Constructors
        static KafkaMessagingDriver()
        {
            InitLog4Net();
        }

        static void InitLog4Net()
        {
            XmlDocument log4netConfig = new XmlDocument();
            log4netConfig.Load(File.OpenRead("log4net.config"));
            var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
            log4net.Config.XmlConfigurator.Configure(repo, log4netConfig["log4net"]);
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            Task.WaitAll(this.ConsumerTasks.ToArray(), 5 * 1000);

            foreach (var p in this.TheProducers)
            {
                p.Dispose();
            }
        }
        #endregion

        #region Methods
        public bool IsBrokerAlive()
        {
            using (var producer = new KafkaProducer<int>())
            {
                // If Kafka was not alive, then InternalProducer will be null
                if (producer.InternalProducer == null)
                    return false;

                try
                {
                    //ADLER - producer.InternalProducer.GetMetadata(false, null, TimeSpan.FromSeconds(1));
                }
                catch (KafkaException)
                {
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        public KafkaConsumer<T> CreateAndStartConsumer<T>(string topic, Dictionary<string, object> config = null, Action<KafkaConsumer<T>> onBeforeStart = null)
        {
            var consumer = this.CreateConsumer<T>(topic, config);
            onBeforeStart?.Invoke(consumer);

            var task = consumer.CreateAndStartConsumer();

            this.ConsumerTasks.Add(task);
            return consumer;
        }

        public KafkaConsumer<T> CreateConsumer<T>(string topic, Dictionary<string, object> config = null)
        {
            return new KafkaConsumer<T>(topic, config);
        }

        public KafkaProducer<TValue> CreateProducer<TValue>(Dictionary<string, object> config = null)
        {
            var producer = new KafkaProducer<TValue>(config);

            this.TheProducers.Add(producer);
            return producer;
        }
        #endregion

        #region Serialization
        internal class ObjectSerializer<T> : ISerializer<T>
        {
            public byte[] Serialize(T data)
            {
                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            }

            public byte[] Serialize(T data, SerializationContext context)
            {
                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
            }
        }

        internal class ObjectDeserializer<T> : IDeserializer<T>
        {
            public T Deserialize(byte[] data)
            {
                string s = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<T>(s);
            }

            public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
            {
                string s = Encoding.UTF8.GetString(data);
                return JsonConvert.DeserializeObject<T>(s);
            }
        }
        #endregion
    }
}
