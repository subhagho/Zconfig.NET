using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;
using LibZConfig.Common.Config;

namespace LibZConfig.Common.Config.Nodes
{
    public class ConfigPathNode : AbstractConfigNode
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
            Contract.Requires(node != null);

            node.Parent = this;
            children[node.Name] = node;
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
            string name = path[index];
            ResolvedName resolved = ConfigUtils.ResolveName(name, Name, Configuration.Settings);
            if (resolved == null)
            {
                if (name == Name)
                {
                    if (index == (path.Count - 1))
                    {
                        return this;
                    }
                    return FindChild(path, index);
                }
            }
            else
            {
                if (resolved.Name == Name)
                {
                    if (String.IsNullOrWhiteSpace(resolved.Abbreviation))
                    {
                        if (children.ContainsKey(resolved.Abbreviation))
                        {
                            AbstractConfigNode child = children[resolved.Abbreviation];
                            if (typeof(ConfigKeyValueNode).IsAssignableFrom(child.GetType()))
                            {
                                ConfigKeyValueNode kv = (ConfigKeyValueNode)child;
                                if (index == (path.Count - 2))
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
            if (name.Length == 1 && name[0] == ConfigurationSettings.NODE_SEARCH_WILDCARD)
            {
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
                if (children.ContainsKey(name))
                {
                    return children[name].Find(path, index + 1);
                }
            }
            else
            {
                name = resolved.Name;
                if (children.ContainsKey(name))
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
