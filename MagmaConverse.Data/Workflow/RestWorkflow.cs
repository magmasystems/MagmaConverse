using System;
using System.IO;
using System.Net;
using System.Text;
using MagmaConverse.Framework;
using MagmaConverse.Framework.Serialization;
using MagmaConverse.Mocks;

namespace MagmaConverse.Data.Workflow
{
    internal interface IRestWorkflow : IFormWorkflow
    {
    }

    internal class RestWorkflow : FormWorkflowBase, IRestWorkflow
    {
        protected bool IsMockRest { get; set; }

        public RestWorkflow(SBSFormSubmissionWorkflowProcessor processor, IFormSubmissionFunction submissionFunc) : base("rest", processor, submissionFunc)
        {
        }

        public override object Execute()
        {
            /*
                "workflow": "rest://localhost:8089/FormManagerService/form/${field:RepeaterGroupForm.Id}/employee/add",
                "properties": {
                    "method": "post",
                    "body": "${form:RepeaterGroupForm}",
                    "bodyFormat": "json"
                }
            */

            Uri uri = this.ConstructWorkflowUri(this.SubmissionFunction);

            string method = "get";
            if (this.SubmissionFunction.Properties.TryGetValue("method", out object omethod))
                method = (string) omethod;

            string bodyFormat = "json";
            if (this.SubmissionFunction.Properties.TryGetValue("bodyFormat", out object obodyFormat))
                bodyFormat = (string) obodyFormat;

            string body = "";
            if (this.SubmissionFunction.Properties.TryGetValue("body", out object obody))
                body = (string) obody;

            switch (method.ToUpper())
            {
                case "POST":
                case "PUT":
                {
                    var response = this.PostOrPut(method.ToLower(), uri, body, bodyFormat, this.SubmissionFunction, out string responsePayload);
                    // We should do something with the response payload
                    return response;
                }
            }

            return null;
        }

        private Uri ConstructWorkflowUri(IFormSubmissionFunction submissionFunc)
        {
            string workflow = new StringSubstitutor().PerformSubstitutions(submissionFunc.Workflow, null, this.Form);

            Uri uri = new Uri(workflow);
            switch (uri.Scheme.ToLower())
            {
                case "mockrest":
                {
                    this.IsMockRest = true;
                    Uri newUri = this.GenerateHttpUri(uri);
                    return newUri;
                }

                case "rest":
                {
                    this.IsMockRest = ApplicationContext.Configuration.UseMocksForRestCalls;
                    Uri newUri = this.GenerateHttpUri(uri);
                    return newUri;
                }
            }

            return null;
        }

        private Uri GenerateHttpUri(Uri uri)
        {
            string addr = $"http://{uri.Host}";
            if (uri.Port != 0)
                addr += $":{uri.Port}";

            foreach (string segment in uri.Segments)
            {
                string newSegment = WebUtility.UrlDecode(segment);
                if (newSegment == null)
                    continue;

                // ${field:RepeaterGroupForm.Id}
                if (newSegment.Contains("${"))
                {
                    newSegment = new StringSubstitutor().PerformSubstitutions(newSegment, null, this.Form);
                }

                addr += newSegment;
            }

            return new Uri(addr);
        }

        private WebResponse PostOrPut(string method, Uri uri, string body, string bodyFormat, IFormSubmissionFunction submissionFunction, out string responsePayload)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/network-programming/how-to-send-data-using-the-webrequest-class
            var request = this.PrepareWebRequest(method, uri);
            var data = this.PreparePayload(request, body, bodyFormat, submissionFunction);

            // The call to the REST endpoint is synchronous here.
            // TODO - support async
            try
            {
                // Send the request and payload (we don't do this for GET requests)
                if (data != null)
                {
                    // Send the request to the REST endpoint
                    Stream dataStream = request.GetRequestStream();
                    dataStream.Write(data, 0, data.Length);
                    dataStream.Close();
                }

                // Get the response from the server

                var response = request.GetResponse();
                {
                    using (var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
                    {
                        responsePayload = reader.ReadToEnd();
                        // Right now, we are not doing anything here with the response payload. We will let the caller handle it.
                    }
                }
                return response;
            }
            catch (WebException webExc)
            {
                this.WorkflowProcessor.Logger.Error(webExc.Message);
                responsePayload = null;
                return webExc.Response;
            }
        }

        private WebRequest PrepareWebRequest(string method, Uri uri)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/network-programming/how-to-send-data-using-the-webrequest-class
            var request = this.CreateWebRequest(uri);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = method;
            return request;
        }

        private byte[] PreparePayload(WebRequest request, string body, string bodyFormat, IFormSubmissionFunction submissionFunction)
        {
            if (string.IsNullOrEmpty(body))
                return null;

            byte[] data;

            // Evaluate the (possible) expression in the body
            object objPayload = body;
            if (this.WorkflowProcessor.BodyExpressionEvaluator != null)
            {
                objPayload = this.WorkflowProcessor.BodyExpressionEvaluator(body, submissionFunction);
            }

            if (bodyFormat.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                body = Json.Serialize(objPayload);
                data = Encoding.UTF8.GetBytes(body);
                request.ContentType = "application/json";
            }
            else
            {
                data = Encoding.UTF8.GetBytes((string)objPayload);
                request.ContentType = "application/text";
            }

            request.ContentLength = data.Length;
            return data;
        }

        private WebRequest CreateWebRequest(Uri uri)
        {
            return this.IsMockRest ? MockWebRequest.Create(uri) : WebRequest.Create(uri);
        }
    }

    internal class MockRestWorkflow : RestWorkflow
    {
        public MockRestWorkflow(SBSFormSubmissionWorkflowProcessor processor, IFormSubmissionFunction submissionFunc) : base(processor, submissionFunc)
        {
            this.IsMockRest = true;
            this.Name = "mockrest";
        }
    }
}