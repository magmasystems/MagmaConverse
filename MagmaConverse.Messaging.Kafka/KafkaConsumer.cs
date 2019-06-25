using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
//using Confluent.Kafka.Serialization;
using log4net;

// https://github.com/confluentinc/confluent-kafka-dotnet/blob/v1.0.1.1/examples/Consumer/Program.cs

namespace MagmaConverse.Messaging.Kafka
{
    public interface IKafkaConsumer<T> : IDisposable
    {
        event Action<IConsumer<string, T>, Message<string, T>> MessageReceived;
        event Func<IConsumer<string, T>, Error, bool> ConsumerError;
        event Action<IConsumer<string, T>, TopicPartitionOffset> ConsumerPartitionEOF;

        int MinCommitCount { get; set; }
        string Topic { get; }
    }

    public class KafkaConsumer<T> : IKafkaConsumer<T>
    {
        #region Events
        public event Action<IConsumer<string, T>, Message<string, T>> MessageReceived = (c, msg) => { };
        public event Func<IConsumer<string, T>, Error, bool> ConsumerError = (c, e) => false;
        public event Action<IConsumer<string, T>, TopicPartitionOffset> ConsumerPartitionEOF = (c, offset) => { };
        #endregion

        #region Variables
        // ReSharper disable once UnusedMember.Local
        protected readonly ILog Logger = LogManager.GetLogger(typeof(KafkaConsumer<T>));

        // ReSharper disable once StaticMemberInGenericType
        public readonly Dictionary<string, object> TheKafkaConsumerConfig = new Dictionary<string, object>
        {
            { "bootstrap.servers", "localhost:9092" },
            { "group.id", "simple-csharp-consumer"  },
            {
                "default.topic.config", new Dictionary<string, object>
                {
                    {"auto.offset.reset", "smallest"}
                }
            }
        };

        internal IConsumer<string, T> InternalConsumer { get; private set; }
        protected CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public int MinCommitCount { get; set; } = 10;
        public string Topic { get; protected set; }
        internal Dictionary<string, object> Config { get; set; }
        #endregion

        #region Constructors
        // ReSharper disable once EmptyConstructor
        public KafkaConsumer(string topic, Dictionary<string, object> config = null)
        {
            this.Topic = topic;
            this.Config = config ?? TheKafkaConsumerConfig;
        }
        #endregion

        #region Cleanup
        public virtual void Dispose()
        {
            this.InternalConsumer?.Dispose();
        }
        #endregion

        #region Methods
        internal Task CreateAndStartConsumer()
        {
            var consumer = this.CreateConsumer(this.Topic);

            var consumerTask = Task.Factory.StartNew(() =>
            {
                this.StartConsumer(consumer);
            }, this.CancellationTokenSource.Token);

            return consumerTask;
        }

        internal IConsumer<string, T> CreateConsumer(string theTopic)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = (string) this.Config["bootstrap.servers"],
                GroupId = (string) this.Config["group.id"],
                EnableAutoCommit = true,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };

            var consumer = new ConsumerBuilder<string, T>(config)
                                .SetErrorHandler((c, error) =>
                                {
                                    bool rc = this.ConsumerError(c, error);
                                    if (!rc)
                                    {
                                        this.CancellationTokenSource.Cancel();
                                    }
                                })
                                .SetKeyDeserializer(new KafkaMessagingDriver.ObjectDeserializer<string>())
                                .SetValueDeserializer(new KafkaMessagingDriver.ObjectDeserializer<T>())
                                .Build();

            // ADLER - consumer.OnPartitionEOF += (_, end) => { this.ConsumerPartitionEOF(consumer, end); };

            consumer.Assign(new List<TopicPartitionOffset> { new TopicPartitionOffset(theTopic, 0, 0) });
            consumer.Subscribe(theTopic);

            this.InternalConsumer = consumer;

            return consumer;
        }

        protected void StartConsumer(IConsumer<string, T> consumer)
        {
            while (!this.CancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(this.CancellationTokenSource.Token);
                    // Note: End of partition notification has not been enabled, so
                    // it is guaranteed that the ConsumeResult instance corresponds
                    // to a Message, and not a PartitionEOF event.
                    this.MessageReceived(consumer, consumeResult.Message);
                }
                catch (ConsumeException e)
                {
                    this.ConsumerError(consumer, e.Error);
                }
            }

            consumer.Dispose();
        }
        #endregion
    }
}
