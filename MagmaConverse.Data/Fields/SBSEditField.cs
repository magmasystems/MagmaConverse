namespace MagmaConverse.Data.Fields
{
    public class SBSEditField : SBSPersistableFormField
    {
        protected override void InitializeValidations(FormTemplateFieldDefinition fieldDef)
        {
            // Set up the validations
            if (fieldDef.Validation.Required)
                this.ValidationRules.Add(ValidatorFactory.Create("Required"));
            if (fieldDef.Validation.Length.HasValue && fieldDef.Validation.Length != 0)
                this.ValidationRules.Add(ValidatorFactory.Create("Length", fieldDef.Validation.Length.Value));
            if (fieldDef.Validation.MinLength.HasValue && fieldDef.Validation.MinLength != 0)
                this.ValidationRules.Add(ValidatorFactory.Create("MinLength", fieldDef.Validation.MinLength.Value));
            if (fieldDef.Validation.MaxLength.HasValue && fieldDef.Validation.MaxLength != 0)
                this.ValidationRules.Add(ValidatorFactory.Create("MaxLength", fieldDef.Validation.MaxLength.Value));
            if (!string.IsNullOrEmpty(fieldDef.Validation.RegEx))
                this.ValidationRules.Add(ValidatorFactory.Create("RegEx", fieldDef.Validation.RegEx));

            base.InitializeValidations(fieldDef);
        }
    }
}