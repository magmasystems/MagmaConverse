using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq.Expressions;
using Magmasystems.Framework;
using Magmasystems.Persistence.Interfaces;

namespace Magmasystems.Persistence
{
	public class DocumentDatabaseAdapter<T> : IDisposable, INotifyPropertyChanged
		where T : class, IPersistableDocumentObject
	{
		#region Variables
		public IDocumentDatabasePersistenceDriver DatabaseDriver { get; set; }
		private DocumentDatabaseAdapterConfiguration AdapterConfig { get; set; }
		public bool IsConnected { get; private set; }

	    /// <summary>
	    /// If the database if offline, then this is true.
	    /// Note that we can poll the database to see if it comes back online.
	    /// </summary>
	    public bool DatabaseIsNotOnline => !this.DatabaseDriver?.IsDatabaseAlive ?? true;

        public string DatabaseVendorName { get; private set; }
	    public string DatabaseName { get; set; }
		public string CollectionName { get; set; }

		private bool UseDatabase => this.AdapterConfig.Behavior.UseDatabase;

        public T Data { get; set; }
		#endregion

		#region Constructors
		public DocumentDatabaseAdapter(string databaseVendorName)
		{
			this.CommonConstruct(databaseVendorName);
		}

		public DocumentDatabaseAdapter(string databaseVendorName, string databaseName, string collectionName)
		{
			this.CommonConstruct(databaseVendorName);

		    this.DatabaseName = databaseName;
			this.CollectionName = collectionName;
		}

		private void CommonConstruct(string databaseVendorName)
		{
			this.DatabaseVendorName = databaseVendorName;

			string adapterName = databaseVendorName + "Adapter";

			this.AdapterConfig = ConfigurationManager.GetSection(adapterName) as DocumentDatabaseAdapterConfiguration;
			if (this.AdapterConfig == null)
			{
				throw new ApplicationException("No " + adapterName + " config section could be found in the App.config file");
			}

			// Need to set up the serializers in order for the object to serialize properly
			DocumentDatabaseSerializers.Initialize();
            this.InitializeTypes();

		    if (this.UseDatabase)
		        this.Connect();
        }

	    private void InitializeTypes()
	    {
	        var typeBehavior = this.AdapterConfig.FindType(typeof(T));
	        if (typeBehavior != null)
	        {
	            this.DatabaseName = typeBehavior.DatabaseName;
	            this.CollectionName = string.IsNullOrEmpty(typeBehavior.CollectionName) ? this.DatabaseName : typeBehavior.CollectionName;
	            if (!string.IsNullOrEmpty(typeBehavior.SerializerInitializer))
	            {
	                Type initializerType = Type.GetType(typeBehavior.SerializerInitializer);
	                if (initializerType != null)
	                {
	                    var initializer = Activator.CreateInstance(initializerType);
	                    if (initializer is IDocumentDatabseSerializationInitializer serializationInitializer)
	                    {
	                        serializationInitializer.Initialize(typeof(T));
	                    }
	                }
	            }
	        }
	        else
	        {
	            // Need to set up the serializers in order for the object to serialize properly
	            //MongoSerializers.Initialize();

	            this.DatabaseName = $"{typeof(T).Name}_Database";
	            this.CollectionName = $"{typeof(T).Name}_Collection";
	        }
        }
		#endregion

		#region Cleanup
		public virtual void Dispose()
		{
		    if (this.DatabaseDriver == null)
                return;

            this.DatabaseDriver.Dispose();
		    this.DatabaseDriver = null;
		    //ApplicationPersistenceSettings.MongoDriver = null;
		}
		#endregion

		#region Connect to the DB
		public void Connect()
		{
		    if (!this.UseDatabase)
		        return;

			if (this.DatabaseDriver == null)
			{
				this.DatabaseDriver = PersistenceDriverFactory.Create(this.DatabaseVendorName, this.AdapterConfig);
				//ApplicationPersistenceSettings.MongoDriver = this.DatabaseDriver;
				this.DatabaseDriver.ConnectionStateChanged += state =>
				{
					this.ShowConnectionStatusMessage(state.ToString());
					this.IsConnected = state == System.Data.ConnectionState.Open;
					this.NotifyPropertyChanged("IsConnected");
				};
			}

			if (this.DatabaseDriver.IsConnected)
			{
				//this.DatabaseDriver.Disconnect();
				return;
			}

            // This can throw a DatabaseIsNotAliveException
		    if (this.DatabaseDriver.Connect())
		    {
		    }
		}

	    private void ShowConnectionStatusMessage(string msg)
		{
			Console.WriteLine(msg);
		}
		#endregion

		#region Notifications

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void NotifyPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
		    handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Helpers
	    private bool ConnectionGuard()
	    {
	        return this.UseDatabase && !this.DatabaseIsNotOnline;
	    }

		public IDocumentCollection GetDefaultCollection()
		{
			return this.GetDefaultCollection(this.DatabaseName, this.CollectionName);
		}

		public IDocumentCollection GetDefaultCollection(string databaseName, string collectionName)
		{
			if (!this.UseDatabase)
				return null;

			if (this.DatabaseDriver == null || !this.DatabaseDriver.IsConnected)
				this.Connect();

			if (!this.DatabaseDriver.IsConnected)
				return null;

			IDocumentDatabase database = this.DatabaseDriver.GetDatabase(databaseName);
			if (database == null)
				return null;

			var collection = this.DatabaseDriver.GetCollection(database, collectionName);

		    return collection;
		}

	    public IDocumentDatabase GetDefaultDatabase()
	    {
	        return this.GetDefaultDatabase(this.DatabaseName);
	    }

        public IDocumentDatabase GetDefaultDatabase(string databaseName)
	    {
	        if (!this.UseDatabase)
	            return null;

	        if (this.DatabaseDriver == null || !this.DatabaseDriver.IsConnected)
	            this.Connect();

	        if (!this.DatabaseDriver.IsConnected)
	            return null;

	        IDocumentDatabase database = this.DatabaseDriver.GetDatabase(databaseName);
	        return database;

	    }

        public string CreateFilter(string fieldname, object value, FilterConditionOperator condition = FilterConditionOperator.Equals)
        {
            return this.DatabaseDriver.CreateFilter(fieldname, value, condition);
        }

        public string CreateFilter(Expression expression)
        {
            return this.DatabaseDriver.CreateFilter(expression);
        }
        #endregion

        #region Loading
        public IEnumerable<T> Load()
		{
			return this.Load(this.DatabaseName, this.CollectionName);
		}

		public IEnumerable<T> Load(string databaseName, string collectionName)
		{
		    if (!this.ConnectionGuard())
		        return null;

			IDocumentCollection collection = this.GetDefaultCollection();
			if (collection == null)
				return null;

			IEnumerable<T> data = this.DatabaseDriver.Get<T>(collection);
			return data;
		}
		#endregion

		#region Saving
		public IDocument Save(T data, bool purge = false)
		{
			return this.Save(data, this.DatabaseName, this.CollectionName, purge);
		}

		public IDocument Save(T data, string databaseName, string collectionName, bool purge = false)
		{
		    if (!this.ConnectionGuard())
		        return null;

            if (!this.UseDatabase)
				return null;

			if (data == null)
				return null;

			if (this.DatabaseDriver == null || !this.DatabaseDriver.IsConnected)
				this.Connect();

			if (this.DatabaseDriver == null)
				return null;

			//bool isList = (data as IEnumerable<T>) != null;


			var collection = this.GetDefaultCollection(databaseName, collectionName);
			if (collection == null)
				return null;

			var document = this.DatabaseDriver.Save(collection, data, delete: purge);
			return document;
		}
        #endregion

        #region Querying
        public bool Exists(string key)
        {
            return this.Exists(this.DatabaseName, this.CollectionName, key);
        }

        public bool Exists(string databaseName, string collectionName, string key)
        {
            if (!this.ConnectionGuard())
                return false;

            var collection = this.GetDefaultCollection(databaseName, collectionName);
            return collection != null && this.DatabaseDriver.Exists<T>(collection, key);
        }

        public T FindById(string id)
		{
			return this.FindById(this.DatabaseName, this.CollectionName, id);
		}

		public T FindById(string databaseName, string collectionName, string id)
		{
		    if (!this.ConnectionGuard())
		        return default(T);

            var collection = this.GetDefaultCollection(databaseName, collectionName);
			if (collection == null)
				return default(T);

			var foundObject = this.DatabaseDriver.GetById<T>(collection, id);
			return foundObject;
		}

        public IEnumerable<T> Query(string query)
        {
            return this.Query(this.DatabaseName, this.CollectionName, query);
        }

        public IEnumerable<T> Query(FilterCondition query)
        {
            return this.Query(this.DatabaseName, this.CollectionName, query);
        }

        public IEnumerable<T> Query(string databaseName, string collectionName, string query)
        {
            if (!this.ConnectionGuard())
                return null;

            var collection = this.GetDefaultCollection(databaseName, collectionName);
            if (collection == null)
                return null;

            var foundObjects = this.DatabaseDriver.Get<T>(collection, query);
            return foundObjects;
        }

        public IEnumerable<T> Query(string databaseName, string collectionName, FilterCondition query)
        {
            if (!this.ConnectionGuard())
                return null;

            var collection = this.GetDefaultCollection(databaseName, collectionName);
            if (collection == null)
                return null;

            var foundObjects = this.DatabaseDriver.Get<T>(collection, query);
            return foundObjects;
        }
        #endregion

        #region Updating a Single Document
        public bool Update(string id, Properties properties, DocumentDatabaseUpdateOptions options = null)
        {
            if (!this.ConnectionGuard())
                return false;

            var collection = this.GetDefaultCollection(this.DatabaseName, this.CollectionName);
            if (collection == null)
                return false;

            bool rc = this.DatabaseDriver.Update<T>(collection, id, properties, options);
            return rc;
        }

        public bool Update(string id, string nestedArrayElement, string idElement, Properties properties, DocumentDatabaseUpdateOptions options = null)
        {
            if (!this.ConnectionGuard())
                return false;

            var collection = this.GetDefaultCollection(this.DatabaseName, this.CollectionName);
            if (collection == null)
                return false;

            bool rc = this.DatabaseDriver.Update<T>(collection, id, nestedArrayElement, idElement, properties, options);
            return rc;
        }

        /// <summary>
        /// This will update the entire element in a nested array (for exaple, replacing an entire Question in a QuestionBank's Questions array)
        /// </summary>
        public bool Update<TElement>(string id, string nestedArrayElement, string idElement, TElement element, DocumentDatabaseUpdateOptions options)
        {
            if (!this.ConnectionGuard())
                return false;

            var collection = this.GetDefaultCollection(this.DatabaseName, this.CollectionName);
            if (collection == null)
                return false;

            bool rc = this.DatabaseDriver.Update<T, TElement>(collection, id, nestedArrayElement, idElement, element, options);
            return rc;
        }
        #endregion

        #region Deleting
        public bool DeleteDatabase()
	    {
	        if (!this.ConnectionGuard())
	            return false;

            var db = this.GetDefaultDatabase();
	        return db != null && this.DatabaseDriver.DropDatabase(db);
	    }

	    public bool DeleteCollection()
	    {
	        if (!this.ConnectionGuard())
	            return false;

            var coll = this.GetDefaultCollection();
	        return coll != null && this.DatabaseDriver.DropCollection(coll);
	    }

	    public bool DeleteDocument(T doc)
	    {
	        if (!this.ConnectionGuard())
	            return false;

            var doc2 = this.DatabaseDriver.GetById(this.GetDefaultCollection(), doc.id);
	        return doc2 != null && this.DatabaseDriver.Delete(doc2);
	    }

	    public bool DeleteDocument(string key)
	    {
	        if (!this.ConnectionGuard())
	            return false;

            var doc2 = this.DatabaseDriver.GetById(this.GetDefaultCollection(), key);
	        return doc2 != null && this.DatabaseDriver.Delete(doc2);
	    }
        #endregion
    }
}