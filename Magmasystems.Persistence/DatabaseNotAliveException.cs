using System;

namespace Magmasystems.Persistence
{
    [Serializable]
    public class DatabaseNotAliveException : ApplicationException
    {
        public DatabaseNotAliveException() : base("The database is not alive")
        {
        }

        public DatabaseNotAliveException(string message) : base(message)
        {
        }
    }
}
