﻿using System;

namespace Magmasystems.Messaging
{
    public interface IMessagingService : IDisposable
    {
        void Publish(string topic, object data);
    }
}