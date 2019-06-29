using System;
using MagmaConverse.Data;
using MagmaConverse.Data.Fields;
using Magmasystems.Persistence.Interfaces;
using MongoDB.Bson.Serialization;

namespace MagmaConverse.Models.MongoSerializationInitializers
{
    public class SBSFormInitializer : IDocumentDatabseSerializationInitializer
    {
        class SBSFormModelBsonClassMap : BsonClassMap
        {
            public SBSFormModelBsonClassMap(Type classType) : base(classType)
            {
                this.AutoMap();
            }
        }

        public void Initialize(Type type)
        {
            BsonClassMap.RegisterClassMap(new SBSFormModelBsonClassMap(typeof(SBSForm)));
            //BsonClassMap.RegisterClassMap<SBSForm>();

            // Register all of the subclasses of SBSFormField. Otherwise, when reading from the 
            // Mongo collection, we will get an "Unknown discriminator" error.

            BsonClassMap.RegisterClassMap<SBSEditField>();
            BsonClassMap.RegisterClassMap<SBSButtonField>();
            BsonClassMap.RegisterClassMap<SBSCheckboxField>();
            BsonClassMap.RegisterClassMap<SBSComboboxField>();
            BsonClassMap.RegisterClassMap<SBSCurrencyEditField>();
            BsonClassMap.RegisterClassMap<SBSEmailAddressEditField>();
            BsonClassMap.RegisterClassMap<SBSImageField>();
            BsonClassMap.RegisterClassMap<SBSIntegerEditField>();
            BsonClassMap.RegisterClassMap<SBSLabelField>();
            BsonClassMap.RegisterClassMap<SBSLinkField>();
            BsonClassMap.RegisterClassMap<SBSListboxField>();
            BsonClassMap.RegisterClassMap<SBSPasswordEditField>();
            BsonClassMap.RegisterClassMap<SBSPhoneNumberEditField>();
            BsonClassMap.RegisterClassMap<SBSRadioButtonField>();
            BsonClassMap.RegisterClassMap<SBSSectionField>();

            BsonClassMap.RegisterClassMap<FieldLengthValidator>();
            BsonClassMap.RegisterClassMap<FieldNumericRangeValidator>();
            BsonClassMap.RegisterClassMap<FieldRegexValidator>();
            BsonClassMap.RegisterClassMap<FieldRequiredValidator>();
            BsonClassMap.RegisterClassMap<FieldRulesValidator>();
        }
    }
}
