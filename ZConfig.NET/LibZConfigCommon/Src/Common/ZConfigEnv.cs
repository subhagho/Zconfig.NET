using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LibZConfig.Common.Config;
using LibZConfig.Common.Utils;

namespace LibZConfig.Common
{
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
        /// <summary>
        /// Parsed configuration handle - Used to load the environment.
        /// </summary>
        public Configuration Configuration { get; }
        /// <summary>
        /// Client instance state.
        /// </summary>
        private EnvState state = new EnvState();

        /// <summary>
        /// Default Constructor with configuration name and settings.
        /// </summary>
        /// <param name="name">Configuration Name</param>
        /// <param name="settings">Configuration Settings</param>
        protected ZConfigEnv(string name, ConfigurationSettings settings)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(name));
            Contract.Requires(settings != null);

            ConfigName = name;
            Configuration = new Configuration(settings);
        }
    }
}
