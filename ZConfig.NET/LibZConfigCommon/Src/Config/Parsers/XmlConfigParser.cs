using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config;
using LibZConfig.Common.Config.Nodes;
using LibZConfig.Common.Config.Readers;

namespace LibZConfig.Common.Config.Parsers
{
    public class XmlConfigParser : AbstractConfigParser
    {
        public const string XML_CONFIG_NODE_HEADER = "header";
        public const string XML_CONFIG_HEADER_ATTR_NAME = "name";
        public const string XML_CONFIG_HEADER_ATTR_GROUP = "group";
        public const string XML_CONFIG_HEADER_ATTR_APP = "application";
        public const string XML_CONFIG_HEADER_ATTR_VERSION = "version";

        public override void Parse(string name, AbstractReader reader, Version version, ConfigurationSettings settings)
        {
            if (settings == null)
            {
                settings = new ConfigurationSettings();
            }
            configuration = new Configuration(settings);
            LogUtils.Info(String.Format("Loading Configuration: [name={0}]", name));
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader.GetStream());

                XmlElement root = doc.DocumentElement;
                Parse(name, version, root);

                configuration.Validate();
                configuration.PostLoad();
                LogUtils.Debug(String.Format("Configuration Loaded: [name={0}]", configuration.Header.Name), configuration);
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                configuration.SetError(ex);
                throw ex;
            }
        }

        private void Parse(string name, Version version, XmlElement root)
        {

            Stack<AbstractConfigNode> nodeStack = new Stack<AbstractConfigNode>();
            configuration.RootConfigNode = ParseRootNode(name, version, root);
            nodeStack.Push(configuration.RootConfigNode);

            if (nodeStack.Count > 0)
            {
                throw new ConfigurationException(String.Format("Error parsing configuration: Node stack is not empty. [name={0}]", name));
            }
        }


        private ConfigPathNode ParseRootNode(string name, Version version, XmlElement root)
        {
            ConfigPathNode node = new ConfigPathNode(configuration, null);
            node.Name = root.Name;
            if (root.HasChildNodes)
            {
                foreach (XmlNode elem in root.ChildNodes)
                {
                    if (elem.Name == XML_CONFIG_NODE_HEADER && elem.NodeType == XmlNodeType.Element)
                    {
                        ParseHeader(name, version, (XmlElement)elem);
                        break;
                    }
                }
            }
            return node;
        }

        private void ParseHeader(string name, Version version, XmlElement elem)
        {
            if (elem.HasAttributes)
            {
                ConfigurationHeader header = new ConfigurationHeader();
                string attr = elem.GetAttribute(XML_CONFIG_HEADER_ATTR_GROUP);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    header.ApplicationGroup = attr;
                }
                attr = elem.GetAttribute(XML_CONFIG_HEADER_ATTR_APP);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    header.Application = attr;
                }
                attr = elem.GetAttribute(XML_CONFIG_HEADER_ATTR_NAME);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    header.Name = attr;
                }
                attr = elem.GetAttribute(XML_CONFIG_HEADER_ATTR_VERSION);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    Version ver = Version.Parse(attr);
                    if (!ver.Equals(version))
                    {
                        throw new ConfigurationException(String.Format("Configuration Version mis-match: [expected={0}][actual={1}]", version.ToString(), ver.ToString()));
                    }
                    header.Version = ver;
                }
                if (IsTextNode(elem))
                {
                    header.Description = elem.InnerText;
                }
                if (name != header.Name)
                {
                    throw new ConfigurationException(String.Format("Invalid Configuration: Name mis-match. [expected={0}][actual={1}]", name, header.Name));
                }
                LogUtils.Debug(String.Format("Loaded Header: [name={0}]", header.Name), header);
                configuration.Header = header;
            }
            else
            {
                throw new ConfigurationException(String.Format("Error loading configuration header: No attributes defined. [name={0}]", name));
            }
        }

        private bool IsTextNode(XmlElement elem)
        {
            if (elem.HasChildNodes && elem.ChildNodes.Count == 1)
            {
                XmlNode cnode = elem.FirstChild;
                if (cnode != null && cnode.NodeType == XmlNodeType.Text)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
