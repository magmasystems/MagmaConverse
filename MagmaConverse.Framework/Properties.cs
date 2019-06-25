using System;

namespace MagmaConverse.Framework
{
    [Serializable]
    public class Properties : DictionaryRepository<object>
    {
        public T Get<T>(string key)
        {
            return this.Get(key, default(T));
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (this.TryGetValue(key, out var obj) && obj is T)
            {
                return (T) obj;
            }

            return defaultValue;
        }

        public bool TryGet<T>(string key, out T val)
        {
            return this.TryGet(key, out val, default(T));
        }

        public bool TryGet<T>(string key, out T val, T defaultValue)
        {
            if (this.TryGetValue(key, out var obj) && obj is T)
            {
                val = (T) obj;
                return true;
            }

            val = defaultValue;
            return false;
        }
    }
}