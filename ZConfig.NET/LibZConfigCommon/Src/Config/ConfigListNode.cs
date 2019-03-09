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
        public const char NODE_ABBREVIATION = '/';
        /// <summary>
        /// Dummy node list name.
        /// </summary>
        public const string NODE_NAME = "LIST_NODE";

        private List<T> values = new List<T>();

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
                if (name == Name && index == (path.Count - 1))
                {
                    return this;
                }
            }
            else
            {
                if (resolved.Abbreviation == NODE_NAME)
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
                if (name == Name && index == (path.Count - 1))
                {
                    return this;
                }
            }
            else
            {
                if (resolved.Abbreviation == NODE_NAME)
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
