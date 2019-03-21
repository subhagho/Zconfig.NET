using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LibZConfig.Common.Utils
{
    /// <summary>
    /// Utility class to provide Assembly searching/loading functionality.
    /// </summary>
    public static class AssemblyUtils
    {
        private static readonly Dictionary<string, Assembly> __assemblies = new Dictionary<string, Assembly>();

        /// <summary>
        /// Get (if already loaded) or Load the specified assembly.
        /// </summary>
        /// <param name="name">Assembly name</param>
        /// <param name="assemblyFile">Assembly DLL path to load from</param>
        /// <returns>Assembly instance</returns>
        public static Assembly GetOrLoadAssembly(string name, string assemblyFile)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(name));

            if (__assemblies.ContainsKey(name))
            {
                return __assemblies[name];
            }
            else
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (assemblies != null && assemblies.Length > 0)
                {
                    foreach (Assembly asm in assemblies)
                    {
                        string aname = asm.FullName.Split(',')[0];
                        if (name == aname)
                        {
                            LogUtils.Debug(String.Format("Found Assembly: [name={0}]", aname));
                            return asm;
                        }
                    }
                }
                LogUtils.Warn(String.Format("Loading Assembly: [name={0}][path={1}]", name, assemblyFile));
                Assembly assembly = Assembly.LoadFrom(assemblyFile);
                if (assembly != null)
                {
                    string aname = assembly.FullName.Split(',')[0];
                    __assemblies[aname] = assembly;
                    return assembly;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Class provides utility functions for using Reflection on types.
    /// </summary>
    public static class TypeUtils
    {
        /// <summary>
        /// Find the specified property for a type. Property string can be a 
        /// nested property reference.
        /// 
        /// Eg: "a.b.c"
        /// </summary>
        /// <param name="type">Type definition</param>
        /// <param name="name">Property name to find</param>
        /// <returns>Property Info</returns>
        public static PropertyInfo FindProperty(Type type, string name)
        {
            Contract.Requires(type != null);
            Contract.Requires(!String.IsNullOrWhiteSpace(name));

            string[] parts = name.Split('.');
            if (parts != null && parts.Length > 0)
            {
                string part = null;
                int index = 0;
                Type ttype = type;
                while (index < parts.Length)
                {
                    part = parts[index];
                    PropertyInfo pi = ttype.GetProperty(part);
                    if (pi != null)
                    {
                        if (index == (parts.Length - 1))
                        {
                            return pi;
                        }
                        ttype = pi.PropertyType;
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the specified field for a type. Field string can be a 
        /// nested field reference.
        /// 
        /// Eg: "a.b.c"
        /// </summary>
        /// <param name="type">Type definition</param>
        /// <param name="name">Field name to find</param>
        /// <returns>Field Info</returns>
        public static FieldInfo FindField(Type type, string name)
        {
            Contract.Requires(type != null);
            Contract.Requires(!String.IsNullOrWhiteSpace(name));

            string[] parts = name.Split('.');
            if (parts != null && parts.Length > 0)
            {
                string part = null;
                int index = 0;
                Type ttype = type;
                while (index < parts.Length)
                {
                    part = parts[index];
                    FieldInfo pi = ttype.GetField(part);
                    if (pi != null)
                    {
                        if (index == (parts.Length - 1))
                        {
                            return pi;
                        }
                        ttype = pi.FieldType;
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the Getter method for the specified field.
        /// Assumes naming convention: Get<Field>().
        /// 
        /// </summary>
        /// <param name="type">Type Definition</param>
        /// <param name="field">Field info</param>
        /// <returns>Method Info</returns>
        public static MethodInfo GetSetter(Type type, FieldInfo field)
        {
            Contract.Requires(type != null);
            Contract.Requires(field != null);

            string name = String.Format("Set{0}", field.Name.Capitalize());
            return type.GetMethod(name);
        }

        /// <summary>
        /// Find the Setter method for the specified field.
        /// Assumes naming convention: Set<Field>().
        /// 
        /// </summary>
        /// <param name="type">Type Definition</param>
        /// <param name="field">Field info</param>
        /// <returns>Method Info</returns>
        public static MethodInfo GetGetter(Type type, FieldInfo field)
        {
            Contract.Requires(type != null);
            Contract.Requires(field != null);

            string name = String.Format("Get{0}", field.Name.Capitalize());
            return type.GetMethod(name);
        }

        /// <summary>
        /// Call the setter method for the specified field.
        /// </summary>
        /// <param name="field">Field info</param>
        /// <param name="instance">Instance to call the </param>
        /// <param name="value">Value to set</param>
        public static void CallSetter(FieldInfo field, object instance, object value)
        {
            Type type = instance.GetType();
            MethodInfo mi = GetSetter(type, field);
            if (mi != null)
            {
                mi.Invoke(instance, new[] { value });
            }
            else
                throw new TargetInvocationException(new Exception(String.Format("Setter Method not found: [type={0}][field={1}]", type.FullName, field.Name)));
        }

        /// <summary>
        /// Call the getter method for the specified field.
        /// </summary>
        /// <param name="field">Field info</param>
        /// <param name="instance">Instance to call the </param>
        /// <returns>Field Value</returns>
        public static object CallGetter(FieldInfo field, object instance)
        {
            Type type = instance.GetType();
            MethodInfo mi = GetGetter(type, field);
            if (mi != null)
            {
                return mi.Invoke(instance, null);
            }
            else
                throw new TargetInvocationException(new Exception(String.Format("Getter Method not found: [type={0}][field={1}]", type.FullName, field.Name)));
        }
    }

    /// <summary>
    /// Generic reflection helper methods.
    /// </summary>
    public static class ReflectionUtils
    {
        /// <summary>
        /// Check if the specified type implements the passed generic interface.
        /// </summary>
        /// <param name="target">Type to check</param>
        /// <param name="intf">Interface to look for</param>
        /// <returns>Implements interface?</returns>
        public static bool ImplementsGenericInterface(Type target, Type intf)
        {
            return target.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == intf);
        }

        /// <summary>
        /// Convert to a primitive type from String.
        /// </summary>
        /// <param name="type">Target type</param>
        /// <param name="value">Input string value</param>
        /// <returns>Converted value</returns>
        public static object ConvertFromString(Type type, string value)
        {
            if (type.IsPrimitive)
            {
                if (type == typeof(Boolean) || type == typeof(bool))
                {
                    return Boolean.Parse(value);
                }
                else if (type == typeof(Byte) || type == typeof(byte))
                {
                    return Byte.Parse(value);
                }
                else if (type == typeof(Char) || type == typeof(char))
                {
                    return Char.Parse(value);
                }
                else if (type == typeof(SByte) || type == typeof(sbyte))
                {
                    return SByte.Parse(value);
                }
                else if (type == typeof(Decimal) || type == typeof(decimal))
                {
                    return Decimal.Parse(value);
                }
                else if (type == typeof(Double) || type == typeof(double))
                {
                    return Double.Parse(value);
                }
                else if (type == typeof(float) || type == typeof(Single))
                {
                    return float.Parse(value);
                }
                else if (type == typeof(int) || type == typeof(Int32))
                {
                    return Int32.Parse(value);
                }
                else if (type == typeof(uint) || type == typeof(UInt32))
                {
                    return UInt32.Parse(value);
                }
                else if (type == typeof(long) || type == typeof(Int64))
                {
                    return Int64.Parse(value);
                }
                else if (type == typeof(ulong) || type == typeof(UInt64))
                {
                    return UInt64.Parse(value);
                }
                else if (type == typeof(short) || type == typeof(Int16))
                {
                    return Int16.Parse(value);
                }
                else if (type == typeof(ushort) || type == typeof(UInt16))
                {
                    return UInt16.Parse(value);
                }
            }
            else if (type == typeof(string) || type == typeof(String))
            {
                return value;
            }
            else if (type.IsEnum)
            {
                return Enum.Parse(type, value);
            }
            return null;
        }
    }
}