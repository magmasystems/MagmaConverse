using System;
using System.Collections.Generic;
using System.Reflection;

// ReSharper disable LoopCanBeConvertedToQuery

namespace MagmaConverse.Framework
{
    /// <summary>
    /// This class contains static helper functions that do all sorts of things utilitizing the System.Reflection assembly.
    /// This includes:
    /// - createing an object from a type
    /// - creating a singleton object
    /// </summary>
    public static class ReflectionHelpers
    {
        /// <summary>
        /// Creates a object of a specified type.
        /// </summary>
        /// <param name="type">Type of the object to be created.</param>
        /// <param name="isStatic">Returned value that indicates whether the object is a stastic object.</param>
        /// <returns>A reference to the newly created object</returns>
        public static object InstantiateType(Type type, out bool isStatic)
        {
            return InstantiateType(type, null, out isStatic);
        }

        public static object InstantiateType(Type type, IArgumentList arglist, out bool isStatic)
        {
            isStatic = false;

            // Sanity check
            if (type == null)
                return null;
            if (type.IsInterface || type.IsAbstract)
                return null;

            object o = null;

            try
            {
                // Can we create an instance using Activator.CreateInstance? We can't if it is a singleton
                // and there is no public constructor.
                if (type.GetConstructor(Type.EmptyTypes) == null)
                {
                    isStatic = true;

                    // If we have a public method called ObjectInstance, then use that to get a reference
                    // to the singleton.
                    o = InstantiateSingleton(type);
                    if (o == null)
                    {
                        // As a last resort, find a non-public empty constructor and call it.
                        ConstructorInfo ci = type.GetConstructor(
                          BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                        if (ci != null)
                            o = ci.Invoke(null, null);
                    }

                    return o;
                }

                if (arglist != null)
                {
                    Type[] types = arglist.GetTypes();
                    ConstructorInfo ci = type.GetConstructor(types);
                    if (ci != null)
                    {
                        o = ci.Invoke(arglist.GetValues());
                        return o;
                    }
                }

                // We have a default constructor, so construct the object.
                o = Activator.CreateInstance(type, false);  // false == use public constructor
            }
            catch (Exception)
            {
                //				Logger.LogError("Could not instantiate type " + type.Name);
            }

            return o;
        }

        /// <summary>
        /// Instantiate a singleton object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <returns>A reference to the newly created object</returns>
        public static object InstantiateSingleton(Type type)
        {
            return InstantiateSingleton(type, null);
        }

        /// <summary>
        /// Instantiate a singleton object
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="creationMethod">The name of the property that is used to create the object.</param>
        /// <returns>A reference to the newly created object</returns>
        public static object InstantiateSingleton(Type type, string creationMethod)
        {
            MethodInfo mi;

            if (string.IsNullOrEmpty(creationMethod))
                creationMethod = "Instance";

            // If we have a public property for the creation method, then use that to get a reference
            // to the singleton.
            PropertyInfo propInfo = type.GetProperty(creationMethod, BindingFlags.Public | BindingFlags.Static);
            if (propInfo != null)
            {
                mi = propInfo.GetGetMethod();
                if (mi != null)
                {
                    object o = mi.Invoke(null, null);
                    return o;
                }
            }

            // If we don't have a property with the creationMethod name, then try a method with that name.
            mi = type.GetMethod(creationMethod, BindingFlags.Public | BindingFlags.Static);
            if (mi != null)
            {
                object o = mi.Invoke(null, null);
                return o;
            }

            return null;
        }

        /// <summary>
        /// This goes through an assembly and accumulated a list of all types that implement a certain interface.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="interfaceName">The name of the interface (ie:"IModel").</param>
        /// <returns>The (possibly empty) list of types that implement that interface.</returns>
        public static List<Type> GetTypesInAssemblyThatImplementAnInterface(Assembly assembly, string interfaceName)
        {
            List<Type> returnedTypes = new List<Type>();

            try
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsInterface || type.IsAbstract)
                        continue;

                    Type interfaceType = type.GetInterface(interfaceName);
                    if (interfaceType != null)
                        returnedTypes.Add(type);
                }
            }

            catch (ReflectionTypeLoadException rtlexc)
            {
                foreach (Exception exc in rtlexc.LoaderExceptions)
                    Console.WriteLine(exc.Message);
            }

            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            return returnedTypes;
        }

        /// <summary>
        /// This goes through an assembly and accumulated a list of all types that implement a certain attribute.
        /// </summary>
        /// <returns>The (possibly empty) list of types that implement that attribute.</returns>
        public static List<Type> GetTypesInAssemblyThatImplementAnAttribute<TSource, TAttr>()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(TSource));
            Type attrType = typeof(TAttr);
            return GetTypesInAssemblyThatImplementAnAttribute(assembly, attrType);
        }

        /// <summary>
        /// This goes through an assembly and accumulated a list of all types that implement a certain attribute.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="attribute">The type of the attribute to look for
        /// (ie: BusinessObjectFieldAttribute).</param>
        /// <returns>The (possibly empty) list of types that implement that attribute.</returns>
        public static List<Type> GetTypesInAssemblyThatImplementAnAttribute(Assembly assembly, Type attribute)
        {
            if (assembly == null || attribute == null)
                return null;

            List<Type> returnedTypes = new List<Type>();

            try
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsInterface || type.IsAbstract)
                        continue;

                    object[] attrs = type.GetCustomAttributes(attribute, true);
                    if (attrs.Length <= 0)
                        continue;
                    returnedTypes.Add(type);
                }
            }

            catch (ReflectionTypeLoadException rtlexc)
            {
                foreach (Exception exc in rtlexc.LoaderExceptions)
                    Console.WriteLine(exc.Message);
            }

            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            return returnedTypes;
        }

        public static List<Type> GetInterfacesInAssemblyThatImplementAnAttribute(Assembly assembly, Type attribute)
        {
            List<Type> returnedTypes = new List<Type>();

            try
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsInterface)
                    {
                        object[] attrs = type.GetCustomAttributes(attribute, true);
                        if (attrs.Length <= 0)
                            continue;
                        returnedTypes.Add(type);
                    }
                }
            }

            catch (ReflectionTypeLoadException rtlexc)
            {
                foreach (Exception exc in rtlexc.LoaderExceptions)
                    Console.WriteLine(exc.Message);
            }

            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            return returnedTypes;
        }


        /// <summary>
        /// This goes through an assembly and accumulated a list of all methods that implement a certain attribute.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <param name="attribute">The type of the attribute to look for
        /// (ie: BusinessObjectFieldAttribute).</param>
        /// <returns>The (possibly empty) list of methods that implement that attribute.</returns>
        public static Dictionary<Attribute, MethodInfo> GetMethodsInAssemblyThatImplementAnAttribute(Assembly assembly, Type attribute)
        {
            Dictionary<Attribute, MethodInfo> returnedMethods = null;

            try
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsInterface || type.IsAbstract)
                        continue;

                    MethodInfo[] methods = type.GetMethods();
                    foreach (MethodInfo mi in methods)
                    {
                        object[] attrs = mi.GetCustomAttributes(attribute, true);
                        if (attrs.Length <= 0)
                            continue;

                        // lazy creation
                        if (returnedMethods == null)
                            returnedMethods = new Dictionary<Attribute, MethodInfo>();
                        returnedMethods.Add((Attribute)attrs[0], mi);
                    }
                }
            }

            catch (ReflectionTypeLoadException rtlexc)
            {
                foreach (Exception exc in rtlexc.LoaderExceptions)
                    Console.WriteLine(exc.Message);
            }

            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
            }

            return returnedMethods;
        }

        public static Dictionary<Attribute, MethodInfo> GetMethodsInTypeThatImplementAnAttribute(Type type, Type attribute)
        {
            Dictionary<Attribute, MethodInfo> returnedMethods = null;

            foreach (MethodInfo mi in type.GetMethods())
            {
                object[] attrs = mi.GetCustomAttributes(attribute, true);
                if (attrs.Length <= 0)
                    continue;

                // lazy creation
                if (returnedMethods == null)
                    returnedMethods = new Dictionary<Attribute, MethodInfo>();
                returnedMethods.Add((Attribute)attrs[0], mi);
            }

            return returnedMethods;
        }

        public static List<PropertyInfo> GetPropertiesInTypeThatImplementAnAttribute(Type type, Type attribute)
        {
            List<PropertyInfo> returnedProps = null;

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                object[] attrs = propertyInfo.GetCustomAttributes(attribute, true);
                if (attrs.Length <= 0)
                    continue;

                // lazy creation
                if (returnedProps == null)
                    returnedProps = new List<PropertyInfo>();
                returnedProps.Add(propertyInfo);
            }

            return returnedProps;
        }

        /// <summary>
        /// Goes through a type, looking for all of the properties that implement an Attribute. For each of those properties, an action is performed.
        /// </summary>
        /// <param name="type">A type to scan through</param>
        /// <param name="attribute">The attribute that the property must implement</param>
        /// <param name="action">An action to do on the property</param>
        /// <returns>True if there was at least one property that implemented the attribute</returns>
        public static bool ProcessPropertiesInTypeThatImplementAnAttribute(Type type, Type attribute, Action<PropertyInfo, Attribute> action)
        {
            if (action == null)
                return false;

            var props = GetPropertiesInTypeThatImplementAnAttribute(type, attribute);
            if (props == null || props.Count == 0)
                return false;

            foreach (var prop in props)
            {
                var attrs = prop.GetCustomAttributes(attribute);
                foreach (var attr in attrs)
                    action(prop, attr);
            }

            return true;
        }

        public static List<PropertyInfo> GetPropertiesInTypeThatDoNotImplementAnAttribute(Type type, Type attribute)
        {
            List<PropertyInfo> returnedProps = null;

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                object[] attrs = propertyInfo.GetCustomAttributes(attribute, true);
                if (attrs.Length > 0)
                    continue;

                // lazy creation
                if (returnedProps == null)
                    returnedProps = new List<PropertyInfo>();
                returnedProps.Add(propertyInfo);
            }

            return returnedProps;
        }

        public static Dictionary<Attribute, PropertyInfo> GetPropertiesInTypeThatImplementAttributes(Type type, Type[] attributes)
        {
            Dictionary<Attribute, PropertyInfo> returnedProps = null;

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                object[] attrs = propertyInfo.GetCustomAttributes(false);
                if (attrs.Length <= 0)
                    continue;

                foreach (Attribute returnedAttr in attrs)
                {
                    foreach (Type a in attributes)
                    {
                        if (returnedAttr.GetType() == a)
                        {
                            // lazy creation
                            if (returnedProps == null)
                                returnedProps = new Dictionary<Attribute, PropertyInfo>();
                            returnedProps.Add(returnedAttr, propertyInfo);
                        }
                    }
                }
            }

            return returnedProps;
        }

        public static bool AreAllFieldsEmpty(object obj)
        {
            Type vt = obj.GetType();

            FieldInfo[] mis = vt.GetFields(
              BindingFlags.DeclaredOnly |
              BindingFlags.Instance |
              BindingFlags.Public);

            foreach (FieldInfo m in mis)
            {
                if (m.Name == ".ctor") //ignore constructor in the case where StoredProcedureIgnore is not applied to it.
                    continue;

                bool bEmpty =
                  (vt.GetField(m.Name).GetValue(obj) == null ||
                  (vt.GetField(m.Name).FieldType == typeof(DateTime) && (DateTime)vt.GetField(m.Name).GetValue(obj) == DateTime.MinValue) ||
                  (vt.GetField(m.Name).FieldType == typeof(string) && ((string)vt.GetField(m.Name).GetValue(obj)).Length == 0) ||
                  (vt.GetField(m.Name).FieldType == typeof(int) && ((int)vt.GetField(m.Name).GetValue(obj)) == -1)
                  );

                if (!bEmpty)
                    return false;
            }

            return true;
        }


        #region Functions that deal with an object's Properties
        public static PropertyInfo HasProperty(object obj, string propertyName)
        {
            if (obj == null)
                return null;
            return HasProperty(obj.GetType(), propertyName);
        }

        public static PropertyInfo HasProperty(Type type, string propertyName)
        {
            PropertyInfo propInfo = type?.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return propInfo;
        }

        public static MethodInfo HasGetter(object obj, string propertyName)
        {
            PropertyInfo propInfo = HasProperty(obj, propertyName);
            return propInfo?.GetGetMethod();
        }

        public static MethodInfo HasSetter(object obj, string propertyName)
        {
            PropertyInfo propInfo = HasProperty(obj, propertyName);
            return propInfo?.GetSetMethod();
        }

        public static object GetFieldValue(object obj, string fieldName)
        {
            FieldInfo mi = obj?.GetType().GetField(fieldName,
              BindingFlags.FlattenHierarchy |
              BindingFlags.IgnoreCase |
              BindingFlags.Instance |
              BindingFlags.NonPublic | BindingFlags.Public);
            return mi?.GetValue(obj);
        }

        // Save constant small object allocs by preallocating a one-element array
        private static readonly object[] m_invokerSecondArg = new object[1];

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null)
                return null;

            MethodInfo mi = HasGetter(obj, propertyName);
            if (mi == null)
                return null;

            m_invokerSecondArg[0] = null;
            return mi.Invoke(obj, null);
        }

        /// <summary>
        /// Gets a property value. Sets a variable called 'found' to true if we have a property called 'propertyName'.
        /// This will distinguish between getting a property that has a null value, and trying to get a property
        /// that wasn't found.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        public static object GetPropertyValue(object obj, string propertyName, out bool found)
        {
            found = false;

            if (obj == null)
                return null;

            MethodInfo mi = HasGetter(obj, propertyName);
            if (mi == null)
                return null;

            found = true;

            m_invokerSecondArg[0] = null;
            return mi.Invoke(obj, null);
        }

        /// <summary>
        /// This version of SetPropertyValue does not attempt to do any setting of sensible default values
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="val"></param>
        public static void SetPropertyValue(object obj, string propertyName, object val)
        {
            if (obj == null)
                return;

            MethodInfo mi = HasSetter(obj, propertyName);
            if (mi == null)
                return;

            m_invokerSecondArg[0] = val;
            mi.Invoke(obj, m_invokerSecondArg);
        }


        /// <summary>
        /// This version of SetPropertyValue attempts to set a sensible non-null value. It is mainly used for
        /// Business Objects and for Models that take values from databases.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="oVal"></param>
        public static void SetSensiblePropertyValue(object obj, string propertyName, object oVal)
        {
            PropertyInfo propInfo = HasProperty(obj, propertyName);
            SetSensiblePropertyValue(obj, propInfo, oVal);
        }

        public static void SetSensiblePropertyValue(object obj, PropertyInfo propInfo, object oVal)
        {
            // Sanity check. Make sure that the property has a setter
            if (propInfo == null || !propInfo.CanWrite)
                return;

            if (propInfo.PropertyType == typeof(string))
            {
                if (oVal == null || oVal == DBNull.Value)
                    propInfo.SetValue(obj, null, null);
                else
                    propInfo.SetValue(obj, ((string)oVal).Trim(), null);
            }
            else if (propInfo.PropertyType == typeof(DateTime))
            {
                if (oVal == null || oVal == DBNull.Value)
                    propInfo.SetValue(obj, DateTime.MinValue, null);
                else if (oVal is string)
                    propInfo.SetValue(obj, DateTime.Parse(((string)oVal).Trim()), null);
                else
                    propInfo.SetValue(obj, oVal, null);
            }
            else
            {
                if (oVal != null && oVal != DBNull.Value)
                    propInfo.SetValue(obj, oVal, null);
            }
        }
        #endregion
    }

    public interface IArgument
    {
        string Name { get; set; }
        object Value { get; set; }
        string Type { get; set; }
    }

    public interface IArgumentList
    {
        /// <summary>
        /// Returns an array of type information corresponding to each argument in the arglist.
        /// </summary>
        /// <returns></returns>
        Type[] GetTypes();

        /// <summary>
        /// Returns an array of values corresponding to each argument in the arglist.
        /// </summary>
        /// <returns></returns>
        object[] GetValues();
    }
}
