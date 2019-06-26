using System;
using System.IO;
using System.Net;
using System.Text;

namespace MagmaConverse.Mocks
{
    [Serializable]
    public class MockHttpWebResponse : WebResponse
    {
        protected MockWebRequest Request { get; set; }
        public MemoryStream Stream { get; set; }
        private const string JsonResponse = "{ status: OK }";
        public override long ContentLength { get; set; }

        internal MockHttpWebResponse(MockWebRequest request)
        {
            this.Request = request;
        }

        public override Stream GetResponseStream()
        {
            this.Stream = new MemoryStream();
            this.Stream.Write(Encoding.UTF8.GetBytes(JsonResponse), 0, 0);
            this.ContentLength = JsonResponse.Length;
            return this.Stream;
        }

        public override void Close()
        {
            this.Stream.Close();
        }
    }

}
