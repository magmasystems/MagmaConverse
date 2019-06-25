using MagmaConverse.Configuration;

namespace MagmaConverse.Framework
{
    public static class ApplicationContext 
    {
        public static string Name { get; set; }

        public static DictionaryRepository<object> Properties { get; } = new DictionaryRepository<object>();
        
        public static object Get(string key) => Properties.TryGetValue(key, out object obj) ? obj : null;

        public static void Add(string key, object value)
        {
            if (Properties.ContainsKey(key))
                Properties[key] = value;
            else
                Properties.Add(key, value);
        }

        private static MagmaConverseConfiguration m_configuration { get; set; }
        public static MagmaConverseConfiguration Configuration => m_configuration ?? (m_configuration = MagmaConverseConfiguration.Read());

        public static bool IsInAutomatedMode { get; set; }
        public static int MaxRepeaterIterations { get; set; }

        /// <summary>
        /// If true, then debug mode is enabled for the forms. This results in some extra log information or printing to the console.
        /// </summary>
        public static bool DebugEnabled { get; set; }
    }
}
