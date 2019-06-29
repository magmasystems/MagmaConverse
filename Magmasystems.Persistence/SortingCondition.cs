using System;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Magmasystems.Persistence
{
    [Serializable]
    [DataContract]
    public enum SortDirection
    {
        [DataMember] Asc,
        [DataMember] Desc
    }

    [Serializable]
    [DataContract]
    public class SortingCondition
    {
        [DataMember(Name = "field")]
        [BsonElement("field")]
        public string Field { get; set; }

        [DataMember(Name = "direction")]
        [BsonElement("direction")]
        public SortDirection Direction { get; set; }

        public SortingCondition()
        {
            this.Direction = SortDirection.Asc;
        }

        public SortingCondition(string field) : this()
        {
            this.Field = field;
        }

        public SortingCondition(string field, SortDirection direction) : this(field)
        {
            this.Direction = direction;
        }
    }
}
