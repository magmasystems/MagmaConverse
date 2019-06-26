namespace MagmaConverse.Data.Fields
{
    public class SBSDateEditField : SBSEditField
    {
        public bool IsCalendar { get; private set; }

        protected override void InitializeValidations(FormTemplateFieldDefinition fieldDef)
        {
            base.InitializeValidations(fieldDef);

            // Set up the validations
            // http://regexlib.com/REDetails.aspx?regexp_id=808
            this.ValidationRules.Add(ValidatorFactory.Create("RegEx", @"^(?:(?:0?[13578]|1[02])|(?:0?[469]|11)(?!\/31)|(?:0?2)(?:(?!\/3[01]|\/29\/(?:(?:0[^48]|[13579][^26]|[2468][^048])00|(?:\d{2}(?:0[^48]|[13579][^26]|[2468][^048]))))))\/(?:0?[1-9]|[12][0-9]|3[01])\/\d{4}$"));

            if (this.GetProp<bool>("calendar"))
            {
                this.IsCalendar = true;
            }
        }
    }
}