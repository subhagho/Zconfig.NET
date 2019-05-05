using System;
using System.Threading;
using System.Collections.Generic;
using LibZConfig.Common;
using LibZConfig.Common.Config;
using LibZConfig.Common.Config.Readers;
using LibZConfig.Common.Config.Nodes;
using LibZConfig.Common.Config.Factories;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Structs;
using Version = LibZConfig.Common.Config.Version;

namespace LibZConfigClient.Src
{
    /// <summary>
    /// Local Struct to store the autowired instances
    /// loaded by this Configuration Manager.
    /// </summary>
    public class AutowiredIndexStruct
    {
        /// <summary>
        /// Instance Type
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// Configuration Name
        /// </summary>
        public string ConfigName { get; set; }
        /// <summary>
        /// Relative Path instance loaded from.
        /// </summary>
        public string RelativePath { get; set; }
        /// <summary>
        /// Loaded instance handle.
        /// </summary>
        public object Instance { get; set; }

        public override string ToString()
        {
            return "AutowiredIndexStruct{" +
                   "configName='" + ConfigName + '\'' +
                   ", type=" + Type.FullName +
                   ", relativePath='" + RelativePath + '\'' +
                   '}';
        }
    }

    /// <summary>
    /// Class loads and manages configuration instances and annotated class instances.
    /// </summary>
    public class ConfigurationManager
    {
        private const int DEFAULT_READ_LOCK_TIMEOUT = 1 * 60 * 1000; // 1 minute
        private const int DEFAULT_WRITE_LOCK_TIMEOUT = 5 * 60 * 1000; // 5 minutes

        private ConfigurationLoader loader = new ConfigurationLoader();
        private Dictionary<string, Configuration> loadedConfigs = new Dictionary<string, Configuration>();
        private Dictionary<string, ReaderWriterLock> configLocks = new Dictionary<string, ReaderWriterLock>();
        private Dictionary<string, object> autowiredObjects = new Dictionary<string, object>();
        private ConcurrentMultiMap<string, AutowiredIndexStruct> autowiredIndex = new ConcurrentMultiMap<string, AutowiredIndexStruct>();

        /// <summary>
        /// Load a configuration from local/remote location.
        /// </summary>
        /// <param name="configName">Configuration name</param>
        /// <param name="configUri">Configuration File URI</param>
        /// <param name="configType">Configuration Type</param>
        /// <param name="version">Configuration Version (expected)</param>
        /// <param name="settings">Configuration Settings</param>
        /// <param name="password">Password (if required)</param>
        /// <returns>Loaded Configruation</returns>
        public Configuration Load(string configName, string configUri,
            EConfigType configType, Version version,
            ConfigurationSettings settings, string password = null)
        {
            Configuration config = null;
            if (!loadedConfigs.ContainsKey(configName))
            {
                lock (loadedConfigs)
                {
                    if (!loadedConfigs.ContainsKey(configName))
                    {
                        config = loader.Load(configName, configUri, configType, version, settings, password);
                        ConfigLoaded(config);
                    }
                }
            }
            config = loadedConfigs[configName];
            if (!config.Header.Version.Equals(version))
            {
                throw new ConfigurationException(String.Format("Versions not compatible. [expected=%s][actual=%s]",
                    version.ToString(), config.Header.Version.ToString()));
            }
            return config;
        }

        /// <summary>
        /// Load a configuration from local/remote location. 
        /// Will load using default settings.
        /// </summary>
        /// <param name="configName">Configuration name</param>
        /// <param name="configUri">Configuration File URI</param>
        /// <param name="configType">Configuration Type</param>
        /// <param name="version">Configuration Version (expected)</param>
        /// <param name="password">Password (if required)</param>
        /// <returns>Loaded Configruation</returns>
        public Configuration Load(string configName, string configUri,
            EConfigType configType, Version version,
            string password = null)
        {
            Configuration config = null;
            if (!loadedConfigs.ContainsKey(configName))
            {
                lock (loadedConfigs)
                {
                    if (!loadedConfigs.ContainsKey(configName))
                    {
                        config = loader.Load(configName, configUri, configType, version, password);
                        ConfigLoaded(config);
                    }
                }
            }
            config = loadedConfigs[configName];
            if (!config.Header.Version.Equals(version))
            {
                throw new ConfigurationException(String.Format("Versions not compatible. [expected=%s][actual=%s]",
                    version.ToString(), config.Header.Version.ToString()));
            }
            return config;
        }

        /// <summary>
        /// Load a configuration from local file.
        /// </summary>
        /// <param name="configName">Configuration name</param>
        /// <param name="configFile">Configuration File Path</param>
        /// <param name="version">Configuration Version (expected)</param>
        /// <param name="settings">Configuration Settings</param>
        /// <param name="password">Password (if required)</param>
        /// <returns>Loaded Configruation</returns>
        public Configuration Load(string configName, string configFile,
             Version version, ConfigurationSettings settings,
            string password = null)
        {
            Configuration config = null;
            if (!loadedConfigs.ContainsKey(configName))
            {
                lock (loadedConfigs)
                {
                    if (!loadedConfigs.ContainsKey(configName))
                    {
                        config = loader.Load(configName, configFile, version, settings, password);
                        ConfigLoaded(config);
                    }
                }
            }
            config = loadedConfigs[configName];
            if (!config.Header.Version.Equals(version))
            {
                throw new ConfigurationException(String.Format("Versions not compatible. [expected=%s][actual=%s]",
                    version.ToString(), config.Header.Version.ToString()));
            }
            return config;
        }

        /// <summary>
        /// Load a configuration from local file.
        /// Will load using default settings.
        /// </summary>
        /// <param name="configName">Configuration name</param>
        /// <param name="configFile">Configuration File Path</param>
        /// <param name="version">Configuration Version (expected)</param>
        /// <param name="password">Password (if required)</param>
        /// <returns>Loaded Configruation</returns>
        public Configuration Load(string configName, string configFile,
            Version version,
            string password = null)
        {
            Configuration config = null;
            if (!loadedConfigs.ContainsKey(configName))
            {
                lock (loadedConfigs)
                {
                    if (!loadedConfigs.ContainsKey(configName))
                    {
                        config = loader.Load(configName, configFile, version, password);
                        ConfigLoaded(config);
                    }
                }
            }
            config = loadedConfigs[configName];
            if (!config.Header.Version.Equals(version))
            {
                throw new ConfigurationException(String.Format("Versions not compatible. [expected=%s][actual=%s]",
                    version.ToString(), config.Header.Version.ToString()));
            }
            return config;
        }

        /// <summary>
        /// Actions to be performed post configuration load.
        /// </summary>
        /// <param name="config">Loaded Configuration</param>
        private void ConfigLoaded(Configuration config)
        {
            LogUtils.Debug(String.Format("Configuration loaded. [name={0}][version={1}]", config.Header.Name, config.Header.Version.ToString()));
            loadedConfigs[config.Header.Name] = config;
            configLocks[config.Header.Name] = new ReaderWriterLock();
        }

        /// <summary>
        /// Get a cached configuration handle.
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <returns>Cached handle or NULL if not in cache</returns>
        public Configuration GetConfiguration(string name)
        {
            Preconditions.CheckArgument(name);
            if (loadedConfigs.ContainsKey(name))
            {
                return loadedConfigs[name];
            }
            return null;
        }

        /// <summary>
        /// Get a configuration handle and lock it for updates.
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <param name="timeout">Milliseconds Timeout</param>
        /// <returns>Configuration handle</returns>
        public Configuration WriteLockConfig(string name, int timeout)
        {
            Configuration config = GetConfiguration(name);
            if (config == null)
            {
                throw new ConfigurationException(String.Format("Specified configuration not loaded. [name={0}]", name));
            }
            ReaderWriterLock mutex = configLocks[config.Header.Name];
            try
            {
                mutex.AcquireWriterLock(timeout);
            }
            catch (ApplicationException)
            {
                LogUtils.Warn(String.Format("Configuration Write Lock request timeout. [name={0}]", name));
                return null;
            }
            return config;
        }

        /// <summary>
        /// Get a configuration handle and lock it for updates.
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <param name="timeout">Milliseconds Timeout</param>
        /// <returns>Configuration handle</returns>
        public Configuration ReadLockConfig(string name, int timeout)
        {
            Configuration config = GetConfiguration(name);
            if (config == null)
            {
                throw new ConfigurationException(String.Format("Specified configuration not loaded. [name={0}]", name));
            }
            ReaderWriterLock mutex = configLocks[config.Header.Name];
            try
            {
                mutex.AcquireReaderLock(timeout);
            }
            catch (ApplicationException)
            {
                LogUtils.Warn(String.Format("Configuration Read Lock request timeout. [name={0}]", name));
                return null;
            }
            return config;
        }

        /// <summary>
        /// Release a read lock on a configuration instance.
        /// 
        /// Warning: This method doesn't do a thread ownership check. Hence if incorrectly
        /// called can release a lock acquired by some other thread.
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <returns>Is released?</returns>
        public bool ConfigReleaseRead(string name)
        {
            Preconditions.CheckArgument(name);

            if (configLocks.ContainsKey(name))
            {
                ReaderWriterLock mutex = configLocks[name];
                if (mutex.IsReaderLockHeld)
                {
                    mutex.ReleaseReaderLock();
                    return true;
                }
            }
            else
            {
                throw new ConfigurationException(String.Format("Specified configuration not loaded. [name={0}]", name));
            }
            return false;
        }

        /// <summary>
        /// Release a write lock on a configuration instance.
        /// 
        /// Warning: This method doesn't do a thread ownership check. Hence if incorrectly
        /// called can release a lock acquired by some other thread.
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <returns>Is released?</returns>
        public bool ConfigReleaseWrite(string name)
        {
            Preconditions.CheckArgument(name);

            if (configLocks.ContainsKey(name))
            {
                ReaderWriterLock mutex = configLocks[name];
                if (mutex.IsWriterLockHeld)
                {
                    mutex.ReleaseWriterLock();
                    return true;
                }
            }
            else
            {
                throw new ConfigurationException(String.Format("Specified configuration not loaded. [name={0}]", name));
            }
            return false;
        }

        /// <summary>
        /// Create/Get an instance of an autowired object.
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="configName">Configuration Name (should be loaded)</param>
        /// <param name="path">Path in the configuration</param>
        /// <returns>Object Instance</returns>
        public T AutowireType<T>(string configName, string path)
        {
            Preconditions.CheckArgument(configName);
            return AutowireType<T>(configName, path, false);
        }

        /// <summary>
        /// Create/Get an instance of an autowired object.
        /// </summary>
        /// <typeparam name="T">Type of Object</typeparam>
        /// <param name="configName">Configuration Name (should be loaded)</param>
        /// <param name="path">Path in the configuration</param>
        /// <param name="update">Node path updated</param>
        /// <returns>Object Instance</returns>
        private T AutowireType<T>(string configName, string path, bool update)
        {
            Preconditions.CheckArgument(configName);

            Type type = typeof(T);
            string key = GetTypeKey(type, path, configName);
            if (!String.IsNullOrWhiteSpace(key))
            {
                if (!autowiredObjects.ContainsKey(key) || update)
                {
                    lock (autowiredObjects)
                    {
                        if (!autowiredObjects.ContainsKey(key) || update)
                        {
                            Configuration config = ReadLockConfig(configName, DEFAULT_READ_LOCK_TIMEOUT);
                            if (config != null)
                            {
                                try
                                {
                                    T value = default(T);
                                    if (update)
                                    {
                                        if (!autowiredObjects.ContainsKey(key))
                                        {
                                            throw new ConfigurationException(String.Format("Update called on an instance not loaded. [type={0}]", type.FullName));
                                        }
                                        value = (T)autowiredObjects[key];
                                    }
                                    else
                                    {
                                        value = Activator.CreateInstance<T>();
                                        autowiredObjects[key] = value;
                                    }
                                    List<string> valuePaths = null;
                                    if (!String.IsNullOrWhiteSpace(path))
                                    {
                                        AbstractConfigNode node = config.Find(path);
                                        if (node == null)
                                        {
                                            throw new ConfigurationException(
                                                String.Format("Specified configuration node not found. [config={0}][path={1}]", configName, path));
                                        }
                                        ConfigurationAnnotationProcessor.Process<T>(node, value, out valuePaths);
                                    }
                                    else
                                    {
                                        ConfigurationAnnotationProcessor.Process<T>(config, value, out valuePaths);
                                    }
                                    if (valuePaths != null && valuePaths.Count > 0 && (config.SyncMode == ESyncMode.BATCH || config.SyncMode == ESyncMode.EVENTS))
                                    {
                                        AutowiredIndexStruct ais = new AutowiredIndexStruct();
                                        ais.ConfigName = configName;
                                        ais.Type = type;
                                        ais.RelativePath = path;
                                        ais.Instance = value;

                                        foreach (string vp in valuePaths)
                                        {
                                            autowiredIndex.Add(vp, ais);
                                        }
                                    }
                                }
                                finally
                                {
                                    ConfigReleaseRead(config.Header.Name);
                                }
                            }
                            else
                            {
                                throw new ConfigurationException(
                                    String.Format("Error getting confguration. (Might be a lock timeout) [name={0}]", configName));
                            }
                        }
                    }
                }
                return (T)autowiredObjects[key];
            }
            throw new ConfigurationException(
                String.Format("Error creating autowired instance. [type={0}][config={1}]", type.FullName, configName));
        }

        /// <summary>
        /// Get the type key to index the annotated instances.
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="path">Search Path</param>
        /// <param name="configName">Configuration name</param>
        /// <returns>Index Key</returns>
        private string GetTypeKey(Type type, string path, string configName)
        {
            string p = GetSearchPath(type, path);
            if (!String.IsNullOrWhiteSpace(p))
            {
                return String.Format("{0}::{1}::{2}", type.FullName, configName, p);
            }
            return null;
        }

        /// <summary>
        /// Get the complete search path.
        /// </summary>
        /// <param name="type">Object type</param>
        /// <param name="path">Appended Search Path</param>
        /// <returns>Search path</returns>
        private string GetSearchPath(Type type, string path)
        {
            string p = ConfigurationAnnotationProcessor.GetAnnotationPath(type);
            if (!String.IsNullOrWhiteSpace(p))
            {
                if (String.IsNullOrWhiteSpace(path))
                {
                    return p;
                }
                else
                {
                    return String.Format("{0}/{1}", path, p);
                }
            }
            return null;
        }
    }
}
