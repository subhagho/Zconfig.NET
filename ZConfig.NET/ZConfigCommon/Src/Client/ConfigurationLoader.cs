using System;
using LibZConfig.Common;
using LibZConfig.Common.Config;
using LibZConfig.Common.Config.Readers;
using LibZConfig.Common.Config.Parsers;
using LibZConfig.Common.Config.Factories;
using LibZConfig.Common.Utils;
using Version = LibZConfig.Common.Config.Version;

namespace LibZConfigClient
{
    /// <summary>
    /// Loader class to provide methods to read and parse configurations.
    /// </summary>
    public class ConfigurationLoader
    {
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
            Preconditions.CheckArgument(configName);
            Preconditions.CheckArgument(configUri);
            Preconditions.CheckArgument(configType);
            Preconditions.CheckArgument(version);

            LogUtils.Info(String.Format("Loading Configuration. [name={0}][version={1}][uri={2}]", configName, version.ToString(), configUri));

            Uri uri = new Uri(configUri);
            using (AbstractReader reader = ConfigProviderFactory.GetReader(uri))
            {
                AbstractConfigParser parser = ConfigProviderFactory.GetParser(configType);
                Postconditions.CheckCondition(parser);

                parser.Parse(configName, reader, version, settings, password);
                
                return parser.GetConfiguration();
            }
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
            ConfigurationSettings settings = new ConfigurationSettings();
            return Load(configName, configUri, configType, version, settings, password);
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
            Preconditions.CheckArgument(configName);
            Preconditions.CheckArgument(configFile);
            Preconditions.CheckArgument(version);

            LogUtils.Info(String.Format("Loading Configuration. [name={0}][version={1}][file={2}]", configName, version.ToString(), configFile));

            using(FileReader reader = new FileReader(configFile))
            {
                AbstractConfigParser parser = ConfigProviderFactory.GetParser(configFile);
                Postconditions.CheckCondition(parser);

                parser.Parse(configName, reader, version, settings, password);

                return parser.GetConfiguration();
            }
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
            ConfigurationSettings settings = new ConfigurationSettings();
            return Load(configName, configFile, version, settings, password);
        }
    }
}
