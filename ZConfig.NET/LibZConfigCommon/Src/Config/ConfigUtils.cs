using System;
using System.Collections.Generic;
using System.Text;
using LibZConfig.Common.Config.Nodes;

namespace LibZConfig.Common.Config
{
    /// <summary>
    /// Struct to parse/resolve search name strings.
    /// </summary>
    public class ResolvedName
    {
        /// <summary>
        /// Resolved node name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Resolved node abbreviation tag
        /// </summary>
        public string Abbreviation { get; set; }
        /// <summary>
        /// Child node name, if any.
        /// </summary>
        public string ChildName { get; set; }
    }

    /// <summary>
    /// Utility methods for configuration search and parsing.
    /// </summary>
    public static class ConfigUtils
    {
        /// <summary>
        /// Resolve the specified name for search.
        /// </summary>
        /// <param name="name">Search node name</param>
        /// <param name="nodeName">Config Node name.</param>
        /// <returns>Resolved name or NULL</returns>
        public static ResolvedName ResolveName(string name, string nodeName, ConfigurationSettings settings)
        {
            if (name.Contains(ConfigAttributesNode.NODE_ABBREVIATION))
            {
                return ResolveName(name, nodeName, ConfigAttributesNode.NODE_ABBREVIATION, settings);
            }
            else if (name.Contains(ConfigParametersNode.NODE_ABBREVIATION))
            {
                return ResolveName(name, nodeName, ConfigParametersNode.NODE_ABBREVIATION, settings);
            }
            else if (name.Contains(ConfigPropertiesNode.NODE_ABBREVIATION))
            {
                return ResolveName(name, nodeName, ConfigPropertiesNode.NODE_ABBREVIATION, settings);
            }
            else if (name.Contains(ConfigListNode<ConfigValueNode>.NODE_ABBREVIATION))
            {
                return ResolveName(name, nodeName, ConfigPropertiesNode.NODE_ABBREVIATION, settings);
            }
            else if (name.Contains(ConfigurationSettings.NODE_SEARCH_WILDCARD))
            {
                ResolvedName resolved = new ResolvedName();
                resolved.Name = nodeName;
                resolved.Abbreviation = null;
                resolved.ChildName = null;
            }
            return null;
        }

        /// <summary>
        /// Resolve the search string for the specified abbreviation tag
        /// </summary>
        /// <param name="name">Search string</param>
        /// <param name="nodeName">Config node name</param>
        /// <param name="abbr">Abbreviation tag</param>
        /// <returns>Resolved name</returns>
        private static ResolvedName ResolveName(string name, string nodeName, char abbr, ConfigurationSettings settings)
        {
            ResolvedName resolved = new ResolvedName();
            switch(abbr)
            {
                case ConfigAttributesNode.NODE_ABBREVIATION:
                    resolved.Abbreviation = settings.AttributesNodeName;
                    break;
                case ConfigParametersNode.NODE_ABBREVIATION:
                    resolved.Abbreviation = settings.ParametersNodeName;
                    break;
                case ConfigPropertiesNode.NODE_ABBREVIATION:
                    resolved.Abbreviation = settings.PropertiesNodeName;
                    break;
                case ConfigListNode<ConfigValueNode>.NODE_ABBREVIATION:
                    resolved.Abbreviation = ConfigListNode<ConfigValueNode>.NODE_NAME;
                    break;
            }

            string[] parts = name.Split(abbr);
            string nname = parts[0];
            string cname = parts[1];

            if (String.IsNullOrWhiteSpace(nname) || (nname.Length == 1 && nname[0] == ConfigurationSettings.NODE_SEARCH_WILDCARD))
            {
                resolved.Name = nodeName;
            }
            else
            {
                resolved.Name = nname;
            }

            if (!String.IsNullOrWhiteSpace(cname))
            {
                resolved.ChildName = cname;
            }
            else
            {
                resolved.ChildName = null;
            }
            return resolved;
        }
    }
}
