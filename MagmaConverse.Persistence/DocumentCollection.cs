using System.Collections;
using System.Collections.Generic;
using MagmaConverse.Persistence.Interfaces;

namespace MagmaConverse.Persistence
{
	public class DocumentCollection : IDocumentCollection
	{
		#region Variables
		public string Name { get; set; }
		public IDocumentDatabasePersistenceDriver DatabaseDriver { get; set; }
		public object NativeCollection { get; set; }
		#endregion

		#region Constructors
		public DocumentCollection()
		{
		}
	
		public DocumentCollection(string name) : this()
		{
			this.Name = name;
		}
		#endregion

		#region Cleanup
		public void Dispose()
		{
			// TODO - call the driver
		}
		#endregion

		#region Loading
		public IEnumerable Load(string sqlQuery = null)
		{
			IEnumerable data = this.DatabaseDriver.Get(this, sqlQuery);
			return data;
		}
		#endregion

		#region Querying
		public IDocument GetById(string id)
		{
			var foundObject = this.DatabaseDriver.GetById(this, id);
			return foundObject;
		}
		#endregion
	}


	public class DocumentCollection<T> : DocumentCollection, IDocumentCollection<T> where T : class, IPersistableDocumentObject
	{
		#region Variables
		#endregion

		#region Constructors
		public DocumentCollection()
		{
		}

		public DocumentCollection(string name) : base(name)
		{
		}
		#endregion

		#region Loading
		public new IEnumerable<T> Load(string sqlQuery = null)
		{
			IEnumerable<T> data = this.DatabaseDriver.Get<T>(this, sqlQuery);
			return data;
		}
		#endregion

		#region Querying
		public new T GetById(string id)
		{
			var foundObject = this.DatabaseDriver.GetById<T>(this, id);
			return foundObject;
		}
		#endregion
	}
}
