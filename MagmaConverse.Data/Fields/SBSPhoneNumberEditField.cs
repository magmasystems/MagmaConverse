namespace MagmaConverse.Data.Fields
{
    public class SBSPhoneNumberEditField : SBSEditField
    {
        protected override void InitializeValidations(FormTemplateFieldDefinition fieldDef)
        {
            base.InitializeValidations(fieldDef);

            // Set up the validations
            // http://regexlib.com/REDetails.aspx?regexp_id=58

            const string regex = @"^(\(?\d{3}\)?|\d{3})( |-)?(\d{3}( |-)?\d{4}|\d{7})$";
            var validator = ValidatorFactory.Create("RegEx", regex);
            validator.ValidationFailedMessage = "This is not a valid phone number";

            this.ValidationRules.Add(validator);
        }
    }
}