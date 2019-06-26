using MagmaConverse.Persistence.Interfaces;

namespace MagmaConverse.Persistence
{
	public class Document : IDocument
	{
		#region Variables
		public string Id { get; set; }
		public IDocumentDatabasePersistenceDriver DatabaseDriver { get; set; }
		public object NativeDocument { get; set; }
		public IDocumentCollection DocumentCollection { get; set; }
		#endregion

		#region Constructors
	    // ReSharper disable once EmptyConstructor
		public Document()
		{
		}
		#endregion

		#region Cleanup
		public virtual void Dispose()
		{
			// TODO - call the driver
		}
		#endregion
	}

	public class Document<T> : Document, IDocument<T> where T : class, IPersistableDocumentObject
	{
	}
}
