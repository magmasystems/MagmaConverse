using System;
using System.Data;

namespace Magmasystems.Persistence.Interfaces
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
