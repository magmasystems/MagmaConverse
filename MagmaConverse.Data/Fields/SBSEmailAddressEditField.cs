namespace MagmaConverse.Data.Fields
{
    public class SBSEmailAddressEditField : SBSEditField
    {
        protected override void InitializeValidations(FormTemplateFieldDefinition fieldDef)
        {
            base.InitializeValidations(fieldDef);

            // Set up the validations
            var validator = ValidatorFactory.Create("RegEx", @"^[\w-]+@([\w-]+\.)+[\w-]+$");
            validator.ValidationFailedMessage = "This is not a valid email address";

            this.ValidationRules.Add(validator);
        }
    }
}