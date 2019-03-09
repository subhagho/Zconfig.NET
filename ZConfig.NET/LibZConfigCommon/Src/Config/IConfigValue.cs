using System;
using System.Collections.Generic;
using System.Text;

namespace LibZConfig.Common.Config.Nodes
{
    /// <summary>
    /// Interface to be implemented by Value nodes in the configuration.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    interface IConfigValue<T>
    {
        /// <summary>
        /// Get the node value.
        /// </summary>
        /// <returns>Node Value</returns>
        T GetValue();
    }
}
