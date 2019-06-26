namespace MagmaConverse.Data.Fields
{
    public class SBSCurrencyEditField : SBSEditField
    {
        protected override void InitializeValidations(FormTemplateFieldDefinition fieldDef)
        {
            base.InitializeValidations(fieldDef);

            // Set up the validations
            // http://regexlib.com/REDetails.aspx?regexp_id=70
            this.ValidationRules.Add(ValidatorFactory.Create("RegEx", @"^\$?([0-9]{1,3},([0-9]{3},)*[0-9]{3}|[0-9]+)(.[0-9][0-9])?$"));
        }
    }
}