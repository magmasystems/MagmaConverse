namespace MagmaConverse.Interfaces
{
    public interface IServiceCreationSettings
    {
        bool NoMessaging { get; set; }
        bool NoPersistence { get; set; }
    }
}