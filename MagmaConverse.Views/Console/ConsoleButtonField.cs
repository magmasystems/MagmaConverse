using System;
using MagmaConverse.Data;

namespace MagmaConverse.Views.Console
{
    public class ConsoleButtonField : ConsoleFieldViewBase
    {
        public ConsoleButtonField(ISBSFormField formField, ISBSFormView formView) : base(formField, formView)
        {
        }

        public override void Render()
        {
            if (!this.CanDisplay)
                return;

            base.Render();
            if (this.SBSFormField.Hidden)
                return;

            bool isValid = false;
            while (!isValid)
            {
                var ch = this.ReadKey('Y');
                switch (char.ToLower(ch))
                {
                    case 'y':
                    case ' ':
                    case '\r':
                    case '\n':
                        this.SBSFormField.Value = true;
                        break;

                    case 'n':
                        this.SBSFormField.Value = false;
                        break;

                    default:
                        this.ColoredOutput("You must type a 'y' or 'n'", ConsoleColor.Red);
                        break;

                }

                isValid = this.Validate();
            }

            if (this.SBSFormField.Value is bool b && b)
            {
                this.OnSubmit();
            }
        }

        protected void OnSubmit()
        {
            bool isCancel = !this.Form.RunWorkflow(this.SBSFormField.SubmissionFunctions);
            this.Form.Submit(isCancel);
        }
    }
}