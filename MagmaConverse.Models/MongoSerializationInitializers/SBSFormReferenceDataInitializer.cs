using System;
using MagmaConverse.Data;
using MagmaConverse.Persistence.Interfaces;
using MongoDB.Bson.Serialization;

namespace MagmaConverse.Models.MongoSerializationInitializers
{
    public class SBSFormReferenceDataInitializer : IDocumentDatabseSerializationInitializer
    {
        class SBSFormReferenceDataModelBsonClassMap : BsonClassMap
        {
            public SBSFormReferenceDataModelBsonClassMap(Type classType) : base(classType)
            {
                this.AutoMap();
            }
        }

        public void Initialize(Type type)
        {
            BsonClassMap.RegisterClassMap(new SBSFormReferenceDataModelBsonClassMap(typeof(FormCreationReferenceData)));
        }
    }
}
