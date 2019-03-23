using System;
using System.Collections.Generic;
using LibZConfig.Common.Utils;

namespace LibZConfig.Common.Config.Nodes
{
    /// <summary>
    /// Node represents a linked/included configuration.
    /// </summary>
    public class ConfigIncludeNode : ConfigElementNode
    {
        
        /// <summary>
        /// Name of the included configuration
        /// </summary>
        public string ConfigName { get; set; }
        /// <summary>
        /// URI of the included configuration.
        /// </summary>
        public Uri Path { get; set; }
        /// <summary>
        /// Reader type for this configuration load.
        /// </summary>
        public EUriScheme ReaderType { get; set; }
        /// <summary>
        /// Version of the included configuration.
        /// </summary>
        public Version Version { get; set; }
        /// <summary>
        /// Parsed configuration node.
        /// </summary>
        public AbstractConfigNode Node { get; set; }
        /// <summary>
        /// Loaded configuration reference.
        /// </summary>
        public Configuration Reference { get; set; }

        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigIncludeNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigIncludeNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
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
            if (Node != null)
            {
                return Node.Find(path, index);
            }
            return null;
        }

        /// <summary>
        /// Method to be invoked post configuration load.
        /// </summary>
        public override void PostLoad()
        {
            if (Node != null)
            {
                Node.PostLoad();
            }
        }

        /// <summary>
        /// Method to recursively update the state of the nodes.
        /// </summary>
        /// <param name="state">Updated state</param>
        public override void UpdateState(ENodeState state)
        {
            State.State = state;
            if (Node != null)
            {
                Node.UpdateState(state);
            }
        }

        /// <summary>
        /// Update the configuration for this node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        public override void UpdateConfiguration(Configuration configuration)
        {
            base.UpdateConfiguration(configuration);
            if (Node != null)
            {
                Node.UpdateConfiguration(configuration);
            }
        }

        /// <summary>
        /// Method to validate the node instance.
        /// </summary>
        public override void Validate()
        {
            base.Validate();
            if (String.IsNullOrWhiteSpace(ConfigName))
            {
                throw ConfigurationException.PropertyMissingException(nameof(ConfigName));
            }
            if (Path == null)
            {
                throw ConfigurationException.PropertyMissingException(nameof(Path));
            }
            if (ReaderType == EUriScheme.none)
            {
                throw ConfigurationException.PropertyMissingException(nameof(ReaderType));
            }
            if (Reference == null)
            {
                throw ConfigurationException.PropertyMissingException("Reference Configuration");
            }
            if (Version == null)
            {
                throw ConfigurationException.PropertyMissingException(nameof(Version));
            }
            if (Node == null)
            {
                throw new ConfigurationException("No configuration loaded.");
            }
            Node.Validate();
        }
    }
}
