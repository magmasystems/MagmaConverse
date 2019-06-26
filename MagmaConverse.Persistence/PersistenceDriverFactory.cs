using System.Collections.Generic;
using System.Reflection;
using MagmaConverse.Persistence.Interfaces;

namespace MagmaConverse.Persistence
{
	public static class PersistenceDriverFactory
	{
        // There should be only one instance of a driver for a specific database
        private static readonly object m_lock = new object();
        private static Dictionary<string, IDocumentDatabasePersistenceDriver> TheDrivers { get; } = new Dictionary<string, IDocumentDatabasePersistenceDriver>();

		public static IDocumentDatabasePersistenceDriver Create(string driverName)
		{
			if (driverName == null)
				driverName = "default";

			switch (driverName.ToLower())
			{
				case "documentdb":
				case "docdb"     :
					return LoadDriver("DocumentDB");

				case "mongo":
				case "mongodb":
					return LoadDriver("MongoDB");

				default:
					return null;
			}
		}

		private static IDocumentDatabasePersistenceDriver LoadDriver(string driverName)
		{
		    lock (m_lock)
		    {
		        if (TheDrivers.TryGetValue(driverName, out IDocumentDatabasePersistenceDriver driver))
		            return driver;

		        Assembly assembly = Assembly.Load("MagmaConverse.Persistence." + driverName);
		        var obj = assembly.CreateInstance("MagmaConverse.Persistence." + driverName + "." + driverName + "PersistenceDriver", true);

		        driver = obj as IDocumentDatabasePersistenceDriver;

		        TheDrivers.Add(driverName, driver);
		        return driver;
		    }
		}
	}
}
