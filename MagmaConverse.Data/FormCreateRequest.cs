using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MagmaConverse.Framework.Core;
using MagmaConverse.Persistence.Interfaces;
using MagmaConverse.Framework;
using Newtonsoft.Json;
using SwaggerWcf.Attributes;

namespace MagmaConverse.Data
{
    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition("FormCreateRequest")]
    public class FormCreateRequest
    {
        [DataMember(Name = "referencedata", IsRequired = false)]
        [SwaggerWcfProperty()]
        public List<FormCreationReferenceData> ReferenceData { get; set; }

        [DataMember(Name = "forms", IsRequired = true)]
        [SwaggerWcfProperty()]
        public List<FormTemplateFormDefinition> Forms { get; set; }

        // An optional list of properties that control the application globally (not per form)
        [DataMember(Name = "properties", IsRequired = false)]
        [SwaggerWcfProperty()]
        public Properties Properties { get; set; }

        [DataMember(Name = "debug", IsRequired = false, EmitDefaultValue = true)]
        [SwaggerWcfProperty()]
        public bool DebuggingEnabled { get; set; }

        //[JsonIgnore]
        //public List<Note> notes { get; set; }

        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            foreach (var form in this.Forms)
            {
                form.Validate(ref errors);
            }

            return errors.Count == 0;
        }
    }

    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition("FormTemplateFormDefinition")]
    public class FormTemplateFormDefinition
    {
        [DataMember(Name = "name", IsRequired = true)]
        [SwaggerWcfProperty()]
        public string Name { get; set; }

        [DataMember(Name = "title")]
        [SwaggerWcfProperty()]
        public string Title { get; set; }

        [DataMember(Name = "subtitle")]
        [SwaggerWcfProperty()]
        public string SubTitle { get; set; }

        [DataMember(Name = "description")]
        [SwaggerWcfProperty()]
        public string Description { get; set; }

        [DataMember(Name = "fields")]
        [SwaggerWcfProperty()]
        public List<FormTemplateFieldDefinition> Fields { get; set; }

        public bool Validate(ref List<string> errors)
        {
            bool rc = true;

            if (string.IsNullOrEmpty(this.Name))
            {
                errors.Add("The form name cannot be null");
                rc = false;
            }

            return rc;
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Title)}: {Title}, {nameof(SubTitle)}: {SubTitle}, {nameof(Description)}: {Description}, {nameof(Fields)}: {Fields}";
        }
    }

    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition("FormTemplateFieldDefinition")]
    public class FormTemplateFieldDefinition
    {
        [DataMember(Name = "name", IsRequired = true)]
        [SwaggerWcfProperty()]
        public string Name { get; set; }

        [DataMember(Name = "prompt", IsRequired = false)]
        [SwaggerWcfProperty()]
        public string Prompt { get; set; }

        [DataMember(Name = "hint")]
        [SwaggerWcfProperty()]
        public string Hint { get; set; }

        [DataMember(Name = "type")]
        [SwaggerWcfProperty()]
        public string FieldType { get; set; }

        [DataMember(Name = "default")]
        [SwaggerWcfProperty()]
        public object DefaultValue { get; set; }

        [DataMember(Name = "hidden")]
        [SwaggerWcfProperty()]
        public bool Hidden { get; set; }

        [DataMember(Name = "disabled")]
        [SwaggerWcfProperty()]
        public bool Disabled { get; set; }

        [DataMember(Name = "validation")]
        [SwaggerWcfProperty()]
        public FieldValidations Validation { get; set; } = new FieldValidations();

        [DataMember(Name = "autocomplete")]
        [SwaggerWcfProperty()]
        public string Autocomplete { get; set; }

        [DataMember(Name = "ref")]
        [SwaggerWcfProperty()]
        public string Reference { get; set; }

        [DataMember(Name = "items")]
        [SwaggerWcfProperty()]
        public List<object> Items { get; set; }

        [DataMember(Name = "actions")]
        [SwaggerWcfProperty()]
        public FieldActions Actions { get; set; }

        [DataMember(Name = "submissionFunctions")]
        [SwaggerWcfProperty()]
        public List<FormSubmissionFunction> SubmissionFunctions { get; set; }

        [DataMember(Name = "properties")]
        [SwaggerWcfProperty()]
        public Properties Properties { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public string __comment { get; set; }
    }

    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition("FieldValidations")]
    public class FieldValidations
    {
        [DataMember(Name = "required")]
        [SwaggerWcfProperty()]
        public bool Required { get; set; }

        [DataMember(Name = "minlength")]
        [SwaggerWcfProperty()]
        public int? MinLength { get; set; }

        [DataMember(Name = "maxlength")]
        [SwaggerWcfProperty()]
        public int? MaxLength { get; set; }

        [DataMember(Name = "length")]
        [SwaggerWcfProperty()]
        public int? Length { get; set; }

        [DataMember(Name = "rules")]
        [SwaggerWcfProperty()]
        public List<FieldValidationRule> Rules { get; set; }

        [DataMember(Name = "regex")]
        [SwaggerWcfProperty()]
        public string RegEx { get; set; }

        [DataMember(Name = "validationFunctions")]
        [SwaggerWcfProperty()]
        public List<FieldValidationFunction> ValidationFunctions { get; set; }
    }

    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition("FieldValidationRule")]
    public class FieldValidationRule
    {
        [DataMember(Name = "rule")]
        [SwaggerWcfProperty()]
        public string Rule { get; set; }

        [DataMember(Name = "error")]
        [SwaggerWcfProperty()]
        public string ErrorMessage { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public object CompiledRule { get; set; }
    }

    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition("FieldValidationFunction")]
    public class FieldValidationFunction
    {
        [DataMember(Name = "function")]
        [SwaggerWcfProperty()]
        public string Function { get; set; }

        [DataMember(Name = "method")]
        [SwaggerWcfProperty()]
        public string Method { get; set; }

        [DataMember(Name = "returnType")]
        [SwaggerWcfProperty()]
        public string ReturnType { get; set; }

        [DataMember(Name = "async")]
        [SwaggerWcfProperty()]
        public bool Async { get; set; }

        [DataMember(Name = "timeout")]
        [SwaggerWcfProperty()]
        public int Timeout { get; set; }

        [DataMember(Name = "failOnFalse")]
        [SwaggerWcfProperty()]
        public bool FailOnFalse { get; set; }
    }

    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition("FormSubmissionFunction")]
    public class FormSubmissionFunction : IFormSubmissionFunction
    {
        [DataMember(Name = "workflow")]
        [SwaggerWcfProperty()]
        public string Workflow { get; set; }

        [DataMember(Name = "async")]
        [SwaggerWcfProperty()]
        public bool Async { get; set; }

        [DataMember(Name = "timeout")]
        [SwaggerWcfProperty()]
        public int Timeout { get; set; }

        [DataMember(Name = "failOnFalse")]
        [SwaggerWcfProperty()]
        public bool FailOnFalse { get; set; }

        [DataMember(Name = "cancel")]
        [SwaggerWcfProperty()]
        public bool Cancel { get; set; }

        [DataMember(Name = "properties")]
        [SwaggerWcfProperty()]
        public Properties Properties { get; set; }

        public override string ToString() =>
            $"{nameof(Properties)}: {Properties.ToFormattedString()}, {nameof(Workflow)}: {Workflow}, {nameof(Cancel)}: {Cancel}, {nameof(Async)}: {Async}, {nameof(Timeout)}: {Timeout}, {nameof(FailOnFalse)}: {FailOnFalse}";
    }

    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition("FormCreationReferenceData")]
    public class FormCreationReferenceData : IPersistableDocumentObject, IHasId, IHasName
    {
        [DataMember(Name = "name")]
        [SwaggerWcfProperty()]
        public string Name { get; set; }

        [DataMember(Name = "sortedDictionary")]
        [SwaggerWcfProperty()]
        public SortedDictionary<string, object> SortedDictionary { get; set; }

        [DataMember(Name = "array")]
        [SwaggerWcfProperty()]
        public List<object> Array { get; set; }

        [JsonIgnore]
        public string Id { get => this.id; set => this.id = value; }

        [JsonIgnore]
        public string id { get; set; }

        public FormCreationReferenceData()
        {
            this.id = string.IsNullOrEmpty(this.Name) ? IdGenerators.RefDataId() : IdGenerators.RefDataId($"ReferenceData.{this.Name}.");
        }
    }

    [Serializable]
    public class FieldActions : Properties
    {
    }

    [Serializable]
    [DataContract]
    [SwaggerWcfDefinition("FormAddFieldsRequest")]
    public class FormAddFieldsRequest
    {
        [DataMember(Name = "target", IsRequired = false)]
        [SwaggerWcfProperty()]
        public string Target { get; set; }

        [DataMember(Name = "index", IsRequired = false)]
        [SwaggerWcfProperty()]
        public int Index { get; set; } = -999;

        [DataMember(Name = "insertMode", IsRequired = false)]
        [SwaggerWcfProperty()]
        public InsertMode InsertMode { get; set; } = InsertMode.Before;

        [DataMember(Name = "fields", IsRequired = true)]
        [SwaggerWcfProperty()]
        public List<FormTemplateFieldDefinition> Fields { get; set; }

        // An optional list of properties that control the application globally (not per form)
        [DataMember(Name = "properties", IsRequired = false)]
        [SwaggerWcfProperty()]
        public Properties Properties { get; set; }

        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            foreach (var field in this.Fields)
            {
            }

            return errors.Count == 0;
        }
    }
}
