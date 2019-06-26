using System;
using System.IO;
using System.Net;

namespace MagmaConverse.Mocks
{
    [Serializable]
    public class MockWebRequest : WebRequest
    {
        protected MemoryStream Stream { get; set; }
        protected WebRequest ActualRequest { get; set; }

        public new static MockWebRequest Create(Uri uri)
        {
            return new MockWebRequest { ActualRequest = WebRequest.Create(uri) };
        }

        public override Stream GetRequestStream()
        {
            this.Stream = new MemoryStream();
            return this.Stream;
        }

        public override WebResponse GetResponse()
        {
            WebResponse response = new MockHttpWebResponse(this);
            return response;
        }

        public override string Method
        {
            get => this.ActualRequest.Method;
            set => this.ActualRequest.Method = value;
        }
        public override string ContentType
        {
            get => this.ActualRequest.ContentType;
            set => this.ActualRequest.ContentType = value;
        }
        public override long ContentLength
        {
            get => this.ActualRequest.ContentLength;
            set => this.ActualRequest.ContentLength = value;
        }
        public override ICredentials Credentials
        {
            get => this.ActualRequest.Credentials;
            set => this.ActualRequest.Credentials = value;
        }
    }
}