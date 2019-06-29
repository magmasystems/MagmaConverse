using System.Collections;
using System.Collections.Generic;

namespace Magmasystems.Persistence.Interfaces
{
	public interface IDocumentCollection
	{
		string Name { get; set; }

		IDocumentDatabasePersistenceDriver DatabaseDriver { get; set; }
		object NativeCollection { get; set; }

		IEnumerable Load(string sqlQuery = null);
		IDocument GetById(string id);
	}

	public interface IDocumentCollection<out T> : IDocumentCollection where T : IPersistableDocumentObject
	{
		new IEnumerable<T> Load(string sqlQuery = null);
		new T GetById(string id);
	}
}