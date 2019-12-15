using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;
using LibZConfig.Common.Config;
using LibZConfig.Common.Utils;
using Version = LibZConfig.Common.Config.Version;
using LibZConfig.Common.Config.Parsers;
using LibZConfig.Common.Config.Readers;
using LibZConfig.Common.Config.Factories;

namespace LibZConfig.Common
{
    /// <summary>
    /// Client/Service instance handle base class.
    /// </summary>
    public class ZConfigInstance
    {
        /// <summary>
        /// Unique client instance ID.
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// Client application group (or service application group).
        /// </summary>
        public string ApplicationGroup { get; set; }
        /// <summary>
        /// Client application Name (or service application Name).
        /// </summary>
        public string ApplicationName { get; set; }
        /// <summary>
        /// Client hostname.
        /// </summary>
        public string Hostname { get; set; }
        /// <summary>
        /// Client IP Address String.
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// Client instance start timestamp.
        /// </summary>
        public DateTime StartTime { get; set; }
    }

    /// <summary>
    /// State enum representing a environment state.
    /// </summary>
    public enum EEnvState
    {
        /// <summary>
        /// Environment State is unknown.
        /// </summary>
        Unknown,
        /// <summary>
        /// Environment has been initialized and available.
        /// </summary>
        Initialized,
        /// <summary>
        /// Environment has been disposed.
        /// </summary>
        Disposed,
        /// <summary>
        /// Environment has error(s).
        /// </summary>
        Error
    }

    /// <summary>
    /// Class represents a environment state instance.
    /// </summary>
    public class EnvState : AbstractState<EEnvState>
    {
        /// <summary>
        /// Default Empty constructor - Initialize state to Unknown.
        /// </summary>
        public EnvState()
        {
            State = EEnvState.Unknown;
        }

        /// <summary>
        /// Get the default error state for this type.
        /// </summary>
        /// <returns>Error State</returns>
        public override EEnvState GetDefaultErrorState()
        {
            return EEnvState.Error;
        }
        /// <summary>
        /// Get the state that represents an error state.
        /// </summary>
        /// <returns>Array Of Error states</returns>
        public override EEnvState[] GetErrorStates()
        {
            return new EEnvState[] { EEnvState.Error };
        }

        /// <summary>
        /// Check if the current state matches the expected.
        /// 
        /// Will throw exception in case state doesn't match.
        /// </summary>
        /// <param name="excepted">Expected State.</param>
        public void CheckState(EEnvState excepted)
        {
            if (State != excepted)
            {
                throw new StateException(String.Format("Invalid State : [expected={0}][actual={1}]", excepted.ToString(), State.ToString()));
            }
        }
    }

    /// <summary>
    /// Abstract base class for defining the operating environment.
    /// </summary>
    public abstract class ZConfigEnv
    {
        /// <summary>
        /// Name of this configuration instance.
        /// </summary>
        public string ConfigName { get; }
        private Configuration __configuration;
        /// <summary>
        /// Parsed configuration handle - Used to load the environment.
        /// </summary>
        public Configuration Configuration
        {
            get
            {
                return __configuration;
            }
        }
        /// <summary>
        /// Env instance state.
        /// </summary>
        private EnvState state = new EnvState();
        /// <summary>
        /// Get the Env instnace state
        /// </summary>
        public EEnvState State
        {
            get
            {
                return state.State;
            }
        }

        /// <summary>
        /// Default Constructor with configuration name and settings.
        /// </summary>
        /// <param name="name">Configuration Name</param>
        protected ZConfigEnv(string name)
        {
            Preconditions.CheckArgument(name);
            ConfigName = name;
        }

        /// <summary>
        /// Create and setup a new node instance handle.
        /// </summary>
        /// <typeparam name="T">Instance Type.</typeparam>
        /// <returns>Node Instance Handle.</returns>
        protected T SetupInstance<T>() where T : ZConfigInstance
        {
            T instance = Activator.CreateInstance<T>();
            instance.ID = Guid.NewGuid().ToString();
            instance.StartTime = DateTime.Now;

            ConfigurationAnnotationProcessor.Process<T>(Configuration, instance);
            instance.IP = NetUtils.GetIpAddress();
            instance.Hostname = NetUtils.GetHostName();
            instance.ApplicationGroup = Configuration.Header.ApplicationGroup;
            instance.ApplicationName = Configuration.Header.Application;

            return instance;
        }

        /// <summary>
        /// Initialize this client environment from the specified configuration file and version.
        /// </summary>
        /// <param name="configfile">Configuration file path</param>
        /// <param name="version">Configuration version (expected)</param>
        /// <param name="password">Password, if configuration has encrypted nodes.</param>
        protected void Init(string configfile, Version version, string password = null)
        {
            Preconditions.CheckArgument(configfile);
            Preconditions.CheckArgument(version);

            try
            {
                AbstractConfigParser parser = ConfigProviderFactory.GetParser(configfile);
                if (parser == null)
                {
                    throw new ConfigurationException(String.Format("Failed to get configuration parser. [config file={0}]", configfile));
                }
                Init(parser, configfile, version, password);
            }
            catch (Exception ex)
            {
                LogUtils.Debug(ex.Message);
                state.SetError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Initialize this client environment from the specified configuration file and version
        /// using the configuration parser.
        /// </summary>
        /// <param name="parser">Configuration parser to use.</param>
        /// <param name="configfile">Configuration file path.</param>
        /// <param name="version">Configuration version (expected)</param>
        /// <param name="password">Password, if configuration has encrypted nodes.</param>
        protected void Init(AbstractConfigParser parser, string configfile,
                              Version version, string password = null)
        {
            try
            {
                LogUtils.Info(String.Format(
                        "Initializing Client Environment : With Configuration file [{0}]...",
                        configfile));
                Uri path = new Uri(NetUtils.FilePathToFileUrl(configfile));
                AbstractReader reader = ConfigProviderFactory.GetReader(path);
                if (reader == null)
                {
                    throw new ConfigurationException(String.Format("Failed to get reader. [URI={0}]", path.ToString()));
                }
                parser.Parse(ConfigName, reader,
                        version,
                        null, password);
                __configuration = parser.GetConfiguration();
                if (__configuration == null)
                {
                    throw new ConfigurationException(String.Format(
                            "Error parsing configuration : NULL configuration read. [file={0}]",
                            configfile));
                }

                PostInit();

                UpdateState(EEnvState.Initialized);
            }
            catch (Exception e)
            {
                throw new ConfigurationException(e);
            }
        }

        /// <summary>
        /// Initialize this client environment from the specified configuration file and version.
        /// </summary>
        /// <param name="configfile">Configuration file path.</param>
        /// <param name="type">Configuration file type (in-case file type cannot be deciphered).</param>
        /// <param name="version">Configuration version (expected)</param>
        /// <param name="password">Password, if configuration has encrypted nodes.</param>
        protected void Init(string configfile,
                              EConfigType type,
                              Version version, string password = null)
        {
            try
            {
                AbstractConfigParser parser = ConfigProviderFactory.GetParser(type);
                if (parser == null)
                {
                    throw new ConfigurationException(String.Format(
                            "Cannot get configuration parser instance. [file={0}]",
                            configfile));
                }
                Init(parser, configfile, version, password);
            }
            catch (Exception e)
            {
                state.SetError(e);
                throw new ConfigurationException(e);
            }
        }

        /// <summary>
        /// Update the state of this instance.
        /// </summary>
        /// <param name="state">Updated State</param>
        protected void UpdateState(EEnvState state)
        {
            this.state.State = state;
        }

        /// <summary>
        /// Check the state of this instance.
        /// 
        /// Exception will be raised if state is not as expected.
        /// </summary>
        /// <param name="state">Expected state</param>
        protected void CheckState(EEnvState state)
        {
            this.state.CheckState(state);
        }

        /// <summary>
        /// Dispose this environment handle.
        /// </summary>
        protected void Dispoase()
        {
            if (state.State == EEnvState.Initialized)
            {
                LogUtils.Warn(String.Format("Disposing environment [timestamp={0}]", DateTime.Now.ToLongDateString()));
                state.State = EEnvState.Disposed;
            }
        }

        /// <summary>
        /// Perform post-initialisation tasks if any.
        /// </summary>
        public abstract void PostInit();

        private static ConfigVault __vault = new ConfigVault();
        /// <summary>
        /// Get a handle to the configuration vault.
        /// </summary>
        public static ConfigVault Vault
        {
            get
            {
                return __vault;
            }
        }

        private static ZConfigEnv __env = null;
        private static object __envLock = new object();

        /// <summary>
        /// Get a handle to the environment singleton.
        /// </summary>
        public static ZConfigEnv Env
        {
            get
            {
                if (__env != null)
                {
                    __env.CheckState(EEnvState.Initialized);
                }
                return __env;
            }
        }
        /// <summary>
        /// Shutdown the environment.
        /// </summary>
        public static void Shutdown()
        {
            Monitor.Enter(__envLock);
            try
            {
                if (__env != null)
                {
                    __env.Dispoase();
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error("Error while disposing environment...");
                LogUtils.Error(ex);
            }
            finally
            {
                Monitor.Exit(__envLock);
                __env = null;
            }
        }

        /// <summary>
        /// Create the environment singleton of the specified type.
        /// </summary>
        /// <typeparam name="T">Environment instance type.</typeparam>
        /// <returns>Environment Instance handle.</returns>
        protected static ZConfigEnv Initialize<T>() where T : ZConfigEnv
        {
            if (!Monitor.TryEnter(__envLock))
            {
                throw new ConfigurationException("Environment Lock not acquired or Lock held by another thread.");
            }
            if (__env == null)
            {
                __env = Activator.CreateInstance<T>();
            }
            return __env;
        }

        /// <summary>
        /// Get the env initialization lock.
        /// </summary>
        protected static void GetEnvLock()
        {
            if (__env == null || __env.State == EEnvState.Disposed)
            {
                throw new ConfigurationException("Environment has already been disposed.");
            }
            Monitor.Enter(__envLock);
        }

        /// <summary>
        /// Release the env initialization lock.
        /// </summary>
        protected static void ReleaseEnvLock()
        {
            if (__env == null || __env.State == EEnvState.Disposed)
            {
                throw new ConfigurationException("Environment has already been disposed.");
            }
            if (Monitor.TryEnter(__envLock))
            {
                Monitor.Exit(__envLock);
            }
        }
    }
}
