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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;
using LibZConfig.Common.Config;

namespace LibZConfig.Common.Config.Nodes
{
    public class ConfigPathNode : ConfigElementNode
    {
        private Dictionary<string, AbstractConfigNode> children = new Dictionary<string, AbstractConfigNode>();

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigPathNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigPathNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }

        /// <summary>
        /// Get all the child nodes.
        /// </summary>
        /// <returns>Child Nodes</returns>
        public Dictionary<string, AbstractConfigNode> GetChildren()
        {
            return children;
        }

        /// <summary>
        /// Set the map of child nodes.
        /// </summary>
        /// <param name="children">Map of child nodes</param>
        public void SetChildren(Dictionary<string, AbstractConfigNode> children)
        {
            this.children = children;
        }

        /// <summary>
        /// Add a new child node.
        /// </summary>
        /// <param name="node">Child Conifg node</param>
        /// <returns>Self</returns>
        public ConfigPathNode AddChildNode(AbstractConfigNode node)
        {
            Preconditions.CheckArgument(node);

            node.Parent = this;
            if (!typeof(ConfigResourceNode).IsAssignableFrom(node.GetType()))
            {
                children[node.Name] = node;
            }
            else
            {
                ConfigResourceNode rnode = (ConfigResourceNode)node;
                children[rnode.ResourceName] = node;
            }
            Updated();

            return this;
        }

        /// <summary>
        /// Remove a child node by name.
        /// </summary>
        /// <param name="name">Node name</param>
        /// <returns>Self</returns>
        public ConfigPathNode RemoveChildNode(string name)
        {
            if (children.ContainsKey(name))
            {
                children.Remove(name);
                Updated();
            }
            return this;
        }

        /// <summary>
        /// Get a child node by name.
        /// </summary>
        /// <param name="name">Node name</param>
        /// <returns>Child node if exists</returns>
        public AbstractConfigNode GetChildNode(string name)
        {
            if (children.ContainsKey(name))
            {
                return children[name];
            }
            return null;
        }

        /// <summary>
        /// Get the defined properties for this node, if any.
        /// </summary>
        /// <returns>Config Properties</returns>
        public ConfigPropertiesNode GetProperties()
        {
            string name = Configuration.Settings.PropertiesNodeName;
            if (children.ContainsKey(name))
            {
                AbstractConfigNode node = children[name];
                if (node.GetType() == typeof(ConfigPropertiesNode))
                {
                    return (ConfigPropertiesNode)node;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the defined parameters for this node, if any.
        /// </summary>
        /// <returns>Config Parameters</returns>
        public ConfigParametersNode GetParameters()
        {
            string name = Configuration.Settings.ParametersNodeName;
            if (children.ContainsKey(name))
            {
                AbstractConfigNode node = children[name];
                if (node.GetType() == typeof(ConfigParametersNode))
                {
                    return (ConfigParametersNode)node;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the parameter for the specified name, if exists.
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <returns>Config Value</returns>
        public ConfigValueNode GetParameter(string name)
        {
            ConfigParametersNode node = GetParameters();
            if (node != null)
            {
                return node.GetValue(name);
            }
            return null;
        }

        /// <summary>
        /// Get the defined attributes for this node, if any.
        /// </summary>
        /// <returns>Config Attributes</returns>
        public ConfigAttributesNode GetAttributes()
        {
            string name = Configuration.Settings.AttributesNodeName;
            if (children.ContainsKey(name))
            {
                AbstractConfigNode node = children[name];
                if (node.GetType() == typeof(ConfigAttributesNode))
                {
                    return (ConfigAttributesNode)node;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the attribute for the specified name, if exists.
        /// </summary>
        /// <param name="name">Attribute name</param>
        /// <returns>Config Value</returns>
        public ConfigValueNode GetAttribute(string name)
        {
            ConfigAttributesNode node = GetAttributes();
            if (node != null)
            {
                return node.GetValue(name);
            }
            return null;
        }

        /// <summary>
        /// Check if this node has any children.
        /// </summary>
        /// <returns>Is Empty?</returns>
        public bool IsEmpty()
        {
            return (children.Count == 0);
        }

        /// <summary>
        /// Get the count of nodes.
        /// </summary>
        /// <returns>Node count</returns>
        public int Count()
        {
            return children.Count;
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
            if (name == ConfigurationSettings.NODE_SEARCH_RECURSIVE_WILDCARD)
            {
                string cname = path[index + 1];
                if (children.Count > 0)
                {
                    List<AbstractConfigNode> nodes = new List<AbstractConfigNode>();
                    if (children.ContainsKey(cname))
                    {
                        AbstractConfigNode node = children[cname].Find(path, index + 1);
                        nodes.Add(node);
                    }

                    foreach (string key in children.Keys)
                    {
                        AbstractConfigNode cnode = children[key].Find(path, index);
                        if (cnode != null)
                        {
                            nodes.Add(cnode);
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
            }
            ResolvedName resolved = ConfigUtils.ResolveName(name, Name, Configuration.Settings);
            if (resolved == null)
            {
                if (name == Name)
                {
                    if (index == (path.Count - 1))
                    {
                        return this;
                    }
                    else
                    {
                        return FindChild(path, index);
                    }
                }
                else if (index == 0)
                {
                    return FindChild(path, index - 1);
                }
            }
            else
            {
                if (resolved.Name == Name)
                {
                    if (!String.IsNullOrWhiteSpace(resolved.AbbrReplacement))
                    {
                        if (children.ContainsKey(resolved.AbbrReplacement))
                        {
                            AbstractConfigNode child = children[resolved.AbbrReplacement];
                            if (typeof(ConfigKeyValueNode).IsAssignableFrom(child.GetType()))
                            {
                                ConfigKeyValueNode kv = (ConfigKeyValueNode)child;
                                if (index == (path.Count - 1))
                                {
                                    if (String.IsNullOrWhiteSpace(resolved.ChildName))
                                    {
                                        return kv;
                                    }
                                    else
                                    {
                                        return kv.GetValue(resolved.ChildName);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        return FindChild(path, index);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find a child node that matches the search.
        /// </summary>
        /// <param name="path">List of tokenized path elements.</param>
        /// <param name="index">Current Index</param>
        /// <returns>Config node if found</returns>
        private AbstractConfigNode FindChild(List<string> path, int index)
        {
            string name = path[index + 1];
            if (name == ConfigurationSettings.NODE_SEARCH_PARENT)
            {
                if (Parent != null)
                {
                    path[index + 1] = Parent.Name;
                    return Parent.Find(path, index + 1);
                }
            }
            if (name.Length == 1 && name[0] == ConfigurationSettings.NODE_SEARCH_WILDCARD)
            {
                if (index == (path.Count - 2))
                {
                    if (children.Count > 0)
                    {
                        List<AbstractConfigNode> nodes = new List<AbstractConfigNode>();
                        foreach (string key in children.Keys)
                        {
                            nodes.Add(children[key]);
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
                if (children.Count > 0)
                {
                    List<AbstractConfigNode> nodes = new List<AbstractConfigNode>();
                    foreach (string key in children.Keys)
                    {
                        AbstractConfigNode sn = children[key].Find(path, index + 1);
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
                List<AbstractConfigNode> nodes = new List<AbstractConfigNode>();
                if (children.ContainsKey(name))
                {
                    return children[name].Find(path, index + 1);
                }
                else if (name == ConfigurationSettings.NODE_SEARCH_RECURSIVE_WILDCARD)
                {
                    foreach (string key in children.Keys)
                    {
                        AbstractConfigNode cnode = children[key].Find(path, index + 1);
                        if (cnode != null)
                        {
                            nodes.Add(cnode);
                        }
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
            else
            {
                name = resolved.Name;
                if (name == Name)
                {
                    if (!String.IsNullOrWhiteSpace(resolved.AbbrReplacement))
                    {
                        if (children.ContainsKey(resolved.AbbrReplacement))
                        {
                            return children[resolved.AbbrReplacement].Find(path, index + 1);
                        }
                    }
                }
                else if (children.ContainsKey(name))
                {
                    return children[name].Find(path, index + 1);
                }
            }
            return null;
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
            foreach (string key in children.Keys)
            {
                children[key].UpdateState(state);
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
                throw ConfigurationException.PropertyMissingException(nameof(children));
            }
            foreach (string key in children.Keys)
            {
                children[key].Validate();
            }
        }
    }
}
