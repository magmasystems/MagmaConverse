using System;

namespace MagmaConverse.Messaging
{
    public interface IMessagingService : IDisposable
    {
        void Publish(string topic, object data);
    }
}