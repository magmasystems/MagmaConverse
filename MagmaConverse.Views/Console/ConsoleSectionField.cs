using MagmaConverse.Data;

namespace MagmaConverse.Views.Console
{
    public class ConsoleSectionField : ConsoleLabelField
    {
        public bool IsPageBreak { get; }

        public ConsoleSectionField(ISBSFormField formField, ISBSFormView formView) : base(formField, formView)
        {
            this.IsPageBreak = formField.PropExists("pagebreak");
        }

        public override void Render()
        {
            if (this.IsPageBreak)
            {
                try
                {
                    System.Console.Clear();  // When running with the Resharper unit tester, this can fail with an invalid handle
                }
                catch
                {
                }
            }

            base.Render();
        }
    }
}