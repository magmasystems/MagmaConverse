using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MagmaConverse.Data;

namespace MagmaConverse.Models
{
    public sealed class SBSFormDefinitionModel : SBSPersistableModelBase<SBSFormDefinition>
    {
        #region Events
        #endregion

        #region Variables
        #endregion

        #region Constructors
        private static SBSFormDefinitionModel m_instance;
        public static SBSFormDefinitionModel Instance => m_instance ?? (m_instance = new SBSFormDefinitionModel());

        private SBSFormDefinitionModel()
        {
            this.GenerateMapOfProperties();
        }
        #endregion

        #region Cleanup
        #endregion

        #region Methods        
        public string CreateFormTemplate(FormTemplateFormDefinition def)
        {
            if (this.Repository.ContainsKey(def.Name))
            {
                throw new ApplicationException($"The form with the name {def.Name} already exists");
            }

            var form = new SBSFormDefinition(def);
            this.Repository.Add(form.Id, form);

            return form.Id;
        }

        public override SBSFormDefinition GetById(string idDefinition)
        {
            var def = base.GetById(idDefinition);
            if (def == null)
            {
                // TODO - Use the persistence driver to find the definition in the database.
                // Add to the repo if found.
            }

            return def;
        }

        public SBSFormDefinition GetByName(string name)
        {
            foreach (var form in this.Repository.Values)
            {
                if (form.Definition.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    return form;
            }
            return null;
        }

        public bool ChangeForm(string id, Dictionary<string, object> changes)
        {
            var def = this.GetById(id);
            if (def == null)
            {
                return false;
            }

            return this.ChangeFormProperties(def.Definition, changes);
        }
        #endregion

        #region Change a form definition's properties
        public bool ChangeFormProperties(FormTemplateFormDefinition def, Dictionary<string, object> changes)
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

                    var field = def.Fields.FirstOrDefault(f => f.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
                    if (field != null)
                    {
                        rc = this.ChangeFormOrFieldProp("Field", propName, kvp.Value, field, FieldDefinitionPropsMap);
                    }
                    else
                    {
                        Logger.Error($"Change Field Definition Property: The key {propName} is not valid.");
                        rc = false;
                    }
                }
                else
                {
                    rc = this.ChangeFormOrFieldProp("Form", kvp.Key, kvp.Value, def, FormDefinitionPropsMap);
                }
            }

            return rc;
        }

        private bool ChangeFormOrFieldProp(string type, string propName, object value, object target, Dictionary<string, PropertyInfo> map)
        {
            if (map.TryGetValue(propName, out var propInfo))
            {
                try
                {
                    propInfo.SetValue(target, value);
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
        /// We want to create a map of all of the DataMember[] properties of the FormTemplateFormDefinition
        /// </summary>
        private readonly Dictionary<string, PropertyInfo> FormDefinitionPropsMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PropertyInfo> FieldDefinitionPropsMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        private void GenerateMapOfProperties()
        {
            foreach (var prop in typeof(FormTemplateFormDefinition).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                DataMemberAttribute attr = prop.GetCustomAttribute(typeof(DataMemberAttribute)) as DataMemberAttribute;
                if (attr == null)
                    continue;
                this.FormDefinitionPropsMap.Add(attr.Name, prop);
            }

            foreach (var prop in typeof(FormTemplateFieldDefinition).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                DataMemberAttribute attr = prop.GetCustomAttribute(typeof(DataMemberAttribute)) as DataMemberAttribute;
                if (attr == null)
                    continue;
                this.FieldDefinitionPropsMap.Add(attr.Name, prop);
            }
        }
        #endregion

        #region Persistence
        public override bool LoadFromDatabase()
        {
            return base.LoadFromDatabase();
#if DEPRECATED
            this.Logger.Info("SBSFormDefinitionModel - starting to load from the database");

            try
            {
                var list = this.DatabaseAdapter.Load();
                if (list == null)
                {
                    this.Logger.Info($"SBSFormDefinitionModel - cannot load from the database. Make sure the database server is running.");
                    return false;
                }

                var sbsFormDefinitions = list as IList<SBSFormDefinition> ?? list.ToList();
                if (sbsFormDefinitions.Count == 0)
                    return true;

                this.Repository.Clear();

                foreach (var formDef in sbsFormDefinitions)
                {
                    if (!this.Repository.ContainsKey(formDef.Id))
                        this.Repository.Add(formDef.Id, formDef);
                }
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                throw;
            }

            this.Logger.Info("SBSFormDefinitionModel - successfully loaded from the database");
            return true;
#endif
        }

        public override bool LoadFromDatabase(string id)
        {
            return base.LoadFromDatabase(id);
#if DEPRECATED
            this.Logger.Info($"SBSFormDefinitionModel - starting to load definition {id} from the database");

            try
            {
                var formDef = this.DatabaseAdapter.FindById(id);
                if (formDef == null)
                {
                    this.Logger.Info($"SBSFormDefinitionModel - cannot load definition {id} from the database");
                    return false;
                }

                if (!this.Repository.ContainsKey(formDef.Id))
                    this.Repository.Add(formDef.Id, formDef);
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                throw;
            }

            this.Logger.Info($"SBSFormDefinitionModel - successfully loaded definition {id} from the database");
            return true;
#endif
        }

        public override bool SaveToDatabase()
        {
            this.Logger.Info("SBSFormDefinitionModel - starting to save to Mongo");
            var rc = true;

            try
            {
                foreach (var formDef in this.Repository.Values)
                {
                    rc = this.SaveToDatabase(formDef);
                    if (rc == false)
                        break;
                }

                this.Logger.Info("SBSFormDefinitionModel - successfully saved to Mongo");
                return rc;
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                return false;
            }
        }

        public override bool SaveToDatabase(SBSFormDefinition formDef)
        {
            return base.SaveToDatabase(formDef);
#if DEPRECATED
            this.Logger.Info($"SBSFormDefinitionModel - starting to save form definition {formDef.Definition.Name} to Mongo");

            try
            {
                this.DatabaseAdapter.Save(formDef);
                this.Logger.Info($"SBSFormDefinitionModel - form definition {formDef.Definition.Name} successfully saved to Mongo");
                return true;
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                return false;
            }
#endif
        }
#endregion
    }
}
