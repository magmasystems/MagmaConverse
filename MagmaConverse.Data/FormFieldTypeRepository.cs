using System;
using MagmaConverse.Data.Fields;
using Magmasystems.Framework;
using log4net;

namespace MagmaConverse.Data
{
    public static class FormFieldTypeRepository
    {
        #region Events
        #endregion

        #region Variables
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FormFieldTypeRepository));
        private static DictionaryRepository<SBSFormFieldType> FormFieldTypes { get; } = new DictionaryRepository<SBSFormFieldType>();
        #endregion

        #region Constructor
        static FormFieldTypeRepository()
        {
            CreatePredefinedFieldTypes();
        }

        private static void CreatePredefinedFieldTypes()
        {
            // ReSharper disable ObjectCreationAsStatement
            _ = new SBSFormFieldType("Edit", typeof(SBSEditField));
            _ = new SBSFormFieldType("Password", typeof(SBSPasswordEditField));
            _ = new SBSFormFieldType("Integer", typeof(SBSIntegerEditField));
            _ = new SBSFormFieldType("Currency", typeof(SBSCurrencyEditField));
            _ = new SBSFormFieldType("PhoneNumber", typeof(SBSPhoneNumberEditField));
            _ = new SBSFormFieldType("Date", typeof(SBSDateEditField));
            _ = new SBSFormFieldType("EmailAddress", typeof(SBSEmailAddressEditField));

            _ = new SBSFormFieldType("Button", typeof(SBSButtonField));
            _ = new SBSFormFieldType("RadioButton", typeof(SBSRadioButtonField));
            _ = new SBSFormFieldType("Checkbox", typeof(SBSCheckboxField));

            _ = new SBSFormFieldType("Combo", typeof(SBSComboboxField));
            _ = new SBSFormFieldType("List", typeof(SBSListboxField));

            _ = new SBSFormFieldType("Section", typeof(SBSSectionField));
            _ = new SBSFormFieldType("Label", typeof(SBSLabelField));
            _ = new SBSFormFieldType("Link", typeof(SBSLinkField));
            _ = new SBSFormFieldType("Image", typeof(SBSImageField));

            _ = new SBSFormFieldType("Repeater", typeof(SBSRepeaterField));

            _ = new SBSFormFieldType("Upload", typeof(SBSUploadField));
            // ReSharper enable ObjectCreationAsStatement
        }
        #endregion

        #region Cleanup
        #endregion

        #region Methods
        public static bool Add(string fieldTypeName, string dotNetTypeName)
        {
            try
            {
                Type type = Type.GetType(dotNetTypeName);
                if (type == null)
                {
                    Logger.Error($"FormFieldTypeRepository: could not load type {dotNetTypeName}");
                    return false;
                }

                if (!type.IsSubclassOf(typeof(SBSFormField)))
                {
                    Logger.Error($"FormFieldTypeRepository: {dotNetTypeName} must be a subclass of SBSFormField");
                    return false;
                }

                Add(new SBSFormFieldType(fieldTypeName, type));
                return true;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return false;
            }
        }

        public static void Add(SBSFormFieldType fieldType)
        {
            if (FormFieldTypes.ContainsKey(fieldType.TypeName))
            {
                string msg = $"The field type {fieldType.TypeName} already exists in the repository";
                throw new ApplicationException(msg);
            }

            FormFieldTypes.Add(fieldType.TypeName, fieldType);
        }

        public static SBSFormFieldType Get(string name)
        {
            return FormFieldTypes.TryGetValue(name, out var fieldType) ? fieldType : null;
        }
        #endregion
    }
}
