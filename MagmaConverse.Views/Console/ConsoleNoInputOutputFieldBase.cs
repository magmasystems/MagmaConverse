using MagmaConverse.Data;

namespace MagmaConverse.Views.Console
{
    public abstract class ConsoleNoInputOutputFieldBase : ConsoleFieldViewBase
    {
        protected ConsoleNoInputOutputFieldBase(ISBSFormField formField, ISBSFormView formView) : base(formField, formView)
        {
        }

        public override bool CanDisplay => false;        // so that the field does not print anything nor access inpuut
        public override bool Validate() { return true; }
    }
}