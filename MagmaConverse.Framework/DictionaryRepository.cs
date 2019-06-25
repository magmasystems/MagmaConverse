using System;
using System.Collections.Generic;

namespace MagmaConverse.Framework
{
    /// <summary>
    /// Implements a generic repository for objects, based on the C# Dictionary class
    /// </summary>
    /// <typeparam name="T">The class that the value field of the dictionary holds</typeparam>
    // ReSharper disable once InheritdocConsiderUsage
    [Serializable]
    public class DictionaryRepository<T> : Dictionary<string, T> where T : class
    {
        public DictionaryRepository() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}