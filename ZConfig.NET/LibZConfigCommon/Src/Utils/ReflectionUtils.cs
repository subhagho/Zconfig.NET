using System;
using System.Collections.Generic;
using System.Reflection;

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
}