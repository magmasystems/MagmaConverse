using System;
using System.Collections.Generic;
using MagmaConverse.Data;
using MagmaConverse.Data.Fields;
using MagmaConverse.Framework;
using MagmaConverse.Utilities;

namespace MagmaConverse.Views.Console
{
    public class ConsoleFormView : SBSFormViewBase
    {
        #region Variables
        private bool ExitRequested { get; set; }
        internal IntPtr ConsoleWindow { get; set; }
        private List<IConsoleFieldView> FieldViews { get; } = new List<IConsoleFieldView>();
        #endregion

        #region Constructors
        public ConsoleFormView(ISBSForm form) : base(form)
        {
        }
        #endregion

        #region Methods
        public override void Render()
        {
            this.ConsoleWindow = ConsoleHelpers.SpawnConsole();

            System.Console.CancelKeyPress += (sender, args) =>
            {
                if (ConsoleHelpers.Choice("CTRL+C Pressed - Do you want to exit (y/n)?"))
                    this.ExitRequested = true;
            };

            // Take each SBSFormField and create a console-specific view of the fields
            foreach (var field in this.Form.Fields)
            {
                this.Bind(field);
            }

            // Starting at the first field, display each field and get the optional input.
            // Note that we need to account for looping within repeater fields
            this.Render(this.FieldViews);
 
            ConsoleHelpers.KillConsole(this.ConsoleWindow);
            this.FireClosedEvent();
        }

        protected virtual void Render(List<IConsoleFieldView> fields)
        {
            this.Render(fields, 0, this.FieldViews.Count, 0);
        }

        protected virtual void Render(List<IConsoleFieldView> fields, int idxStart, int idxEnd, int level)
        {
            int idx = idxStart;
            while (!this.ExitRequested && idx <= idxEnd && idx < fields.Count)
            {
                IConsoleFieldView field = fields[idx];
                field.Render();
                if (field is ConsoleRepeaterField repeater)
                {
                    // The max repeater iterations can be controlled by a global property defined in the App Context
                    int maxIterations = ApplicationContext.MaxRepeaterIterations;

                    while (!repeater.IsEnded && repeater.IterationCount < maxIterations)
                    {
                        this.Render(fields, idx + 1, repeater.EndingIndex, level + 1);
                        repeater.IterationCount++;
                    }
                    idx = repeater.EndingIndex;
                }
                var result = field.PerformActions(idx);
                if (result != null && result.Success && result.NewIndex != idx)
                    idx = result.NewIndex;  // PerformActions may have jumped to a new field
                else
                    idx++;                  // go to next field in the list
            }
        }

        protected override void Bind(ISBSFormField field)
        {
            IConsoleFieldView fieldView = null;

            switch (field)
            {
                case SBSUploadField _:
                    fieldView = new ConsoleUploadField(field, this);
                    break;
                case SBSEditField _:
                    if (field is SBSPasswordEditField)
                        fieldView = new ConsolePasswordField(field, this);
                    else
                        fieldView = new ConsoleEditField(field, this);
                    break;
                case SBSSectionField _:
                    fieldView = new ConsoleSectionField(field, this);
                    break;
                case SBSLabelField _:
                    fieldView = new ConsoleLabelField(field, this);
                    break;
                case SBSComboboxField _:
                case SBSListboxField _:
                    fieldView = new ConsoleListboxField(field, this);
                    break;
                case SBSButtonField _:
                    fieldView = new ConsoleButtonField(field, this);
                    break;
                case SBSCheckboxField _:
                    fieldView = new ConsoleCheckboxField(field, this);
                    break;
                case SBSRadioButtonField _:
                    fieldView = new ConsoleRadiobuttonField(field, this, this.FieldViews);
                    break;
                case SBSRepeaterField _:
                    fieldView = new ConsoleRepeaterField(field, this);
                    break;
                default:
                    ConsoleHelpers.ColoredWriteLine($"Bind: The field {field.Prompt} of type {field.FieldTypeName} was not recognized", ConsoleColor.Magenta);
                    break;
            }

            if (fieldView != null)
            {
                this.FieldViews.Add(fieldView);
            }
        }
        #endregion
    }
}
