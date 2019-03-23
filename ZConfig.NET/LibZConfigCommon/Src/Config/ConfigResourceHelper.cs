using System;
using System.Diagnostics.Contracts;
using System.IO;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config.Nodes;

namespace LibZConfig.Common.Config
{
    public static class ConfigResourceHelper
    {
        public static StreamReader GetResourceStream(Configuration configuration, string path)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(!String.IsNullOrWhiteSpace(path));

            AbstractConfigNode node = configuration.Find(path);
            if (node != null && typeof(ConfigResourceNode).IsAssignableFrom(node.GetType()))
            {
                ConfigResourceNode rnode = (ConfigResourceNode)node;
                
            }
            return null;
        }
    }
}
