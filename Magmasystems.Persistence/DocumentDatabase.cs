using Magmasystems.Persistence.Interfaces;

namespace Magmasystems.Persistence
{
	public class DocumentDatabase : IDocumentDatabase
	{
		#region Variables
		public IDocumentDatabasePersistenceDriver DatabaseDriver { get; set; }
		public object NativeDatabase { get; set; }
		public string Name { get; set; }
		#endregion

		#region Constructors
		public DocumentDatabase()
		{
		}

		public DocumentDatabase(string databaseName) : this()
		{
			this.Name = databaseName;
		}
		#endregion

		#region Cleanup
		public virtual void Dispose()
		{
			// TODO - call the driver
		}
		#endregion
	}
}
