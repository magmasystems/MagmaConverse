using log4net;

namespace MagmaConverse.Messaging
{
    public static class MessagingServiceFactory
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MessagingServiceFactory));

        public static IMessagingService Create(string driver = "kafka")
        {
            switch (driver.ToLower())
            {
                case "kafka":
                    return new KafkaMessagingService();
                default:
                    Logger.Error($"Cannot locate a messaging service for {driver}");
                    return null;

            }
        }
    }
}
