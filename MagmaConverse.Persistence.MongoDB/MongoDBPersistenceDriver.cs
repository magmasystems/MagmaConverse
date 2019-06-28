using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using MagmaConverse.Persistence.Interfaces;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace MagmaConverse.Persistence.MongoDB
{
    public class MongoDBPersistenceDriver : IDocumentDatabasePersistenceDriver
    {
        #region Events
        public event Action<ConnectionState> ConnectionStateChanged = state => { };
        #endregion

        #region Variables
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MongoDBPersistenceDriver));

        private static bool? DatabaseIsNotOnline { get; set; }
        public bool IsDatabaseAlive => DatabaseIsNotOnline.HasValue && DatabaseIsNotOnline.Value == false;

        public DatabaseVendors Vendor => DatabaseVendors.MongoDB;
        public MongoClient Client { get; protected set; }
        public bool IsConnected { get; set; }

        protected TimeSpan ConnectionTimeOutInterval { get; }
        protected string ConnectionString { get; private set; }
        protected string DriverName { get; }
        public DocumentDatabaseAdapterConfiguration AdapterConfiguration { get; }
        #endregion

        #region Constructors
        public MongoDBPersistenceDriver(DocumentDatabaseAdapterConfiguration adapterConfig)
        {
            this.DriverName = "mongodb";
            this.AdapterConfiguration = adapterConfig;
            this.ConnectionTimeOutInterval = new TimeSpan(0, 0, 10);

            this.Initialize();
        }
        #endregion

        #region Cleanup
        public void Dispose()
        {
            this.Disconnect();
            this.Client = null;
        }
        #endregion

        #region Initialization
        protected bool Initialize()
        {
            return true;
        }
        #endregion

        #region Connect/Disconnect
        public bool Connect(string connectionString = null)
        {
            // See if the database is online.
            // A future enhancement is that we can set up polling to see if the db comes back online.
            if (DatabaseIsNotOnline.HasValue && DatabaseIsNotOnline == true)
                return false;

            // If we did not pass in a connection string, try to use the previously-used connection string
            if (string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(this.ConnectionString))
                connectionString = this.ConnectionString;

            // If there is still no connection string, try reading it from the app.config file.
            // As a last resort, try connecting to the local instance of Mongo.
            if (string.IsNullOrEmpty(connectionString))
            {
                var connectionStringSettings = ConfigurationManager.ConnectionStrings[this.DriverName];
                if (!string.IsNullOrEmpty(connectionStringSettings?.ConnectionString) && connectionStringSettings.ProviderName.Equals("mongodb", StringComparison.InvariantCultureIgnoreCase))
                {
                    // The connection string can be separated into "url;username;password"
                    string[] connectionParts = connectionStringSettings.ConnectionString.Split(';');
                    connectionString = connectionParts[0];
                }
                else
                {
                    connectionString = "mongodb://localhost";
                }
            }

            // Record the connection string for subsequent use
            this.ConnectionString = connectionString;

            // Create a Mongo client
            this.Client = new MongoClient(connectionString);

            // See if Mongo is running
            var defaultDatabaseName = this.AdapterConfiguration.Behavior.DatabaseName;
            if (!this.TestIfDatabaseAlive(defaultDatabaseName))
            {
                string errorMsg = $"MongoDB - It is not alive for connection {this.ConnectionString}";
                Logger.Error(errorMsg);
                DatabaseIsNotOnline = true;
                throw new DatabaseNotAliveException(errorMsg);
            }

            Logger.Info($"MongoDB - Connected to {this.ConnectionString}");

            // Change from 1.0 - there is no client.Server and no Connect() call in the new Mongo driver
            DatabaseIsNotOnline = false;
            this.IsConnected = true;
            return true;
        }

        public bool Disconnect()
        {
            if (this.Client == null)
                return false;

            try
            {
                Logger.Info("MongoDB - Disconnecting");

                // Change from 1.0 - there is no Disconnect() call in the new Mongo driver
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TestIfDatabaseAlive(string dbName)
        {
            var isAlive = this.Client.GetDatabase(dbName).RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
            return isAlive;
        }
        #endregion

        #region Create/Delete/Get Databases
        public IDocumentDatabase CreateDatabase(string databaseName, bool deleteExistingDB = false)
        {
            var databases = this.Client.ListDatabasesAsync().Result.ToListAsync().Result;
            foreach (BsonDocument database in databases)
            {
                if (database["name"].Equals(databaseName))
                {
                    // Wait synchronously for the database to be deleted
                    if (deleteExistingDB)
                        this.Client.DropDatabaseAsync(databaseName).Wait();
                }
            }

            Logger.Info($"MongoDB - Creating database {databaseName}");
            IMongoDatabase nativeDatabase = this.Client.GetDatabase(databaseName, new MongoDatabaseSettings());
            IDocumentDatabase wrapper = this.WrapNativeDatabase(nativeDatabase);
            return wrapper;
        }

        public IEnumerable<IDocumentDatabase> GetAllDatabases()
        {
            var bsonDocs = this.Client.ListDatabasesAsync().Result.ToListAsync().Result;
            List<IMongoDatabase> nativeDatabases = bsonDocs.Select(bsonDoc => this.Client.GetDatabase((string)bsonDoc["name"])).ToList();
            return nativeDatabases.Select(this.WrapNativeDatabase).ToList();
        }

        public IDocumentDatabase GetDatabase(string databaseName)
        {
            IMongoDatabase nativeDatabase = this.Client.GetDatabase(databaseName, new MongoDatabaseSettings());
            IDocumentDatabase wrapper = this.WrapNativeDatabase(nativeDatabase);
            return wrapper;
        }

        public bool DropDatabase(IDocumentDatabase database)
        {
            if (database == null)
                return false;

            Logger.Info($"MongoDB - Dropping database {database.Name}");

            this.Client.DropDatabaseAsync(database.Name).Wait();
            return true;
        }
        #endregion

        #region Create/Delete/Get Collection(s)
        public IDocumentCollection CreateCollection(IDocumentDatabase database, string collectionName, bool deleteCollection = false)
        {
            if (!(database?.NativeDatabase is IMongoDatabase nativeDatabase))
                return null;

            if (deleteCollection)
            {
                nativeDatabase.DropCollectionAsync(collectionName).Wait();
            }

            Logger.Info($"MongoDB - Creating collection {collectionName}");

            nativeDatabase.CreateCollectionAsync(collectionName, new CreateCollectionOptions()).Wait();
            var nativeCollection = nativeDatabase.GetCollection<BsonDocument>(collectionName);
            if (nativeCollection == null)
                return null;

            return this.WrapNativeCollection(nativeCollection);
        }

        public IDocumentCollection GetCollection(IDocumentDatabase database, string collectionName)
        {
            if (!(database?.NativeDatabase is IMongoDatabase nativeDatabase))
                return null;

            IMongoCollection<BsonDocument> nativeCollection = nativeDatabase.GetCollection<BsonDocument>(collectionName);
            if (nativeCollection == null)
                return null;

            return this.WrapNativeCollection(nativeCollection);
        }

        public IEnumerable<IDocumentCollection> GetAllCollections(IDocumentDatabase database)
        {
            if (!(database?.NativeDatabase is IMongoDatabase nativeDatabase))
                return null;

            IAsyncCursor<BsonDocument> collectionList = nativeDatabase.ListCollectionsAsync().Result;

            List<BsonDocument> nativeCollections = collectionList?.ToListAsync().Result;

            return nativeCollections?.Where(bson => !bson["name"].AsString.StartsWith("system.", StringComparison.CurrentCulture))
                .Select(nativeCollection => this.WrapNativeCollection(nativeDatabase.GetCollection<BsonDocument>(nativeCollection["name"].AsString)))
                .ToList();
        }

        public bool CollectionExists(IDocumentDatabase database, string collectionName)
        {
            if (!(database?.NativeDatabase is IMongoDatabase nativeDatabase))
                return false;

            IAsyncCursor<BsonDocument> collectionList = nativeDatabase.ListCollectionsAsync().Result;

            List<BsonDocument> nativeCollections = collectionList?.ToListAsync().Result;
            if (nativeCollections == null)
                return false;

            return nativeCollections.Any(bson => bson["name"].AsString.Equals(collectionName, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool DropCollection(IDocumentCollection collection)
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return false;

            nativeCollection.Database.DropCollectionAsync(collection.Name).Wait();

            return true;
        }

        public bool ClearCollection(IDocumentCollection collection)
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return false;

            long numRecords = nativeCollection.CountDocumentsAsync(document => true).Result;
            var rc = nativeCollection.DeleteManyAsync(document => true).Result;

            return rc.DeletedCount == numRecords;
        }
        #endregion

        #region Query
        public IEnumerable<IDocument> Get(IDocumentCollection collection, string sqlQuery = null)
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return null;

            List<BsonDocument> nativeDocuments;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (sqlQuery != null)
            {
                nativeDocuments = nativeCollection.FindAsync(filter: sqlQuery).Result.ToListAsync().Result;
            }
            else
            {
                nativeDocuments = nativeCollection.FindAsync(filter: new BsonDocument()).Result.ToListAsync().Result;
            }

            return nativeDocuments?.Select(nativeDocument => this.WrapNativeDocument(collection, nativeDocument)).ToList();
        }

        public IEnumerable<T> Get<T>(IDocumentCollection collection, string sqlQuery = null) where T : class
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return null;

            List<T> objects;
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (sqlQuery != null)
            {
                objects = nativeCollection.FindAsync<T>(filter: sqlQuery).Result.ToListAsync().Result;
            }
            else
            {
                objects = nativeCollection.FindAsync<T>(filter: new BsonDocument()).Result.ToListAsync().Result;
            }
            return objects;
        }

        public IDocument GetById(IDocumentCollection collection, string id)
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return null;

            FilterDefinition<BsonDocument> filter = "{_id: '" + id + "'}";
            List<BsonDocument> nativeDocuments = nativeCollection.FindAsync(filter: filter).Result.ToListAsync().Result;
            if (nativeDocuments == null || nativeDocuments.Count == 0)
                return null;

            return this.WrapNativeDocument(collection, nativeDocuments[0]);
        }

        public T GetById<T>(IDocumentCollection collection, string id)
        {
            if (collection == null)
                return default;

            if (!(collection.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return default;

            FilterDefinition<BsonDocument> filter = "{_id: '" + id + "'}";
            List<T> nativeDocuments = nativeCollection.FindAsync<T>(filter).Result.ToListAsync().Result;
            if (nativeDocuments == null || nativeDocuments.Count == 0)
                return default;

            return nativeDocuments[0];
        }
        #endregion

        #region Insert/Update data
        public IDocument Save<T>(IDocumentCollection collection, T data, bool delete = false) where T : class, IPersistableDocumentObject
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return null;

            BsonDocument bsonData = data.ToBsonDocument();
            Logger.Info($"MongoDB - Saving document {data.id} into collection {collection.Name}");
            nativeCollection.InsertOneAsync(bsonData).Wait();

            return this.WrapNativeDocument(collection, bsonData);
        }

        public bool Update<T>(IDocument document, T data) where T : class, IPersistableDocumentObject
        {
            if (document == null || data == null)
                return false;

            BsonDocument nativeDocument = document.NativeDocument as BsonDocument;
            if (nativeDocument == null)
                return false;

            if (!(document.DocumentCollection.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return false;

            Logger.Info($"MongoDB - Updating document {data.id}");

            // Serialize data into Bson
            BsonDocument bsonReplacement = data.ToBsonDocument();
            FilterDefinition<BsonDocument> filter = "{_id: '" + document.Id + "'}";
            BsonDocument rc = nativeCollection.FindOneAndReplaceAsync(filter: filter, replacement: bsonReplacement).Result;

            return rc != null;
        }
        #endregion

        #region Delete Data
        public bool Delete(IDocument document)
        {
            if (document == null)
                return false;

            BsonDocument nativeDocument = document.NativeDocument as BsonDocument;
            if (nativeDocument == null)
                return false;

            if (!(document.DocumentCollection.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return false;

            Logger.Info($"MongoDB - Deleting document {document.Id}");
            FilterDefinition<BsonDocument> filter = "{_id: '" + document.Id + "'}";
            var rc = nativeCollection.DeleteOneAsync(filter).Result;

            return rc.DeletedCount == 1;
        }
        #endregion

        #region Helper Functions
        protected IDocumentDatabase WrapNativeDatabase(IMongoDatabase nativeDatabase)
        {
            return new DocumentDatabase(nativeDatabase.DatabaseNamespace.DatabaseName) { DatabaseDriver = this, NativeDatabase = nativeDatabase };
        }

        protected IDocumentCollection WrapNativeCollection(IMongoCollection<BsonDocument> nativeCollection)
        {
            return new DocumentCollection { DatabaseDriver = this, NativeCollection = nativeCollection, Name = nativeCollection.CollectionNamespace.CollectionName };
        }

        protected IDocument WrapNativeDocument(IDocumentCollection collection, BsonDocument nativeDocument)
        {
            // ReSharper disable RedundantNameQualifier
            return new Document
            {
                DatabaseDriver = this,
                NativeDocument = nativeDocument,
                Id = nativeDocument["_id"].AsString,
                DocumentCollection = collection,
            };
        }
        #endregion
    }
}
