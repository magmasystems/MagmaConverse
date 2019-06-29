using System;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Magmasystems.Persistence
{
    [Serializable]
    [DataContract]
    public enum FilterConditionOperator
    {
        [DataMember(Name = "eq")] 
        Equals,

        [DataMember(Name = "ne")] 
        NotEquals,

        [DataMember(Name = "lt")]
        LessThan,

        [DataMember(Name = "le")]
        LessThanOrEquals,

        [DataMember(Name = "gt")]
        GreaterThan,

        [DataMember(Name = "ge")]
        GreaterThanOrEquals,

        [DataMember(Name = "and")]
        And,

        [DataMember(Name = "or")]
        Or,

        [DataMember(Name = "not")]
        Not,
    }

    [Serializable]
    [DataContract(Name = "filter")]
    public class FilterCondition
    {
        #region Variables
        [DataMember(Name = "field", IsRequired = true)]
        public string FieldName { get; set; }

        [DataMember(Name = "value", IsRequired = true)]
        public object Value { get; set; }

        [DataMember(Name = "op", IsRequired = false)]
        [JsonConverter(typeof(StringEnumConverter))]  // JSON.Net
        [BsonRepresentation(BsonType.String)]
        public FilterConditionOperator Op { get; set; }
        #endregion

        #region Constructors
        public FilterCondition()
        {
            this.Op = FilterConditionOperator.Equals;
        }

        public FilterCondition(string field, object value) : this()
        {
            this.FieldName = field;
            this.Value = value;
        }

        public FilterCondition(string field, FilterConditionOperator op, object value) : this(field, value)
        {
            this.Op = op;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return $"{FieldName} {Op} {Value}";
        }
        #endregion
    }
}