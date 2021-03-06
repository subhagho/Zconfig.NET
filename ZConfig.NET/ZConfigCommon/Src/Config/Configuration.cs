﻿#region copyright
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
using System.IO;
using System.Diagnostics.Contracts;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config.Nodes;

namespace LibZConfig.Common.Config
{
    /// <summary>
    /// Exception class to be used to propogate configuration errors.
    /// </summary>
    public class ConfigurationException : Exception
    {
        private static readonly string __PREFIX = "Configuration Error : {0}";

        /// <summary>
        /// Constructor with error message.
        /// </summary>
        /// <param name="mesg">Error message</param>
        public ConfigurationException(string mesg) : base(String.Format(__PREFIX, mesg))
        {

        }

        /// <summary>
        /// Constructor with error message and cause.
        /// </summary>
        /// <param name="mesg">Error message</param>
        /// <param name="cause">Cause</param>
        public ConfigurationException(string mesg, Exception cause) : base(String.Format(__PREFIX, mesg), cause)
        {

        }

        /// <summary>
        /// Constructor with cause.
        /// </summary>
        /// <param name="exception">Cause</param>
        public ConfigurationException(Exception exception) : base(String.Format(__PREFIX, exception.Message), exception)
        {

        }

        /// <summary>
        /// Create a property missing exception.
        /// </summary>
        /// <param name="property">Property name</param>
        /// <returns>Configuration Exception</returns>
        public static ConfigurationException PropertyMissingException(string property)
        {
            return new ConfigurationException(String.Format("Required property missing : [name={0}]", property));
        }
    }

    /// <summary>
    /// Configuration Sync Mode settings.
    /// </summary>
    public enum ESyncMode
    {
        /// <summary>
        /// Configuration Sync is Manual
        /// </summary>
        MANUAL,
        /// <summary>
        /// Configuration Sync is Automatic in batches
        /// </summary>
        BATCH,
        /// <summary>
        /// Configuration Sync is Automatic in events.
        /// </summary>
        EVENTS
    }

    /// <summary>
    /// Class to set modification information.
    /// </summary>
    public class ModifiedBy
    {
        /// <summary>
        /// Modified By User
        /// </summary>
        public string User { get; set; }
        /// <summary>
        /// Modification timestamp
        /// </summary>
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Configuration Header definition.
    /// </summary>
    public class ConfigurationHeader
    {
        /// <summary>
        /// Unique Configuration ID.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Application Group this configuration belong to.
        /// </summary>
        public string ApplicationGroup { get; set; }
        /// <summary>
        /// Application this configuration belong to.
        /// </summary>
        public string Application { get; set; }
        /// <summary>
        /// Configuration name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Configuration description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Configuration version.
        /// </summary>
        public Version Version { get; set; }
        /// <summary>
        /// Configuration created by
        /// </summary>
        public ModifiedBy CreatedBy { get; set; }
        /// <summary>
        /// Confgiuration last updated by
        /// </summary>
        public ModifiedBy ModifiedBy { get; set; }
        /// <summary>
        /// Hashed Encryption Key
        /// </summary>
        public string EncryptionHash { get; set; }
        /// <summary>
        /// Configuration Load Timestamp.
        /// </summary>
        public long Timestamp { get; set; }
    }

    /// <summary>
    /// Class defines a configuration instance.
    /// </summary>
    public class Configuration : IDisposable
    {
        /// <summary>
        /// Configuration Settings used to load this instance.
        /// </summary>
        public ConfigurationSettings Settings { get; set; }
        /// <summary>
        /// Configuration Header information.
        /// </summary>
        public ConfigurationHeader Header { get; set; }
        /// <summary>
        /// Parsed configuration root node.
        /// </summary>
        public ConfigPathNode RootConfigNode { get; set; }
        /// <summary>
        /// Configuration Sync setting
        /// </summary>
        public ESyncMode SyncMode { get; set; }
        private string instanceId;
        private NodeState configState = new NodeState();

        /// <summary>
        /// Default Empty Constructor
        /// </summary>
        public Configuration()
        {
            instanceId = Guid.NewGuid().ToString();
            configState.State = ENodeState.Loading;
            Settings = new ConfigurationSettings();
            SyncMode = ESyncMode.MANUAL;
        }

        /// <summary>
        /// Constructor with defined configuration settings.
        /// </summary>
        /// <param name="settings">Configuration Settings</param>
        public Configuration(ConfigurationSettings settings)
        {
            instanceId = Guid.NewGuid().ToString();
            configState.State = ENodeState.Loading;
            if (settings == null)
            {
                Settings = new ConfigurationSettings();
            } 
            else
            {
                Settings = settings;
            }
            SyncMode = ESyncMode.MANUAL;
        }
        
        /// <summary>
        /// Get the local instance ID of this configuration handle.
        /// </summary>
        /// <returns></returns>
        public string GetInstanceId()
        {
            return instanceId;
        }
        
        /// <summary>
        /// Get the current state of this configuration.
        /// </summary>
        /// <returns>Current state.</returns>
        public ENodeState GetState()
        {
            return configState.State;
        }

        /// <summary>
        /// Set the configuration to error state with the specified exception.
        /// </summary>
        /// <param name="ex">Exception</param>
        public void SetError(Exception ex)
        {
            configState.SetError(ENodeState.Error, ex);
        }

        /// <summary>
        /// Perform Post-Load operations.
        /// </summary>
        public void PostLoad()
        {
            if (configState.HasError())
            {
                throw new ConfigurationException("Configuration in error state.",
                                                 configState.GetError());
            }
            if (RootConfigNode != null)
            {
                RootConfigNode.PostLoad();
            }
        }

        /// <summary>
        /// Validate this configuration instance.
        /// </summary>
        public void Validate()
        {
            if (Header == null)
            {
                throw ConfigurationException.PropertyMissingException(nameof(Header));
            }
            if (String.IsNullOrWhiteSpace(Header.Id))
            {
                throw ConfigurationException.PropertyMissingException(nameof(Header.Id));
            }
            if (String.IsNullOrWhiteSpace(Header.ApplicationGroup))
            {
                throw ConfigurationException.PropertyMissingException(nameof(Header.ApplicationGroup));
            }
            if (String.IsNullOrWhiteSpace(Header.Application))
            {
                throw ConfigurationException.PropertyMissingException(nameof(Header.Application));
            }
            if (Header.Version == null)
            {
                throw ConfigurationException.PropertyMissingException(nameof(Header.Version));
            }
            if (RootConfigNode == null)
            {
                throw ConfigurationException.PropertyMissingException(nameof(RootConfigNode));
            }
            RootConfigNode.Validate();
        }

        /// <summary>
        /// Find a specified node in this configuration based on the search Path.
        /// </summary>
        /// <param name="path">Search Path</param>
        /// <returns>Configuration Node</returns>
        public AbstractConfigNode Find(string path)
        {
            Preconditions.CheckArgument(path);

            if (path.StartsWith(ConfigurationSettings.NODE_SEARCH_SEPERATOR))
            {
                path = path.Substring(1);
            }
            return RootConfigNode.Find(path);
        }

        /// <summary>
        /// Find a specified node relative to the specified node based on the search Path.
        /// </summary>
        /// <param name="node">Config node to search under</param>
        /// <param name="path">Search path</param>
        /// <returns>Configuratio Node</returns>
        public AbstractConfigNode Find(AbstractConfigNode node, string path)
        {
            Preconditions.CheckArgument(node);
            Preconditions.CheckArgument(path);
            
            return node.Find(path);
        }

        /// <summary>
        /// Get the temporary directory path to store instance data. Sub-Directory will be appended
        /// if specified.
        /// </summary>
        /// <param name="subdir">Relative Sub-Directory</param>
        /// <returns>Temporary Folder path</returns>
        public string GetInstancePath(string subdir)
        {
            Postconditions.CheckCondition(Settings);
            Postconditions.CheckCondition(Header);
            
            lock(this)
            {
                string dir = String.Format("{0}{1}{2}{1}{3}{1}{4}", Header.ApplicationGroup, Path.PathSeparator, Header.Application, Header.Name, Header.Version.ToString());
                if (!String.IsNullOrWhiteSpace(subdir))
                {
                    dir = String.Format("{0}{1}{2}", dir, Path.PathSeparator, subdir);
                }
                return Settings.GetTempDirectory(dir);
            }
        }

        /// <summary>
        /// Dispose this configuration instance.
        /// </summary>
        public void Dispose()
        {
            if (Settings.ShutdownOptions == EShutdownOptions.ClearDataOnShutdown)
            {
                FileUtils.DeleteTempDirectory(Settings.GetTempDirectory());
            }
        }
    }
}
