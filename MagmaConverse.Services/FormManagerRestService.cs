using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using MagmaConverse.Communications;
using MagmaConverse.Data;
using MagmaConverse.Framework;
using MagmaConverse.Framework.Core;
using MagmaConverse.Framework.Serialization;
using log4net;
using SwaggerWcf.Attributes;
// ReSharper disable RedundantArgumentDefaultValue

namespace MagmaConverse.Services
{
    [ServiceContract]
    public interface IFormManagerRestServiceContract
    {
        ///////////////////////////////////////////////////// FORM DEFINITIONS ///////////////////////////////////////////////////////////////

        /// <summary>
        /// Loads a form definition from a file. This definition is a template that can be used to instantiate an actual form.
        /// </summary>
        /// <param name="filename">The name of the json file that holds the definition (without the .json extension)</param> 
        /// <returns>A ResponseStatus that contains the Form Template</returns>
        [SwaggerWcfPath(summary: "Loads a form definition from a file. This definition is a template that can be used to instantiate an actual form.")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/definition/load/{filename}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<string> LoadFormDefinitionFromFile(string filename);

        /// <summary>
        /// Create a form definition. This definition is a template that can be used to instantiate an actual form.
        /// </summary>
        /// <param name="request">The defintion of one or more forms and all of their fields plus reference data</param>
        /// <returns>A ResponseStatus that contains the IDs of the templates</returns>
        [SwaggerWcfPath(summary: "Create a form definition. This definition is a template that can be used to instantiate an actual form.")]
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/form/definition/create", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<NameIdPair[]> CreateForm(FormCreateRequest request);

        /// <summary>
        /// Gets a single form template by the unique ID
        /// </summary>
        /// <param name="id">The unique id of the form definition. This id was returned by CreateForm</param>
        /// <returns>A ResponseStatus that contains the Form Definition</returns>
        [SwaggerWcfPath(summary: "Gets a single form template by the unique ID")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/definition/id/{id}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<SBSFormDefinition> FindFormDefinitionById(string id);

        /// <summary>
        /// Gets a single form template by name
        /// </summary>
        /// <param name="name">The unique name of the form definition. This name was specified by CreateForm</param>
        /// <returns>A ResponseStatus that contains the Form Definition</returns>
        [SwaggerWcfPath(summary: "Gets a single form template by name")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/definition/name/{name}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<SBSFormDefinition> FindFormDefinitionByName(string name);

        /// <summary>
        /// Clears all form definitions from the repository.
        /// </summary>
        /// <returns>A ResponseStatus that is a simple OK</returns>
        [SwaggerWcfPath(summary: "Clears all form definitions from the repository")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/definitions", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus ClearFormDefinitions();

        /// <summary>
        /// Deletes a single form definition
        /// </summary>
        /// <param name="id">The unique id of the form definition</param>
        /// <returns>A ResponseStatus with the value True if the form definition was successfully deleted</returns>
        [SwaggerWcfPath(summary: "Deletes a single form definition")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/definition/{id}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<bool> DeleteFormDefinition(string id);

        /// <summary>
        /// Deletes multiple form definitions
        /// </summary>
        /// <param name="ids">The unique ids of the form definitions, separated by a comma</param>
        /// <returns>A ResponseStatus with the value True if ALL the form definitions were successfully deleted</returns>
        [SwaggerWcfPath(summary: "Deletes multiple form definitions")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/definitions/{ids}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<bool> DeleteFormDefinitions(string ids);

        /// <summary>
        /// Gets all of the form templates in the repository
        /// </summary>
        /// <returns>A list of all of the form templates</returns>
        [SwaggerWcfPath(summary: "Gets all of the form templates in the repository")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/definitions", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<List<SBSFormDefinition>> GetAllFormDefinitions();

        /// <summary>
        /// Modifies a number of properties of a form or field definition or instance
        /// </summary>
        /// <param name="id">The unique id of the form definition</param>
        /// <param name="changes">A dictionary of changes, each being a name/value pair</param>
        /// <returns>A ResponseStatus with True in the value if changed successfully</returns>
        [SwaggerWcfPath(summary: "Modifies a number of properties of a form or field definition or instance")]
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/form/change", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        ResponseStatus<bool> ChangeForm(string id, Dictionary<string, object> changes);

        ///////////////////////////////////////////////////// FORM INSTANCES ///////////////////////////////////////////////////////////////

        /// <summary>
        /// Given the id of a form template, creates a new instance of a form (but does not run the form)
        /// </summary>
        /// <param name="id">The id of the form template (as returned by CreateForm)</param>
        /// <returns>A ResponseStatus that contains the instance id of the new form</returns>
        [SwaggerWcfPath(summary: "Given the id of a form template, creates a new instance of a form (but does not run the form)")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/new/{id}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<string> NewForm(string id);

        /// <summary>
        /// Runs a form. The form is materialized for the type of view specified in the "driver" parameter
        /// </summary>
        /// <param name="id">The id of the instance of the form</param>
        /// <param name="driver">The view driver. The default is to run a form on a console.</param>
        /// <returns>A response status that has OK</returns>
        /// <remarks>The form is run asynchronously</remarks>
        [SwaggerWcfPath(summary: "Runs a form. The form is materialized for the type of view specified in the 'driver' parameter")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/run/{id}?view={driver}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<bool> RunForm(string id, string driver = "terminal");

        /// <summary>
        /// Combines the instantiation and the running of a form
        /// </summary>
        /// <param name="id">The id of the form template (as returned by CreateForm)</param>
        /// <param name="driver">The view driver. The default is to run a form on a console.</param>
        /// <returns>A response status that has OK</returns>
        /// <remarks>The form is run asynchronously</remarks>
        [SwaggerWcfPath(summary: "Combines the instantiation and the running of a form")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/runnew/{id}?view={driver}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<bool> RunNewForm(string id, string driver = "terminal");

        /// <summary>
        /// Finds a field within a form. 
        /// </summary>
        /// <param name="idInstance">The id of the form</param>
        /// <param name="fieldName">The name of the field</param>
        /// <returns></returns>
        [SwaggerWcfPath(summary: "Finds a field within a form")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/{idInstance}/field/{fieldName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<SBSFormField> GetField(string idInstance, string fieldName);

        /// <summary>
        /// Adds a new field at the end of an instance of a form
        /// </summary>
        /// <param name="idInstance">The instance of the form (sent in the Json body)</param>
        /// <param name="fieldDef">The definition of the field</param>
        /// <returns>A ResponseStatus with True if the field was added</returns>
        [SwaggerWcfPath(summary: "Adds a new field at the end of an instance of a form")]
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/form/field/add", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        ResponseStatus<bool> AddField(string idInstance, FormTemplateFieldDefinition fieldDef);

        /// <summary>
        /// Inserts a new field at a certain location in an instance of a form
        /// </summary>
        /// <param name="idInstance">The instance of the form (sent in the Json body)</param>
        /// <param name="fieldDef">The definition of the field</param>
        /// <param name="index">The 0-based index of where to insert the field. If index is -1, then the field is added to the end.</param>
        /// <returns>A ResponseStatus with True if the field was inserted</returns>
        [SwaggerWcfPath(summary: "Inserts a new field at a certain location in an instance of a form")]
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/form/field/insert", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        ResponseStatus<bool> InsertFieldAtIndex(string idInstance, FormTemplateFieldDefinition fieldDef, int index = -1);

        /// <summary>
        /// Inserts a new field before or after a specific field in an instance of a form
        /// </summary>
        /// <param name="idInstance">The instance of the form (sent in the Json body)</param>
        /// <param name="fieldDef">The definition of the field</param>
        /// <param name="target">The name of the field to insert the new field before or after</param>
        /// <param name="mode">0 for Before and 1 for After</param>
        /// <returns>A ResponseStatus with True if the field was inserted</returns>
        [SwaggerWcfPath(summary: "Inserts a new field before or after a specific field in an instance of a form")]
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/form/field/namedinsert", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        ResponseStatus<bool> InsertFieldAtNamed(string idInstance, FormTemplateFieldDefinition fieldDef, string target, InsertMode mode = InsertMode.After);

        /// <summary>
        /// Deletes all fields in an instance of a form
        /// </summary>
        /// <param name="idInstance">The instance of the form</param>
        /// <returns>A ResponseStatus of OK</returns>
        [SwaggerWcfPath(summary: "Deletes all fields in an instance of a form")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/{idInstance}/fields", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus ClearFields(string idInstance);

        /// <summary>
        /// Deletes a single field from a form
        /// </summary>
        /// <param name="idInstance">The instance of the form</param>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>A ResponseStatus that is True if the deletion succeeded</returns>
        [SwaggerWcfPath(summary: "Deletes a single field from a form")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/{idInstance}/field/{fieldName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<bool> DeleteField(string idInstance, string fieldName);

        /// <summary>
        /// Gets the data for an instance of a form
        /// </summary>
        /// <param name="idInstance">The id of the form instance (as returned by NewForm)</param>
        /// <returns>A Json version of an SBSForm object, which contains the data of the form</returns>
        [SwaggerWcfPath(summary: "Gets the data for an instance of a form")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/{idInstance}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<SBSForm> GetForm(string idInstance);

        /// <summary>
        /// Deletes all fields in an instance of a form
        /// </summary>
        /// <param name="idInstance">The instance of the form</param>
        /// <returns>A ResponseStatus of OK</returns>
        [SwaggerWcfPath(summary: "Saves an instance of the form to the database")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/save/{idInstance}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<bool> SaveForm(string idInstance);

        /// <summary>
        /// Gets the field data for an instance of a form
        /// </summary>
        /// <param name="idInstance">The id of the form instance (as returned by NewForm)</param>
        /// <returns>A Json version of an SBSForm object, which contains the data of the form</returns>
        [SwaggerWcfPath(summary: "Gets the field data for an instance of a form")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/{idInstance}/fields", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<Dictionary<string, object>> GetFieldValues(string idInstance);

        [OperationContract]
        [WebGet(UriTemplate = "/auth/{resource}?user={user}&password={password}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<string> GetAuthorizationToken(string user, string password, string resource);

        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/database", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<bool> DeleteDatabase();

        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/collection/{collection}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<bool> DeleteCollection(string collection);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/form/manager/settings", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        ResponseStatus<bool> SetFormManagerServiceSettings(FormManagerServiceSettings settings);

        //[SwaggerWcfPath(summary: "Gets a single form template by name")]
        [OperationContract]
        [WebGet(UriTemplate = "/user/activate/{formId}/{activationCode}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ResponseStatus<bool> ActivateUser(string formId, string activationCode);
    }


    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, IncludeExceptionDetailInFaults = true, AddressFilterMode = AddressFilterMode.Any)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [SwaggerWcf("/FormManagerService")]
    public class FormManagerRestService :
#if !USE_APPCONFIG_FOR_WCF
        WCFRestService<IFormManagerRestServiceContract>,
#endif
        // ReSharper disable once RedundantExtendsListEntry
        IFormManagerRestServiceContract, IDisposable
    {
        #region Variables
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FormManagerRestService));
        public IFormManagerService Service { get; }
        #endregion

        #region Constructors
        internal FormManagerRestService(IFormManagerService service)
#if !USE_APPCONFIG_FOR_WCF
            : base(service?.Name ?? "FormManagerService", new NewtonsoftJsonBehavior())
#endif
        {
            this.Service = service;
        }

#if USE_APPCONFIG_FOR_WCF
        public FormManagerRestService()
        {
            this.Service = new FormManagerService();
        }
#endif
        #endregion

        #region Cleanup
#if USE_APPCONFIG_FOR_WCF
        public void Dispose()
        { }
#endif
        #endregion

        #region Helpers
        private void SetHttpStatusCode(HttpStatusCode code)
        {
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.StatusCode = code;
        }
        #endregion

        #region REST Methods
        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.Created, "The Name/ID pairs of the forms definitions that were created")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        [SwaggerWcfReturnType(typeof(NameIdPair[]))]
        public ResponseStatus<NameIdPair[]> CreateForm(
            [SwaggerWcfParameter(Description = "Contains the definition of one or more forms, plus optional reference data", Required = true)]
            FormCreateRequest request
            )
        {
            try
            {
                Logger.Info($"CreateForm({request}) called");
                var response = this.Service.CreateForm(request, true);

                Logger.Info($"CreateForm({request}) returned {Json.Serialize(response)}");
                this.SetHttpStatusCode(HttpStatusCode.Created);
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                throw new WebFaultException<ResponseStatus>(new ResponseStatus(exc), HttpStatusCode.BadRequest);
            }
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Removes all form definitions")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus ClearFormDefinitions()
        {
            Logger.Info("ClearFormDefinitions() called");
            ResponseStatus response = this.Service.ClearFormDefinitions();

            Logger.Info($"ClearFormDefinitions() returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The Json of the form definition")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<string> LoadFormDefinitionFromFile(
            [SwaggerWcfParameter(Description = "The filename of the form definition", Required = true)]
            string filename
        )
        {
            Logger.Info($"LoadFormDefinitionFromFile({filename}) called");
            ResponseStatus<string> response = this.Service.LoadFormDefinitionFromFile(filename);

            Logger.Info($"LoadFormDefinitionFromFile({filename}) returned {Json.Serialize(response).Substring(0, 32)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The Json of the form definition")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<SBSFormDefinition> FindFormDefinitionById(
            [SwaggerWcfParameter(Description = "The id of the form definition, as returned by the CreateForm API", Required = true)]
            string idDefinition
            )
        {
            Logger.Info($"FindFormDefinitionById({idDefinition}) called");
            ResponseStatus<SBSFormDefinition> response = this.Service.FindFormDefinitionById(idDefinition);

            Logger.Info($"FindFormDefinitionById({idDefinition}) returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The Json of the form definition")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<SBSFormDefinition> FindFormDefinitionByName(
            [SwaggerWcfParameter(Description = "The name of the form definition, as it was specified in the FormCreateRequest payload", Required = true)]
            string name
            )
        {
            Logger.Info($"FindFormDefinitionByName({name}) called");
            ResponseStatus<SBSFormDefinition> response = this.Service.FindFormDefinitionByName(name);

            Logger.Info($"FindFormDefinitionByName({name}) returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The Json of all of the form definitions")]
        public ResponseStatus<List<SBSFormDefinition>> GetAllFormDefinitions()
        {
            Logger.Info("GetAllFormDefinitions() called");
            ResponseStatus<List<SBSFormDefinition>> response = this.Service.GetAllFormDefinitions();

            Logger.Info($"GetAllFormDefinitions() returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the form definition was deleted")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> DeleteFormDefinition(
            [SwaggerWcfParameter(Description = "The id of the form definition, as returned by the CreateForm API", Required = true)]
            string id
            )
        {
            Logger.Info($"DeleteFormDefinition({id}) called");
            ResponseStatus<bool> response = this.Service.DeleteFormDefinition(id);

            Logger.Info($"DeleteFormDefinition({id}) returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the form definitions were deleted")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> DeleteFormDefinitions(
            [SwaggerWcfParameter(Description = "The id of the form definitions, as returned by the CreateForm API", Required = true)]
            string ids
            )
        {
            Logger.Info($"DeleteFormDefinitions({ids} called");
            ResponseStatus<bool> response = this.Service.DeleteFormDefinitions(ids.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries));

            Logger.Info($"DeleteFormDefinitions({ids}) returned {Json.Serialize(response)}");
            return response;
        }

        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if every form or field property was changed")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> ChangeForm(
            [SwaggerWcfParameter(Description = "The id of either a form definition or a form instance", Required = true)]
            string id,
            [SwaggerWcfParameter(Description = "A map of name/value pairs. Each name is the name of a property of a form, or a property of a field of the form. A field property is specified by fieldname.property", Required = true)]
            Dictionary<string, object> changes
            )
        {
            Logger.Info($"ChangeForm({id}) called");
            ResponseStatus<bool> response = this.Service.ChangeForm(id, changes);

            Logger.Info($"ChangeForm({id}) returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The InstanceContext ID of the newly instantiated form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<string> NewForm(
            [SwaggerWcfParameter(Description = "The id of the form definition, as returned by the CreateForm API", Required = true)]
            string idDefinition
            )
        {
            Logger.Info($"NewForm({idDefinition}) called");
            var response = this.Service.NewForm(idDefinition);

            Logger.Info($"NewForm({idDefinition}) returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the form was run successfully. The form is run asynchronously.")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> RunForm(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance,
            [SwaggerWcfParameter(Description = "The type of view to use. The default is 'terminal', which brings up a console.", Required = false)]
            string driver = "terminal"
            )
        {
            if (string.IsNullOrEmpty(driver))
                driver = "terminal";

            Logger.Info($"RunForm({idInstance}) called");
            var response = this.Service.RunForm(idInstance, driver);

            Logger.Info($"RunForm({idInstance}) returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the form was created and run successfully. The form is run asynchronously.")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> RunNewForm(
            [SwaggerWcfParameter(Description = "The id of a form definition, as returned by the CreateForm call", Required = true)]
            string idDefinition,
            [SwaggerWcfParameter(Description = "The type of view to use. The default is 'terminal', which brings up a console.", Required = false)]
            string driver = "terminal"
            )
        {
            var responseNewForm = this.NewForm(idDefinition);
            if (responseNewForm.StatusCode != ResponseStatusCodes.OK)
                return new ResponseStatus<bool>(responseNewForm.StatusCode, responseNewForm.ErrorMessage, false);

            return this.RunForm(responseNewForm.Value, driver);
        }

        /// <inheritdoc/>
        public ResponseStatus<bool> ActivateUser(string formId, string activationCode)
        {
            return this.Service.ActivateUser(formId, activationCode);
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The field that is in the instance of the form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<SBSFormField> GetField(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance,
            [SwaggerWcfParameter(Description = "The name of the field within the form", Required = true)]
            string fieldName
            )
        {
            Logger.Info($"GetField({idInstance}, {fieldName}) called");
            var response = this.Service.GetField(idInstance, fieldName);

            Logger.Info($"GetField({idInstance}, {fieldName}) returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the field was added")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> AddField(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance,
            [SwaggerWcfParameter(Description = "A description of the field", Required = true)]
            FormTemplateFieldDefinition fieldDef
            )
        {
            Logger.Info($"AddField({idInstance}) called");
            var response = this.Service.AddField(idInstance, fieldDef);

            Logger.Info($"AddField({idInstance}) returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the field was inserted")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> InsertFieldAtIndex(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance,
            [SwaggerWcfParameter(Description = "A description of the field", Required = true)]
            FormTemplateFieldDefinition fieldDef,
            [SwaggerWcfParameter(Description = "The 0-based index of the position to insert the new field", Required = true)]
            int index = -1
            )
        {
            Logger.Info($"InsertFieldAtIndex({idInstance}, index = {index}) called");

            if (index < -1)
            {
                string errorMsg = $"The index {index} cannot be less than -1";
                throw new WebFaultException<string>(errorMsg, HttpStatusCode.BadRequest);
            }

            var response = this.Service.InsertField(idInstance, fieldDef, index);

            Logger.Info($"InsertFieldAtIndex({idInstance}, index = {index}) returned {Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the field was inserted")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> InsertFieldAtNamed(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance,
            [SwaggerWcfParameter(Description = "A description of the field", Required = true)]
            FormTemplateFieldDefinition fieldDef,
            [SwaggerWcfParameter(Description = "The name of the field within the form to find. The new field will be inserted before or after this field.", Required = true)]
            string targetFieldName,
            [SwaggerWcfParameter(Description = "Specifies whether the new field should be inserted before or after the target field.", Required = true)]
            InsertMode insertMode = InsertMode.After
            )
        {
            Logger.Info($"InsertFieldAtNamed({idInstance}, target = {targetFieldName}, mode = {insertMode}) called");

            if (string.IsNullOrEmpty(targetFieldName))
            {
                string errorMsg = "The target field cannot be null or empty";
                throw new WebFaultException<string>(errorMsg, HttpStatusCode.BadRequest);
            }

            var response = this.Service.InsertField(idInstance, fieldDef, targetFieldName, insertMode);

            Logger.Info($"InsertFieldAtNamed({idInstance},target = {targetFieldName}, mode = {insertMode}) returned {Json.Serialize(response)}");
            return new ResponseStatus<bool>(true);
        }

        /// <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The fields were cleared from the form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus ClearFields(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance
            )
        {
            Logger.Info($"ClearFields({idInstance})");
            var response = this.Service.ClearFields(idInstance);

            Logger.Info($"ClearFields({idInstance}) returned {Json.Serialize(response)}");
            return response;
        }

        ///  <inheritdoc/>
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The field was cleared from the form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> DeleteField(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance,
            [SwaggerWcfParameter(Description = "The name of the field within the form", Required = true)]
            string fieldName
            )
        {
            Logger.Info($"DeleteField({idInstance}, {fieldName}) called");
            var response = this.Service.DeleteField(idInstance, fieldName);

            Logger.Info($"DeleteField({idInstance}, {fieldName}) returned {Json.Serialize(response)}");
            return response;
        }

        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Saved the form's data")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<bool> SaveForm(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance
        )
        {
            Logger.Info($"SaveForm({idInstance}) called");
            var response = this.Service.SaveForm(idInstance);

            Logger.Info($"SaveForm({idInstance}) returned {Json.Serialize(response)}");
            return response;
        }

        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Returns the form's data")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<SBSForm> GetForm(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance
            )
        {
            Logger.Info($"GetForm({idInstance}) called");
            var response = this.Service.GetForm(idInstance);

            Logger.Info($"GetForm({idInstance}) returned {Json.Serialize(response)}");
            return response;
        }

        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Returns a list of name/value pairs for the fields in the form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ResponseStatus<Dictionary<string, object>> GetFieldValues(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance
        )
        {
            Logger.Info($"GetFieldValues({idInstance}) called");
            var response = this.Service.GetFieldValues(idInstance);

            Logger.Info($"GetFieldValues({idInstance}) returned {Json.Serialize(response)}");
            return response;
        }

        public ResponseStatus<string> GetAuthorizationToken(string user, string password, string resource)
        {
            return this.Service.GetAuthorizationToken(user, password, resource);
        }

        public ResponseStatus<bool> DeleteDatabase()
        {
            string token = this.GetAuthorizationToken();
            if (string.IsNullOrEmpty(token))
            {
                return new ResponseStatus<bool>(ResponseStatusCodes.Error, "The authorization token must not be null");
            }

            return this.Service.DeleteDatabase(token);
        }

        public ResponseStatus<bool> DeleteCollection(string collection)
        {
            string token = this.GetAuthorizationToken();
            if (string.IsNullOrEmpty(token))
            {
                return new ResponseStatus<bool>(ResponseStatusCodes.Error, "The authorization token must not be null");
            }

            if (string.IsNullOrEmpty(collection))
            {
                return new ResponseStatus<bool>(ResponseStatusCodes.Error, "The collection name must not be null");
            }

            return this.Service.DeleteCollection(token, collection);
        }

        private string GetAuthorizationToken()
        {
            string token = null;
            if (WebOperationContext.Current != null)
            {
                IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                WebHeaderCollection headers = request.Headers;
                token = headers["MagmaConverse-AuthToken"];
            }

            return token;
        }
        #endregion

        #region Control Settings
        public ResponseStatus<bool> SetFormManagerServiceSettings(FormManagerServiceSettings settings)
        {
            return this.Service.SetFormManagerServiceSettings(settings);
        }
        #endregion
    }
}
