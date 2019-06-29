using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Magmasystems.Persistence.Interfaces;
using log4net;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Magmasystems.Framework;
using Magmasystems.Framework;
using System.Text;
using Newtonsoft.Json.Linq;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

#pragma warning disable IDE0045 // Convert to conditional expression
#pragma warning disable IDE0046 // Convert to conditional expression
#pragma warning disable IDE1006 // Naming Styles

namespace Magmasystems.Persistence.MongoDB
{
    public class MongoDBPersistenceDriver : IDocumentDatabasePersistenceDriver
    {
        #region Events
        public event Action<ConnectionState> ConnectionStateChanged = state => { };
        public event Action<string> DatabaseChanged = (databaseName) => { };
        #endregion

        #region Variables
        private static readonly ILog Logger = LogManager.GetLogger(typeof(MongoDBPersistenceDriver));

        private static bool? DatabaseIsNotOnline { get; set; }
        public bool IsDatabaseAlive => DatabaseIsNotOnline.HasValue && DatabaseIsNotOnline.Value == false;

        public DatabaseVendors Vendor => DatabaseVendors.MongoDB;
        public IMongoClient Client { get; protected set; }
        public bool IsConnected { get; set; }

        protected TimeSpan ConnectionTimeOutInterval { get; }
        protected string ConnectionString { get; private set; }
        protected string DriverName { get; }
        public DocumentDatabaseAdapterConfiguration AdapterConfiguration { get; }

        private static ConcurrentDictionary<string, (Task task, CancellationTokenSource cancellationTokenSource)> DatabaseWatchers { get; set; } = new ConcurrentDictionary<string, (Task task, CancellationTokenSource cancellationTokenSource)>();
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
            foreach (var watcher in DatabaseWatchers)
            {
                watcher.Value.cancellationTokenSource.Cancel();
            }

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
            string databaseName = this.AdapterConfiguration?.DatabaseName;
            if (!this.TestIfDatabaseAlive(databaseName))
            {
                string errorMsg = $"MongoDB - It is not alive for connection {this.ConnectionString}";
                Logger.Error(errorMsg);
                DatabaseIsNotOnline = true;
                throw new DatabaseNotAliveException(errorMsg);
            }

            Logger.Debug($"MongoDB - Connected to {this.ConnectionString}");

            #if ONLY_WORKS_WITH_MONGO_REPLICASETS
            this.CreateDatabaseWatcher(databaseName);
            #endif

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
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(5 * 1000);
            var isAlive = this.Client.GetDatabase(dbName).RunCommandAsync((Command<BsonDocument>)"{ping:1}"/*, cancellationToken: cancellationTokenSource.Token*/);
            return isAlive.Result != null;
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
            return this.GetDatabase(databaseName);
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

            if (DatabaseWatchers.TryGetValue(database.Name, out var watcher))
            {
                // Remove the watcher
                watcher.cancellationTokenSource.Cancel();
                DatabaseWatchers.TryRemove(database.Name, out _);
            }

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

        public IEnumerable<IDocument> Get(IDocumentCollection collection, FilterCondition filter)
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return null;

            List<BsonDocument> nativeDocuments = nativeCollection.FindAsync(filter: this.CreateFilter(filter)).Result.ToListAsync().Result;
            return nativeDocuments?.Select(nativeDocument => this.WrapNativeDocument(collection, nativeDocument)).ToList();
        }

        public IEnumerable<T> Get<T>(IDocumentCollection collection, FilterCondition filter) where T : class
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return null;

            List<T> objects = nativeCollection.FindAsync<T>(filter: this.CreateFilter(filter)).Result.ToListAsync().Result;
            return objects;
        }

        public IDocument GetById(IDocumentCollection collection, string id)
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return null;

            var filter = this.GenerateIdFilter(id);
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

            var filter = this.GenerateIdFilter(id);
            List<T> nativeDocuments = nativeCollection.FindAsync<T>(filter).Result.ToListAsync().Result;
            if (nativeDocuments == null || nativeDocuments.Count == 0)
                return default(T);

            return nativeDocuments[0];
        }

        public bool Exists<T>(IDocumentCollection collection, string id)
        {
            if (collection == null)
                return false;

            if (!(collection.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return false;

            var filter = this.GenerateIdFilter(id);
            return nativeCollection.CountDocuments(filter, new CountOptions { Limit = 1 }) == 1;
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
            var filter = this.GenerateIdFilter(document.Id);
            BsonDocument rc = nativeCollection.FindOneAndReplaceAsync(filter: filter, replacement: bsonReplacement).Result;

            return rc != null;
        }

        public bool Update<T>(IDocumentCollection collection, string id, Properties properties, DocumentDatabaseUpdateOptions updateOptions = null) where T : class, IPersistableDocumentObject
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return false;

            Logger.Info($"MongoDB - Updating document {id}");

            var propsList = properties.ToList();
            UpdateDefinition<BsonDocument> update;

            if (updateOptions != null && updateOptions.DeleteFromArray)
            {
                update = this.DeleteFromArrayBuilder(properties, updateOptions);
            }
            else
            {
                update = this.InsertIntoArrayBuilder(properties, updateOptions);
            }

            var filter = this.GenerateIdFilter(id);
            var updateResult = nativeCollection.UpdateOne(filter, update, new UpdateOptions { IsUpsert = true, BypassDocumentValidation = true });

            return updateResult.ModifiedCount == 1;
        }

        /// <summary>
        /// Updates an element of an array which is nested inside the larger document.
        /// For example, we can use this to update the fields of one of the Questions inside of a QuestionBank
        /// </summary>
        public bool Update<T>(IDocumentCollection collection, string id, string nestedArrayName, string idElement, Properties properties, DocumentDatabaseUpdateOptions updateOptions = null)
            where T : class, IPersistableDocumentObject
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return false;

            Logger.Info($"MongoDB - Updating document {id}");

            var propsList = properties.ToList();
            UpdateDefinition<BsonDocument> update = this.UpdateFieldsBuilder(nestedArrayName, properties, updateOptions);

            var filter = this.GenerateIdFilter(id, nestedArrayName, idElement);
            var updateResult = nativeCollection.UpdateOne(filter, update, new UpdateOptions { IsUpsert = false, BypassDocumentValidation = true });

            return updateResult.ModifiedCount == 1;
        }

        /// <summary>
        /// Updates an element of an array which is nested inside the larger document.
        /// For example, we can use this to update the fields of one of the Questions inside of a QuestionBank
        /// </summary>
        public bool Update<T, TElement>(IDocumentCollection collection, string id, string nestedArrayName, string idElement, TElement element, DocumentDatabaseUpdateOptions updateOptions = null)
            where T : class, IPersistableDocumentObject
        {
            if (!(collection?.NativeCollection is IMongoCollection<BsonDocument> nativeCollection))
                return false;

            Logger.Info($"MongoDB - Updating document {id}");

            /*
             * Adds the entire question as a new field in the question's document           
             */

            /*
             * This works
             * 
                db.getCollection('QuestionBank').update(
                  { 
                     "_id":           "questionbank:7a936bb0",
                     "questions._id": "q:6850fe33"
                  },

                  { $set: 
                      { 
                          "questions.$":
                          {
                            "questionType" : "OneOfMany",
                            "questionFormat" : "Text",
                            "tags" : [ "C# 3.0" ],
                            "questionText" : "Which one of the following was a new language addition to C# 3.0?",
                            "choices" : [ { "choiceText" : "Arrays2", "isCorrectAnswer" : false } ],
                            "answer" : {
                                "answerChecker" : null,
                                "correctAnswerChoices" : [ 2 ],
                                "correctAnswerText" : null,
                                "explanation" : "Generics have been added to support parameterized types in C#.",
                                "format" : "Text"
                            },
                            "isPremium" : false
                          }
                      }
                  },

                  { upsert: false, multi: false }
                );
             */

            var filter = this.GenerateIdFilter(id, nestedArrayName, idElement);
            var update = Builders<BsonDocument>.Update.Set($"{nestedArrayName}.$", element.ToBsonDocument());
            var updateResult = nativeCollection.UpdateOne(filter, update, new UpdateOptions { IsUpsert = false, BypassDocumentValidation = true });



            return updateResult.ModifiedCount == 1;
        }

        private UpdateDefinition<BsonDocument> UpdateFieldsBuilder<TElement>(string nestedArrayName, string key, TElement element, DocumentDatabaseUpdateOptions updateOptions)
        {
            var updateBuilder = Builders<BsonDocument>.Update;
            UpdateDefinition<BsonDocument> update = null;

            string fieldName = $"{nestedArrayName}.$.{key}";
            update = updateBuilder.AddToSet(fieldName, element);

            return update;
        }

        private UpdateDefinition<BsonDocument> UpdateFieldsBuilder(string nestedArrayName, Properties properties, DocumentDatabaseUpdateOptions updateOptions = null)
        {
            /*
             * To update a single field of a specific question
             * 
                    db.getCollection('QuestionBank').updateOne( 
                        { _id: 'questionbank:832b261c', 'Questions._id': 'q:5cc60eea' },
                        { $set: { IsPremium: true } }
                    )
             */

            var propsList = properties.ToList();
            var updateBuilder = Builders<BsonDocument>.Update;
            UpdateDefinition<BsonDocument> update = null;

            foreach (var prop in propsList)
            {
                string fieldName = $"{nestedArrayName}.$.{prop.Key}";
                // Possibly convert the object from a JArray to a generic list, if the vaue is indeed a JArray
                object value = Magmasystems.Framework.Serialization.Json.JArrayToList(prop.Value);

                if (update == null)
                    update = updateBuilder.Set(fieldName, value);
                else
                    update = update.Set(fieldName, value);
            }

            return update;
        }

        private UpdateDefinition<BsonDocument> InsertIntoArrayBuilder(Properties properties, DocumentDatabaseUpdateOptions updateOptions = null)
        {
            var propsList = properties.ToList();
            var updateBuilder = Builders<BsonDocument>.Update;
            UpdateDefinition<BsonDocument> update = null;
            bool isAppendingToArray = updateOptions != null && updateOptions.AppendToExistingArray;

            string typenameOfFollowingProperty = null;

            foreach (var prop in propsList)
            {
                var key = prop.Key;
                object value = prop.Value;

                /*
                   If a property has the keyword <type> as the key, then the value is the name of a .NET type.  
                   The next property in the list will be deserialized into that type.
                                  
                   "properties": {
                        "<type>": "MagmaQuiz.Data.TestAnswer, MagmaQuiz.Data",
                        "answers.0": {
                            "testAnswerDuration": "00:00:30",               
                */
                if (key == "<type>")
                {
                    typenameOfFollowingProperty = (string)value;
                    continue;
                }

                // If the Json.Net deserializer passes us a JArray (such as in a Properties dictionary), then convert
                // it to a List<>. We need to examine the first element of the Json array and use that type for the
                // generic list.
                if (Magmasystems.Framework.Serialization.Json.IsJArray(value))
                {
                    value = Magmasystems.Framework.Serialization.Json.JArrayToList(value);
                }

                else if (value is JObject jObject && typenameOfFollowingProperty != null)
                {
                    var typeToConvertTo = Type.GetType(typenameOfFollowingProperty);
                    if (typeToConvertTo == null)
                        throw new Exception($"MongoDB Driver - cannot find type {typeToConvertTo}");
                    value = jObject.ToObject(typeToConvertTo);
                    typenameOfFollowingProperty = null;
                }

                // See if we passed a generic list into the properties. 
                // If so, then iterate over the list and use AddToSet() in order to append the elements to the existing array.
                if (TypeHelpers.IsGenericList(value))
                {
                    foreach (var element in (System.Collections.IList)value)
                    {
                        if (update == null)
                            update = isAppendingToArray ? updateBuilder.Push(key, element) : updateBuilder.Set(key, element);
                        else
                            update = update.Push(key, element);
                    }
                }
                // See if we just want to add a simple element to an array
                else if (isAppendingToArray)
                {
                    update = updateBuilder.Push(key, value);
                }
                // The default is just to set a field to a value
                else
                {
                    if (update == null)
                        update = updateBuilder.Set(key, value);
                    else
                        update = update.Set(key, value);
                }
            }

            return update;
        }

        private UpdateDefinition<BsonDocument> DeleteFromArrayBuilder(Properties properties, DocumentDatabaseUpdateOptions updateOptions = null)
        {
            var propsList = properties.ToList();
            var updateBuilder = Builders<BsonDocument>.Update;
            UpdateDefinition<BsonDocument> update = null;

            /*
                db.getCollection('QuestionBank').update( 
                    { _id: 'questionbank:d7ceb882' },                  <- this is the filter
                    { 
                        $pull: { Questions: { _id: 'q:7c7c53d1' } }    <- the operation and the array name and the matching condition
                    }
                )
             */

            foreach (var prop in propsList)
            {
                var key = prop.Key;
                object value = prop.Value;

                if (TypeHelpers.IsTuple(value))
                {
                    var tuple = (ValueTuple<string, List<string>>)value;

                    // The tuple might be the "_id" field name and a list of ids to delete
                    var arrayName = key;
                    var arrayFieldName = tuple.Item1;
                    var idList = tuple.Item2;

                    foreach (var id in idList)
                    {
                        FilterDefinition<BsonDocument> pullFilter = $"{{ {arrayFieldName}: '{id}' }}";
                        if (update == null)
                            update = updateBuilder.PullFilter(arrayName, pullFilter);
                        else
                            update = update.PullFilter(arrayName, pullFilter);
                    }
                }

                // Now let's see if the request is just to delete an ordinary element from an array. In this case,
                // we are passed a Json aray of values to delete from the array.
                else if (value is JArray jArray && jArray?.Count > 0 && jArray[0] != null)
                {
                    JValue jValue = jArray[0] as JValue;
                    Type underlyingType = jValue.Value.GetType();
                    var listType = typeof(List<>).MakeGenericType(underlyingType);
                    // Convert the JArray to a .NET generic list
                    value = jArray.ToObject(listType);

                    foreach (var element in (System.Collections.IList)value)
                    {
                        if (update == null)
                            update = updateBuilder.Pull(key, element);
                        else
                            update = update.Pull(key, element);
                    }
                }

                // We are just passed an element to delete from the array.
                else
                {
                    update = updateBuilder.Pull(key, value);
                }
            }

            return update;
        }

        private BsonDocument GenerateBsonUpdate(Properties properties)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{ ");
            foreach (var prop in properties)
            {
                sb.AppendLine($"{prop.Key}: {ValueToString(prop.Value)},");
            }
            sb.Append(" }");

            return sb.ToBsonDocument();
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
            var filter = this.GenerateIdFilter(document.Id);
            var rc = nativeCollection.DeleteOneAsync(filter).Result;

            return rc.DeletedCount == 1;
        }
        #endregion

        #region Wrap Native Documents
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

        #region Filters
        protected FilterDefinition<BsonDocument> GenerateIdFilter(string id)
        {
            FilterDefinition<BsonDocument> filter = $"{{ _id: '{id}' }}";
            return filter;
        }

        protected FilterDefinition<T> GenerateIdFilter<T>(string id)
        {
            FilterDefinition<T> filter = $"{{ _id: '{id}' }}";
            return filter;
        }

        protected FilterDefinition<BsonDocument> GenerateIdFilter(string id, string nestedArrayName, string idElement)
        {
            FilterDefinition<BsonDocument> filter = $"{{ _id: '{id}', '{nestedArrayName}._id': '{idElement}' }}";
            return filter;
        }

        public string CreateFilter(string fieldname, object value, FilterConditionOperator condition)
        {
            var sValue = this.ValueToString(value);

            switch (condition)
            {
                case FilterConditionOperator.Equals:
                    return $"{{ {fieldname}: {sValue} }}";
                case FilterConditionOperator.NotEquals:
                    return $"{{ $ne: {{ {fieldname}: {sValue} }} }}";
                case FilterConditionOperator.LessThan:
                    return $"{{ lt: {{ {fieldname}: {sValue} }} }}"; ;
                case FilterConditionOperator.LessThanOrEquals:
                    return $"{{ $lte: {{ {fieldname}: {sValue} }} }}";
                case FilterConditionOperator.GreaterThan:
                    return $"{{ $gt: {{ {fieldname}: {sValue} }} }}";
                case FilterConditionOperator.GreaterThanOrEquals:
                    return $"{{ $gte: {{ {fieldname}: {sValue} }} }}";
                default:
                    return $"{{ {fieldname}: {sValue} }}";
            }
        }

        public string CreateFilter(Expression expression)
        {
            return $"";
        }

        public string CreateFilter(FilterCondition filter)
        {
            return this.CreateFilter(filter.FieldName, filter.Value, filter.Op);
        }

        private string ValueToString(object o)
        {
            if (o is string s)
                return $"'{s}'";

            return o.ToString();
        }
        #endregion

        #region Watch for Change Events
        private void CreateDatabaseWatcher(string databaseName)
        {
            bool usePipelines = true;

            // Set up a watcher to monitor all database changes
            if (DatabaseWatchers.ContainsKey(databaseName))
                return;
            DatabaseWatchers.TryAdd(databaseName, (null, null));  // Immediately put a sentinel in to stop re-entry.

            var db = this.GetDatabase(databaseName).NativeDatabase as IMongoDatabase;
            var taskAndCancellationSource = usePipelines ? this.WatchForChanges(db) : this.WatchForChangesNoPipeline(db);
            DatabaseWatchers[databaseName] = taskAndCancellationSource;
        }

        private void RemoveAllWatchers()
        {
            foreach (var watcher in DatabaseWatchers)
            {
                watcher.Value.cancellationTokenSource.Cancel();
            }

            DatabaseWatchers.Clear();
        }

        private (Task, CancellationTokenSource) WatchForChanges(IMongoDatabase database)
        {
            try
            {
                BsonDocument resumptionPoint = null;
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                var task = Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>().Match("{ operationType: { $in: [ 'replace', 'insert', 'update' ] } }");

                        Logger.Info($"MongoDBPersistenceDriver.WatchForChanges - calling Watch");
                        using (var cursor = database.Watch(pipeline, new ChangeStreamOptions { ResumeAfter = resumptionPoint }))
                        {
                            Logger.Info($"The Mongodb Database Watcher found some changes. Enumerating the changes now ....");
                            await cursor.ForEachAsync(change =>
                            {
                                // process change event
                                resumptionPoint = change.ResumeToken;
                                Logger.Info($"change: Key={change.DocumentKey}, OpType={change.OperationType}, Desc={change.UpdateDescription}");
                            });

                            Logger.Info($"**** Reached the end of the watched changes ****");
                        }
                        Logger.Info($"database.Watch disposed - returning to the top of the Watch loop");
                    }
                }, cancellationTokenSource.Token);

                return (task, cancellationTokenSource);
            }
            catch (Exception exc)
            {
                Logger.Error($"MongoDBPersistenceDriver.WatchForChangesNoPipeline - {ExceptionHelpers.Format(exc)}");
                return (null, null);
            }
        }

        private (Task, CancellationTokenSource) WatchForChangesNoPipeline(IMongoDatabase database)
        {
            try
            {
                BsonDocument resumptionPoint = null;
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                var task = Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        //var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>().Match("{ operationType: { $in: [ 'replace', 'insert', 'update' ] } }");

                        Logger.Info($"MongoDBPersistenceDriver.WatchForChangesNoPipeline - calling WatchAsync");
                        using (var cursor = await database.WatchAsync())
                        {
                            Logger.Info($"The Mongodb Database Watcher found some changes. Enumerating the changes now ....");
                            await cursor.ForEachAsync(change =>
                            {
                                // process change event
                                resumptionPoint = change.ResumeToken;
                                Logger.Info($"change: Key={change.DocumentKey}, OpType={change.OperationType}, Desc={change.UpdateDescription}");
                            });

                            Logger.Info($"**** Reached the end of the watched changes ****");
                        }
                    }
                }, cancellationTokenSource.Token);

                return (task, cancellationTokenSource);
            }
            catch (Exception exc)
            {
                Logger.Error($"MongoDBPersistenceDriver.WatchForChangesNoPipeline - {ExceptionHelpers.Format(exc)}");
                return (null, null);
            }
        }
        #endregion
    }
}
