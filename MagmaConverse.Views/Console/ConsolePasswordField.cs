using MagmaConverse.Data;
using Magmasystems.Framework;

namespace MagmaConverse.Views.Console
{
    public class ConsolePasswordField : ConsoleEditField
    {
        public ConsolePasswordField(ISBSFormField formField, ISBSFormView formView) : base(formField, formView)
        {
        }

        protected override string Input()
        {
            if (ApplicationContext.IsInAutomatedMode)
                return string.Empty;

            string input = ConsoleHelpers.ReadPassword();
            return input;
        }
    }
}