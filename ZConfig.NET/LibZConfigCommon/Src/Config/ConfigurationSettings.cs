using System;
using System.Collections.Generic;
using System.IO;
using LibZConfig.Common.Utils;

namespace LibZConfig.Common.Config
{
    /// <summary>
    /// Enum to specify startup options.
    /// </summary>
    public enum EDownloadOptions
    {
        /// <summary>
        /// Load Remote resources on startup.
        /// </summary>
        LoadRemoteResourcesOnStartup,
        /// <summary>
        /// Load Remote resources on demand.
        /// </summary>
        LoadRemoteResourcesOnDemand
    }

    /// <summary>
    /// Enum to specify shutdown options.
    /// </summary>
    public enum EShutdownOptions
    {
        /// <summary>
        /// Clear all downloaded data on shutdown
        /// </summary>
        ClearDataOnShutdown,
        /// <summary>
        /// Keep downloaded data and reuse
        /// </summary>
        ReuseData
    }

    /// <summary>
    /// Configuration settings to initialize the configuration instance.
    /// </summary>
    public class ConfigurationSettings
    {
        /// <summary>
        /// Wildcard search tag.
        /// </summary>
        public static readonly char NODE_SEARCH_WILDCARD = '*';

        private static readonly string DEFAULT_PROPERTIES_NODE_NAME = "properties";
        private static readonly string DEFAULT_ATTRIBUTES_NODE_NAME = "@";
        private static readonly string DEFAULT_PARAMETERS_NODE_NAME = "parameters";

        /// <summary>
        /// Node name for properties nodes.
        /// </summary>
        public string PropertiesNodeName { get; set; }
        /// <summary>
        /// Node name for attributes nodes.
        /// </summary>
        public string AttributesNodeName { get; set; }
        /// <summary>
        /// Node name for parameters nodes.
        /// </summary>
        public string ParametersNodeName { get; set; }
        /// <summary>
        /// Temporary folder to be used by ZConfig.
        /// </summary>
        public string TemporaryFolder { get; set; }
        /// <summary>
        /// Default download options.
        /// </summary>
        public EDownloadOptions DownloadOptions { get; set; }
        /// <summary>
        /// Default shutdown options.
        /// </summary>
        public EShutdownOptions ShutdownOptions { get; set; }

        /// <summary>
        /// Default empty constructor: Setup the defaults.
        /// </summary>
        public ConfigurationSettings()
        {
            PropertiesNodeName = DEFAULT_PROPERTIES_NODE_NAME;
            AttributesNodeName = DEFAULT_ATTRIBUTES_NODE_NAME;
            ParametersNodeName = DEFAULT_PARAMETERS_NODE_NAME;

            TemporaryFolder = FileUtils.GetTempDirectory("ZConfig");
            DownloadOptions = EDownloadOptions.LoadRemoteResourcesOnDemand;
            ShutdownOptions = EShutdownOptions.ReuseData;
        }

        /// <summary>
        /// Return an path to the temporary directory with the sub-directory is specified.
        /// Will create the path if required.
        /// </summary>
        /// <param name="subdir">Sub-directory path</param>
        /// <returns>Temporary directory path</returns>
        public string GetTempDirectory(string subdir)
        {
            string dir = TemporaryFolder;
            if (!String.IsNullOrWhiteSpace(subdir))
            {
                dir = String.Format("{0}{1}{2}", dir, Path.PathSeparator, subdir);
            }
            DirectoryInfo di = new DirectoryInfo(dir);
            if (!di.Exists)
            {
                LogUtils.Debug(String.Format("Creating temporary folder: [path={0}]", di.FullName));
                di.Create();
            } 
            return di.FullName;
        }
    }
}
