using System.Runtime.Serialization;
using MagmaConverse.Interfaces;

namespace MagmaConverse.Services
{
    [DataContract]
    public class ServiceCreationSettings : IServiceCreationSettings
    {
        [DataMember(Name="noMessaging", IsRequired = false)]
        public bool NoMessaging { get; set; }

        [DataMember(Name = "noPersistence", IsRequired = false)]
        public bool NoPersistence { get; set; }
    }
}
