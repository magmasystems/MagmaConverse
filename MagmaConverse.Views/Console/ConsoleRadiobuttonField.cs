using System;
using System.Collections.Generic;
using MagmaConverse.Data;

namespace MagmaConverse.Views.Console
{
    public class ConsoleRadiobuttonField : ConsoleFieldViewBase
    {
        private List<IConsoleFieldView> FieldViews { get; }

        public ConsoleRadiobuttonField(ISBSFormField formField, ISBSFormView formView, List<IConsoleFieldView> fieldViews) : base(formField, formView)
        {
            this.FieldViews = fieldViews;
        }

        protected bool IsStartOfRadioGroup()
        {
            return this.SBSFormField.PropExists("start");
        }

        public override void Render()
        {
            if (!this.CanDisplay)
                return;

            // Ignore any radio button fields that are not the first member of a radio group
            if (!this.IsStartOfRadioGroup())
                return;

            // Scan the list of fields to find the end of the radio group. The end is either a field that is not a radio button or the start of another radio group.
            int idxStart = -1;
            int idxEnd = -1;
            for (int i = 0; i < this.FieldViews.Count; i++)
            {
                IConsoleFieldView fv = this.FieldViews[i];
                if (fv == this)
                {
                    idxStart = idxEnd = i;
                }
                else if (idxStart >= 0)
                {
                    if (!(fv is ConsoleRadiobuttonField rb) || rb.IsStartOfRadioGroup())
                        break;
                    idxEnd = i;
                }
            }

            // Print out the members of the radio group
            for (int i = idxStart; i <= idxEnd; i++)
            {
                this.ColoredOutput($"{i - idxStart + 1}) {this.FieldViews[i].SBSFormField.Prompt}", ConsoleColor.Cyan);
            }

            // Get the input. The inoput should be a number between 1 and the count of radio buttons in the group.
            bool isValid = false;
            while (!isValid)
            {
                string line = this.Input();
                if (!string.IsNullOrEmpty(line))
                    continue;

                if (int.TryParse(line, out int answer))
                    continue;

                if (answer < 1 || answer > idxEnd - idxStart + 1)
                    continue;

                // Get the actual index of the answer within the entire list of fields.
                answer = idxStart + answer - 1;

                this.SBSFormField.Value = this.FieldViews[answer].SBSFormField.Prompt;
                isValid = this.Validate();
            }
        }
    }
}