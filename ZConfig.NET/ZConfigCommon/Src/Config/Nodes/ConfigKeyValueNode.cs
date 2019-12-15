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
    /// Abstract node for Key/Node values.
    /// </summary>
    public abstract class ConfigKeyValueNode : ConfigElementNode
    {
        private Dictionary<string, ConfigValueNode> values = new Dictionary<string, ConfigValueNode>();

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        protected ConfigKeyValueNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        protected ConfigKeyValueNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }

        /// <summary>
        /// Get the Dictionary of Values.
        /// </summary>
        /// <returns>Dictionary of Values</returns>
        public Dictionary<string, ConfigValueNode> GetValues()
        {
            return values;
        }

        /// <summary>
        /// Set the Key/Values.
        /// </summary>
        /// <param name="values">Dictionary of Values</param>
        public void SetValues(Dictionary<string, ConfigValueNode> values)
        {
            this.values = values;
        }

        /// <summary>
        /// Get the value for the specified name.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Value if found.</returns>
        public ConfigValueNode GetValue(string name)
        {
            if (values.ContainsKey(name))
            {
                return values[name];
            }
            return null;
        }

        /// <summary>
        /// Check if a value exists for the specified name.
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Exists?</returns>
        public bool HasKey(string name)
        {
            return values.ContainsKey(name);
        }

        /// <summary>
        /// Is this node empty?
        /// </summary>
        /// <returns>Is Empty?</returns>
        public bool IsEmpty()
        {
            return (values.Count == 0);
        }

        /// <summary>
        /// Add all the key/values from the passed Dictionary
        /// </summary>
        /// <param name="values">Dictionary of Values.</param>
        public void AddAll(Dictionary<string, ConfigValueNode> values)
        {
            Preconditions.CheckArgument(values != null && values.Count > 0);
            foreach (string key in values.Keys)
            {
                this.values[key] = values[key];
            }
        }

        /// <summary>
        /// Add a value node to this map.
        /// </summary>
        /// <param name="node">Node to Add</param>
        public void Add(ConfigValueNode node)
        {
            Preconditions.CheckArgument(node);
            Preconditions.CheckArgument(node.Name);

            values[node.Name] = node;
            Updated();
        }

        /// <summary>
        /// Remove a value from this node.
        /// </summary>
        /// <param name="name">Name</param>
        public void Remove(string name)
        {
            if (values.ContainsKey(name))
            {
                values.Remove(name);
                Updated();
            }
        }

        /// <summary>
        /// Method to recursively update the state of the nodes.
        /// </summary>
        /// <param name="state">Updated state</param>
        public override void UpdateState(ENodeState state)
        {
            State.State = state;
            foreach (string key in values.Keys)
            {
                values[key].UpdateState(state);
            }
        }

        /// <summary>
        /// Update the configuration for this node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        public override void UpdateConfiguration(Configuration configuration)
        {
            Configuration = configuration;
            foreach (string key in values.Keys)
            {
                values[key].UpdateConfiguration(configuration);
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
                throw ConfigurationException.PropertyMissingException("Key/Values");
            }
            foreach (string key in values.Keys)
            {
                values[key].Validate();
            }
        }

        /// <summary>
        /// Find the node relative to this node instance for
        /// the specified search path.
        /// </summary>
        /// <param name="path">Search path relative to this node.</param>
        /// <returns>Config node instance</returns>
        public override AbstractConfigNode Find(string path)
        {
            return base.Find(path);
        }

        /// <summary>
        /// Find the node based on the node abbreviation.
        /// </summary>
        /// <param name="name">Name key</param>
        /// <param name="abbr">Node abbreviation</param>
        /// <returns>Child node</returns>
        protected AbstractConfigNode Find(string name, char abbr)
        {
            if (name.Length == 1 && name[0] == abbr)
            {
                return this;
            }
            else if (name.StartsWith(abbr))
            {
                name = name.Substring(1);
            }
            if (HasKey(name))
            {
                return GetValue(name);
            }
            return null;
        }

        /// <summary>
        /// Find the node based on the node abbreviation.
        /// </summary>
        /// <param name="path">List of Path elements</param>
        /// <param name="index">Current path index</param>
        /// <param name="abbr">Node abbreviation</param>
        /// <returns>Child node</returns>
        protected AbstractConfigNode Find(List<string> path, int index, char abbr)
        {
            AbstractConfigNode sn = CheckParentSearch(path, index);
            if (sn != null)
            {
                return sn;
            }

            string name = path[index];
            if (name.Length == 1 && name[0] == abbr)
            {
                return this;
            }
            else if (name.Contains(abbr))
            {
                string[] parts = name.Split(abbr);
                if (parts.Length == 2)
                {
                    if (String.IsNullOrWhiteSpace(parts[0]))
                    {
                        return GetValue(parts[1]);
                    }
                }
            }
            else
            {
                if (name == Name)
                {
                    return this;
                }
            }
            return null;
        }

        /// <summary>
        /// Override ToString()
        /// </summary>
        /// <returns>Name/Value</returns>
        public override string ToString()
        {
            return String.Format("[name={0} : values={{1}}]", Name, values.ToString());
        }
    }

    /// <summary>
    /// Configuration Properties Node - Node to define property key/values.
    /// The properties are available as variable replacement in scope.
    /// </summary>
    public class ConfigPropertiesNode : ConfigKeyValueNode
    {
        public const char NODE_ABBREVIATION = '$';

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigPropertiesNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigPropertiesNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {
            Name = configuration.Settings.PropertiesNodeName;
        }

        /// <summary>
        /// Find the node relative to this node instance for
        /// the specified search path.
        /// </summary>
        /// <param name="path">Search path relative to this node.</param>
        /// <returns>Config node instance</returns>
        public override AbstractConfigNode Find(string path)
        {
            return Find(path, NODE_ABBREVIATION);
        }

        /// <summary>
        /// Abstract method to be implemented to enable searching.
        /// </summary>
        /// <param name="path">List of tokenized path elements.</param>
        /// <param name="index">Current Index in the List</param>
        /// <returns>Configuration Node</returns>
        public override AbstractConfigNode Find(List<string> path, int index)
        {
            return Find(path, index, NODE_ABBREVIATION);
        }

        public override void PostLoad()
        {
            // Nothing to be done.
        }
    }

    /// <summary>
    /// Configuration Parameters Node - Node to be used to specify Parameters/Arguments for startup to 
    /// wired/autowired instances.
    /// </summary>
    public class ConfigParametersNode : ConfigKeyValueNode
    {
        public const char NODE_ABBREVIATION = '#';

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigParametersNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigParametersNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {
            Name = configuration.Settings.ParametersNodeName;
        }

        /// <summary>
        /// Find the node relative to this node instance for
        /// the specified search path.
        /// </summary>
        /// <param name="path">Search path relative to this node.</param>
        /// <returns>Config node instance</returns>
        public override AbstractConfigNode Find(string path)
        {
            return Find(path, NODE_ABBREVIATION);
        }

        /// <summary>
        /// Abstract method to be implemented to enable searching.
        /// </summary>
        /// <param name="path">List of tokenized path elements.</param>
        /// <param name="index">Current Index in the List</param>
        /// <returns>Configuration Node</returns>
        public override AbstractConfigNode Find(List<string> path, int index)
        {
            return Find(path, index, NODE_ABBREVIATION);
        }

        public override void PostLoad()
        {
            // Nothing to be done.
        }
    }

    /// <summary>
    /// Configuration Attributes Node - Useful in XML configurations as the the element
    /// attribues are stored in this class, not really useful for JSON configurations.
    /// </summary>
    public class ConfigAttributesNode : ConfigKeyValueNode
    {
        public const char NODE_ABBREVIATION = '@';

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigAttributesNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigAttributesNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {
            Name = configuration.Settings.AttributesNodeName;
        }

        /// <summary>
        /// Find the node relative to this node instance for
        /// the specified search path.
        /// </summary>
        /// <param name="path">Search path relative to this node.</param>
        /// <returns>Config node instance</returns>
        public override AbstractConfigNode Find(string path)
        {
            path = ConfigUtils.CheckSearchPath(path, this);
            if (path == ".")
            {
                return this;
            }
            else if (path.StartsWith(ConfigurationSettings.NODE_SEARCH_SEPERATOR))
            {
                return Configuration.Find(path);
            }
            return Find(path, NODE_ABBREVIATION);
        }

        /// <summary>
        /// Abstract method to be implemented to enable searching.
        /// </summary>
        /// <param name="path">List of tokenized path elements.</param>
        /// <param name="index">Current Index in the List</param>
        /// <returns>Configuration Node</returns>
        public override AbstractConfigNode Find(List<string> path, int index)
        {
            return Find(path, index, NODE_ABBREVIATION);
        }

        public override void PostLoad()
        {
            // Nothing to be done.
        }
    }
}
