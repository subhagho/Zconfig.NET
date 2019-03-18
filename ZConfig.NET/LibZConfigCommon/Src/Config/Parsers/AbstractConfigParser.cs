using System;
using System.Collections.Generic;
using System.Text;
using LibZConfig.Common.Config;
using LibZConfig.Common.Config.Readers;

namespace LibZConfig.Common.Config.Parsers
{
    /// <summary>
    /// Abstract base class to define configuration parsers.
    /// </summary>
    public abstract class AbstractConfigParser
    {
        /// <summary>
        /// Configuration Settings.
        /// </summary>
        protected ConfigurationSettings settings;
        /// <summary>
        /// Parsed configuration instance.
        /// </summary>
        protected Configuration configuration;

        /// <summary>
        /// Get the configuration settings used to parse.
        /// </summary>
        /// <returns>Configuration Settings.</returns>
        public ConfigurationSettings GetConfigurationSettings()
        {
            return settings;
        }

        /// <summary>
        /// Get the Parsed configuration instance.
        /// </summary>
        /// <returns></returns>
        public Configuration GetConfiguration()
        {
            return configuration;
        }

        /// <summary>
        /// Parse the configuration loaded by the reader handle.
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <param name="reader">Reader handle to read the configuration</param>
        /// <param name="version">Expected Version of the configuration</param>
        /// <param name="settings">Configuration Settings to use</param>
        public abstract void Parse(string name, AbstractReader reader, Version version, ConfigurationSettings settings);
    }
}
