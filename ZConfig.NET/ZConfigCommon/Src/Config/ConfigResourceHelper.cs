#region copyright
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2019
// Date: 2019-3-23
// Project: LibZConfigCommon
// Subho Ghosh (subho dot ghosh at outlook.com)
//
//
#endregion
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
        /// Get a File/Blob resource as a stream. Method is applicable only for File Resources.
        /// </summary>
        /// <param name="configuration">Configuration instance.</param>
        /// <param name="path">Search path for the resource node.</param>
        /// <returns>Stream Reader</returns>
        public static StreamReader GetResourceStream(this Configuration configuration, string path)
        {
            Preconditions.CheckArgument(configuration);
            Preconditions.CheckArgument(path);
            
            AbstractConfigNode node = configuration.Find(path);
            if (node != null && typeof(ConfigResourceNode).IsAssignableFrom(node.GetType()))
            {
                ConfigResourceNode rnode = (ConfigResourceNode)node;
                if (rnode.GetType() == typeof(ConfigResourceFile))
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
                        lock (fnode)
                        {
                            DownloadResource(fnode);
                            if (fnode.Type == EResourceType.ZIP)
                            {
                                UnzipResource((ConfigResourceZip)fnode);
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

        /// <summary>
        /// Get a File/Blob resource as a stream. Method is applicable only for Zip/Directory Resources.
        /// </summary>
        /// <param name="configuration">Configuration instance.</param>
        /// <param name="path">Search path for the resource node.</param>
        /// <param name="file">Sub-path for the file</param>
        /// <returns>Stream Reader</returns>
        public static StreamReader GetResourceStream(this Configuration configuration, string path, string file)
        {
            Preconditions.CheckArgument(configuration);
            Preconditions.CheckArgument(path);
            Preconditions.CheckArgument(file);
            
            AbstractConfigNode node = configuration.Find(path);
            if (node != null && typeof(ConfigResourceNode).IsAssignableFrom(node.GetType()))
            {
                ConfigResourceNode rnode = (ConfigResourceNode)node;
                if (rnode.GetType() == typeof(ConfigResourceZip))
                {
                    ConfigResourceZip fnode = (ConfigResourceZip)rnode;
                    if (fnode.Downloaded)
                    {
                        Conditions.NotNull(fnode.File);
                        FileInfo fi = new FileInfo(fnode.File.FullName);
                        if (!fi.Exists)
                        {
                            throw new ConfigurationException(String.Format("Invalid File Resource Node: File not found. [file={0}]", fi.FullName));
                        }
                        string filename = String.Format("{0}/{1}", fi.FullName, file);
                        fi = new FileInfo(filename);
                        if (!fi.Exists)
                        {
                            throw new ConfigurationException(String.Format("File not found. [file={0}]", fi.FullName));
                        }
                        return new StreamReader(fi.FullName);
                    }
                    else
                    {
                        lock (fnode)
                        {
                            DownloadResource(fnode);
                            UnzipResource(fnode);
                        }
                        string filename = String.Format("{0}/{1}", fnode.Directory.FullName, file);
                        FileInfo fi = new FileInfo(filename);
                     
                        if (!fi.Exists)
                        {
                            throw new ConfigurationException(String.Format("Invalid File Resource Node: File not found. [file={0}]", fi.FullName));
                        }
                        return new StreamReader(fi.FullName);
                    }
                }
                else if (rnode.GetType() == typeof(ConfigDirectoryResource))
                {
                    ConfigDirectoryResource dnode = (ConfigDirectoryResource)rnode;
                    if (!dnode.Directory.Exists)
                    {
                        throw new ConfigurationException(String.Format("Directory not found. [file={0}]", dnode.Directory.FullName));
                    }
                    string filename = String.Format("{0}/{1}", dnode.Directory.FullName, file);
                    FileInfo fi = new FileInfo(filename);

                    if (!fi.Exists)
                    {
                        throw new ConfigurationException(String.Format("Invalid File Resource Node: File not found. [file={0}]", fi.FullName));
                    }
                    return new StreamReader(fi.FullName);
                }
            }
            return null;
        }

        /// <summary>
        /// Unzip the Zip Resource.
        /// </summary>
        /// <param name="znode">Zip Resource Node</param>
        /// <returns>Unzipped Directory</returns>
        private static string UnzipResource(ConfigResourceZip znode)
        {
            FileInfo fi = znode.File;
            if (!fi.Exists)
            {
                throw new ConfigurationException(String.Format("Zip Resource not found: [file={0}]", fi.FullName));
            }
            string outputDir = znode.Configuration.Settings.GetTempDirectory(String.Format("{0}/{1}", ConfigResourceZip.CONST_ZIP_FOLDER_NAME, znode.ResourceName));
            outputDir = FileUtils.ExtractZipFile(fi.FullName, outputDir);
            DirectoryInfo di = new DirectoryInfo(outputDir);
            if (!di.Exists)
            {
                throw new ConfigurationException(String.Format("Error extracting Zip resource: [directory={0}]", di.FullName));
            }
            znode.Directory = di;

            return di.FullName;
        }

        /// <summary>
        /// Download a resource file specified in the configuration node passed.
        /// </summary>
        /// <param name="fnode">File Resource Node</param>
        /// <returns>Downloaded File path</returns>
        public static string DownloadResource(ConfigResourceFile fnode)
        {
            using (AbstractReader reader = ReaderTypeHelper.GetReader(fnode.Location))
            {
                Conditions.NotNull(reader);
                reader.Open();
                string f = FileUtils.WriteLocalFile(reader.GetStream(), fnode.ResourceName, fnode.Configuration.Settings.GetTempDirectory());
                FileInfo fi = new FileInfo(f);
                if (!fi.Exists)
                {
                    throw new ConfigurationException(String.Format("Erorr downloading file: File not created. [file={0}]", fi.FullName));
                }
                fnode.File = fi;
                fnode.Downloaded = true;

                return fi.FullName;
            }
        }
    }
}
