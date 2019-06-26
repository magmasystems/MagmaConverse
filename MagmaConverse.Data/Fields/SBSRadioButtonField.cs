﻿namespace MagmaConverse.Data.Fields
{
    public class SBSRadioButtonField : SBSPersistableFormField
    {
        protected override void InitializeValidations(FormTemplateFieldDefinition fieldDef)
        {
            // Set up the validations
            if (fieldDef.Validation.Required)
                this.ValidationRules.Add(ValidatorFactory.Create("Required"));

            base.InitializeValidations(fieldDef);
        }
    }
}