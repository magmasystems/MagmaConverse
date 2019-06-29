using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Web;
using System.Threading.Tasks;
using MagmaConverse.Controllers;
using MagmaConverse.Data;
using MagmaConverse.Data.Workflow;
using Magmasystems.Framework;
using Magmasystems.Framework.Core;
using MagmaConverse.Interfaces;
using MagmaConverse.Models;

using ISBSForm = MagmaConverse.Data.ISBSForm;

namespace MagmaConverse.Services
{
    public interface IFormManagerService : ISBSService
    {
        event Action<ISBSForm> RunFormEnded;

        ResponseStatus<NameIdPair[]> CreateForm(FormCreateRequest request, bool save = false);
        ResponseStatus ClearFormDefinitions();
        ResponseStatus<string> NewForm(string id);
        ResponseStatus<string> LoadFormDefinitionFromFile(string filename);
        ResponseStatus<SBSFormDefinition> FindFormDefinitionById(string idDefinition);
        ResponseStatus<SBSFormDefinition> FindFormDefinitionByName(string name);
        ResponseStatus<List<SBSFormDefinition>> GetAllFormDefinitions();
        ResponseStatus<bool> DeleteFormDefinition(string id);
        ResponseStatus<bool> DeleteFormDefinitions(string[] ids);
        ResponseStatus<bool> ChangeForm(string id, Dictionary<string, object> changes);

        ResponseStatus<bool> ActivateUser(string formId, string activationCode);

        ResponseStatus<SBSFormField> GetField(string idInstance, string fieldName);
        ResponseStatus<bool> AddFields(string idInstance, FormAddFieldsRequest request);
        ResponseStatus<bool> InsertFields(string idInstance, FormAddFieldsRequest request);
        ResponseStatus ClearFields(string idInstance);
        ResponseStatus<bool> DeleteField(string idInstance, string fieldName);

        ResponseStatus<bool> RunForm(string idInstance, string driver = "terminal");
        ResponseStatus<bool> SaveForm(string idInstances);
        ResponseStatus<SBSForm> GetForm(string idInstance);
        ResponseStatus<Dictionary<string, object>> GetFieldValues(string idInstance);

        ResponseStatus<string> GetAuthorizationToken(string user, string password, string resource);
        ResponseStatus<bool> DeleteDatabase(string token);
        ResponseStatus<bool> DeleteCollection(string token, string collection);

        ResponseStatus<bool> SetFormManagerServiceSettings(FormManagerServiceSettings settings);

        void LoadData();
    }

    public class FormManagerService : SBSServiceBase<SBSFormModel, SBSForm>, IFormManagerService
    {
        #region Events
        public event Action<ISBSForm> RunFormEnded = form => { };
        #endregion

        #region Variables
        private SBSFormDefinitionModel      FormDefinitionModel { get; }
        private SBSFormModel                FormModel { get; }
        private SBSFormReferenceDataModel   RefDataModel { get; }
        #endregion

        #region Constructors
        public FormManagerService(IFormManagerServiceSettings settings = null) : base("FormManagerService", /*SBSFormModel.Instance*/null, settings ?? FormManagerServiceSettings.FromConfig())
        {

            AppDomain.CurrentDomain.AssemblyLoad += (sender, eventArgs) =>
            {
                var types = ReflectionHelpers.GetTypesInAssemblyThatImplementAnAttribute(eventArgs.LoadedAssembly, typeof(SBSWorkflowAttribute));
                if (types == null)
                    return;
                foreach (var type in types)
                {
                    var name = type.GetCustomAttribute<SBSWorkflowAttribute>().Name;
                    WorkflowRepository.Instance.Add(name, type.AssemblyQualifiedName);
                }
            };

            this.FormDefinitionModel = SBSModelCreator.Instance.Create<SBSFormDefinitionModel>(settings);
            this.FormModel = SBSModelCreator.Instance.Create<SBSFormModel>(settings);
            this.RefDataModel = SBSModelCreator.Instance.Create<SBSFormReferenceDataModel>(settings);
        }

        #endregion

        #region Cleanup
        public override void Dispose()
        {
            foreach (var model in SBSModelCreator.Instance)
            {
                model.Dispose();
            }

            base.Dispose();
        }
        #endregion

        #region Initialization
        public void LoadData()
        {
            foreach (var model in SBSModelCreator.Instance)
            {
                model.InitialDataLoad();
            }
        }
        #endregion

        #region Methods that are called by the Form Manager Rest Service
        public ResponseStatus<NameIdPair[]> CreateForm(FormCreateRequest request, bool save = false)
        {
            if (request == null)
            {
                return new ResponseStatus<NameIdPair[]>(ResponseStatusCodes.Error, "The Form Defintion Request was null");
            }

            bool isValid = request.Validate(out var errors);
            if (!isValid)
            {
                return new ResponseStatus<NameIdPair[]>(ResponseStatusCodes.Error, string.Join(Environment.NewLine, errors));
            }

            // Add any ad-hoc global configuration options to the ApplicationContext's dictionary of global props
            if (request.Properties != null)
            {
                foreach (var kvp in request.Properties)
                {
                    ApplicationContext.Add(kvp.Key, kvp.Value);
                }
            }

            if (request.DebuggingEnabled)
            {
                ApplicationContext.DebugEnabled = true;
            }

            // Add the reference data and save it to the database
            this.RefDataModel.AddData(request);
            this.RefDataModel.SaveToDatabase();

            var responseIds = new List<NameIdPair>();
            foreach (var form in request.Forms)
            {
                // Create the SBSFormDefinition (which contains the template) and get the id of the SBSFormDefinition object
                var id = this.FormDefinitionModel.CreateFormTemplate(form);
                responseIds.Add(new NameIdPair(form.Name, id));

                if (!save || string.IsNullOrEmpty(id))
                    continue;

                // Save the form definition to the database
                try
                {
                    var sbsFormDef = this.FormDefinitionModel.GetById(id);
                    this.FormDefinitionModel.SaveToDatabase(sbsFormDef);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }
            }

            return new ResponseStatus<NameIdPair[]>(responseIds.ToArray());
        }

        public ResponseStatus ClearFormDefinitions()
        {
            this.FormDefinitionModel.Clear();
            return new ResponseStatus(null);
        }

        public ResponseStatus<List<SBSFormDefinition>> GetAllFormDefinitions()
        {
            var formDefs = this.FormDefinitionModel.GetAll();
            return new ResponseStatus<List<SBSFormDefinition>>(formDefs);
        }

        public ResponseStatus<bool> DeleteFormDefinition(string id)
        {
            var rc = this.FormDefinitionModel.Remove(id);
            return new ResponseStatus<bool>(rc);
        }

        public ResponseStatus<bool> DeleteFormDefinitions(string[] ids)
        {
            var rc = this.FormDefinitionModel.Remove(ids);
            return new ResponseStatus<bool>(rc);
        }

        public ResponseStatus<bool> ChangeForm(string id, Dictionary<string, object> changes)
        {
            bool rc;

            // Decide whether the id represents a form definition or a form instance, and route appropriately
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (this.FormModel.GetFormInstance(id) != null)
                rc = this.FormModel.ChangeForm(id, changes);
            else
                rc = this.FormDefinitionModel.ChangeForm(id, changes);
            return new ResponseStatus<bool>(rc);
        }

        public ResponseStatus<bool> ActivateUser(string formId, string activationCode)
        {
            bool isActivated = this.FormModel.ActivateUser(formId, activationCode);
            return new ResponseStatus<bool>(isActivated);
        }

        public ResponseStatus<string> NewForm(string idDefinition)
        {
            // We need to look up the form definition (by id) in the FormDef repo
            //   If it's not there, then return an error
            // Call the form model to materialize the form
            // Return the form instance ID to the caller

            var response = this.FindFormDefinitionById(idDefinition);
            if (response.StatusCode == ResponseStatusCodes.Error)
            {
                return new ResponseStatus<string>(ResponseStatusCodes.Error, response.ErrorMessage);
            }

            var form = this.FormModel.MaterializeForm(response.Value);
            return new ResponseStatus<string>(form.Id);
        }

        public ResponseStatus<string> LoadFormDefinitionFromFile(string filename)
        {
            if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                filename += ".json";

            if (!File.Exists(filename))
            {
                string errorMsg = $"Cannot find a form definition in the file {filename}";
                Logger.Error(errorMsg);
                return new ResponseStatus<string>(ResponseStatusCodes.Error, errorMsg);
            }

            return new ResponseStatus<string>(File.ReadAllText(filename));

        }

        public ResponseStatus<SBSFormDefinition> FindFormDefinitionById(string idDefinition)
        {
            var def = this.FormDefinitionModel.GetById(idDefinition);
            if (def == null)
            {
                string errorMsg = $"Cannot find a form definition with the id {idDefinition}";
                Logger.Error(errorMsg);
                return new ResponseStatus<SBSFormDefinition>(ResponseStatusCodes.Error, errorMsg);
            }

            return new ResponseStatus<SBSFormDefinition>(def);
        }

        public ResponseStatus<SBSFormDefinition> FindFormDefinitionByName(string name)
        {
            var def = this.FormDefinitionModel.GetByName(name);
            if (def == null)
            {
                string errorMsg = $"Cannot find a form definition with the name {name}";
                Logger.Error(errorMsg);
                return new ResponseStatus<SBSFormDefinition>(ResponseStatusCodes.Error, errorMsg);
            }

            return new ResponseStatus<SBSFormDefinition>(def);
        }

        public ResponseStatus<bool> AddFields(string idInstance, FormAddFieldsRequest request)
        {
            var rc = this.FormModel.AddFields(idInstance, request);
            return new ResponseStatus<bool>(rc);
        }

        public ResponseStatus<bool> InsertFields(string idInstance, FormAddFieldsRequest request)
        {
            var rc = this.FormModel.InsertFields(idInstance, request);
            return new ResponseStatus<bool>(rc);
        }

        public ResponseStatus<SBSFormField> GetField(string idInstance, string fieldName)
        {
            var rc = this.FormModel.GetField(idInstance, fieldName);
            return new ResponseStatus<SBSFormField>((SBSFormField) rc);
        }

        public ResponseStatus<bool> DeleteField(string idInstance, string fieldName)
        {
            var rc = this.FormModel.DeleteField(idInstance, fieldName);
            return new ResponseStatus<bool>(rc);
        }

        public ResponseStatus ClearFields(string idInstance)
        {
            this.FormModel.ClearFields(idInstance);
            return ResponseStatus.OK;
        }

        public ResponseStatus<bool> RunForm(string idInstance, string driver = "terminal")
        {
            if (!this.FormModel.InstanceExists(idInstance))
            {
                return new ResponseStatus<bool>(ResponseStatusCodes.Error, $"The form with instance id {idInstance} does not exist");
            }

            switch (driver.ToLower())
            {
                case "terminal":
                    Task.Factory.StartNew(() =>
                    {
                        var controller = new ConsoleFormController(idInstance);
                        controller.FormClosed += form =>
                        {
                            this.RunFormEnded(controller.Form);
                        };
                        controller.Run();
                    });
                    break;
                // ReSharper disable once RedundantEmptySwitchSection
                default:
                    break;
            }

            return new ResponseStatus<bool>(true);
        }

        public ResponseStatus<bool> SaveForm(string idInstance)
        {
            if (!this.FormModel.InstanceExists(idInstance))
            {
                return new ResponseStatus<bool>(ResponseStatusCodes.Error, $"The form with instance id {idInstance} does not exist");
            }

            var form = this.FormModel.GetFormInstance(idInstance);
            var rc = this.FormModel.SaveToDatabase(form);

            return new ResponseStatus<bool>(rc);
        }

        public ResponseStatus<SBSForm> GetForm(string idInstance)
        {
            if (!this.FormModel.InstanceExists(idInstance))
            {
                return new ResponseStatus<SBSForm>(ResponseStatusCodes.Error, $"The form with instance id {idInstance} does not exist");
            }

            return new ResponseStatus<SBSForm>(this.FormModel.GetFormInstance(idInstance));
        }

        public ResponseStatus<Dictionary<string, object>> GetFieldValues(string idInstance)
        {
            if (!this.FormModel.InstanceExists(idInstance))
            {
                return new ResponseStatus<Dictionary<string, object>>(ResponseStatusCodes.Error, $"The form with instance id {idInstance} does not exist");
            }

            var form = this.FormModel.GetFormInstance(idInstance);
            Dictionary<string, object> nvs = form.Fields.ToDictionary(field => field.Name, field => field.Value);
            return new ResponseStatus<Dictionary<string, object>>(nvs);
        }
        #endregion

        #region Persistence Backdoors to help drive Postman
        public ResponseStatus<string> GetAuthorizationToken(string user, string password, string resource)
        {
            // TODO - look up the user and the domain and return a token

            string token = user.GetHashCode().ToString();
            return new ResponseStatus<string>(token);
        }

        public ResponseStatus<bool> DeleteDatabase(string token)
        {
            if (ApplicationContext.Configuration.NoPersistence)
                return new ResponseStatus<bool>(false);

            // TODO- look up token and see if it gives the user auth to delete the database
            if (!this.IsAuthorized(token, "database", "delete"))
            {
                this.SetHttpStatusCode(HttpStatusCode.Forbidden);
                return new ResponseStatus<bool>(ResponseStatusCodes.Error, "Not authorized to delete a database");
            }

            var rc = FormDefinitionModel.DatabaseAdapter.DeleteDatabase();
            return new ResponseStatus<bool>(rc);
        }

        public ResponseStatus<bool> DeleteCollection(string token, string collection)
        {
            if (ApplicationContext.Configuration.NoPersistence)
                return new ResponseStatus<bool>(false);

            if (string.IsNullOrEmpty(collection))
            {
                this.SetHttpStatusCode(HttpStatusCode.BadRequest);
                return new ResponseStatus<bool>(ResponseStatusCodes.Error, "Collection name was not specified");
            }

            // TODO- look up token and see if it gives the user auth to delete the database
            if (!this.IsAuthorized(token, "collection", "delete"))
            {
                this.SetHttpStatusCode(HttpStatusCode.Forbidden);
                return new ResponseStatus<bool>(ResponseStatusCodes.Error, "Not authorized to delete a collection");
            }

            bool rc;

            switch (collection.ToLower())
            {
                case "definitions":
                    rc = this.FormDefinitionModel.DatabaseAdapter.DeleteCollection();
                    break;
                case "instances":
                    rc = this.FormModel.DatabaseAdapter.DeleteCollection();
                    break;
                case "refdata":
                    rc = this.RefDataModel.DatabaseAdapter.DeleteCollection();
                    break;
                default:
                    return new ResponseStatus<bool>(ResponseStatusCodes.Error, $"There is no entity named {collection}");
            }

            return new ResponseStatus<bool>(rc);
        }

        // ReSharper disable UnusedParameter.Local
        private bool IsAuthorized(string token, string resource, string action)
            // ReSharper restore UnusedParameter.Local
        {
            return true;
        }
        // ReSharper enable UnusedParameter.Local

        private void SetHttpStatusCode(HttpStatusCode code)
        {
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.StatusCode = code;
        }
        #endregion

        #region Control Settings
        public ResponseStatus<bool> SetFormManagerServiceSettings(FormManagerServiceSettings settings)
        {
            ApplicationContext.IsInAutomatedMode = settings.AutomatedInput;

            return new ResponseStatus<bool>(true);
        }
        #endregion
    }
}
