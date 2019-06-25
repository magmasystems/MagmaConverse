using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Confluent.Kafka;
using log4net;

namespace MagmaConverse.Messaging.Kafka
{
    public interface IKafkaProducer<T> : IDisposable
    {
        event Action<IProducer<string, T>> ProducerCreated;
        event Action<IProducer<string, T>, Error> ProducerError;
        event Action<IProducer<string, T>, Message<string, T>> ProducerMessagePublished;

        Task Publish(string topic, string key, T value);
    }

    public class KafkaProducer<T> : IKafkaProducer<T>
    {
        #region Events
        public event Action<IProducer<string, T>> ProducerCreated = p => { };
        public event Action<IProducer<string, T>, Error> ProducerError = (p, e) => { };
        public event Action<IProducer<string, T>, Message<string, T>> ProducerMessagePublished = (p, msg) => { };
        #endregion

        #region Variables
        private readonly ILog Logger = LogManager.GetLogger(typeof(KafkaProducer<T>));

        // ReSharper disable once StaticMemberInGenericType
        public readonly Dictionary<string, object> TheKafkaProducerConfig = new Dictionary<string, object>
        {
            { "bootstrap.servers", "localhost:9092" },
            { "client.id", Dns.GetHostName() },
            { "default.topic.config", new Dictionary<string, object>
                {
                    { "acks", "all" }
                }
            }
        };

        internal IProducer<string, T> InternalProducer { get; set; }
        internal Dictionary<string, object> Config { get; set; }
        #endregion

        #region Constructors
        public KafkaProducer(Dictionary<string, object> config = null)
        {
            this.Config = config ?? TheKafkaProducerConfig;
            this.InternalProducer = this.CreateProducer();
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            this.InternalProducer?.Dispose();
        }
        #endregion

        #region Methods
        protected IProducer<string, T> CreateProducer()
        {
            IProducer<string, T> producer = null;
            var config = new ProducerConfig { BootstrapServers = (string) this.Config["bootstrap.servers"] };

            try
            {
                producer = new ProducerBuilder<string, T>(config).Build();
                this.ProducerCreated(producer);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
                Logger.Error(exc);
            }

            return producer;
        }

        public async Task Publish(string topic, string key, T value)
        {
            try
            {
                var deliveryResult = await this.InternalProducer.ProduceAsync(topic, new Message<string, T> { Key = key, Value = value });
                Message<string, T> msg = deliveryResult.Message;
                this.ProducerMessagePublished(this.InternalProducer, msg);


                // Tasks are not waited on synchronously (ContinueWith is not synchronous),
                // so it's possible they may still in progress here.
            }
            catch (ProduceException<string, string> e)
            {
                Console.WriteLine($"failed to deliver message: {e.Message} [{e.Error.Code}]");
                this.ProducerError(this.InternalProducer, e.Error);
            }
        }
        #endregion
    }
}
