using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using SwaggerWcf.Attributes;

using MagmaConverse.Data;
using MagmaConverse.Framework;
using MagmaConverse.Framework.Core;
using MagmaConverse.Services;
using Json = MagmaConverse.Framework.Serialization.Json;
using System.ServiceModel.Web;

namespace MagmaConverse.Controllers
{

    [Route("FormManagerService")]
    [ApiController]
    public class FormManagerServiceController : BaseController, IFormManagerRestServiceContract
    {
        private IFormManagerService Service { get; set; }

        public FormManagerServiceController()
        {
            this.Service = new FormManagerService();
        }

        #region REST Methods
        /// <inheritdoc/>
        [HttpPost("form/definition/create", Name = "CreateForm")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.Created, "The Name/ID pairs of the forms definitions that were created")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        [SwaggerWcfReturnType(typeof(NameIdPair[]))]
        public ActionResult<ResponseStatus<NameIdPair[]>> CreateForm(
                [SwaggerWcfParameter(Description = "Contains the definition of one or more forms, plus optional reference data", Required = true)]
                [FromBody] FormCreateRequest request)
        {
            try
            {
                Logger.Info($"CreateForm({request}) called");
                var response = this.Service.CreateForm(request, true);
                Logger.Info($"CreateForm({request}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");

                this.Response.StatusCode = (int) HttpStatusCode.Created;
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpDelete("form/definitions", Name = "ClearFormDefinitions")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Removes all form definitions")]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus> ClearFormDefinitions()
        {
            Logger.Info("ClearFormDefinitions() called");
            ResponseStatus response = this.Service.ClearFormDefinitions();

            Logger.Info($"ClearFormDefinitions() returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
            return response;
        }

        /// <inheritdoc/>
        [HttpGet("form/definition/load/{filename}", Name = "LoadFormDefinitionFromFile")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The Json of the form definition")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<string>> LoadFormDefinitionFromFile(
            [SwaggerWcfParameter(Description = "The filename of the form definition", Required = true)]
            string filename
        )
        {
            try
            {
                Logger.Info($"LoadFormDefinitionFromFile({filename}) called");
                var response = this.Service.LoadFormDefinitionFromFile(filename);
                Logger.Info($"LoadFormDefinitionFromFile({filename}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response).Substring(0, 32)}");

                this.Response.StatusCode = (int) (response.StatusCode == ResponseStatusCodes.Error ? HttpStatusCode.NotFound : HttpStatusCode.OK);
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("form/definition/id/{id}", Name = "FindFormDefinitionById")]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The Json of the form definition")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<SBSFormDefinition>> FindFormDefinitionById(
            [SwaggerWcfParameter(Description = "The id of the form definition, as returned by the CreateForm API", Required = true)]
            string id
            )
        {
            try
            { 
                Logger.Info($"FindFormDefinitionById({id}) called");
                ResponseStatus<SBSFormDefinition> response = this.Service.FindFormDefinitionById(id);

                Logger.Info($"FindFormDefinitionById({id}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
}

        /// <inheritdoc/>
        [HttpGet("form/definition/name/{name}", Name = "FindFormDefinitionByName")]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The Json of the form definition")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<SBSFormDefinition>> FindFormDefinitionByName(
            [SwaggerWcfParameter(Description = "The name of the form definition, as it was specified in the FormCreateRequest payload", Required = true)]
            string name
            )
        {
            try
            { 
                Logger.Info($"FindFormDefinitionByName({name}) called");
                ResponseStatus<SBSFormDefinition> response = this.Service.FindFormDefinitionByName(name);

                Logger.Info($"FindFormDefinitionByName({name}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("form/definitions", Name = "GetAllFormDefinitions")]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The Json of all of the form definitions")]
        public ActionResult<ResponseStatus<List<SBSFormDefinition>>> GetAllFormDefinitions()
        {
            try
            { 
                Logger.Info("GetAllFormDefinitions() called");
                ResponseStatus<List<SBSFormDefinition>> response = this.Service.GetAllFormDefinitions();

                Logger.Info($"GetAllFormDefinitions() returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpDelete("form/definition/{id}", Name = "DeleteFormDefinition")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the form definition was deleted")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> DeleteFormDefinition(
            [SwaggerWcfParameter(Description = "The id of the form definition, as returned by the CreateForm API", Required = true)]
            string id
            )
        {
            try
            { 
                Logger.Info($"DeleteFormDefinition({id}) called");
                ResponseStatus<bool> response = this.Service.DeleteFormDefinition(id);

                Logger.Info($"DeleteFormDefinition({id}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpDelete("form/definitions/{ids}", Name = "DeleteFormDefinitions")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the form definitions were deleted")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> DeleteFormDefinitions(
            [SwaggerWcfParameter(Description = "The id of the form definitions, as returned by the CreateForm API", Required = true)]
            string ids
            )
        {
            try
            { 
                Logger.Info($"DeleteFormDefinitions({ids} called");
                ResponseStatus<bool> response = this.Service.DeleteFormDefinitions(ids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                Logger.Info($"DeleteFormDefinitions({ids}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpPut("form/change/{id}", Name = "ChangeForm")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if every form or field property was changed")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> ChangeForm(
            [SwaggerWcfParameter(Description = "The id of either a form definition or a form instance", Required = true)]
            string id,
            [SwaggerWcfParameter(Description = "A map of name/value pairs. Each name is the name of a property of a form, or a property of a field of the form. A field property is specified by fieldname.property", Required = true)]
            [FromBody] Dictionary<string, object> changes
            )
        {
            try
            { 
                Logger.Info($"ChangeForm({id}) called");
                ResponseStatus<bool> response = this.Service.ChangeForm(id, changes);

                Logger.Info($"ChangeForm({id}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("form/new/{id}", Name = "NewForm")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The InstanceContext ID of the newly instantiated form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<string>> NewForm(
            [SwaggerWcfParameter(Description = "The id of the form definition, as returned by the CreateForm API", Required = true)]
            string id
            )
        {
            try
            { 
                Logger.Info($"NewForm({id}) called");
                var response = this.Service.NewForm(id);

                Logger.Info($"NewForm({id}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("form/run/{id}", Name = "RunForm")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the form was run successfully. The form is run asynchronously.")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> RunForm(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string id
            )
        {
            try
            {
                string driver = this.Request.Query["view"];
                if (string.IsNullOrEmpty(driver))
                    driver = "terminal";

                Logger.Info($"RunForm({id}) called");
                var response = this.Service.RunForm(id, driver);

                Logger.Info($"RunForm({id}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("form/runnew/{id}", Name = "RunNewForm")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the form was created and run successfully. The form is run asynchronously.")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> RunNewForm(
            [SwaggerWcfParameter(Description = "The id of a form definition, as returned by the CreateForm call", Required = true)]
            string id
            )
        {
            var responseNewForm = this.NewForm(id).Value;
            if (responseNewForm.StatusCode != ResponseStatusCodes.OK)
            {
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return new ResponseStatus<bool>(responseNewForm.StatusCode, responseNewForm.ErrorMessage, false);
            }

            return this.RunForm(responseNewForm.Value);
        }

        /// <inheritdoc/>
        [HttpGet("user/activate/{formId}/{activationCode}", Name = "ActivateUser")]
        //[ValidateAntiForgeryToken]
        public ActionResult<ResponseStatus<bool>> ActivateUser(string formId, string activationCode)
        {
            try
            { 
                return this.Service.ActivateUser(formId, activationCode);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("form/{idInstance}/field/{fieldName}", Name = "GetField")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The field that is in the instance of the form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<SBSFormField>> GetField(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance,
            [SwaggerWcfParameter(Description = "The name of the field within the form", Required = true)]
            string fieldName
            )
        {
            try
            { 
                Logger.Info($"GetField({idInstance}, {fieldName}) called");
                var response = this.Service.GetField(idInstance, fieldName);

                Logger.Info($"GetField({idInstance}, {fieldName}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpPut("form/field/add/{id}", Name = "AddField")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the field was added")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> AddField(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string id,
            [SwaggerWcfParameter(Description = "A description of the field", Required = true)]
            [FromBody] FormTemplateFieldDefinition fieldDef
            )
        {
            try
            { 
                Logger.Info($"AddField({id}) called");
                var response = this.Service.AddField(id, fieldDef);

                Logger.Info($"AddField({id}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpPut("form/field/insert/{id}", Name = "InsertFieldAtIndex")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the field was inserted")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> InsertFieldAtIndex(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string id,
            [SwaggerWcfParameter(Description = "A description of the field", Required = true)]
            [FromBody] FormTemplateFieldDefinition fieldDef,
            [SwaggerWcfParameter(Description = "The 0-based index of the position to insert the new field", Required = true)]
            int index = -1
            )
        {
            try
            { 
                Logger.Info($"InsertFieldAtIndex({id}, index = {index}) called");

                if (index < -1)
                {
                    string errorMsg = $"The index {index} cannot be less than -1";
                    throw new WebFaultException<string>(errorMsg, HttpStatusCode.BadRequest);
                }

                var response = this.Service.InsertField(id, fieldDef, index);

                Logger.Info($"InsertFieldAtIndex({id}, index = {index}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpPut("form/field/namedinsert/{id}", Name = "InsertFieldAtNamed")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "True if the field was inserted")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> InsertFieldAtNamed(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string id,
            [SwaggerWcfParameter(Description = "A description of the field", Required = true)]
            [FromBody] FormTemplateFieldDefinition fieldDef,
            [SwaggerWcfParameter(Description = "The name of the field within the form to find. The new field will be inserted before or after this field.", Required = true)]
            string targetFieldName,
            [SwaggerWcfParameter(Description = "Specifies whether the new field should be inserted before or after the target field.", Required = true)]
            InsertMode insertMode = InsertMode.After
            )
        {
            try
            { 
                Logger.Info($"InsertFieldAtNamed({id}, target = {targetFieldName}, mode = {insertMode}) called");

                if (string.IsNullOrEmpty(targetFieldName))
                {
                    string errorMsg = "The target field cannot be null or empty";
                    throw new WebFaultException<string>(errorMsg, HttpStatusCode.BadRequest);
                }

                var response = this.Service.InsertField(id, fieldDef, targetFieldName, insertMode);

                Logger.Info($"InsertFieldAtNamed({id},target = {targetFieldName}, mode = {insertMode}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return new ResponseStatus<bool>(true);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpDelete("form/{idInstance}/fields", Name = "ClearFields")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The fields were cleared from the form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus> ClearFields(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance
            )
        {
            try
            { 
                Logger.Info($"ClearFields({idInstance})");
                var response = this.Service.ClearFields(idInstance);

                Logger.Info($"ClearFields({idInstance}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        ///  <inheritdoc/>
        [HttpDelete("form/{idInstance}/field/{fieldName}", Name = "DeleteField")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfTag("Field")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "The field was cleared from the form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> DeleteField(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance,
            [SwaggerWcfParameter(Description = "The name of the field within the form", Required = true)]
            string fieldName
            )
        {
            try
            { 
                Logger.Info($"DeleteField({idInstance}, {fieldName}) called");
                var response = this.Service.DeleteField(idInstance, fieldName);

                Logger.Info($"DeleteField({idInstance}, {fieldName}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("form/save/{idInstance}", Name = "SaveForm")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Saved the form's data")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<bool>> SaveForm(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance
        )
        {
            try
            { 
                Logger.Info($"SaveForm({idInstance}) called");
                var response = this.Service.SaveForm(idInstance);

                Logger.Info($"SaveForm({idInstance}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("form/{idInstance}", Name = "GetForm")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Returns the form's data")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<SBSForm>> GetForm(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance
            )
        {
            try
            { 
                Logger.Info($"GetForm({idInstance}) called");
                var response = this.Service.GetForm(idInstance);

                Logger.Info($"GetForm({idInstance}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("form/{idInstance}/fields", Name = "GetFieldValues")]
        //[ValidateAntiForgeryToken]
        [SwaggerWcfTag("Form")]
        [SwaggerWcfResponse(HttpStatusCode.OK, "Returns a list of name/value pairs for the fields in the form")]
        [SwaggerWcfResponse(HttpStatusCode.BadRequest, "Bad request", true)]
        [SwaggerWcfResponse(HttpStatusCode.InternalServerError, "Internal error", true)]
        public ActionResult<ResponseStatus<Dictionary<string, object>>> GetFieldValues(
            [SwaggerWcfParameter(Description = "The id of a form instance, as returned by the NewForm call", Required = true)]
            string idInstance
        )
        {
            try
            { 
                Logger.Info($"GetFieldValues({idInstance}) called");
                var response = this.Service.GetFieldValues(idInstance);

                Logger.Info($"GetFieldValues({idInstance}) returned {MagmaConverse.Framework.Serialization.Json.Serialize(response)}");
                return response;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpGet("auth/{resource}", Name = "GetAuthorizationToken")]
        //[ValidateAntiForgeryToken]
        public ActionResult<ResponseStatus<string>> GetAuthorizationToken(string resource)
        {
            try
            {
                string user = this.Request.Query["user"];
                string password = this.Request.Query["password"]; 
                return this.Service.GetAuthorizationToken(user, password, resource);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpDelete("form/database", Name = "DeleteDatabase")]
        //[ValidateAntiForgeryToken]
        public ActionResult<ResponseStatus<bool>> DeleteDatabase()
        {
            try
            { 
                string token = this.GetAuthorizationToken();
                if (string.IsNullOrEmpty(token))
                {
                    return new ResponseStatus<bool>(ResponseStatusCodes.Error, "The authorization token must not be null");
                }

                return this.Service.DeleteDatabase(token);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        /// <inheritdoc/>
        [HttpDelete("form/collection/{collection}", Name = "DeleteCollection")]
        //[ValidateAntiForgeryToken]
        public ActionResult<ResponseStatus<bool>> DeleteCollection(string collection)
        {
            try
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
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }

        private string GetAuthorizationToken()
        {
            string token = null;
            if (WebOperationContext.Current != null)
            {
                IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                WebHeaderCollection headers = request.Headers;
                token = headers["DIYOnboarding-AuthToken"];
            }

            return token;
        }
        #endregion

        #region Control Settings
        /// <inheritdoc/>
        [HttpPut("form/manager/settings", Name = "SetFormManagerServiceSettings")]
        //[ValidateAntiForgeryToken]
        public ActionResult<ResponseStatus<bool>> SetFormManagerServiceSettings([FromBody] FormManagerServiceSettings settings)
        {
            try
            { 
                return this.Service.SetFormManagerServiceSettings(settings);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                this.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return null;
            }
        }
        #endregion

    }
}
