using System;
using System.Configuration;
using System.Linq;

namespace MagmaConverse.Persistence
{
	/*
	* To access the config information from the app, use this line of code:
	* 
		AdapterConfiguration config = ConfigurationManager.GetSection("FooSection") as AdapterConfiguration;
	*/

	public class DocumentDatabaseAdapterConfiguration : ConfigurationSection
	{
        [ConfigurationProperty("driver", IsRequired = true)]
        public string Driver
        {
            get => this["driver"] as string;
            set => this["driver"] = value;
        }

        [ConfigurationProperty("behavior", IsDefaultCollection = false, IsRequired = false)]
		public Behavior Behavior => this["behavior"] as Behavior;

	    [ConfigurationProperty("typeBehaviors", IsDefaultCollection = false, IsRequired = false)]
		[ConfigurationCollection(typeof (TypeBehavior), AddItemName = "typeBehavior")]
		public TypeBehaviors Behaviors => this["typeBehaviors"] as TypeBehaviors;

	    public TypeBehavior FindType(string name)
		{
		    return Behaviors?.Cast<TypeBehavior>().FirstOrDefault(service => service.Type.Equals(name, StringComparison.InvariantCultureIgnoreCase));
		}

		public TypeBehavior FindType(Type type)
		{
		    return Behaviors?.Cast<TypeBehavior>().FirstOrDefault(service => service.DotNetType == type);
		}
	}

	#region TypeBehaviors
	public class TypeBehaviors : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new TypeBehavior();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			TypeBehavior e = (TypeBehavior)element;
			return e.Type;
		}

		public TypeBehavior this[int index]
		{
			get => (TypeBehavior)this.BaseGet(index);
		    set
			{
				if (this.BaseGet(index) != null)
				{
					this.BaseRemove(index);
				}
				BaseAdd(index, value);
			}
		}

		public TypeBehavior Get(string name)
		{
			for (int i = 0; i < this.Count; i++)
			{
				if (this[i].Type.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return this[i];
			}
			return null;
		}


		public new int Count => base.Count;

	    public int IndexOf(TypeBehavior endPoint)
		{
			return this.BaseIndexOf(endPoint);
		}

		public bool Contains(TypeBehavior item)
		{
			return this.BaseIndexOf(item) >= 0;
		}

		public new bool IsReadOnly => true;
	}
	#endregion

	#region TypeBehavior
	public class TypeBehavior : ConfigurationElement
	{
		[ConfigurationProperty("type", IsRequired = true)]
		public string Type
		{
			get => this["type"] as string;
		    set => this["type"] = value;
		}

		[ConfigurationProperty("collectionName", IsRequired = false, DefaultValue = "DefaultCollection")]
		public string CollectionName
		{
			get => this["collectionName"] as string;
		    set => this["collectionName"] = value;
		}

		[ConfigurationProperty("databaseName", IsRequired = true)]
		public string DatabaseName
		{
			get => this["databaseName"] as string;
		    set => this["databaseName"] = value;
		}

		[ConfigurationProperty("connectionString", IsRequired = false)]
		public string ConnectionString
		{
			get => this["connectionString"] as string;
		    set => this["connectionString"] = value;
		}

		[ConfigurationProperty("serializationInitializer")]
		public string SerializerInitializer
		{
			get => this["serializationInitializer"] as string;
		    set => this["serializationInitializer"] = value;
		}

		public Type DotNetType => System.Type.GetType(this.Type);
	}
	#endregion

	public class Behavior : ConfigurationElement
	{
		[ConfigurationProperty("collectionName", IsRequired = false, DefaultValue = "DefaultCollection")]
		public string CollectionName
		{
			get => this["collectionName"] as string;
		    set => this["collectionName"] = value;
		}

		[ConfigurationProperty("databaseName", IsRequired = true)]
		public string DatabaseName
		{
			get => this["databaseName"] as string;
		    set => this["databaseName"] = value;
		}

		[ConfigurationProperty("useDatabase", IsRequired = false, DefaultValue = true)]
		public bool UseDatabase
		{
			get => (bool)this["useDatabase"];
		    set => this["useDatabase"] = value;
		}
	}
}

