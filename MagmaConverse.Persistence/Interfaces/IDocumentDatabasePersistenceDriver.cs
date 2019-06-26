using System.Collections.Generic;

namespace MagmaConverse.Persistence.Interfaces
{
	public interface IDocumentDatabasePersistenceDriver : IPersistenceDriver
	{
        // Health
	    bool IsDatabaseAlive { get; }

        // Operations on databases
        IDocumentDatabase CreateDatabase(string databaseName, bool deleteExistingDB = false);
		IEnumerable<IDocumentDatabase> GetAllDatabases();
		IDocumentDatabase GetDatabase(string name);
		bool DropDatabase(IDocumentDatabase database);

		// Add, remove and clear document collections
		IDocumentCollection CreateCollection(IDocumentDatabase database, string collectionName, bool deleteCollection = false);
		bool DropCollection(IDocumentCollection collection);
		bool ClearCollection(IDocumentCollection collection);
		IDocumentCollection GetCollection(IDocumentDatabase database, string collectionName);
		bool CollectionExists(IDocumentDatabase database, string collectionName);

		// Get all collections in a document database
		IEnumerable<IDocumentCollection> GetAllCollections(IDocumentDatabase database);

		// Get all data in a collection
		IEnumerable<IDocument> Get(IDocumentCollection collection, string sqlQuery = null);
		IEnumerable<T> Get<T>(IDocumentCollection collection, string sqlQuery = null) where T : class;
		
		// Get a specific record
		T GetById<T>(IDocumentCollection collection, string id);
		IDocument GetById(IDocumentCollection collection, string id);

		// Update/Insert data
		IDocument Save<T>(IDocumentCollection collection, T data, bool delete = false) where T : class, IPersistableDocumentObject;
		bool Update<T>(IDocument document, T data) where T : class, IPersistableDocumentObject;

		// Delete data
		bool Delete(IDocument document);
	}
}
