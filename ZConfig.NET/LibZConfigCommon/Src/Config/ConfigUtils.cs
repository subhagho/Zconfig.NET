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
        public string AbbrReplacement { get; set; }
        /// <summary>
        /// Child node name, if any.
        /// </summary>
        public string ChildName { get; set; }
        /// <summary>
        /// Abbreviation tag
        /// </summary>
        public char Abbr { get; set; }
    }

    /// <summary>
    /// Utility methods for configuration search and parsing.
    /// </summary>
    public static class ConfigUtils
    {
        public static void CheckSearchRoot(List<string> path, string name, ConfigurationSettings settings)
        {
            if (path.Count > 0)
            {
                string nname = path[0];
                ResolvedName resolved = ResolveName(nname, name, settings);
                if (resolved == null)
                {
                    if (nname != name)
                    {
                        path.Insert(0, name);
                    }
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(resolved.ChildName))
                    {
                        nname = String.Format("{0}{1}", resolved.Name, resolved.Abbr);
                    }
                    else
                    {
                        nname = String.Format("{0}{1}{2}", resolved.Name, resolved.Abbr, resolved.ChildName);
                    }
                    path[0] = nname;
                }
            }
        }

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
                return ResolveName(name, nodeName, ConfigListNode<ConfigValueNode>.NODE_ABBREVIATION, settings);
            }
            else if (name.Length == 1 && name[0] == ConfigurationSettings.NODE_SEARCH_WILDCARD)
            {
                ResolvedName resolved = new ResolvedName();
                resolved.Name = nodeName;
                resolved.AbbrReplacement = null;
                resolved.ChildName = null;

                return resolved;
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
            switch (abbr)
            {
                case ConfigAttributesNode.NODE_ABBREVIATION:
                    resolved.AbbrReplacement = settings.AttributesNodeName;
                    break;
                case ConfigParametersNode.NODE_ABBREVIATION:
                    resolved.AbbrReplacement = settings.ParametersNodeName;
                    break;
                case ConfigPropertiesNode.NODE_ABBREVIATION:
                    resolved.AbbrReplacement = settings.PropertiesNodeName;
                    break;
                case ConfigListNode<ConfigValueNode>.NODE_ABBREVIATION:
                    resolved.AbbrReplacement = ConfigListNode<ConfigValueNode>.NODE_NAME;
                    break;
            }
            resolved.Abbr = abbr;

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

        public static string MaskSearchPath(string path)
        {
            int si = path.IndexOf('[');
            if (si >= 0)
            {
                string s0 = null;
                if (si > 0)
                {
                    s0 = path.Substring(0, si);
                }
                int ei = path.IndexOf(']');
                if (ei > 0)
                {
                    string s1 = path.Substring(si, (ei - si) + 1);
                    string s2 = path.Substring(ei + 1);

                    if (s1.Contains('.'))
                    {
                        s1 = s1.Replace(@".", @"###");
                        if (s1 != null)
                            path = String.Format("{0}{1}{2}", s0, s1, s2);
                        else
                            path = String.Format("{0}{1}", s1, s2);
                    }
                }
            }
            return path;
        }

        public static string UnmaskSearchPath(string path)
        {
            path = path.Replace(@"###", @".");
            path = path.Replace("[", "");
            path = path.Replace("]", "");

            return path;
        }
    }
}
