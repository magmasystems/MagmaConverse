using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using log4net;
using log4net.Config;
using MagmaConverse.Data;
using MagmaConverse.Data.Fields;
using MagmaConverse.Models;
using MagmaConverse.Services;
using Magmasystems.Framework;
using Magmasystems.Framework.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MagmaConverse.Tests
{
    [TestClass]
    public class FormServiceTests
    {
        [TestInitialize]
        public void Initialize()
        {
            InitLogging();
        }

        internal static void InitLogging()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            var currAppDir = AppDomain.CurrentDomain.BaseDirectory;
            var logfile = new FileInfo(currAppDir + "log4net.config");
            XmlConfigurator.Configure(logRepository, logfile);
        }

        [TestMethod]
        public void TestCreateCheckboxInForm()
        {
            FormCreateRequest request = new FormCreateRequest
            {
                Forms = new List<FormTemplateFormDefinition>
                {
                    new FormTemplateFormDefinition
                    {
                        Title = "Test Marc Form",
                        Name = "MarcForm",
                        Description = "This is a test form",
                        Fields = new List<FormTemplateFieldDefinition>
                        {
                            new FormTemplateFieldDefinition
                            {
                                FieldType = "checkbox",
                                Prompt = "Are you a small business?"
                            }
                        }
                    }
                }
            };

            Assert.IsNotNull(request.Forms[0].Fields[0]);
            var field = request.Forms[0].Fields[0];
            Assert.IsTrue(field.FieldType.Equals("checkbox", StringComparison.OrdinalIgnoreCase));

            this.CreateAndRunForm(request);
        }

        [TestMethod]
        public void TestCreateAndRunDIYOnboardingForm()
        {
            var serviceSettings = new FormManagerServiceSettings 
            { 
                AutomatedInput = true, 
                NoCreateRestService = true,
                NoMessaging = true,
                NoPersistence = true
            };

            using (FormManagerService service = new FormManagerService(serviceSettings))
            {
                // Load the form definition
                var loadDefResponse = service.LoadFormDefinitionFromFile("./DIYOnboardingForm.json");
                Assert.IsNotNull(loadDefResponse);
                Assert.AreEqual(loadDefResponse.StatusCode, ResponseStatusCodes.OK);

                var createRequest = Json.Deserialize<FormCreateRequest>(loadDefResponse.Value);
                Assert.IsNotNull(createRequest);

                // Create the internal form definition
                var createResponse = service.CreateForm(createRequest);
                Assert.IsNotNull(createResponse);
                Assert.AreEqual(createResponse.StatusCode, ResponseStatusCodes.OK);
                Assert.IsNotNull(createResponse.Value);

                // We should get back an array of name/value pairs
                var formDefId = createResponse.Value[0].Id;
                
                // Create an instance of the DIYOnboarding form from the template
                var newformResponse = service.NewForm(formDefId);
                Assert.IsNotNull(newformResponse);
                Assert.AreEqual(newformResponse.StatusCode, ResponseStatusCodes.OK);

                // Read the instance and make sure it really exists
                var idFormInstance = newformResponse.Value;
                var formInstance = SBSFormModel.Instance.GetFormInstance(idFormInstance);
                Assert.IsNotNull(formInstance);

                // Detect when the form is done running
                ManualResetEvent eventStop = new ManualResetEvent(false);
                formInstance.Submitted += form => { eventStop.Set(); };
                formInstance.Cancelled += form => { eventStop.Set(); };
                service.RunFormEnded += form =>
                {
                    if (form.Id == idFormInstance)
                        eventStop.Set();
                };

                // Run the form asynchronously
                var runformResponse = service.RunForm(idFormInstance);
                Assert.IsNotNull(runformResponse);
                Assert.AreEqual(runformResponse.StatusCode, ResponseStatusCodes.OK);

                // Wait for the form to end
                bool rc = eventStop.WaitOne();
                Assert.IsTrue(rc, "The test timed out");
            }
        }

        private void CreateAndRunForm(FormCreateRequest request)
        {
            using (FormManagerService service = new FormManagerService(new FormManagerServiceSettings { AutomatedInput = true } ))
            {
                var response = service.CreateForm(request);
                Assert.IsNotNull(response);
                Assert.AreEqual(response.StatusCode, ResponseStatusCodes.OK);

                var formDefId = response.Value[0].Id;

                var newformResponse = service.NewForm(formDefId);
                Assert.IsNotNull(newformResponse);
                Assert.AreEqual(newformResponse.StatusCode, ResponseStatusCodes.OK);

                var idFormInstance = newformResponse.Value;
                var formInstance = SBSFormModel.Instance.GetFormInstance(idFormInstance);
                Assert.IsNotNull(formInstance);

                ManualResetEvent eventStop = new ManualResetEvent(false);
                formInstance.Submitted += form => { eventStop.Set(); };
                formInstance.Cancelled += form => { eventStop.Set(); };
                service.RunFormEnded += form =>
                {
                    if (form.Id == idFormInstance)
                        eventStop.Set();
                };

                var runformResponse = service.RunForm(idFormInstance);
                Assert.IsNotNull(runformResponse);
                Assert.AreEqual(runformResponse.StatusCode, ResponseStatusCodes.OK);

                bool rc = eventStop.WaitOne(5 * 1000);
                Assert.IsTrue(rc, "The test timed out");
            }
        }

        [TestMethod]
        public void TestChangeFormAndFormField()
        {
            FormCreateRequest request = new FormCreateRequest
            {
                Forms = new List<FormTemplateFormDefinition>
                {
                    new FormTemplateFormDefinition
                    {
                        Title = "Test Marc Form",
                        Name = "MarcForm",
                        Description = "This is a test form",
                        Fields = new List<FormTemplateFieldDefinition>
                        {
                            new FormTemplateFieldDefinition
                            {
                                Name = "cbSmallBiz",
                                FieldType = "checkbox",
                                Prompt = "Are you a small business?"
                            }
                        }
                    }
                }
            };

            using (FormManagerService service = new FormManagerService(new FormManagerServiceSettings{NoCreateRestService = true}))
            {
                var response = service.CreateForm(request);
                Assert.IsNotNull(response);
                Assert.AreEqual(response.StatusCode, ResponseStatusCodes.OK);
                var formDefId = response.Value[0].Id;

                const string newTitle = "Test Geeta Form";
                const string newPrompt = "Are you are large business?";
                var responseChange = service.ChangeForm(formDefId, new Dictionary<string, object>
                {
                    { "title", newTitle },
                    { "cbSmallBiz.prompt", newPrompt }
                });
                Assert.IsNotNull(responseChange);
                Assert.AreEqual(responseChange.StatusCode, ResponseStatusCodes.OK);
                Assert.IsTrue(responseChange.Value);

                var responseDef = service.FindFormDefinitionById(formDefId);
                Assert.IsNotNull(responseDef);
                Assert.AreEqual(responseDef.StatusCode, ResponseStatusCodes.OK);
                Assert.IsNotNull(responseDef.Value);
                Assert.AreEqual(responseDef.Value.Definition.Title, newTitle);
                Assert.AreEqual(responseDef.Value.Definition.Fields[0].Prompt, newPrompt);
            }
        }

        [TestMethod]
        public void TestRepeaterGroup()
        {
            FormCreateRequest request = new FormCreateRequest
            {
                Forms = new List<FormTemplateFormDefinition>
                {
                    new FormTemplateFormDefinition
                    {
                        Title = "Test Repeater Marc Form",
                        Name = "RepeaterGroupForm",
                        Description = "This is a form for testing a repeater group",
                        Fields = new List<FormTemplateFieldDefinition>
                        {
                            new FormTemplateFieldDefinition { Name = "fieldOne", FieldType = "edit", Prompt = "Hit anything", DefaultValue = "foo" },
                            new FormTemplateFieldDefinition
                            {
                                Name = "employeeDetailsGroup",
                                FieldType = "repeater",
                                Properties = new Properties
                                {
                                    { "groupname", "Employees" },
                                    { "suffix", "${index}"     },
                                    { "end", "cbAddAnother"    },
                                    { "continuevalue", true    }   // if the value of cbAddAnother is true, then loop back
                                }
                            },
                            new FormTemplateFieldDefinition { Name = "a", FieldType = "edit", Prompt = "a" },
                            new FormTemplateFieldDefinition { Name = "b", FieldType = "edit", Prompt = "b" },
                            new FormTemplateFieldDefinition { Name = "c", FieldType = "edit", Prompt = "c" },
                            new FormTemplateFieldDefinition
                            {
                                Name = "btnSave", FieldType = "button", Prompt = "Save Employee", SubmissionFunctions = new List<FormSubmissionFunction>
                                {
                                    new FormSubmissionFunction
                                    {
                                        Workflow = "rest://localhost:8089/DIYOnboardingService/form/${var:RepeaterGroupForm.Id}/employee/add",
                                        Properties = new Properties
                                        {
                                            { "method", "post" },
                                            { "body", "${form:RepeaterGroupForm}" },
                                            { "bodyFormat",  "json" }
                                        }
                                    }
                                }
                            },
                            new FormTemplateFieldDefinition { Name = "cbAddAnother", FieldType = "checkbox", Prompt = "Add another employee" },
                            new FormTemplateFieldDefinition { Name = "fieldTwo", FieldType = "edit", Prompt = "Hit anything", DefaultValue = "baz" },
                        }
                    }
                }
            };

            // Make sure that this request is serializable into Json
            Json.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            var json = Json.Serialize(request);
            Assert.IsNotNull(json);

            using (FormManagerService service = new FormManagerService(new FormManagerServiceSettings { NoCreateRestService = true, NoMessaging = true, AutomatedInput = true }))
            {
                // Create the form definition
                var responseCreate = service.CreateForm(request);
                Assert.IsNotNull(responseCreate);
                Assert.AreEqual(responseCreate.StatusCode, ResponseStatusCodes.OK);

                // Create an instance of the form.
                // This will exercise the logic that creates the SBSRepeaterField
                var formDefId = responseCreate.Value[0].Id;
                var responseNewForm = service.NewForm(formDefId);
                Assert.IsNotNull(responseNewForm);
                Assert.AreEqual(responseNewForm.StatusCode, ResponseStatusCodes.OK);

                var idFormInstance = responseNewForm.Value;

                // Make sure that we can find the repeater field within the form
                var responseField = service.GetField(idFormInstance, "employeeDetailsGroup");
                Assert.IsNotNull(responseField);
                Assert.AreEqual(responseField.StatusCode, ResponseStatusCodes.OK);
                Assert.IsNotNull(responseField.Value);

                var field = responseField.Value;
                Assert.IsTrue(field is SBSRepeaterField);

                // Make sure that the properties took hold
                SBSRepeaterField repeaterField = (SBSRepeaterField) field;
                Assert.AreEqual(repeaterField.EndingFieldName, "cbAddAnother");

                // Get the instance of the form
                var formInstance = SBSFormModel.Instance.GetFormInstance(idFormInstance);
                Assert.IsNotNull(formInstance);

                // Prepare for the form to run asynchronously
                ManualResetEvent eventStop = new ManualResetEvent(false);
                formInstance.Submitted += form => { eventStop.Set(); };
                formInstance.Cancelled += form => { eventStop.Set(); };
                service.RunFormEnded += form =>
                {
                    if (form.Id == idFormInstance)
                        eventStop.Set();
                };

                // Run the form asynchronously
                var runformResponse = service.RunForm(idFormInstance);
                Assert.IsNotNull(runformResponse);
                Assert.AreEqual(runformResponse.StatusCode, ResponseStatusCodes.OK);

                // Wait for the form to be finished
                bool rc = eventStop.WaitOne();
                Assert.IsTrue(rc, "The test failed");
            }
        }

        [TestMethod]
        public void TestBuildDefinitionWithAction()
        {
            const string json =
            @"{
              'forms': [
                {
                    'name': 'frmDIYCustomerOnboarding',
                    'title': 'ADP Do-it-yourself Payroll Registration for Small Businesses',
                    'subtitle': 'The perfect payroll solution for businesses with 50 or less employees',
                    'description': 'This forms onboards a new customer for ADP\'s RUN payroll system',
                    'fields': [
                        {
                          'name': 'cbSignatorySameAsOwner',
                          'prompt': 'Are you, as the owner, the signatory for the company',
                          'hint': 'If you are the signatory, then you can skip the next section.',
                          'type': 'checkbox',
                          'actions': {
                            'onTrue': {
                              'jump': 'sectionAfterSignatory'
                            }
                           }
                        }
                    ]
                }
              ]
            }";

            FormCreateRequest request = Json.Deserialize<FormCreateRequest>(json);
            Assert.IsNotNull(request);
            Assert.IsNotNull(request.Forms);
            Assert.IsTrue(request.Forms.Count == 1);
            Assert.IsTrue(request.Forms[0].Fields.Count > 0);

            var fldCheckbox = request.Forms[0].Fields[0];
            Assert.IsNotNull(fldCheckbox.Actions);
            Assert.IsTrue(fldCheckbox.Actions.Count > 0);
            Assert.IsNotNull(fldCheckbox.Actions["onTrue"]);

            var propOnTrue = fldCheckbox.Actions["onTrue"];
            JObject jObject = propOnTrue as JObject;
            Assert.IsNotNull(jObject);

            var target = jObject["jump"].Value<string>();
            Assert.IsNotNull(target);
            Assert.IsTrue(target == "sectionAfterSignatory");

            dynamic dynJobject = propOnTrue;
            var target2 = dynJobject.jump;
            Assert.IsNotNull(target2);
            Assert.IsTrue(target2 == "sectionAfterSignatory");
        }
    }
}

