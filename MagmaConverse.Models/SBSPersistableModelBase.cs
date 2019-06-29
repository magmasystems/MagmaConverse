using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Magmasystems.Framework;
using Magmasystems.Framework.Core;
using Magmasystems.Persistence;
using Magmasystems.Persistence.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace MagmaConverse.Models
{
    public abstract class SBSPersistableModelBase<TData> : SBSModelBase<TData>, IPersistableModel
        where TData : class, IPersistableDocumentObject
    {
        #region Events
        public static event Action<Type> DatabaseLoaded = type => { };
        #endregion

        #region Variables
        private const string DatabaseVendor = "MongoDB";

        [BsonIgnore]
        public DocumentDatabaseAdapter<TData> DatabaseAdapter { get; protected set; }

        [BsonIgnore]
        protected bool InitialDataLoadCompleted { get; set; }
        #endregion

        #region Property Change Support
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Constructors
        protected SBSPersistableModelBase() : this(DatabaseVendor)
        {
        }

        protected SBSPersistableModelBase(string dbVendor)
        {
            var config = ApplicationContext.Configuration;
            if (config.NoPersistence)
                return;

            this.DatabaseAdapter = new DocumentDatabaseAdapter<TData>(dbVendor);
        }
        #endregion

        #region Cleanup
        public override void Dispose()
        {
            if (this.DatabaseAdapter != null)
            {
                this.DatabaseAdapter.Dispose();
                this.DatabaseAdapter = null;
            }

            base.Dispose();
        }
        #endregion

        #region Initialization
        public virtual void InitialDataLoad()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    this.LoadFromDatabase();
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

            }).ContinueWith(task =>
            {
                this.InitialDataLoadCompleted = true;
                DatabaseLoaded(typeof(TData));
            });
        }
        #endregion

        #region Persistence
        public virtual bool LoadFromDatabase()
        {
            if (this.DatabaseAdapter == null)
                return false;

            this.Logger.Info($"{this.Name} - starting to load from the database");

            try
            {
                var records = this.DatabaseAdapter.Load();
                if (records == null)
                {
                    this.Logger.Info($"{this.Name} - cannot load from the database. Make sure the database server is running.");
                    return false;
                }

                IList<TData> list = records as IList<TData> ?? records.ToList();
                if (list.Count == 0)
                {
                    this.Logger.Info($"{this.Name} - found no records in the database.");
                    return true;
                }

                this.Repository.Clear();

                foreach (var obj in list)
                {
                    if (obj is IHasId objWithId)
                    {
                        if (!this.Repository.ContainsKey(objWithId.Id))
                            this.Repository.Add(objWithId.Id, obj);
                    }
                }
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                throw;
            }

            this.Logger.Info($"{this.Name} - successfully loaded from the database");
            return true;
        }

        public virtual bool LoadFromDatabase(string id)
        {
            if (this.DatabaseAdapter == null)
                return false;

            this.Logger.Info($"{this.Name} - starting to load {typeof(TData).Name} {id} from the database");

            try
            {
                var record = this.DatabaseAdapter.FindById(id);
                if (record == null)
                {
                    this.Logger.Info($"{this.Name} - cannot load {typeof(TData).Name} {id} from the database");
                    return false;
                }

                if (record is IHasId objWithId)
                {
                    if (!this.Repository.ContainsKey(objWithId.Id))
                        this.Repository.Add(objWithId.Id, record);
                }
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                throw;
            }

            this.Logger.Info($"{this.Name} - successfully loaded {typeof(TData).Name} {id} from the database");
            return true;
        }

        public virtual bool SaveToDatabase()
        {
            if (this.DatabaseAdapter == null)
                return false;

            this.Logger.Info($"{this.Name} - starting to save to the database");
            var rc = true;

            try
            {
                foreach (var formDef in this.Repository.Values)
                {
                    rc = this.SaveToDatabase(formDef);
                    if (rc == false)
                        break;
                }

                this.Logger.Info($"{this.Name} - successfully saved to the database");
                return rc;
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                return false;
            }
        }

        public virtual bool SaveToDatabase(TData data)
        {
            if (this.DatabaseAdapter == null)
                return false;

            this.Logger.Info($"{this.Name} - starting to save {typeof(TData).Name} to the database");

            try
            {
                this.DatabaseAdapter.Save(data);
                this.Logger.Info($"{this.Name} - {typeof(TData).Name} successfully saved to the database");
                return true;
            }
            catch (Exception exc)
            {
                this.Logger.Error(exc.Message);
                return false;
            }
        }

        #endregion

        #region Repository Methods that work with Persistence
        public override bool Add(string key, TData value, bool overwrite = false)
        {
            // Insert into the in-memory cache
            var rc = base.Add(key, value, overwrite);

            // Put this into the database
            this.DatabaseAdapter.Save(value);

            return rc;
        }

        public override TData GetById(string key)
        {
            // Get from the cache
            var data = base.GetById(key);

            // Cache miss?
            if (data == null)
            {
                data = this.DatabaseAdapter.FindById(key);
            }

            return data;
        }

        // ReSharper disable once RedundantOverriddenMember
        public override List<TData> GetAll()
        {
            return base.GetAll();
        }

        public override bool Remove(string key)
        {
            // Remove from the cache
            var rc = base.Remove(key);

            // Delete from the database
            this.DatabaseAdapter.DeleteDocument(key);

            return rc;
        }

        // ReSharper disable once RedundantOverriddenMember
        public override void Clear()
        {
            base.Clear();
        }
        #endregion
    }
}