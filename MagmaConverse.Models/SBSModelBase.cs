using System.Collections.Generic;
using System.Linq;
using Magmasystems.Framework;
using MagmaConverse.Interfaces;
using log4net;

namespace MagmaConverse.Models
{
    public abstract class SBSModelBase<TData> : 
        IHasRepository<TData>, 
        ISBSModel<TData> where TData : class
    {
        #region Events
        #endregion

        #region Variables
        protected readonly ILog Logger;
        public DictionaryRepository<TData> Repository { get; } = new DictionaryRepository<TData>();
        public string Name { get; }
        #endregion

        #region Constructors
        protected SBSModelBase()
        {
            this.Logger = LogManager.GetLogger(this.GetType());
            this.Name = $"Model.{typeof(TData).Name}";
        }

        protected SBSModelBase(string name) : this()
        {
            this.Name = name;
        }
        #endregion

        #region Cleanup
        public virtual void Dispose()
        {
            this.Clear();
        }
        #endregion

        #region Repository Methods
        public virtual bool Add(string key, TData value, bool overwrite = false)
        {
            bool exists = this.Repository.ContainsKey(key);
            if (exists && !overwrite)
                return false;

            if (exists)
                this.Repository[key] = value;
            else
                this.Repository.Add(key, value);
            return true;
        }

        public virtual int Count => this.Repository?.Count ?? 0;

        public virtual void Clear()
        {
            this.Repository?.Clear();
        }

        public virtual bool Remove(string key)
        {
            if (this.Repository.ContainsKey(key))
            {
                this.Repository.Remove(key);
                return true;
            }

            return false;
        }

        public virtual bool Remove(string[] keys)
        {
            bool rc = true;

            foreach (var key in keys)
            {
                bool rc2 = this.Remove(key);
                if (rc2 == false)
                    rc = false;
            }

            return rc;
        }

        public virtual List<TData> GetAll()
        {
            return this.Repository?.Values.ToList();
        }

        public virtual TData GetById(string key)
        {
            return this.Repository.TryGetValue(key, out TData value) ? value : null;
        }
        #endregion
    }
}