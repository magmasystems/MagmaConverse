using System;
using System.Runtime.Serialization;

namespace Magmasystems.Persistence
{
    [Serializable]
    [DataContract]
    public class DocumentDatabaseUpdateOptions
    {
        [DataMember] public bool UpdateFields    { get; set; }
        [DataMember] public bool AppendToExistingArray { get; set; }
        [DataMember] public bool DeleteFromArray { get; set; }
        [DataMember] public bool UpdateEntire    { get; set; }
    }
}