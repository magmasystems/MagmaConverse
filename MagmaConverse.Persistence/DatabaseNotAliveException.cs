using System;

namespace MagmaConverse.Persistence
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
