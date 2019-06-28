using System;
using System.Collections.Generic;
using MagmaConverse.Persistence.Interfaces;
using MagmaConverse.Utilities;

namespace MagmaConverse.Persistence
{
	public static class PersistenceDriverFactory
	{
        // There should be only one instance of a driver for a specific database
        private static readonly object m_lock = new object();
        private static Dictionary<string, IDocumentDatabasePersistenceDriver> TheDrivers { get; } = new Dictionary<string, IDocumentDatabasePersistenceDriver>();

		public static IDocumentDatabasePersistenceDriver Create(string driverName, DocumentDatabaseAdapterConfiguration adapterConfig)
		{
			if (driverName == null)
				driverName = "default";

			switch (driverName.ToLower())
			{
				case "documentdb":
				case "docdb"     :
					return LoadDriver("DocumentDB", adapterConfig);

				case "mongo":
				case "mongodb":
					return LoadDriver("MongoDB", adapterConfig);

				default:
					return null;
			}
		}

		private static IDocumentDatabasePersistenceDriver LoadDriver(string driverName, DocumentDatabaseAdapterConfiguration adapterConfig)
		{
		    lock (m_lock)
		    {
		        if (TheDrivers.TryGetValue(driverName, out IDocumentDatabasePersistenceDriver driver))
		            return driver;

                var driverType = TypeHelpers.LoadType2(adapterConfig.Driver);
		        driver = Activator.CreateInstance(driverType, adapterConfig) as IDocumentDatabasePersistenceDriver;
                if (driver == null)
                {
                    return null;
                }

		        TheDrivers.Add(driverName, driver);
		        return driver;
		    }
		}
	}
}
