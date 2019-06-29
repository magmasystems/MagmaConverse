using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Magmasystems.Framework;

namespace MagmaConverse.Data.Workflow
{
    public class WorkflowRepository
    {
        private DictionaryRepository<Type> Repository { get; } = new DictionaryRepository<Type>();
        private static WorkflowRepository s_instance;
        public static WorkflowRepository Instance => s_instance ?? (s_instance = new WorkflowRepository());

        public List<string> WorkflowNames => this.Repository.Keys.ToList();

        private WorkflowRepository()
        {
            this.Repository.Add("rest", typeof(RestWorkflow));
            this.Repository.Add("mockrest", typeof(MockRestWorkflow));

            if (ApplicationContext.Configuration.Workflows != null)
            {
                for (int i = 0;  i < ApplicationContext.Configuration.Workflows.Count;  i++)
                {
                    var workflow = ApplicationContext.Configuration.Workflows[i];
                    this.Add(workflow.Name, workflow.Type);
                }
            }
        }

        public Type GetType(string protocol)
        {
            protocol = this.StripProtocol(protocol);

            if (!this.Repository.TryGetValue(protocol, out Type type))
            {
                return null;
            }

            return type;
        }

        public IWorkflow Load(string protocol)
        {
            Type type = this.GetType(protocol);
            if (type == null)
            {
                return null;
            }

            return Activator.CreateInstance(type) as IWorkflow;
        }

        public void Add(string protocol, string typeName)
        {
            Type type = this.LoadType(typeName);
            if (type == null)
            {
                return;
            }

            protocol = this.StripProtocol(protocol);
            if (!this.Repository.ContainsKey(protocol))
            {
                this.Repository.Add(protocol, type);
            }
        }

        private string StripProtocol(string protocol)
        {
            const string protocolEnding = "://";

            if (protocol.EndsWith(protocolEnding))
            {
                protocol = protocol.Replace(protocolEnding, "");
            }

            return protocol;
        }

        private Type LoadType(string typeName)
        {
            Type type = Type.GetType(typeName);

            if (type == null)
            {
                string[] parts = typeName.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var assembly = Assembly.Load(parts[1]);
                    if (assembly != null)
                    {
                        type = assembly.GetType(parts[0]);
                    }
                }
            }

            if (type == null || !typeof(IWorkflow).IsAssignableFrom(type))
            {
                return null;
            }
            return type;
        }
    }
}
