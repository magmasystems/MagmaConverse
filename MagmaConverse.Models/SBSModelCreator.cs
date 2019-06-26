using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MagmaConverse.Interfaces;

namespace MagmaConverse.Models
{
    public sealed class SBSModelCreator : IEnumerable<IPersistableModel>
    {
        private static SBSModelCreator m_instance;

        public List<IPersistableModel> Models { get; } = new List<IPersistableModel>();

        private SBSModelCreator()
        {
        }

        public static SBSModelCreator Instance => m_instance ?? (m_instance = new SBSModelCreator());

        public T Create<T>(IFormManagerServiceSettings settings = null) where T : IPersistableModel 
        {
            IPersistableModel model = typeof(T).GetProperty("Instance")?.GetValue(null) as IPersistableModel ?? Activator.CreateInstance<T>();
            this.Models.Add(model);
            return (T) model;
        }

        public T GetModel<T>() where T : IPersistableModel
        {
            return this.Models.OfType<T>().FirstOrDefault();
        }

        public IEnumerator<IPersistableModel> GetEnumerator()
        {
            return this.Models.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
