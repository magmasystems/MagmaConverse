using MagmaConverse.Data;

namespace MagmaConverse.Views
{
    /// <summary>
    /// This is a field that is contained within a particular view of the form (console, web, WPF, WinForms)
    /// </summary>
    public interface IFormFieldView
    {
        /// <summary>
        /// The underlying form field, which is really a model for the field
        /// </summary>
        ISBSFormField SBSFormField { get; }

        void Render();
        bool Validate();
        FieldActionResult PerformActions(int idxCurrent);

        bool CanDisplay { get; }
        bool IsHidden { get; }
    }
}