#region copyright
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2019
// Date: 2019-3-23
// Project: LibZConfigCommon
// Subho Ghosh (subho dot ghosh at outlook.com)
//
//
#endregion
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        private const string ARRAY_INDEX_REGEX = "^(\\w*)\\[(\\d*)\\]$";
        private static Regex ARRAY_INDEX_RE = new Regex(ARRAY_INDEX_REGEX);

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
                    else if (!String.IsNullOrWhiteSpace(resolved.ChildName))
                    {
                        if (resolved.Abbr != '\0')
                        {
                            nname = String.Format("{0}{1}{2}", resolved.Name, resolved.Abbr, resolved.ChildName);
                        }
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
            else if (ARRAY_INDEX_RE.IsMatch(name))
            {
                MatchCollection mc = ARRAY_INDEX_RE.Matches(name);
                if (mc != null && mc.Count > 0 && mc[0].Groups != null && mc[0].Groups.Count > 1)
                {
                    ResolvedName resolved = new ResolvedName();
                    resolved.AbbrReplacement = ConfigListNode<ConfigValueNode>.NODE_NAME;
                    resolved.Name = mc[0].Groups[1].Value;
                    resolved.ChildName = mc[0].Groups[2].Value;

                    return resolved;
                }
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

        public static string CheckSearchPath(string path, AbstractConfigNode node)
        {
            path = path.Trim();
           if (path.StartsWith('.'))
            {
                if (path.StartsWith(ConfigurationSettings.NODE_SEARCH_PARENT))
                {
                    path = string.Format("{0}/{1}", node.Name, path);
                }
                else
                {
                    path = path.Substring(1);
                    path = string.Format("{0}/{1}", node.Name, path);
                }
            }
            return path;
        }

        public static string MaskSearchPath(string path)
        {
            if (ARRAY_INDEX_RE.IsMatch(path))
            {
                return path;
            }

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
                        s1 = s1.Replace(ConfigurationSettings.NODE_SEARCH_SEPERATOR, @"###");
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
            if (ARRAY_INDEX_RE.IsMatch(path))
            {
                return path;
            }

            path = path.Replace(@"###", ConfigurationSettings.NODE_SEARCH_SEPERATOR);
            path = path.Replace("[", "");
            path = path.Replace("]", "");

            return path;
        }
    }
}
