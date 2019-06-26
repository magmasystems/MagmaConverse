using System;
using MagmaConverse.Data;
using MagmaConverse.Data.Fields;

namespace MagmaConverse.Views.Console
{
    public class ConsoleListboxField : ConsoleFieldViewBase
    {
        public ConsoleListboxField(ISBSFormField formField, ISBSFormView formView) : base(formField, formView)
        {
        }

        public override void Render()
        {
            if (!this.CanDisplay)
                return;

            base.Render();  // print out the prompt

            if (!(this.SBSFormField is SBSListboxField listboxField))
                return;

            // Now print out the choices
            foreach (var pair in listboxField.Items)
            {
                this.ColoredOutput(pair.Name, ConsoleColor.Blue);
            }

            // Get the input until we get a valid key
            bool isValid = false;
            while (!isValid)
            {
                string line = this.InputWithDefault();
                object val = listboxField.LookupKey(line);
                if (val != null)
                    this.SBSFormField.Value = val;
                isValid = this.Validate();
            }
        }
    }
}