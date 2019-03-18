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
        /// BLOB File Resource
        /// </summary>
        BLOB
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
        /// Default Empty constructor
        /// </summary>
        protected ConfigResourceNode() : base()
        {
            Type = EResourceType.NONE;
        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        protected ConfigResourceNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {
            Type = EResourceType.NONE;
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
            string name = path[index];
            if (name == Name && index == (path.Count - 1))
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

        /// <summary>
        /// Method to validate the node instance.
        /// </summary>
        public override void Validate()
        {
            base.Validate();
            if (File == null)
            {
                throw ConfigurationException.PropertyMissingException(nameof(File));
            }
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

        /// <summary>
        /// Method to validate the node instance.
        /// </summary>
        public override void Validate()
        {
            base.Validate();
            if (Directory == null)
            {
                throw ConfigurationException.PropertyMissingException(nameof(File));
            }
        }
    }

    /// <summary>
    /// BLOB Resource Handle
    /// </summary>
    public class ConfigResourceBlob : ConfigResourceFile
    {
        /// <summary>
        /// Default Empty constructor
        /// </summary>
        public ConfigResourceBlob() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        public ConfigResourceBlob(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }
    }
}
