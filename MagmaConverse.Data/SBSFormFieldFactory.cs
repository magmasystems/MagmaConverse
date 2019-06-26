using System;

namespace MagmaConverse.Data
{
    public class SBSFormFieldFactory
    {
        public static ISBSFormField Create(ISBSForm form, string fieldTypeName)
        {
            if (string.IsNullOrEmpty(fieldTypeName) || fieldTypeName.Equals("text", StringComparison.OrdinalIgnoreCase))
                fieldTypeName = "edit";

            var fieldType = FormFieldTypeRepository.Get(fieldTypeName);
            if (fieldType == null)
            {
                string errorMsg = $"The field type {fieldTypeName} is not recognized";
                throw new ApplicationException(errorMsg);
            }

            SBSFormField instance = Activator.CreateInstance(fieldType.DotNetType) as SBSFormField;
            if (instance != null)
            {
                instance.FieldTypeName = fieldTypeName;
                instance.Form = form;
            }

            return instance;
        }
    }
}
