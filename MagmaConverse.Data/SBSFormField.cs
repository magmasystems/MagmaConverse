using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MagmaConverse.Data.Fields;
using MagmaConverse.Framework;
using MagmaConverse.Interfaces;
using Newtonsoft.Json;

namespace MagmaConverse.Data
{
    public interface ISBSFormField
    {
        event Action<ISBSFormField> Exited;
        event Action<string, object> PropertyChanged;

        /// <summary>
        /// The name of the field
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Unique Id of the field. By default, this is a GUID, although we can override the ID creator.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The Id of the form that this field belongs to
        /// </summary>
        string FormId { get; set; }

        /// <summary>
        /// The prompt for the field. (ie: What is your name)
        /// </summary>
        string Prompt { get; set; }

        /// <summary>
        /// Optional help for a field
        /// </summary>
        string Hint { get; set; }

        /// <summary>
        /// The value that was entered into the field
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Optional default value for the field
        /// </summary>
        object DefaultValue { get; set; }

        /// <summary>
        /// True if the field is hidden
        /// </summary>
        bool Hidden { get; set; }

        /// <summary>
        /// True if enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// A list of validation rules that applies to the data that is entered in the field
        /// </summary>
        List<IFieldValidator> ValidationRules { get; set; }

        /// <summary>
        /// A list of things to do upon submission (valid only for buttons)
        /// </summary>
        List<FormSubmissionFunction> SubmissionFunctions { get; set; }

        /// <summary>
        /// A list of things to do when a field loses focus
        /// </summary>
        FieldActions Actions { get; set; }

        /// <summary>
        /// A general list of name/value properties
        /// </summary>
        Properties Properties { get; set; }

        /// <summary>
        /// True if the field is a static field, like a label or a section
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// The name of the type of the field (ie: edit, label, radiobutton)
        /// </summary>
        string FieldTypeName { get; }

        /// <summary>
        /// Validate the field's value
        /// </summary>
        /// <param name="errors">A list of error messages</param>
        /// <param name="form">The containing form</param>
        /// <returns>True if the field is valid, false if not</returns>
        bool Validate(out List<string> errors, ISBSForm form);

        /// <summary>
        /// Get a property
        /// </summary>
        object GetProp(string name);

        T GetProp<T>(string name);

        /// <summary>
        /// See if a property exists
        /// </summary>
        bool PropExists(string name);
    }

    [DataContract]
    [KnownType(typeof(SBSEditField))]
    [KnownType(typeof(SBSButtonField))]
    [KnownType(typeof(SBSCheckboxField))]
    [KnownType(typeof(SBSComboboxField))]
    [KnownType(typeof(SBSCurrencyEditField))]
    [KnownType(typeof(SBSDateEditField))]
    [KnownType(typeof(SBSEmailAddressEditField))]
    [KnownType(typeof(SBSImageField))]
    [KnownType(typeof(SBSIntegerEditField))]
    [KnownType(typeof(SBSLabelField))]
    [KnownType(typeof(SBSLinkField))]
    [KnownType(typeof(SBSListboxField))]
    [KnownType(typeof(SBSPasswordEditField))]
    [KnownType(typeof(SBSPhoneNumberEditField))]
    [KnownType(typeof(SBSRadioButtonField))]
    [KnownType(typeof(SBSRepeaterField))]
    [KnownType(typeof(SBSSectionField))]
    [KnownType(typeof(SBSUploadField))]
    public abstract class SBSFormField : ISBSFormField
    {
        #region Events
        public event Action<ISBSFormField> Exited = f => { };
        public event Action<string, object> PropertyChanged = (key, value) => { };
        #endregion

        #region Variables
        /// <inheritdoc />
        [DataMember(Name = "name")]
        public string Name { get; set; }

        /// <inheritdoc />
        [DataMember(Name = "id")]
        public string Id { get; private set; }

        /// <inheritdoc />
        [DataMember(Name = "formid")]
        public string FormId { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public ISBSForm Form { get; set; }

        /// <inheritdoc />
        [DataMember(Name = "prompt")]
        public string Prompt { get; set; }

        /// <inheritdoc />
        [DataMember(Name = "hint")]
        public string Hint { get; set; }

        /// <inheritdoc />
        [DataMember(Name = "value")]
        public virtual object Value
        {
            get => this.m_value ?? this.DefaultValue;
            set => this.m_value = value;
        }
        private object m_value { get; set; }

        /// <inheritdoc />
        [DataMember(Name = "defaultvalue")]
        public object DefaultValue { get; set; }

        /// <inheritdoc />
        [DataMember(Name = "hidden")]
        public bool Hidden { get; set; }

        /// <inheritdoc />
        [DataMember(Name = "enabled")]
        public bool Enabled { get; set; } = true;
        
        /// <inheritdoc />
        [DataMember(Name = "validationrules")]
        public List<IFieldValidator> ValidationRules { get; set; } = new List<IFieldValidator>();

        /// <inheritdoc />
        public bool IsStatic { get; protected set; }

        /// <inheritdoc />
        [DataMember(Name = "fieldtypename")]
        public string FieldTypeName { get; internal set; }

        /// <inheritdoc />
        [DataMember(Name = "properties")]
        public Properties Properties { get; set; }

        // TODO
        [DataMember(Name = "autocomplete")]
        public string Autocomplete { get; set; }

        [DataMember(Name = "reference")]
        public string Reference { get; set; }                                   // NOTE - we should subscribe to ref data changes?

        [DataMember(Name = "submissionfunctions")]
        public List<FormSubmissionFunction> SubmissionFunctions { get; set; }

        [DataMember(Name = "actions")]
        public FieldActions Actions { get; set; }
        #endregion

        #region Constructors
        protected SBSFormField()
        {
            this.Id = IdGenerators.FieldId();
        }
        #endregion

        #region Cleanup
        #endregion

        #region Methods
        public static ISBSFormField Create(ISBSForm form, FormTemplateFieldDefinition fieldDef, IHasLookup referenceDataRepo = null)
        {
            if (!(SBSFormFieldFactory.Create(form, fieldDef.FieldType) is SBSFormField field))
                return null;

            if (!string.IsNullOrEmpty(fieldDef.Name))
                field.Id = IdGenerators.FieldId($"FormField.{fieldDef.Name}");

            field.Name = fieldDef.Name; 
            field.FormId = form.Id;
            field.Prompt = fieldDef.Prompt;
            field.Hint = fieldDef.Hint;
            field.DefaultValue = fieldDef.DefaultValue;
            field.Hidden = fieldDef.Hidden;
            field.Enabled = !fieldDef.Disabled;
            field.SubmissionFunctions = fieldDef.SubmissionFunctions;
            field.Actions = fieldDef.Actions;
            field.Properties = fieldDef.Properties;

            field.InitializeValidations(fieldDef);
            field.InitializeData(fieldDef, referenceDataRepo: referenceDataRepo);

            return field;
        }

        protected virtual void InitializeValidations(FormTemplateFieldDefinition fieldDef)
        {
            if (fieldDef.Validation.Rules != null)
                this.ValidationRules.Add(ValidatorFactory.Create("Rules", fieldDef.Validation.Rules));
        }

        protected virtual void InitializeData(FormTemplateFieldDefinition fieldDef, object data = null, IHasLookup referenceDataRepo = null)
        {
        }

        public virtual bool Validate(out List<string> errors, ISBSForm form = null)
        {
            errors = null;

            foreach (var validator in this.ValidationRules)
            {
                if (validator.Validate(this, form))
                    continue;

                string error = validator.ValidationFailedMessage;
                if (errors == null)
                    errors = new List<string>();
                errors.Add(error);
            }

            return errors == null;
        }

        public object GetProp(string propname)
        {
            if (this.Properties == null)
                return null;
            return this.Properties.TryGetValue(propname, out var val) ? val : null;
        }

        public T GetProp<T>(string propname) => this.PropExists(propname) ? (T) this.GetProp(propname) : default(T);

        public bool PropExists(string propname)
        {
            return this.Properties != null && this.Properties.ContainsKey(propname);
        }
        #endregion
    }

    public abstract class SBSPersistableFormField : SBSFormField
    {
    }
}
