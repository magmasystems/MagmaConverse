using System;
using System.Net;
using System.ServiceModel;

namespace System.ServiceModel
{
    public enum AddressFilterMode
    {
        Any
    }

    public enum InstanceContextMode
    {
        Single
    }

    public class ServiceBehavior : Attribute
    {
        public bool IncludeExceptionDetailInFaults { get; set; }
        public AddressFilterMode AddressFilterMode { get; set; }
        public InstanceContextMode InstanceContextMode { get; set; }
    }
}

namespace System.ServiceModel.Web
{
    public class WebInvokeAttribute : Attribute
    {
        public string Method { get; set; }
        public WebMessageBodyStyle BodyStyle { get; set; }
        public string UriTemplate { get; set; }
        public WebMessageFormat RequestFormat { get; set; }
        public WebMessageFormat ResponseFormat { get; set; }
    }

    public enum WebMessageFormat
    {
        Json
    }

    public enum WebMessageBodyStyle
    {
        Bare,
        Wrapped
    }

    public class WebGetAttribute : Attribute
    {
        public string UriTemplate { get; set; }
        public WebMessageBodyStyle BodyStyle { get; set; }
        public WebMessageFormat ResponseFormat { get; set; }
        public WebMessageFormat RequestFormat { get; set; }
    }

    public class WebFaultException<T> : Exception
    {
        public T Error { get; }
        public HttpStatusCode StatusCode { get; }

        public WebFaultException()
        {
        }

        public WebFaultException(T error, HttpStatusCode code)
        {
            this.Error = error;
            this.StatusCode = code;
        }
    }

    public class WebOperationContext
    {
        public static WebOperationContext Current { get; }
        public WebOperationContextResponse OutgoingResponse { get; set; }
        public IncomingWebRequestContext IncomingRequest { get; set; }
    }

    public class WebOperationContextResponse
    {
        public HttpStatusCode StatusCode { get; set; }
    }

    public class IncomingWebRequestContext
    {
        public WebHeaderCollection Headers { get; set; }
    }

    public class WebServiceHost
    {
    }
}

namespace System.ServiceModel.Activation
{
    public enum AspNetCompatibilityRequirementsMode
    {
        Allowed
    }

    public class AspNetCompatibilityRequirements : Attribute
    {
        public AspNetCompatibilityRequirementsMode RequirementsMode { get; set; }
    }
}

namespace MagmaConverse.Communications
{
    public class NewtonsoftJsonBehavior
    {
    }

    public class WCFRestService<T> : IDisposable
    {
        public string Name { get; }
        public NewtonsoftJsonBehavior Behavior { get; }
        public T Foo { get; set; }

        public WCFRestService()
        {

        }

        public WCFRestService(string name) : this()
        {
            this.Name = name;
        }

        public WCFRestService(string name, NewtonsoftJsonBehavior behavior) : this(name)
        {
            this.Behavior = behavior;
        }

        public void Dispose()
        {
        }
    }
}