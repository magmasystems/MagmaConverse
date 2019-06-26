using MagmaConverse.Data;

namespace MagmaConverse.Views.Console
{
    public class ConsoleEditField : ConsoleFieldViewBase
    {
        public ConsoleEditField(ISBSFormField formField, ISBSFormView formView) : base(formField, formView)
        {
        }

        public override void Render()
        {
            if (!this.CanDisplay)
                return;

            base.Render();

            bool isValid = false;
            while (!isValid)
            {
                string line = this.InputWithDefault();
                this.SBSFormField.Value = line;
                isValid = this.Validate();
            }
        }
    }
}