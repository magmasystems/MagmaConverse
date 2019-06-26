using System;
using System.Data;

namespace MagmaConverse.Persistence.Interfaces
{
	public interface IPersistenceDriver : IDisposable
	{
		event Action<ConnectionState> ConnectionStateChanged;

		DatabaseVendors Vendor { get; }

		bool Connect(string connectionString = null);
		bool Disconnect();
		bool IsConnected { get; }
	}
}
