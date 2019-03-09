using System;
using System.Collections.Generic;
using System.Text;

namespace LibZConfig.Common.Config.Nodes
{
    /// <summary>
    /// Configuration Value node.
    /// </summary>
    public class ConfigValueNode : AbstractConfigNode, IConfigValue<string>
    {
        /// <summary>
        /// Configuration Value
        /// </summary>
        private string value;

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigValueNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigValueNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }

        /// <summary>
        /// Set the value of this node.
        /// </summary>
        /// <param name="value">String Value</param>
        public void SetValue(string value)
        {
            this.value = value;
            Updated();
        }

        /// <summary>
        /// Get the value of this node.
        /// </summary>
        /// <returns>String Value</returns>
        public string GetValue()
        {
            return value;
        }

        /// <summary>
        /// Execute find on this node, will return self if terminal node in search and name matches.
        /// </summary>
        /// <param name="path">Path list</param>
        /// <param name="index">Current index</param>
        /// <returns>This node or NULL</returns>
        public override AbstractConfigNode Find(List<string> path, int index)
        {
            if (index == (path.Count - 1))
            {
                string name = path[index];
                if (name == this.Name)
                {
                    return this;
                }
            }
            return null;
        }

        /// <summary>
        /// Method to be invoked post configuration load.
        /// </summary>
        public override void PostLoad()
        {
            // Nothing to be done.
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
        /// Override ToString()
        /// </summary>
        /// <returns>Name/Value</returns>
        public override string ToString()
        {
            return String.Format("[name={0}, value={1}]", Name, value);
        }
    }
}
