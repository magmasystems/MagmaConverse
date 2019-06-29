using System.Runtime.Serialization;
using Magmasystems.Framework;
using MagmaConverse.Interfaces;

namespace MagmaConverse.Services
{
    [DataContract]
    public class FormManagerServiceSettings : ServiceCreationSettings, IFormManagerServiceSettings
    {
        [DataMember(Name = "noCreateRestService", IsRequired = false)]
        public bool NoCreateRestService { get; set; }

        [DataMember(Name = "automateInput", IsRequired = false)]
        public bool AutomatedInput
        {
            get => ApplicationContext.IsInAutomatedMode;
            set => ApplicationContext.IsInAutomatedMode = value;
        }

        [DataMember(Name = "maxRepeaterIterations", IsRequired = false)]
        public int MaxRepeaterIterations
        {
            get => ApplicationContext.MaxRepeaterIterations;
            set => ApplicationContext.MaxRepeaterIterations = value;
        }

        public static IFormManagerServiceSettings FromConfig()
        {
            var config = ApplicationContext.Configuration;

            return new FormManagerServiceSettings
            {
                NoMessaging = config?.NoMessaging ?? false,
                NoPersistence = config?.NoPersistence ?? false,
                NoCreateRestService = config?.NoCreateRestService ?? false,
                AutomatedInput = config?.AutomatedInput ?? false,
                MaxRepeaterIterations = config?.MaxRepeaterIterations ?? int.MaxValue
            };
        }
    }
}