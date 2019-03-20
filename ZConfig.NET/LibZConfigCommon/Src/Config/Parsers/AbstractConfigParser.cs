using System;
using System.Collections.Generic;
using System.Text;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config.Nodes;
using LibZConfig.Common.Config.Readers;

namespace LibZConfig.Common.Config.Parsers
{
    /// <summary>
    /// Abstract base class to define configuration parsers.
    /// </summary>
    public abstract class AbstractConfigParser
    {
        /// <summary>
        /// Configuration Settings.
        /// </summary>
        protected ConfigurationSettings settings;
        /// <summary>
        /// Parsed configuration instance.
        /// </summary>
        protected Configuration configuration;

        /// <summary>
        /// Get the configuration settings used to parse.
        /// </summary>
        /// <returns>Configuration Settings.</returns>
        public ConfigurationSettings GetConfigurationSettings()
        {
            return settings;
        }

        /// <summary>
        /// Get the Parsed configuration instance.
        /// </summary>
        /// <returns></returns>
        public Configuration GetConfiguration()
        {
            return configuration;
        }

        /// <summary>
        /// Post Load function to be called after loading the configuration.
        /// </summary>
        protected void PostLoad()
        {
            Dictionary<string, ConfigValueNode> properties = new Dictionary<string, ConfigValueNode>();
            NodePostLoad(configuration.RootConfigNode, properties);

            configuration.Validate();
            configuration.RootConfigNode.UpdateState(ENodeState.Synced);
        }

        /// <summary>
        /// Recursively replace defined variables with properties in the configuration nodes.
        /// 
        /// </summary>
        /// <param name="node">Configuration Node.</param>
        /// <param name="inProps">Scoped properties map</param>
        private void NodePostLoad(AbstractConfigNode node, Dictionary<string, ConfigValueNode> inProps)
        {
            Dictionary<string, ConfigValueNode> properties = General.Clone<string, ConfigValueNode>(inProps);
            if (node.GetType() == typeof(ConfigPathNode))
            {
                ConfigPathNode pnode = (ConfigPathNode)node;
                ConfigPropertiesNode props = pnode.GetProperties();
                if (props != null && !props.IsEmpty())
                {
                    Dictionary<string, ConfigValueNode> pd = props.GetValues();
                    foreach (string key in pd.Keys)
                    {
                        if (!properties.ContainsKey(key))
                        {
                            properties.Add(key, pd[key]);
                        }
                        else
                        {
                            properties[key] = pd[key];
                        }
                    }
                }
                if (!pnode.IsEmpty())
                {
                    foreach (string key in pnode.GetChildren().Keys)
                    {
                        NodePostLoad(pnode.GetChildren()[key], properties);
                    }
                }
            }
            else
            {
                if (node.GetType() == typeof(ConfigParametersNode))
                {
                    ConfigParametersNode pnode = (ConfigParametersNode)node;
                    if (!pnode.IsEmpty())
                    {
                        foreach (string key in pnode.GetValues().Keys)
                        {
                            ConfigValueNode vn = pnode.GetValue(key);
                            string value = vn.GetValue();
                            if (!String.IsNullOrWhiteSpace(value))
                            {
                                string nv = ReplaceVariable(value, properties);
                                vn.SetValue(nv);
                            }
                        }
                    }
                }
                else if (node.GetType() == typeof(ConfigAttributesNode))
                {
                    ConfigAttributesNode pnode = (ConfigAttributesNode)node;
                    if (!pnode.IsEmpty())
                    {
                        foreach (string key in pnode.GetValues().Keys)
                        {
                            ConfigValueNode vn = pnode.GetValue(key);
                            string value = vn.GetValue();
                            if (!String.IsNullOrWhiteSpace(value))
                            {
                                string nv = ReplaceVariable(value, properties);
                                vn.SetValue(nv);
                            }
                        }
                    }
                }
                else if (node.GetType() == typeof(ConfigListValueNode))
                {
                    ConfigListValueNode pnode = (ConfigListValueNode)node;
                    if (!pnode.IsEmpty())
                    {
                        foreach (ConfigValueNode vn in pnode.GetValues())
                        {
                            string value = vn.GetValue();
                            if (!String.IsNullOrWhiteSpace(value))
                            {
                                string nv = ReplaceVariable(value, properties);
                                vn.SetValue(nv);
                            }
                        }
                    }
                }
                else if (node.GetType() == typeof(ConfigElementListNode))
                {
                    ConfigElementListNode pnode = (ConfigElementListNode)node;
                    if (!pnode.IsEmpty())
                    {
                        foreach (ConfigElementNode vn in pnode.GetValues())
                        {
                            NodePostLoad(vn, properties);
                        }
                    }
                }
                else if (node.GetType() == typeof(ConfigValueNode))
                {
                    ConfigValueNode vn = (ConfigValueNode)node;
                    string value = vn.GetValue();
                    if (!String.IsNullOrWhiteSpace(value))
                    {
                        string nv = ReplaceVariable(value, properties);
                        vn.SetValue(nv);
                    }
                }
            }
        }

        /// <summary>
        /// Check and Replace variables in the input string.
        /// 
        /// Variable replacement first looks in the passed properties and 
        /// then will try to replace using environment variables.
        /// 
        /// </summary>
        /// <param name="input">Input String</param>
        /// <param name="properties">Properties Map</param>
        /// <returns>Updated String</returns>
        private string ReplaceVariable(String input, Dictionary<string, ConfigValueNode> properties)
        {
            if (!String.IsNullOrWhiteSpace(input))
            {
                if (VariableRegexParser.HasVariable(input))
                {
                    List<string> vars = VariableRegexParser.GetVariables(input);
                    if (vars != null && vars.Count > 0)
                    {
                        foreach (string var in vars)
                        {
                            if (properties.ContainsKey(var))
                            {
                                string r = properties[var].GetValue();
                                string rv = String.Format("${{{0}}}", var);
                                input = input.Replace(rv, r);
                            }
                            else
                            {
                                string r = Environment.GetEnvironmentVariable(var);
                                if (!String.IsNullOrWhiteSpace(r))
                                {
                                    string rv = String.Format("${{{0}}}", var);
                                    input = input.Replace(rv, r);
                                }
                            }
                        }
                    }
                }
            }
            return input;
        }

        /// <summary>
        /// Parse the configuration loaded by the reader handle.
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <param name="reader">Reader handle to read the configuration</param>
        /// <param name="version">Expected Version of the configuration</param>
        /// <param name="settings">Configuration Settings to use</param>
        public abstract void Parse(string name, AbstractReader reader, Version version, ConfigurationSettings settings);
    }
}
