using System;
using MagmaConverse.Data;

namespace MagmaConverse.Views.Console
{
    public class ConsoleCheckboxField : ConsoleFieldViewBase
    {
        public ConsoleCheckboxField(ISBSFormField formField, ISBSFormView formView) : base(formField, formView)
        {
        }

        public override void Render()
        {
            if (!this.CanDisplay)
                return;

            string orig = this.SBSFormField.Prompt;
            this.SBSFormField.Prompt += " (y/n)";
            base.Render();
            this.SBSFormField.Prompt = orig;

            bool isValid = false;
            while (!isValid)
            {
                var ch = this.ReadKey('Y');
                switch (char.ToLower(ch))
                {
                    case 'y':
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
            System.Console.WriteLine();
        }
    }
}