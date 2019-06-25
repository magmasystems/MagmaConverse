using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MagmaConverse.Framework
{
    [Serializable]
    [DataContract]
    public class NameValuePair
    {
        [DataMember(Name = "name")]
        public string Name { get; }

        [DataMember(Name = "value")]
        public object Value { get; }

        public NameValuePair(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }
    }

    [Serializable]
    [DataContract]
    public class NameValueList : List<NameValuePair>
    {
    }
}
