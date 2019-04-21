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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;

namespace LibZConfig.Common.Config.Nodes
{
    /// <summary>
    /// Node is a List of Configuration elements.
    /// </summary>
    /// <typeparam name="T">Configuration Element type.</typeparam>
    public abstract class ConfigListNode<T> : ConfigElementNode where T : AbstractConfigNode
    {
        /// <summary>
        /// Node abbreviation for search.
        /// </summary>
        public const char NODE_ABBREVIATION = '%';
        /// <summary>
        /// Dummy node list name.
        /// </summary>
        public const string NODE_NAME = "LIST_NODE";

        private List<T> values = new List<T>();

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        protected ConfigListNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        protected ConfigListNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }

        /// <summary>
        /// Get the list of values.
        /// </summary>
        /// <returns>List of Values</returns>
        public List<T> GetValues()
        {
            return values;
        }

        /// <summary>
        /// Set the List of values.
        /// </summary>
        /// <param name="values">List of Values</param>
        public void SetValues(List<T> values)
        {
            this.values = values;
        }

        /// <summary>
        /// Add a configuration node to this list.
        /// </summary>
        /// <param name="value">Configuation node</param>
        public void Add(T value)
        {
            Contract.Requires(value != null);
            values.Add(value);
        }

        /// <summary>
        /// Add all the elements in the passed list.
        /// </summary>
        /// <param name="values">List of Elements</param>
        public void AddAll(List<T> values)
        {
            Contract.Requires(values != null && values.Count > 0);
            foreach (T value in values)
            {
                this.values.Add(value);
            }
        }

        /// <summary>
        /// Get the configuration node at the specified index.
        /// </summary>
        /// <param name="index">List index</param>
        /// <returns>Configuration node</returns>
        public T GetValue(int index)
        {
            if (index >= 0 && index < values.Count)
            {
                return values[index];
            }
            return null;
        }

        /// <summary>
        /// Remove the specified configuration node from the list.
        /// </summary>
        /// <param name="value">Configuration node</param>
        /// <returns>Is Removed?</returns>
        public bool Remove(T value)
        {
            if (value != null)
            {
                return values.Remove(value);
            }
            return false;
        }

        /// <summary>
        /// Check if the list is empty.
        /// </summary>
        /// <returns>Is Empty?</returns>
        public bool IsEmpty()
        {
            return (values.Count == 0);
        }

        /// <summary>
        /// Get the count of elements in the list.
        /// </summary>
        /// <returns>Count of elements</returns>
        public int Count()
        {
            return values.Count;
        }

        /// <summary>
        /// Method to be invoked post configuration load.
        /// </summary>
        public override void PostLoad()
        {
            if (State.HasError())
            {
                throw new ConfigurationException(State.GetError());
            }
            UpdateState(ENodeState.Synced);
        }

        /// <summary>
        /// Method to recursively update the state of the nodes.
        /// </summary>
        /// <param name="state">Updated state</param>
        public override void UpdateState(ENodeState state)
        {
            State.State = state;
            foreach (T value in values)
            {
                value.UpdateState(state);
            }
        }

        /// <summary>
        /// Update the configuration for this node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        public override void UpdateConfiguration(Configuration configuration)
        {
            Configuration = configuration;
            foreach (T value in values)
            {
                value.UpdateConfiguration(configuration);
            }
        }

        /// <summary>
        /// Method to validate the node instance.
        /// </summary>
        public override void Validate()
        {
            base.Validate();
            if (IsEmpty())
            {
                throw ConfigurationException.PropertyMissingException("Values");
            }
            foreach (T value in values)
            {
                value.Validate();
            }
        }
    }

    /// <summary>
    /// Configuration List node of Configuration Values.
    /// </summary>
    public class ConfigListValueNode : ConfigListNode<ConfigValueNode>
    {

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigListValueNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigListValueNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }

        /// <summary>
        /// Get the Config Values as a List of strings.
        /// </summary>
        /// <returns>Values as String List</returns>
        public List<string> GetValueList()
        {
            List<ConfigValueNode> values = GetValues();
            if (values != null && values.Count > 0)
            {
                List<string> vs = new List<string>(values.Count);
                foreach(ConfigValueNode cv in values)
                {
                    vs.Add(cv.GetValue());
                }
                return vs;
            }
            return null;
        }

        /// <summary>
        /// Abstract method to be implemented to enable searching.
        /// </summary>
        /// <param name="path">List of tokenized path elements.</param>
        /// <param name="index">Current Index in the List</param>
        /// <returns>Configuration Node</returns>
        public override AbstractConfigNode Find(List<string> path, int index)
        {
            AbstractConfigNode psn = CheckParentSearch(path, index);
            if (psn != null)
            {
                return psn;
            }
            string name = path[index];
            if (name.Length == 1 && name[0] == ConfigurationSettings.NODE_SEARCH_WILDCARD)
            {
                if (GetValues().Count > 0)
                {
                    List<AbstractConfigNode> nodes = new List<AbstractConfigNode>();
                    foreach (ConfigValueNode value in GetValues())
                    {
                        AbstractConfigNode sn = value.Find(path, index + 1);
                        if (sn != null)
                        {
                            nodes.Add(sn);
                        }
                    }
                    if (nodes.Count == 1)
                    {
                        return nodes[0];
                    }
                    else if (nodes.Count > 1)
                    {
                        ConfigSearchResult result = new ConfigSearchResult();
                        result.Configuration = Configuration;
                        result.AddAll(nodes);
                        return result;
                    }
                }
                return null;
            }
            ResolvedName resolved = ConfigUtils.ResolveName(name, Name, Configuration.Settings);
            if (resolved == null)
            {
                if (name == Name && index == (path.Count - 1))
                {
                    return this;
                }
            }
            else
            {
                if (resolved.AbbrReplacement == NODE_NAME)
                {
                    if (resolved.Name == Name && index == (path.Count - 1))
                    {
                        int indx = Int32.Parse(resolved.ChildName);
                        return GetValue(indx);
                    }
                }
            }
            return null;
        }
    }

    /// <summary>
    /// Configuration List node of Element Nodes.
    /// </summary>
    public class ConfigElementListNode : ConfigListNode<ConfigElementNode>
    {
        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigElementListNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigElementListNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }

        /// <summary>
        /// Abstract method to be implemented to enable searching.
        /// </summary>
        /// <param name="path">List of tokenized path elements.</param>
        /// <param name="index">Current Index in the List</param>
        /// <returns>Configuration Node</returns>
        public override AbstractConfigNode Find(List<string> path, int index)
        {
            string name = path[index];
            if (name.Length == 1 && name[0] == ConfigurationSettings.NODE_SEARCH_WILDCARD)
            {
                if (GetValues().Count > 0)
                {
                    List<AbstractConfigNode> nodes = new List<AbstractConfigNode>();
                    foreach (ConfigElementNode value in GetValues())
                    {
                        AbstractConfigNode sn = value.Find(path, index + 1);
                        if (sn != null)
                        {
                            nodes.Add(sn);
                        }
                    }
                    if (nodes.Count == 1)
                    {
                        return nodes[0];
                    }
                    else if (nodes.Count > 1)
                    {
                        ConfigSearchResult result = new ConfigSearchResult();
                        result.Configuration = Configuration;
                        result.AddAll(nodes);
                        return result;
                    }
                }
                return null;
            }
            ResolvedName resolved = ConfigUtils.ResolveName(name, Name, Configuration.Settings);
            if (resolved == null)
            {
                if (name == Name && index == (path.Count - 1))
                {
                    return this;
                } else
                {
                    string cname = path[index + 1];
                    if (cname == ConfigurationSettings.NODE_SEARCH_PARENT)
                    {
                        path[index + 1] = Parent.Name;
                        return Parent.Find(path, index + 1);
                    }
                }
            }
            else
            {
                if (resolved.AbbrReplacement == NODE_NAME)
                {
                    if (resolved.Name == Name)
                    {
                        int indx = Int32.Parse(resolved.ChildName);
                        if (index == (path.Count - 1))
                        {
                            return GetValue(indx);
                        }
                        else
                        {
                            ConfigElementNode node = GetValue(indx);
                            if (node != null)
                            {
                                path.Insert(index + 1, node.Name);
                                return node.Find(path, index + 1);
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
