using System.Collections.Generic;
using Magmasystems.Messaging.Kafka;

namespace Magmasystems.Messaging
{
    public class KafkaMessagingService : IMessagingService
    {
        public IKafkaMessagingDriver Driver { get; }
        protected bool IsMessageBrokerAlive { get; set; }
        protected Dictionary<string, KafkaProducer<object>> KafkaTopicProducers { get; } = new Dictionary<string, KafkaProducer<object>>();

        public KafkaMessagingService()
        {
            this.Driver = new KafkaMessagingDriver();
            this.IsMessageBrokerAlive = this.Driver.IsBrokerAlive();
        }

        public void Dispose()
        {
            foreach (var kvp in this.KafkaTopicProducers)
            {
                kvp.Value.Dispose();
            }
            this.KafkaTopicProducers.Clear();

            this.Driver?.Dispose();
        }

        public void Publish(string topic, object data)
        {
            if (!this.IsMessageBrokerAlive)
                return;

            if (!this.KafkaTopicProducers.TryGetValue(topic, out KafkaProducer<object> producer))
                producer = this.Driver?.CreateProducer<object>();

            producer?.Publish(topic, null, data);
        }
    }
}
