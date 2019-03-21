using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics.Contracts;

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

    public static class TypeUtils
    {
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

        public static MethodInfo GetSetter(Type type, FieldInfo field)
        {
            Contract.Requires(type != null);
            Contract.Requires(field != null);

            string name = String.Format("Set{0}", field.Name.Capitalize());
            return type.GetMethod(name);
        }

        public static MethodInfo GetGetter(Type type, FieldInfo field)
        {
            Contract.Requires(type != null);
            Contract.Requires(field != null);

            string name = String.Format("Get{0}", field.Name.Capitalize());
            return type.GetMethod(name);
        }
    }

    public static class ReflectionUtils
    {
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