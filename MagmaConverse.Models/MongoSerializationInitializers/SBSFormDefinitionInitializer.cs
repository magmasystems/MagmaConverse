using System;
using MagmaConverse.Data;
using MagmaConverse.Persistence.Interfaces;
using MongoDB.Bson.Serialization;

namespace MagmaConverse.Models.MongoSerializationInitializers
{
    public class SBSFormDefinitionInitializer : IDocumentDatabseSerializationInitializer
    {
        class SBSFormDefinitionModelBsonClassMap : BsonClassMap
        {
            public SBSFormDefinitionModelBsonClassMap(Type classType) : base(classType)
            {
                this.AutoMap();
            }
        }

        public void Initialize(Type type)
        {
            BsonClassMap.RegisterClassMap(new SBSFormDefinitionModelBsonClassMap(typeof(SBSFormDefinition)));
            //BsonClassMap.RegisterClassMap<SBSFormDefinition>();
        }
    }
}
