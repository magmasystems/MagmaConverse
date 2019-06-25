using System;
using System.Runtime.Serialization;

namespace MagmaConverse.Framework.Core
{
    [Serializable]
    [DataContract]
    public class NameIdPair
    {
        [DataMember(Name = "name")]
        public string Name { get; }

        [DataMember(Name = "id")]
        public string Id { get; }

        public NameIdPair(string name, string id)
        {
            this.Name = name;
            this.Id = id;
        }
    }
}
