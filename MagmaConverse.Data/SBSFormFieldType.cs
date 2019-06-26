using System;

namespace MagmaConverse.Data
{
    public class SBSFormFieldType
    {
        #region Events
        public Action<ISBSFormField> LostFocus = field => { };
        #endregion

        #region Variables
        public string TypeName { get; set; }
        public Type DotNetType { get; set; }
        #endregion

        #region Constructor
        internal SBSFormFieldType(string typename, Type dotnetType)
        {
            this.TypeName = typename.ToLower();
            this.DotNetType = dotnetType;
            FormFieldTypeRepository.Add(this);
        }
        #endregion

        #region Cleanup
        #endregion

        #region Methods
        #endregion
    }
}
