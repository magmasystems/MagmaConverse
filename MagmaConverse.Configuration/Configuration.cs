using System;
using System.Configuration;
using System.Reflection;

namespace MagmaConverse.Configuration
{
    public class MagmaConverseConfiguration : ConfigurationSection
    {
        public static MagmaConverseConfiguration Read()
        {
            // There is an issue with .NET Core Unit Tests and config files, so you need to open the exe config
            if (!(ConfigurationManager.GetSection("MagmaConverse") is MagmaConverseConfiguration config))
            {
                var location = Assembly.GetEntryAssembly().Location;
                var appDomainName = AppDomain.CurrentDomain.FriendlyName;
                if (appDomainName == "testhost")
                    location = location.Replace("testhost", "MagmaConverse.Tests");
                config = ConfigurationManager.OpenExeConfiguration(location).GetSection("MagmaConverse") as MagmaConverseConfiguration;
            }

            if (config == null)
            {
                config = new MagmaConverseConfiguration();
            }

            return config;
        }

        public static ConfigurationSection GetSection(string sectionName)
        {
            // There is an issue with .NET Core Unit Tests and config files, so you need to open the exe config
            if (!(ConfigurationManager.GetSection(sectionName) is ConfigurationSection section))
            {
                var location = Assembly.GetEntryAssembly().Location;
                var appDomainName = AppDomain.CurrentDomain.FriendlyName;
                if (appDomainName == "testhost")
                    location = location.Replace("testhost", "MagmaConverse.Tests");
                section = ConfigurationManager.OpenExeConfiguration(location).GetSection(sectionName) as ConfigurationSection;
            }

            return section;
        }

        public string Evaluate(string configPath)
        {
            string[] parts = configPath.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            switch (parts[0].ToLower())
            {
                case "server":
                    return this.Servers.Get(parts[1])?.Url;
                case "workflow":
                    return this.Workflows.Get(parts[1])?.Type;
                default:
                    return configPath;
            }
        }


        [ConfigurationProperty("automatedInput", IsRequired = false, DefaultValue = false)]
        public bool AutomatedInput
        {
            get => (bool)this["automatedInput"];
            set => this["automatedInput"] = value;
        }

        [ConfigurationProperty("maxRepeaterIterations", IsRequired = false, DefaultValue = int.MaxValue)]
        public int MaxRepeaterIterations
        {
            get => (int)this["maxRepeaterIterations"];
            set => this["maxRepeaterIterations"] = value;
        }

        [ConfigurationProperty("noCreateRestService", IsRequired = false, DefaultValue = false)]
        public bool NoCreateRestService
        {
            get => (bool)this["noCreateRestService"];
            set => this["noCreateRestService"] = value;
        }

        [ConfigurationProperty("noMessaging", IsRequired = false, DefaultValue = false)]
        public bool NoMessaging
        {
            get => (bool) this["noMessaging"];
            set => this["noMessaging"] = value;
        }

        [ConfigurationProperty("noPersistence", IsRequired = false, DefaultValue = false)]
        public bool NoPersistence
        {
            get => (bool) this["noPersistence"];
            set => this["noPersistence"] = value;
        }

        [ConfigurationProperty("purgeDatabaseOnStartup", IsRequired = false, DefaultValue = false)]
        public bool PurgeDatabaseOnStartup
        {
            get => (bool) this["purgeDatabaseOnStartup"];
            set => this["purgeDatabaseOnStartup"] = value;
        }

        [ConfigurationProperty("useMocksForRestCalls", IsRequired = false, DefaultValue = false)]
        public bool UseMocksForRestCalls
        {
            get => (bool)this["useMocksForRestCalls"];
            set => this["useMocksForRestCalls"] = value;
        }

        // Collections 

        [ConfigurationProperty("workflows", IsDefaultCollection = false, IsRequired = false)]
        [ConfigurationCollection(typeof(WorkflowConfigurations), AddItemName = "workflow")]
        public WorkflowConfigurations Workflows
        {
            get => this["workflows"] as WorkflowConfigurations;
            set => this["workflows"] = value;
        }

        [ConfigurationProperty("servers", IsDefaultCollection = false, IsRequired = false)]
        [ConfigurationCollection(typeof(ServerConfigurations), AddItemName = "server")]
        public ServerConfigurations Servers
        {
            get => this["servers"] as ServerConfigurations;
            set => this["servers"] = value;
        }

        #region Workflows
        [ConfigurationCollection(typeof(WorkflowConfiguration))]
        public class WorkflowConfigurations : ConfigurationElementCollection
        {
            internal const string PropertyName = "workflow";

            protected override ConfigurationElement CreateNewElement()
            {
                return new WorkflowConfiguration();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                WorkflowConfiguration sub = (WorkflowConfiguration)element;
                return sub;
            }

            protected override string ElementName => PropertyName;

            protected override bool IsElementName(string elementName)
            {
                return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
            }

            public WorkflowConfiguration this[int index]
            {
                get => (WorkflowConfiguration)this.BaseGet(index);
                set
                {
                    if (index < this.Count && this.BaseGet(index) != null)
                    {
                        this.BaseRemove(index);
                    }
                    BaseAdd(index, value);
                }
            }

            public WorkflowConfiguration Get(string name)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        return this[i];
                }
                return null;
            }

            public new int Count => base.Count;

            public int IndexOf(WorkflowConfiguration endPoint)
            {
                return this.BaseIndexOf(endPoint);
            }

            public bool Contains(WorkflowConfiguration item)
            {
                return this.BaseIndexOf(item) >= 0;
            }

            public new bool IsReadOnly => true;
        }

        public class WorkflowConfiguration : ConfigurationElement
        {
            [ConfigurationProperty("name", IsRequired = true)]
            public string Name
            {
                get => this["name"] as string;
                set => this["name"] = value;
            }

            [ConfigurationProperty("type", IsRequired = true)]
            public string Type
            {
                get => this["type"] as string;
                set => this["type"] = value;
            }

            [ConfigurationProperty("properties", IsRequired = false)]
            [ConfigurationCollection(typeof(PropertyConfigurations), AddItemName = "property")]
            public PropertyConfigurations Props
            {
                get => this["properties"] as PropertyConfigurations;
                set => this["properties"] = value;
            }
        }
        #endregion

        #region Servers
        [ConfigurationCollection(typeof(ServerConfiguration))]
        public class ServerConfigurations : ConfigurationElementCollection
        {
            internal const string PropertyName = "server";

            protected override ConfigurationElement CreateNewElement()
            {
                return new ServerConfiguration();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                ServerConfiguration sub = (ServerConfiguration)element;
                return sub;
            }

            protected override string ElementName => PropertyName;

            protected override bool IsElementName(string elementName)
            {
                return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
            }

            public ServerConfiguration this[int index]
            {
                get => (ServerConfiguration)this.BaseGet(index);
                set
                {
                    if (index < this.Count && this.BaseGet(index) != null)
                    {
                        this.BaseRemove(index);
                    }
                    BaseAdd(index, value);
                }
            }

            public ServerConfiguration Get(string name)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        return this[i];
                }
                return null;
            }

            public new int Count => base.Count;

            public int IndexOf(ServerConfiguration endPoint)
            {
                return this.BaseIndexOf(endPoint);
            }

            public bool Contains(ServerConfiguration item)
            {
                return this.BaseIndexOf(item) >= 0;
            }

            public new bool IsReadOnly => true;
        }

        public class ServerConfiguration : ConfigurationElement
        {
            [ConfigurationProperty("name", IsRequired = true)]
            public string Name
            {
                get => this["name"] as string;
                set => this["name"] = value;
            }

            [ConfigurationProperty("url", IsRequired = true)]
            public string Url
            {
                get => this["url"] as string;
                set => this["url"] = value;
            }

            [ConfigurationProperty("properties", IsRequired = false)]
            [ConfigurationCollection(typeof(PropertyConfigurations), AddItemName = "property")]
            public PropertyConfigurations Props
            {
                get => this["properties"] as PropertyConfigurations;
                set => this["properties"] = value;
            }
        }
        #endregion

        #region Properties
        [ConfigurationCollection(typeof(PropertyConfiguration))]
        public class PropertyConfigurations : ConfigurationElementCollection
        {
            internal const string PropertyName = "property";

            protected override ConfigurationElement CreateNewElement()
            {
                return new PropertyConfiguration();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                PropertyConfiguration sub = (PropertyConfiguration)element;
                return sub;
            }

            protected override string ElementName => PropertyName;

            protected override bool IsElementName(string elementName)
            {
                return elementName.Equals(PropertyName, StringComparison.InvariantCultureIgnoreCase);
            }

            public PropertyConfiguration this[int index]
            {
                get => (PropertyConfiguration)this.BaseGet(index);
                set
                {
                    if (index < this.Count && this.BaseGet(index) != null)
                    {
                        this.BaseRemove(index);
                    }
                    BaseAdd(index, value);
                }
            }

            public PropertyConfiguration Get(string name)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                        return this[i];
                }
                return null;
            }

            public string Value(string name)
            {
                var prop = this.Get(name);
                return prop?.Value;
            }

            public new int Count => base.Count;

            public int IndexOf(PropertyConfiguration endPoint)
            {
                return this.BaseIndexOf(endPoint);
            }

            public bool Contains(PropertyConfiguration item)
            {
                return this.BaseIndexOf(item) >= 0;
            }

            public new bool IsReadOnly => true;

            public void Add(PropertyConfiguration prop)
            {
                this.BaseAdd(prop);
            }
        }

        public class PropertyConfiguration : ConfigurationElement
        {
            [ConfigurationProperty("name", IsRequired = true)]
            public string Name
            {
                get => this["name"] as string;
                set => this["name"] = value;
            }

            [ConfigurationProperty("value", IsRequired = true)]
            public string Value
            {
                get => this["value"] as string;
                set => this["value"] = value;
            }
        }
        #endregion
    }
}
