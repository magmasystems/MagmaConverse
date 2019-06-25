namespace MagmaConverse.Interfaces
{
    public interface IFormManagerServiceSettings : IServiceCreationSettings
    {
        bool NoCreateRestService { get; set; }
        bool AutomatedInput { get; set; }
        int MaxRepeaterIterations { get; set; }
    }
}