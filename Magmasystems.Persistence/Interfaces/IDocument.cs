using System;

namespace Magmasystems.Persistence.Interfaces
{
	public interface IDocument : IDisposable
	{
		string Id { get; set; }  // This is the unique document id as set by the native database
		
		IDocumentDatabasePersistenceDriver DatabaseDriver { get; set; }
		object NativeDocument { get; set; }
		IDocumentCollection DocumentCollection { get; set; }
	}

	public interface IDocument<T> : IDocument where T : class, IPersistableDocumentObject
	{
		
	}
}