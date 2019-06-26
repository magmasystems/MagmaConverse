using System;

namespace MagmaConverse.Persistence.Interfaces
{
	public interface IDocumentDatabase : IDisposable
	{
		string Name { get; set; }
		
		IDocumentDatabasePersistenceDriver DatabaseDriver { get; set; }
		object NativeDatabase { get; set; }
	}
}