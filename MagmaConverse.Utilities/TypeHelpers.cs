using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace MagmaConverse.Utilities
{
    public static class TypeHelpers
    {
        public static Type LoadType(string typeName)
        {
            const string theProbingDirectories = ".";

            // First try a simple fetch
            Type type = Type.GetType(typeName);
            if (type != null)
                return type;

            // Look at the <$Root> element, and see if we have a list of directories to probe
            string[] probingDirectories = theProbingDirectories.Split(';', ',');
            if (probingDirectories.Length == 0)
                return null;

            // The typename is something like this:
            // MagmaConverse.MessagingService.GetBroker, MagmaConverse.MessagingService, Version=13.1.0.0, Culture=neutral, PublicKeyToken=3835f10dbe79083f
            // We want to isolate the assembly name.
            string[] typeParts = typeName.Split(new[] { ", " }, StringSplitOptions.None);
            if (typeParts.Length < 2)
                return null;

            // Go through each directory in the list of dirs to probe for the assembly
            foreach (string dir in probingDirectories)
            {
                // Form the full path name of the assembly to load.
                string path = $"{dir}{(!dir.EndsWith("\\", StringComparison.CurrentCulture) ? "\\" : "")}{typeParts[1]}.dll";
                if (!File.Exists(path))
                    continue;

                try
                {
                    // Try loading the class from the assembly
                    type = Type.GetType(typeName, typeResolver: null, throwOnError: false, assemblyResolver: name => Assembly.LoadFrom(path));
                    if (type != null)
                        return type;
                }
                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
                {
                    // ignored
                }
                #pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            // Sorry ....
            return null;
        }

        public static Type LoadType2(string typeName)
        {
            var assemblyName = typeName.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath($"{AppDomain.CurrentDomain.BaseDirectory}{assemblyName}.dll");
            if (assembly == null)
                return null;
            var type = Type.GetType(typeName);
            return type;
        }
    }
}
