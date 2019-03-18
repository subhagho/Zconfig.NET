using System;
using System.Collections.Generic;
using System.Text;

namespace LibZConfig.Common.Config.Nodes
{
    /// <summary>
    /// Element nodes represent path nodes in the configuration.
    /// </summary>
    public abstract class ConfigElementNode : AbstractConfigNode
    {
        /// <summary>
        /// Default Empty constructor
        /// </summary>
        protected ConfigElementNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        protected ConfigElementNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

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
        
    }
}
