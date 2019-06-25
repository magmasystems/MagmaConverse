using System;
namespace MagmaConverse.Framework
{
    public class Shims
    {
        public Shims()
        {
        }
    }
}

namespace SwaggerWcf
{
    namespace Attributes
    {
        public class SwaggerWcfDefinition : Attribute
        {

        }

        public class SwaggerWcfProperty : Attribute
        {
            public string Description { get; set; }
        }
    }
}
