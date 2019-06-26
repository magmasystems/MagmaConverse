using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MagmaConverse.Data
{
    [Serializable]
    [DataContract]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum InsertMode
    {
        [DataMember(Name="before")]
        Before = 0,

        [DataMember(Name="after")]
        After = 1,
    }
}
