namespace MagmaConverse.Data.Fields
{
    public class SBSIntegerEditField : SBSEditField
    {
        protected override void InitializeValidations(FormTemplateFieldDefinition fieldDef)
        {
            base.InitializeValidations(fieldDef);
            
            // Set up the validations
            this.ValidationRules.Add(ValidatorFactory.Create("RegEx", @"^(\+|-)?\d+$"));
        }
    }
}