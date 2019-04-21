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
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace LibZConfig.Common.Config.Nodes
{
    /// <summary>
    /// Enum for defining Resource Types.
    /// </summary>
    public enum EResourceType
    {
        /// <summary>
        /// No Type specified.
        /// </summary>
        NONE,
        /// <summary>
        /// Basic File Resource
        /// </summary>
        FILE,
        /// <summary>
        /// Directory Resource
        /// </summary>
        DIRECTORY,
        /// <summary>
        /// Zip File Resource
        /// </summary>
        ZIP
    }

    /// <summary>
    /// Abstract base class for defining Resource Nodes.
    /// </summary>
    public abstract class ConfigResourceNode : ConfigElementNode
    {
        /// <summary>
        /// Type of the resource node.
        /// </summary>
        public EResourceType Type { get; set; }
        /// <summary>
        /// URI Location of the remote/local resource
        /// </summary>
        public Uri Location { get; set; }
        /// <summary>
        /// Resource name.
        /// </summary>
        public string ResourceName { get; set; }
        /// <summary>
        /// Has the resource been downloaded?
        /// </summary>
        public bool Downloaded { get; set; }

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        protected ConfigResourceNode() : base()
        {
            Type = EResourceType.NONE;
            Downloaded = false;
        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        protected ConfigResourceNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {
            Type = EResourceType.NONE;
            Downloaded = false;
        }

        /// <summary>
        /// Method to be invoked post configuration load.
        /// </summary>
        public override void PostLoad()
        {
            UpdateState(ENodeState.Synced);
        }

        /// <summary>
        /// Update the configuration for this node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        public override void UpdateConfiguration(Configuration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Method to recursively update the state of the nodes.
        /// </summary>
        /// <param name="state">Updated state</param>
        public override void UpdateState(ENodeState state)
        {
            State.State = state;
        }

        /// <summary>
        /// Method to validate the node instance.
        /// </summary>
        public override void Validate()
        {
            base.Validate();
            if (String.IsNullOrWhiteSpace(ResourceName))
            {
                throw ConfigurationException.PropertyMissingException(nameof(ResourceName));
            }
            if (Location == null)
            {
                throw ConfigurationException.PropertyMissingException(nameof(Location));
            }
            if (Type == EResourceType.NONE)
            {
                throw ConfigurationException.PropertyMissingException(nameof(Type));
            }
        }

        /// <summary>
        /// Abstract method to be implemented to enable searching.
        /// </summary>
        /// <param name="path">List of tokenized path elements.</param>
        /// <param name="index">Current Index in the List</param>
        /// <returns>Configuration Node</returns>
        public override AbstractConfigNode Find(List<string> path, int index)
        {
            AbstractConfigNode sn = CheckParentSearch(path, index);
            if (sn != null)
            {
                return sn;
            }
            string name = path[index];
            if (name == ResourceName && index == (path.Count - 1))
            {
                return this;
            }
            return null;
        }
    }

    /// <summary>
    /// Basic File resource handle.
    /// </summary>
    public class ConfigResourceFile : ConfigResourceNode
    {
        /// <summary>
        /// Local file-handle or the downloaded remote file.
        /// </summary>
        public FileInfo File { get; set; }

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigResourceFile() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigResourceFile(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }
    }

    /// <summary>
    /// Directory Resource handle
    /// </summary>
    public class ConfigDirectoryResource : ConfigResourceNode
    {
        /// <summary>
        /// Local directory-handle or the downloaded remote directory.
        /// </summary>
        public DirectoryInfo Directory { get; set; }

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigDirectoryResource() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigDirectoryResource(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }
    }

    /// <summary>
    /// BLOB Resource Handle
    /// </summary>
    public class ConfigResourceZip : ConfigResourceFile
    {
        public const string CONST_ZIP_FOLDER_NAME = "Archives";

        /// <summary>
        /// Local where the content is unzipped.
        /// </summary>
        public DirectoryInfo Directory { get; set; }

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigResourceZip() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigResourceZip(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }
    }
}
