using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using SwaggerWcf.Attributes;

using MagmaConverse.Data;
using Magmasystems.Framework;
using Magmasystems.Framework.Core;
using MagmaConverse.Services;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace MagmaConverse.Controllers
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
        ActionResult<ResponseStatus<string>> LoadFormDefinitionFromFile(string filename);

        /// <summary>
        /// Create a form definition. This definition is a template that can be used to instantiate an actual form.
        /// </summary>
        /// <param name="request">The defintion of one or more forms and all of their fields plus reference data</param>
        /// <returns>A ResponseStatus that contains the IDs of the templates</returns>
        [SwaggerWcfPath(summary: "Create a form definition. This definition is a template that can be used to instantiate an actual form.")]
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/form/definition/create", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<NameIdPair[]>> CreateForm(FormCreateRequest request);

        /// <summary>
        /// Clears all form definitions from the repository.
        /// </summary>
        /// <returns>A ResponseStatus that is a simple OK</returns>
        [SwaggerWcfPath(summary: "Clears all form definitions from the repository")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/definitions", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus> ClearFormDefinitions();

        /// <summary>
        /// Gets a single form template by the unique ID
        /// </summary>
        /// <param name="id">The unique id of the form definition. This id was returned by CreateForm</param>
        /// <returns>A ResponseStatus that contains the Form Definition</returns>
        [SwaggerWcfPath(summary: "Gets a single form template by the unique ID")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/definition/id/{id}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<SBSFormDefinition>> FindFormDefinitionById(string id);

        /// <summary>
        /// Gets a single form template by name
        /// </summary>
        /// <param name="name">The unique name of the form definition. This name was specified by CreateForm</param>
        /// <returns>A ResponseStatus that contains the Form Definition</returns>
        [SwaggerWcfPath(summary: "Gets a single form template by name")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/definition/name/{name}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<SBSFormDefinition>> FindFormDefinitionByName(string name);

        /// <summary>
        /// Deletes a single form definition
        /// </summary>
        /// <param name="id">The unique id of the form definition</param>
        /// <returns>A ResponseStatus with the value True if the form definition was successfully deleted</returns>
        [SwaggerWcfPath(summary: "Deletes a single form definition")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/definition/{id}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<bool>> DeleteFormDefinition(string id);

        /// <summary>
        /// Deletes multiple form definitions
        /// </summary>
        /// <param name="ids">The unique ids of the form definitions, separated by a comma</param>
        /// <returns>A ResponseStatus with the value True if ALL the form definitions were successfully deleted</returns>
        [SwaggerWcfPath(summary: "Deletes multiple form definitions")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/definitions/{ids}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<bool>> DeleteFormDefinitions(string ids);

        /// <summary>
        /// Gets all of the form templates in the repository
        /// </summary>
        /// <returns>A list of all of the form templates</returns>
        [SwaggerWcfPath(summary: "Gets all of the form templates in the repository")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/definitions", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<List<SBSFormDefinition>>> GetAllFormDefinitions();

        /// <summary>
        /// Modifies a number of properties of a form or field definition or instance
        /// </summary>
        /// <param name="id">The unique id of the form definition</param>
        /// <param name="changes">A dictionary of changes, each being a name/value pair</param>
        /// <returns>A ResponseStatus with True in the value if changed successfully</returns>
        [SwaggerWcfPath(summary: "Modifies a number of properties of a form or field definition or instance")]
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/form/change/{id}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        ActionResult<ResponseStatus<bool>> ChangeForm(string id, Dictionary<string, object> changes);

        ///////////////////////////////////////////////////// FORM INSTANCES ///////////////////////////////////////////////////////////////

        /// <summary>
        /// Given the id of a form template, creates a new instance of a form (but does not run the form)
        /// </summary>
        /// <param name="id">The id of the form template (as returned by CreateForm)</param>
        /// <returns>A ResponseStatus that contains the instance id of the new form</returns>
        [SwaggerWcfPath(summary: "Given the id of a form template, creates a new instance of a form (but does not run the form)")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/new/{id}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<string>> NewForm(string id);

        /// <summary>
        /// Runs a form. The form is materialized for the type of view specified in the "driver" parameter
        /// </summary>
        /// <param name="id">The id of the instance of the form</param>
        /// <returns>A response status that has OK</returns>
        /// <remarks>The form is run asynchronously</remarks>
        [SwaggerWcfPath(summary: "Runs a form. The form is materialized for the type of view specified in the 'driver' parameter")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/run/{id}?view={driver}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<bool>> RunForm(string id);

        /// <summary>
        /// Combines the instantiation and the running of a form
        /// </summary>
        /// <param name="id">The id of the form template (as returned by CreateForm)</param>
        /// <returns>A response status that has OK</returns>
        /// <remarks>The form is run asynchronously</remarks>
        [SwaggerWcfPath(summary: "Combines the instantiation and the running of a form")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/runnew/{id}?view={driver}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<bool>> RunNewForm(string id);

        /// <summary>
        /// Finds a field within a form. 
        /// </summary>
        /// <param name="idInstance">The id of the form</param>
        /// <param name="fieldName">The name of the field</param>
        /// <returns></returns>
        [SwaggerWcfPath(summary: "Finds a field within a form")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/{idInstance}/field/{fieldName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<SBSFormField>> GetField(string idInstance, string fieldName);

        /// <summary>
        /// Adds a new fields at the end of an instance of a form
        /// </summary>
        /// <param name="idInstance">The instance of the form (sent in the Json body)</param>
        /// <param name="request">The definition of the field</param>
        /// <returns>A ResponseStatus with True if the field was added</returns>
        [SwaggerWcfPath(summary: "Adds a new field at the end of an instance of a form")]
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/form/{id}/field/add", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        ActionResult<ResponseStatus<bool>> AddFields(string idInstance, FormAddFieldsRequest request);

        /// <summary>
        /// Inserts a new fields at a certain location in an instance of a form
        /// </summary>
        /// <param name="idInstance">The instance of the form (sent in the Json body)</param>
        /// <param name="request">The definition of the field</param>
        /// <returns>A ResponseStatus with True if the field was inserted</returns>
        [SwaggerWcfPath(summary: "Inserts a new field at a certain location in an instance of a form")]
        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/form/{id}/field/insert", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        ActionResult<ResponseStatus<bool>> InsertFields(string idInstance, FormAddFieldsRequest request);

        /// <summary>
        /// Deletes all fields in an instance of a form
        /// </summary>
        /// <param name="idInstance">The instance of the form</param>
        /// <returns>A ResponseStatus of OK</returns>
        [SwaggerWcfPath(summary: "Deletes all fields in an instance of a form")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/{idInstance}/fields", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus> ClearFields(string idInstance);

        /// <summary>
        /// Deletes a single field from a form
        /// </summary>
        /// <param name="idInstance">The instance of the form</param>
        /// <param name="fieldName">The name of the field</param>
        /// <returns>A ResponseStatus that is True if the deletion succeeded</returns>
        [SwaggerWcfPath(summary: "Deletes a single field from a form")]
        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/{idInstance}/field/{fieldName}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<bool>> DeleteField(string idInstance, string fieldName);

        /// <summary>
        /// Gets the data for an instance of a form
        /// </summary>
        /// <param name="idInstance">The id of the form instance (as returned by NewForm)</param>
        /// <returns>A Json version of an SBSForm object, which contains the data of the form</returns>
        [SwaggerWcfPath(summary: "Gets the data for an instance of a form")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/{idInstance}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<SBSForm>> GetForm(string idInstance);

        /// <summary>
        /// Deletes all fields in an instance of a form
        /// </summary>
        /// <param name="idInstance">The instance of the form</param>
        /// <returns>A ResponseStatus of OK</returns>
        [SwaggerWcfPath(summary: "Saves an instance of the form to the database")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/save/{idInstance}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<bool>> SaveForm(string idInstance);

        /// <summary>
        /// Gets the field data for an instance of a form
        /// </summary>
        /// <param name="idInstance">The id of the form instance (as returned by NewForm)</param>
        /// <returns>A Json version of an SBSForm object, which contains the data of the form</returns>
        [SwaggerWcfPath(summary: "Gets the field data for an instance of a form")]
        [OperationContract]
        [WebGet(UriTemplate = "/form/{idInstance}/fields", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<Dictionary<string, object>>> GetFieldValues(string idInstance);

        [OperationContract]
        [WebGet(UriTemplate = "/auth/{resource}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<string>> GetAuthorizationToken(string resource);

        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/database", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<bool>> DeleteDatabase();

        [OperationContract]
        [WebInvoke(Method = "DELETE", UriTemplate = "/form/collection/{collection}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<bool>> DeleteCollection(string collection);

        [OperationContract]
        [WebInvoke(Method = "PUT", UriTemplate = "/form/manager/settings", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        ActionResult<ResponseStatus<bool>> SetFormManagerServiceSettings(FormManagerServiceSettings settings);

        //[SwaggerWcfPath(summary: "Gets a single form template by name")]
        [OperationContract]
        [WebGet(UriTemplate = "/user/activate/{formId}/{activationCode}", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        ActionResult<ResponseStatus<bool>> ActivateUser(string formId, string activationCode);
    }
}
