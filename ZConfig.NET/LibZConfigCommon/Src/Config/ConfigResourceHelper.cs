using System;
using System.Diagnostics.Contracts;
using System.IO;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config.Nodes;
using LibZConfig.Common.Config.Readers;

namespace LibZConfig.Common.Config
{
    /// <summary>
    /// Helper class to get resources specified in the configuration.
    /// </summary>
    public static class ConfigResourceHelper
    {
        /// <summary>
        /// Get a File/Blob resource as a stream.
        /// </summary>
        /// <param name="configuration">Configuration instance.</param>
        /// <param name="path">Search path for the resource node.</param>
        /// <returns>Stream Reader</returns>
        public static StreamReader GetResourceStream(Configuration configuration, string path)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(!String.IsNullOrWhiteSpace(path));

            AbstractConfigNode node = configuration.Find(path);
            if (node != null && typeof(ConfigResourceNode).IsAssignableFrom(node.GetType()))
            {
                ConfigResourceNode rnode = (ConfigResourceNode)node;
                if (rnode.GetType() == typeof(ConfigResourceFile) || rnode.GetType() == typeof(ConfigResourceBlob))
                {
                    ConfigResourceFile fnode = (ConfigResourceFile)rnode;
                    if (fnode.Downloaded)
                    {
                        if (fnode.File == null)
                        {
                            throw new ConfigurationException(String.Format("Invalid File Resource Node: File is NULL. [path={0}]", fnode.GetSearchPath()));
                        }
                        FileInfo fi = new FileInfo(fnode.File.FullName);
                        if (!fi.Exists)
                        {
                            throw new ConfigurationException(String.Format("Invalid File Resource Node: File not found. [file={0}]", fi.FullName));
                        }
                        return new StreamReader(fi.FullName);
                    }
                    else
                    {
                        lock(fnode)
                        {
                            using (AbstractReader reader = ReaderTypeHelper.GetReader(fnode.Location))
                            {
                                if (reader == null)
                                {
                                    throw new ConfigurationException(String.Format("No reader found for URI. [uri={0}]", fnode.Location.ToString()));
                                }
                                reader.Open();
                                string f = FileUtils.WriteLocalFile(reader.GetStream(), fnode.ResourceName, fnode.Configuration.Settings.GetTempDirectory());
                                FileInfo fi = new FileInfo(f);
                                if (!fi.Exists)
                                {
                                    throw new ConfigurationException(String.Format("Erorr downloading file: File not created. [file={0}]", fi.FullName));
                                }
                                fnode.File = fi;
                                fnode.Downloaded = true;
                            }
                        }
                        FileInfo file = new FileInfo(fnode.File.FullName);
                        if (!file.Exists)
                        {
                            throw new ConfigurationException(String.Format("Invalid File Resource Node: File not found. [file={0}]", file.FullName));
                        }
                        return new StreamReader(file.FullName);
                    }
                }

            }
            return null;
        }
    }
}
