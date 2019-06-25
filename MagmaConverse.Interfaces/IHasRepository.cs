using System.Collections.Generic;

namespace MagmaConverse.Interfaces
{
    public interface IHasRepository<T> where T : class
    {
        /// <summary>
        /// Get all of the vaklues in the repository
        /// </summary>
        /// <returns></returns>
        List<T> GetAll();

        /// <summary>
        /// Find a pafrticular value by the key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetById(string key);

        /// <summary>
        /// Add an item, to the repository
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        bool Add(string key, T value, bool overwrite = false);

        /// <summary>
        /// Get the number of items in the repository
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Empty the repository
        /// </summary>
        void Clear();

        /// <summary>
        /// Removes a single entry
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Remove(string key);

        /// <summary>
        /// Removes a list of items
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        bool Remove(string[] keys);

    }
}
