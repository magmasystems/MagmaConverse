using System;

namespace MagmaConverse.Data.Workflow
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SBSWorkflowAttribute : Attribute
    {
        public string Name { get; }

        public SBSWorkflowAttribute(string name)
        {
            this.Name = name;
        }
    }
}
