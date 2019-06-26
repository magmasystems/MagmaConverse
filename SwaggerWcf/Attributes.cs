using System;

namespace SwaggerWcf.Attributes
{
    public class SwaggerWcfDefinition : Attribute
    {
        public string Name { get; private set; }

        public SwaggerWcfDefinition()
        {
        }

        public SwaggerWcfDefinition(string name) : this()
        {
            this.Name = name;
        }
    }

    public class SwaggerWcfProperty : Attribute
    {
        public string Description { get; set; }
    }
}