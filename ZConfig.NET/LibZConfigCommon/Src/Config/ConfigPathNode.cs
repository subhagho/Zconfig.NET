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

        public Dictionary<string, AbstractConfigNode> GetChildren()
        {
            return children;
        }

        public void SetChildren(Dictionary<string, AbstractConfigNode> children)
        {
            this.children = children;
        }

        public ConfigPathNode AddChildNode(AbstractConfigNode node)
        {
            Contract.Requires(node != null);

            node.Parent = this;
            children[node.Name] = node;
            Updated();

            return this;
        }

        public ConfigPathNode RemoveChildNode(string name)
        {
            if (children.ContainsKey(name))
            {
                children.Remove(name);
                Updated();
            }
            return this;
        }

        public AbstractConfigNode GetChildNode(string name)
        {
            if (children.ContainsKey(name))
            {
                return children[name];
            }
            return null;
        }

        public bool IsEmpty()
        {
            return (children.Count == 0);
        }

        public int Count()
        {
            return children.Count;
        }

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

        private AbstractConfigNode FindChild(List<string> path, int index)
        {
            string name = path[index + 1];
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

        public override void PostLoad()
        {
            if (State.HasError())
            {
                throw new ConfigurationException(State.GetError());
            }
            UpdateState(ENodeState.Synced);
        }

        public override void UpdateState(ENodeState state)
        {
            State.State = state;
            foreach(string key in children.Keys)
            {
                children[key].UpdateState(state);
            }
        }

        public override void Validate()
        {
            base.Validate();
            if (IsEmpty())
            {
                throw ConfigurationException.PropertyMissingException("Children");
            }
            foreach (string key in children.Keys)
            {
                children[key].Validate();
            }
        }
    }
}
