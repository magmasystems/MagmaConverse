using System;
using System.Collections.Generic;
using MagmaConverse.Data;
using Magmasystems.Framework;

namespace MagmaConverse.Views.Console
{
    public interface IConsoleFieldView : IFormFieldView
    {
    }

    public abstract class ConsoleFieldViewBase : IConsoleFieldView
    {
        #region Variables
        /// <summary>
        /// The field definition
        /// </summary>
        public ISBSFormField SBSFormField { get; }

        /// <summary>
        /// This is the containing form. Useful to have for rules-based validation of the field against other parts of the form.
        /// </summary>
        protected ISBSForm Form { get; }
        protected ISBSFormView FormView { get; }
        #endregion

        #region Constructors
        protected ConsoleFieldViewBase(ISBSFormField formField, ISBSFormView formView)
        {
            this.SBSFormField = formField;
            this.FormView = formView;
            this.Form = formView.Form;
        }
        #endregion
        
        #region Methods
        public virtual void Render()
        {
            if (!this.CanDisplay)
                return;

            string sDefault = SBSFormField.DefaultValue != null ? $"({SBSFormField.DefaultValue.ToString()})" : "";
            sDefault = StringSubstitutor.SubstituteVariablesInExpression(sDefault);
            string prompt = this.SBSFormField.Prompt;
            if (prompt != null && prompt.StartsWith("$html:"))
            {
                prompt = prompt.Replace("$html:", "");
                prompt = HtmlRemoval.StripTagsRegex(prompt);
            }

            this.ColoredOutput($"({this.SBSFormField.FieldTypeName}) {prompt}: {sDefault}", ConsoleColor.Cyan);
        }

        public bool IsHidden => this.SBSFormField.Hidden;
        public bool Enabled => this.SBSFormField.Enabled;
        public virtual bool CanDisplay => !this.IsHidden;

        public virtual bool Validate()
        {
            bool isValid = this.SBSFormField.Validate(out List<string> errors, this.Form);
            if (isValid)
            {
                ((SBSFormViewBase)this.FormView).FireFormFieldChanged(this, this.SBSFormField.Value);
                return true;
            }

            foreach (var error in errors)
                this.ColoredOutput(error, ConsoleColor.Red);
            this.ColoredOutput("Please enter a valid value", ConsoleColor.Yellow);
            return false;
        }

        /// <summary>
        /// Once a field has a new value, we can perform a list of actions, including jumping to a new field.
        /// </summary>
        /// <param name="idxCurrent">Possibly the index of a field that we should jump to</param>
        /// <returns></returns>
        public virtual FieldActionResult PerformActions(int idxCurrent)
        {
            if (this.SBSFormField.Actions == null)
                return null;

            var processor = new SBSFormFieldActionProcessor(this.SBSFormField, idxCurrent);
            processor.PerformActions();

            return processor.Result;
        }

        /// <summary>
        /// Reads a line of text from the console. This is a virtual function so that subclasses
        /// can support other kinds of views.
        /// </summary>
        /// <returns>The string, or null if the user pressed the Return key.</returns>
        protected virtual string Input()
        {
            // If we are in a unit test, then there is no console to read a line from.
            // As an enhancement, we can send in a list of lines to the FormView.
            if (((ConsoleFormView) this.FormView).ConsoleWindow == (IntPtr) 0)
                return string.Empty;

            if (ApplicationContext.IsInAutomatedMode)
                return string.Empty;

            // For a disabled field, we should just get a single keystroke from the user and return an empty string
            if (!this.Enabled)
            {
                System.Console.ReadKey(true);
                return string.Empty;
            }

            return System.Console.ReadLine();
        }

        /// <summary>
        /// Reads a line of text from the console. This is a virtual function so that subclasses
        /// can support other kinds of views.
        /// </summary>
        /// <returns>The string, or the default value if the user pressed the Return key.</returns>
        protected virtual string InputWithDefault()
        {
            string line = this.Input();
            if (string.IsNullOrEmpty(line) && this.SBSFormField.DefaultValue != null)
                line = StringSubstitutor.SubstituteVariablesInExpression(this.SBSFormField.DefaultValue.ToString());
            return line;
        }

        /// <summary>
        /// Reads a keystroke from the console
        /// </summary>
        /// <param name="default">The key that is returned if the user presses the RETURN key</param>
        /// <returns>The character that was pressed</returns>
        protected virtual char ReadKey(char @default = 'y')
        {
            // If we are in a unit test, then there is no console to read a char from.
            if (((ConsoleFormView) this.FormView).ConsoleWindow == (IntPtr) 0)
                return @default;

            if (ApplicationContext.IsInAutomatedMode)
                return @default;

            // For a disabled field, we should just get a single keystroke from the user and return an empty string
            if (!this.Enabled)
                return @default;

            var keyInfo = System.Console.ReadKey();
            return keyInfo.KeyChar;
        }

        /// <summary>
        /// Writes a line of text to the console. This is a virtual function, so that subclasses
        /// can do things like write to a WinForms or WPF multiline edit control. etc.
        /// </summary>
        /// <param name="s">The string to write</param>
        protected virtual void Output(string s)
        {
            System.Console.WriteLine(s);
        }

        /// <summary>
        /// Writes a line of text to the console in a color. This is a virtual function, so that subclasses
        /// can do things like write to a WinForms or WPF multiline edit control. etc.
        /// </summary>
        /// <param name="s">The string to write</param>
        /// <param name="color">The foreground color</param>
        protected virtual void ColoredOutput(string s, ConsoleColor color)
        {
            ConsoleHelpers.ColoredWriteLine(s, color);
        }
        #endregion
    }
}
