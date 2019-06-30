using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using MagmaConverse.Data;
using MagmaConverse.Data.Fields;
using MagmaConverse.Services;
using Magmasystems.Framework;
using Magmasystems.Framework.Core;
using Magmasystems.Framework.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net;
using log4net.Config;


namespace MagmaConverse.Tests
{
    [TestClass]
    public class FormManagerRestTests
    {
        private FormManagerService TheService { get; set; }
        private string LoadedJsonFormRequest { get; set; }
        private string FormDefinitionId { get; set; }
        private string FormInstanceId { get; set; }

        [TestInitialize]
        public void Initialize()
        {
            InitLogging();

            this.TheService = new FormManagerService(new FormManagerServiceSettings
            {
                NoCreateRestService = false,
                NoMessaging = true,
                NoPersistence = true,
                AutomatedInput = false,
            });
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (this.TheService == null)
                return;
            this.TheService.Dispose();
            this.TheService = null;
        }

        internal static void InitLogging()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            var currAppDir = AppDomain.CurrentDomain.BaseDirectory;
            var logfile = new FileInfo(currAppDir + "log4net.config");
            XmlConfigurator.Configure(logRepository, logfile);
        }

        [TestMethod]
        public void LoadFormDefinitionFromFileTest()
        {
            const string uri = "http://localhost:8089/FormManagerService/form/definition/load/DIYOnboardingForm";
            var request = this.CreateGetRequest(uri);

            // Get the response from the server
            ResponseStatus<string> responseStatus = this.GetWebResponse<string>(request);
            Assert.IsTrue(responseStatus.StatusCode == ResponseStatusCodes.OK, "The response status was not OK");
            var formCreateRequest = Json.Deserialize<FormCreateRequest>(responseStatus.Value);

            Assert.IsNotNull(formCreateRequest);
            Assert.IsNotNull(formCreateRequest.Forms);
            Assert.IsTrue(formCreateRequest.Forms.Count == 1);

            Assert.IsNotNull(formCreateRequest.ReferenceData);
            Assert.IsTrue(formCreateRequest.ReferenceData.Count > 0);
            var states = formCreateRequest.ReferenceData[0];
            Assert.IsTrue(states.Name == "USStates");
            Assert.IsNotNull(states.SortedDictionary);
            Assert.IsTrue(states.SortedDictionary.Count == 52);

            this.LoadedJsonFormRequest = responseStatus.Value;
        }

        [TestMethod]
        public void CreateFormDefinitionFromLoadedJson()
        {
            this.LoadFormDefinitionFromFileTest();

            const string uri = "http://localhost:8089/FormManagerService/form/definition/create";
            var request = this.CreatePostRequest(uri, this.LoadedJsonFormRequest, contentType: "application/text");
            
            // Get the response from the server
            var responseStatus = this.GetWebResponse<NameIdPair[]>(request);
            Assert.IsTrue(responseStatus.StatusCode == ResponseStatusCodes.OK, "The response status was not OK");

            /*
             "Value": [
                 {
                    "name": "frmDIYCustomerOnboarding",
                    "id": "FormDefinition.4bd64c46"
                 }
             ]
             */

            var pairs = responseStatus.Value;
            Assert.IsNotNull(pairs);
            Assert.IsTrue(pairs.Length == 1);
            Assert.IsTrue(pairs[0].Name == "frmDIYCustomerOnboarding");

            this.FormDefinitionId = pairs[0].Id;
        }

        [TestMethod]
        public void CreateFormInstance()
        {
            this.CreateFormDefinitionFromLoadedJson();

            string uri = "http://localhost:8089/FormManagerService/form/new/{{DIYOnboarding-Form1-ID}}".Replace("{{DIYOnboarding-Form1-ID}}", this.FormDefinitionId);
            var request = this.CreateGetRequest(uri);

            // Get the response from the server
            ResponseStatus<string> responseStatus = this.GetWebResponse<string>(request);
            Assert.IsTrue(responseStatus.StatusCode == ResponseStatusCodes.OK, "The response status was not OK");

            this.FormInstanceId = responseStatus.Value;
        }

        [TestMethod]
        public void RunForm()
        {
            this.CreateFormInstance();
            this.AutomateInput();

            string uri = "http://localhost:8089/FormManagerService/form/run/{{DIYOnboarding-Form1-InstanceID}}".Replace("{{DIYOnboarding-Form1-InstanceID}}", this.FormInstanceId);
            var request = this.CreateGetRequest(uri);

            // Wait until the form has completed
            ManualResetEvent eventFormEnded = new ManualResetEvent(false);
            this.TheService.RunFormEnded += form =>
            {
                eventFormEnded.Set();
            };

            // Get the response from the server
            var responseStatus = this.GetWebResponse<bool>(request);
            Assert.IsTrue(responseStatus.StatusCode == ResponseStatusCodes.OK, "The response status was not OK");

            if (!eventFormEnded.WaitOne(/*120 * 1000*/))
            {
                Assert.Fail("The form did not complete running within 60 seconds");
            }

            var completedForm = this.TheService.GetForm(this.FormInstanceId).Value;
            Assert.IsNotNull(completedForm);

            Assert.AreEqual(completedForm.DefinitionId, this.FormDefinitionId);
            Assert.AreEqual(completedForm.Id, this.FormInstanceId);
            Assert.IsNotNull(completedForm.Fields);

            /*
             	[5]	{MagmaConverse.Data.Fields.SBSEditField}
		            FieldTypeName	"edit"
		            FormId	        "FormInstance.frmDIYCustomerOnboarding.386b4c6a"
		            Id	            "FormField.ownerName474ba9b7"
		            Name	        "ownerName"
		            Value	        "Marc Adler"
            */

            // See if a field is present and has a value
            var field = completedForm.FindField("ownerName");
            Assert.IsNotNull(field);
            Assert.IsTrue(field.Value is string);
            Assert.AreEqual((string) field.Value, "Marc Adler");

            // Retrieve the same field with a REST call
            var field2 = this.GetFieldInForm("ownerName");
            Assert.IsNotNull(field2);
            Assert.IsTrue(field2.Value is string);
            Assert.AreEqual((string) field2.Value, "Marc Adler");

            // See if the repeating employee details group is good
            var employees = completedForm.FindField("employeeDetailsGroup") as SBSRepeaterField;
            Assert.IsNotNull(employees);
            Assert.IsNotNull(employees.SavedObjects);
            Assert.IsTrue(employees.SavedObjects.Count == 4);

            // Retrieve the same field with a REST call
            var employees2 = this.GetFieldInForm("employeeDetailsGroup") as SBSRepeaterField;
            Assert.IsNotNull(employees2);
            Assert.IsNotNull(employees2.SavedObjects);
            Assert.IsTrue(employees2.SavedObjects.Count == 4);
        }

        private void AutomateInput()
        {
            FormManagerServiceSettings settings = new FormManagerServiceSettings {AutomatedInput = true, MaxRepeaterIterations = 4};
            const string uri = "http://localhost:8089/FormManagerService/form/manager/settings";
            var request = this.CreatePostRequest(uri, Json.Serialize(settings), method: "PUT", contentType: "application/text");

            var responseStatus = this.GetWebResponse<bool>(request);
            Assert.IsTrue(responseStatus.Value, "The response payload was null or empty");
        }

        private SBSFormField GetFieldInForm(string fieldName)
        {
            string uri = "http://localhost:8089/FormManagerService/form/{{DIYOnboarding-Form1-InstanceID}}/field/{{field}}"
                .Replace("{{DIYOnboarding-Form1-InstanceID}}", this.FormInstanceId)
                .Replace("{{field}}", fieldName)
                ;
            var request = this.CreateGetRequest(uri);

            var responseStatus = this.GetWebResponse<SBSFormField>(request);
            Assert.IsTrue(responseStatus.StatusCode == ResponseStatusCodes.OK, "The response status was not OK");

            return responseStatus.Value;
        }

        #region Http Helpers
        private WebRequest CreateGetRequest(string uri)
        {
            var request = WebRequest.Create(uri);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;
            return request;
        }

        private WebRequest CreatePostRequest(string uri, string payload, string method = "POST", string contentType = "application/json")
        {
            var request = WebRequest.Create(uri);
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Method = method;
            request.ContentType = contentType;

            var data = Encoding.UTF8.GetBytes(payload);
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);  
                stream.Close();
            }

            return request;
        }

        private string GetWebResponse(WebRequest request)
        {
            // Get the response from the server
            using (HttpWebResponse response = (HttpWebResponse) request.GetResponse())
            {
                Assert.IsNotNull(response, "The response to the request is null");

                using (var reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    var sPayload = reader.ReadToEnd();
                    return sPayload;
                }
            }
        }

        private ResponseStatus<T> GetWebResponse<T>(WebRequest request)
        {
            string sPayload = this.GetWebResponse(request);
            Assert.IsFalse(string.IsNullOrEmpty(sPayload), "The response payload was null or empty");

            var responseStatus = Json.Deserialize<ResponseStatus<T>>(sPayload);
            return responseStatus;
        }
        #endregion
    }
}
