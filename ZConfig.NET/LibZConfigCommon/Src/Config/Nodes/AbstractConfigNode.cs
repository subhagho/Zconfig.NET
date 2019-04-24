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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LibZConfig.Common.Config.Nodes
{
    /// <summary>
    /// Exception class to be used to propogate configuration search errors.
    /// </summary>
    public class ConfigNotFound : Exception
    {
        private static readonly string __PREFIX = "Node Not Found : {0}";

        /// <summary>
        /// Constructor with error message.
        /// </summary>
        /// <param name="mesg">Error message</param>
        public ConfigNotFound(string mesg) : base(String.Format(__PREFIX, mesg))
        {

        }

        /// <summary>
        /// Constructor with error message and cause.
        /// </summary>
        /// <param name="mesg">Error message</param>
        /// <param name="cause">Cause</param>
        public ConfigNotFound(string mesg, Exception cause) : base(String.Format(__PREFIX, mesg), cause)
        {

        }

        /// <summary>
        /// Constructor with cause.
        /// </summary>
        /// <param name="exception">Cause</param>
        public ConfigNotFound(Exception exception) : base(String.Format(__PREFIX, exception.Message), exception)
        {

        }
    }

    /// <summary>
    /// Enum to define the state of a configuration node.
    /// </summary>
    public enum ENodeState
    {
        /// <summary>
        /// Configuration is loading.
        /// </summary>
        Loading,
        /// <summary>
        /// Node is in synchronized state (with the source)
        /// </summary>
        Synced,
        /// <summary>
        /// Node has been newly created.
        /// </summary>
        New,
        /// <summary>
        /// Local node instance has been updated.
        /// </summary>
        Updated,
        /// <summary>
        /// Local node instance has been deleted.
        /// </summary>
        Deleted,
        /// <summary>
        /// Node is in error state.
        /// </summary>
        Error
    }

    /// <summary>
    /// Class encapsulates the state of a configuration node.
    /// </summary>
    public class NodeState : AbstractState<ENodeState>
    {
        /// <summary>
        /// Check if the Node is in Synced mode.
        /// </summary>
        /// <returns>Is Synced?</returns>
        public bool IsSynced()
        {
            return State.Equals(ENodeState.Synced);
        }

        /// <summary>
        /// Check if the Node is Deleted.
        /// </summary>
        /// <returns>Is Deleted?</returns>
        public bool IsDeleted()
        {
            return State.Equals(ENodeState.Deleted);
        }

        /// <summary>
        /// Check if the Node is Updated.
        /// </summary>
        /// <returns></returns>
        public bool IsUpdated()
        {
            return State.Equals(ENodeState.Updated);
        }

        /// <summary>
        /// Abstract method to be implemented for specifying the error state.
        /// </summary>
        /// <returns>Error state enum</returns>
        public override ENodeState[] GetErrorStates()
        {
            return new ENodeState[] { ENodeState.Error };
        }

        /// <summary>
        /// Get the default error state for this type.
        /// </summary>
        /// <returns>Error State</returns>
        public override ENodeState GetDefaultErrorState()
        {
            return ENodeState.Error;
        }
    }

    /// <summary>
    /// Abstract base class for defining configuration nodes.
    /// </summary>
    public abstract class AbstractConfigNode
    {
        /// <summary>
        /// Configuration instance this node belong to.
        /// </summary>
        [JsonIgnore]
        public Configuration Configuration { get; set; }
        /// <summary>
        /// Parent node of this instance.
        /// </summary>
        [JsonIgnore]
        public AbstractConfigNode Parent { get; set; }
        /// <summary>
        /// Node name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Node instance state.
        /// </summary>
        public NodeState State { get; set; }

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        protected AbstractConfigNode()
        {
            State = new NodeState();
            State.State = ENodeState.Loading;
        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        protected AbstractConfigNode(Configuration configuration, AbstractConfigNode parent)
        {
            Contract.Requires(configuration != null);

            this.Configuration = configuration;
            this.Parent = parent;

            State = new NodeState();
            State.State = ENodeState.Loading;
        }

        /// <summary>
        /// Get the absolute path of this node instance.
        /// </summary>
        /// <returns>Absolute path</returns>
        public string GetAbsolutePath()
        {
            string path = null;
            if (Parent != null)
            {
                path = Parent.GetAbsolutePath();
                path = string.Format("{0}/{1}", path, Name);
            }
            else
            {
                path = string.Format("/{0}", Name);
            }
            return path;
        }

        /// <summary>
        /// Get the Search path for this instance. Search path represent the dot seperated 
        /// path that can be used to reach this node using the Find() method.
        /// </summary>
        /// <returns>Search path</returns>
        public string GetSearchPath()
        {
            string path = null;
            if (Parent != null)
            {
                path = Parent.GetSearchPath();
                path = string.Format("{0}/{1}", path, Name);
            }
            else
            {
                path = Name;
            }
            return path;
        }

        /// <summary>
        /// Find the node relative to this node instance for
        /// the specified search path.
        /// </summary>
        /// <param name="path">Search path relative to this node.</param>
        /// <returns>Config node instance</returns>
        public virtual AbstractConfigNode Find(string path)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(path));

            path = ConfigUtils.CheckSearchPath(path, this);
            if (path == ".")
            {
                return this;
            }
            else if (path.StartsWith(ConfigurationSettings.NODE_SEARCH_SEPERATOR))
            {
                return Configuration.Find(path);
            }
            path = ConfigUtils.MaskSearchPath(path);
            string[] parts = path.Split(ConfigurationSettings.NODE_SEARCH_SEPERATOR);
            if (parts != null && parts.Length > 0)
            {
                List<string> pList = new List<string>();
                foreach (string part in parts)
                {
                    if (String.IsNullOrWhiteSpace(part))
                    {
                        continue;
                    }
                    string npart = ConfigUtils.UnmaskSearchPath(part);
                    pList.Add(npart);
                }
                ConfigUtils.CheckSearchRoot(pList, Name, Configuration.Settings);
                return Find(pList, 0);
            }
            return null;
        }

        /// <summary>
        /// Mark this node has been locally updated.
        /// </summary>
        public void Updated()
        {
            if (!State.IsSynced())
            {
                State.State = ENodeState.Updated;
            }
        }

        /// <summary>
        /// Mark this node has been locally deleted.
        /// </summary>
        /// <returns>Is Deleted?</returns>
        public bool Deleted()
        {
            if (State.IsSynced() || State.IsUpdated())
            {
                State.State = ENodeState.Deleted;
            }
            return State.IsDeleted();
        }

        /// <summary>
        /// Mark this node is synced with the source.
        /// </summary>
        public void Synced()
        {
            State.State = ENodeState.Synced;
        }

        /// <summary>
        /// Update the configuration for this node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        public virtual void UpdateConfiguration(Configuration configuration)
        {
            Contract.Requires(configuration != null);
            this.Configuration = configuration;
        }

        /// <summary>
        /// Abstract method to be implemented to enable searching.
        /// </summary>
        /// <param name="path">List of tokenized path elements.</param>
        /// <param name="index">Current Index in the List</param>
        /// <returns>Configuration Node</returns>
        public abstract AbstractConfigNode Find(List<string> path, int index);

        /// <summary>
        /// Method to be invoked post configuration load.
        /// </summary>
        public abstract void PostLoad();

        /// <summary>
        /// Method to validate the node instance.
        /// </summary>
        public virtual void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw ConfigurationException.PropertyMissingException(nameof(Name));
            }
        }

        /// <summary>
        /// Method to recursively update the state of the nodes.
        /// </summary>
        /// <param name="state">Updated state</param>
        public abstract void UpdateState(ENodeState state);

        protected AbstractConfigNode CheckParentSearch(List<string> path, int index)
        {
            string name = path[index];
            if (name == ConfigurationSettings.NODE_SEARCH_PARENT)
            {
                if (Parent != null)
                {
                    path[index] = Parent.Name;
                    return Parent.Find(path, index);
                }
            }
            return null;
        }
    }
}