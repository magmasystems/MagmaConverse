using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MagmaConverse.Data;
using MagmaConverse.Data.Workflow;

namespace MagmaConverse.Models
{
    public class SBSFormModel : SBSPersistableModelBase<SBSForm>
    {
        #region Events
        public event Action<SBSForm> FormInstanceCreated = form => { };
        public event Action<SBSForm, string, object, object> FormPropertyChanged = (form, prop, oldValue, newValue) => { };
        public event Action<SBSFormField, string, object, object> FieldPropertyChanged = (field, prop, oldValue, newValue) => { };

        public event Action<string, string> UserActivated = (formId, code) => { };
        #endregion

        #region Variables
        private WorkflowRepository WorkflowRepository { get; }
        #endregion

        #region Constructors
        private static SBSFormModel m_instance;
        public static SBSFormModel Instance => m_instance ?? (m_instance = new SBSFormModel());

        private SBSFormModel()
        {
            this.GenerateMapOfProperties();
            this.WorkflowRepository = WorkflowRepository.Instance;
        }

        /// <summary>
        /// Do not do an initial data load
        /// </summary>
        public override void InitialDataLoad()
        {
        }
        #endregion

        #region Cleanup
        #endregion

        #region Methods        
        public SBSForm MaterializeForm(SBSFormDefinition def)
        {
            if (def == null)
                return null;

            // Create the form
            SBSForm form = new SBSForm(def);
            form.MaterializeFields(def, SBSFormReferenceDataModel.Instance);
            //if (!(SBSForm.Materialize(def, SBSFormReferenceDataModel.Instance) is SBSForm form))
            //    return null;

            // Add this form to the repo of form instances
            this.Repository.Add(form.Id, form);

            // Fire an event to tell all listeners that the form has been created
            this.FormInstanceCreated(form);

            return form;
        }

        public bool AddField(string idFormInstance, FormTemplateFieldDefinition fieldDef)
        {
            SBSForm form = this.GetFormInstance(idFormInstance);
            if (form == null)
                throw new ApplicationException($"The form with instance id {idFormInstance} does not exist");

            var field = form.AddField(fieldDef, SBSFormReferenceDataModel.Instance);
            if (field == null)
                return false;

            return true;
        }

        public bool InsertField(string idFormInstance, FormTemplateFieldDefinition fieldDef, int index = -1)
        {
            SBSForm form = this.GetFormInstance(idFormInstance);
            if (form == null)
                throw new ApplicationException($"The form with instance id {idFormInstance} does not exist");

            var field = form.InsertField(fieldDef, index, SBSFormReferenceDataModel.Instance);
            if (field == null)
                return false;

            return true;
        }

        public bool InsertField(string idFormInstance, FormTemplateFieldDefinition fieldDef, string targetFieldName, InsertMode insertMode = InsertMode.After)
        {
            SBSForm form = this.GetFormInstance(idFormInstance);
            if (form == null)
                throw new ApplicationException($"The form with instance id {idFormInstance} does not exist");

            var field = form.InsertField(fieldDef, targetFieldName, insertMode, SBSFormReferenceDataModel.Instance);
            if (field == null)
                return false;

            return true;
        }

        public ISBSFormField GetField(string idFormInstance, string fieldName)
        {
            SBSForm form = this.GetFormInstance(idFormInstance);
            if (form == null)
                throw new ApplicationException($"The form with instance id {idFormInstance} does not exist");

            return form.FindField(fieldName);
        }

        public bool DeleteField(string idFormInstance, string name)
        {
            SBSForm form = this.GetFormInstance(idFormInstance);
            if (form == null)
                throw new ApplicationException($"The form with instance id {idFormInstance} does not exist");

            return form.DeleteField(name);

        }

        public void ClearFields(string idFormInstance)
        {
            SBSForm form = this.GetFormInstance(idFormInstance);
            if (form == null)
                throw new ApplicationException($"The form with instance id {idFormInstance} does not exist");

            form.ClearFields();
        }

        public bool InstanceExists(string idInstance)
        {
            return this.Repository.ContainsKey(idInstance);
        }

        public SBSForm GetFormInstance(string idInstance)
        {
            return this.Repository.TryGetValue(idInstance, out SBSForm form) ? form : null;
        }
        #endregion

        #region Change the Properties of a form instance
        public bool ChangeForm(string id, Dictionary<string, object> changes)
        {
            var form = this.GetById(id);
            if (form == null)
            {
                return false;
            }

            return this.ChangeFormProperties(form, changes);
        }

        public bool ChangeFormProperties(SBSForm form, Dictionary<string, object> changes)
        {
            if (changes == null)
                return false;

            bool rc = true;

            foreach (var kvp in changes)
            {
                if (kvp.Key.Contains("."))
                {
                    string[] parts = kvp.Key.Split('.');
                    string fieldName = parts[0];
                    string propName = parts[1];

                    var field = form.Fields.FirstOrDefault(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                    if (field != null)
                    {
                        rc = this.ChangeFormOrFieldProp(form, "Field", propName, kvp.Value, field, FieldDefinitionPropsMap);
                    }
                    else
                    {
                        Logger.Error($"Change Field Definition Property: The key {propName} is not valid.");
                        rc = false;
                    }
                }
                else
                {
                    rc = this.ChangeFormOrFieldProp(form, "Form", kvp.Key, kvp.Value, form, FormDefinitionPropsMap);
                }
            }

            return rc;
        }

        private bool ChangeFormOrFieldProp(SBSForm form, string type, string propName, object value, object target, Dictionary<string, PropertyInfo> propMap)
        {
            if (propMap.TryGetValue(propName, out var propInfo))
            {
                try
                {
                    object oldValue = propInfo.GetValue(target);
                    // Change the actual value of a property of the form or field
                    propInfo.SetValue(target, value);

                    switch (type.ToLower())
                    {
                        case "form":
                            form.OnFormPropertyChanged(propName, oldValue, value);
                            this.FormPropertyChanged(form, propName, oldValue, value);
                            break;
                        case "field":
                            form.OnFieldPropertyChanged(target as SBSFormField, propName, oldValue, value);
                            this.FieldPropertyChanged(target as SBSFormField, propName, oldValue, value);
                            break;
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error($"Change {type} Definition Property: Error setting {propName} - {exc.Message}");
                    return false;
                }
            }
            else
            {
                Logger.Error($"Change {type} Definition Property: The key {propName} is not valid.");
                return false;
            }

            return true;
        }
        #endregion

        #region Property Maps
        /// <summary>
        /// We want to create a map of all of the public properties of the SBSForm and SBSFormField classes
        /// </summary>
        private readonly Dictionary<string, PropertyInfo> FormDefinitionPropsMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PropertyInfo> FieldDefinitionPropsMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        private void GenerateMapOfProperties()
        {
            foreach (var prop in typeof(SBSForm).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.GetSetMethod() == null)
                    continue;
                this.FormDefinitionPropsMap.Add(prop.Name, prop);
            }

            foreach (var prop in typeof(SBSFormField).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (prop.GetSetMethod() == null)
                    continue;
                this.FieldDefinitionPropsMap.Add(prop.Name, prop);
            }
        }
        #endregion

        #region Methods
        public bool ActivateUser(string formId, string activationCode)
        {
            this.UserActivated(formId, activationCode);
            return true;
        }
        #endregion
    }
}
