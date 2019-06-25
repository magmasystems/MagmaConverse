using System;
using System.Runtime.Serialization;
using SwaggerWcf.Attributes;

namespace MagmaConverse.Framework
{
    public interface IResponseStatus<T>
    {
        ResponseStatusCodes StatusCode { get; set; }
        string ErrorMessage { get; set; }
        T Value { get; set; }
    }

    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition()]
    public class ResponseStatus<T> : IResponseStatus<T>
    {
        [DataMember]
        [SwaggerWcfProperty(Description = "The status code for the operation.")]
        public ResponseStatusCodes StatusCode { get; set; }

        [DataMember]
        [SwaggerWcfProperty(Description = "The optional error message")]
        public string ErrorMessage { get; set; }

        [DataMember]
        [SwaggerWcfProperty(Description = "The returned value, if the operation was successful")]
        public T Value { get; set; }

        public ResponseStatus() : this(ResponseStatusCodes.OK)
        {
        }

        public ResponseStatus(T value) : this(ResponseStatusCodes.OK, null, value)
        {
        }

        public ResponseStatus(ResponseStatusCodes statusCode, string errorMessage = null, T value = default)
        {
            this.StatusCode = statusCode;
            this.ErrorMessage = errorMessage;
            this.Value = value;
        }

        public ResponseStatus(ResponseStatus src)
        {
            this.StatusCode = src.StatusCode;
            this.ErrorMessage = src.ErrorMessage;
            this.Value = (T) src.Value;
        }

        public static implicit operator ResponseStatus(ResponseStatus<T> src)
        {
            return new ResponseStatus(src.StatusCode, src.ErrorMessage, src.Value);
        }

        public override string ToString()
        {
            return $"{nameof(StatusCode)}: {StatusCode}, {nameof(Value)}: {Value}, {nameof(ErrorMessage)}: {ErrorMessage}";
        }
    }

    [Serializable]
    [DataContract]
    public class ResponseStatus : ResponseStatus<object>
    {
        public ResponseStatus(object value) : base(value)
        {
        }

        public ResponseStatus(Exception exc) : base(ResponseStatusCodes.Error, exc.Message)
        {
        }

        public ResponseStatus(ResponseStatusCodes status, object value) : this(status, null, value)
        {
        }

        public ResponseStatus(ResponseStatusCodes status, string errorMsg, object value) : base(status, errorMsg, value)
        {
        }

        public static ResponseStatus OK => new ResponseStatus(null);
        public static ResponseStatus Error => new ResponseStatus(ResponseStatusCodes.Error, null);
    }
}
