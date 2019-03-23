using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LibZConfig.Common.Config.Parsers;
using LibZConfig.Common.Config.Nodes;

namespace LibZConfig.Common.Config.Writers
{
    /// <summary>
    /// Class writes a configuration instance to the XML format file.
    /// </summary>
    public class XmlConfigWriter : AbstractConfigWriter
    {
        /// <summary>
        /// Write this instance of the configuration to a string buffer.
        /// </summary>
        /// <param name="configuration">Configuration instance to write.</param>
        /// <returns>Return the string buffer</returns>
        public override string Write(Configuration configuration)
        {
            Contract.Requires(configuration != null);
            StringWriter sw = new StringWriter();

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(sw, settings))
            {
                 writer.WriteStartDocument();
                {
                    if (configuration.RootConfigNode != null)
                    {
                        writer.WriteStartElement(configuration.RootConfigNode.Name);
                        {
                            WriteHeader(writer, configuration.Header);
                            if (!configuration.RootConfigNode.IsEmpty())
                            {
                                Dictionary<string, AbstractConfigNode> nodes = configuration.RootConfigNode.GetChildren();
                                foreach (string key in nodes.Keys)
                                {
                                    WriteNode(writer, nodes[key], configuration.Settings);
                                }
                            }
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndDocument();
            }
            return sw.ToString();
        }

        /// <summary>
        /// Write this instance of the configuration to the specified output location.
        /// </summary>
        /// <param name="configuration">Configuration instance to write.</param>
        /// <param name="path">Output location to write to.</param>
        /// <returns>Return the path of the output file created.</returns>
        public override string Write(Configuration configuration, string path)
        {
            Contract.Requires(configuration != null);
            Contract.Requires(!String.IsNullOrWhiteSpace(path));
            string filename = String.Format("{0}/{1}.xml", path, configuration.Header.Name);

            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter writer = XmlWriter.Create(filename, settings))
            {
                writer.WriteStartDocument();
                {
                    if (configuration.RootConfigNode != null)
                    {
                        writer.WriteStartElement(configuration.RootConfigNode.Name);
                        {
                            WriteHeader(writer, configuration.Header);
                            if (!configuration.RootConfigNode.IsEmpty())
                            {
                                Dictionary<string, AbstractConfigNode> nodes = configuration.RootConfigNode.GetChildren();
                                foreach (string key in nodes.Keys)
                                {
                                    WriteNode(writer, nodes[key], configuration.Settings);
                                }
                            }
                        }
                        writer.WriteEndElement();
                    }
                }
                writer.WriteEndDocument();
            }
            return filename;
        }

        private void WriteNode(XmlWriter writer, AbstractConfigNode node, ConfigurationSettings settings)
        {
            if (node.GetType() == typeof(ConfigPathNode))
            {
                ConfigPathNode pnode = (ConfigPathNode)node;
                writer.WriteStartElement(node.Name);
                {
                    ConfigAttributesNode attrs = pnode.GetAttributes();
                    if (attrs != null)
                    {
                        Dictionary<string, ConfigValueNode> values = attrs.GetValues();
                        if (values != null && values.Count > 0)
                        {
                            foreach (string key in values.Keys)
                            {
                                ConfigValueNode vn = values[key];
                                if (vn != null)
                                {
                                    writer.WriteAttributeString(vn.Name, vn.GetValue());
                                }
                            }
                        }
                    }
                    Dictionary<string, AbstractConfigNode> nodes = pnode.GetChildren();
                    foreach (string key in nodes.Keys)
                    {
                        AbstractConfigNode cnode = nodes[key];
                        if (cnode.Name == settings.AttributesNodeName)
                        {
                            continue;
                        }
                        WriteNode(writer, cnode, settings);
                    }
                }
                writer.WriteEndElement();
            }
            else if (node.GetType() == typeof(ConfigValueNode))
            {
                ConfigValueNode vn = (ConfigValueNode)node;
                writer.WriteStartElement(vn.Name);
                writer.WriteString(vn.GetValue());
                writer.WriteEndElement();
            }
            else if (node.GetType() == typeof(ConfigParametersNode) || node.GetType() == typeof(ConfigPropertiesNode))
            {
                string name = null;
                if (node.GetType() == typeof(ConfigParametersNode))
                {
                    name = settings.ParametersNodeName;
                }
                else
                {
                    name = settings.PropertiesNodeName;
                }
                ConfigKeyValueNode kvnode = (ConfigKeyValueNode)node;
                WriteKeyValueNode(writer, kvnode, name);
            }
            else if (node.GetType() == typeof(ConfigListValueNode))
            {
                WriteListValueNode(writer, (ConfigListValueNode)node);
            }
            else if (node.GetType() == typeof(ConfigElementListNode))
            {
                WriteListElementNode(writer, (ConfigElementListNode)node, settings);
            }
            else if (node.GetType() == typeof(ConfigIncludeNode))
            {
                ConfigIncludeNode inode = (ConfigIncludeNode)node;
                WriteNode(writer, inode.Node, settings);
            }
            else if (typeof(ConfigResourceNode).IsAssignableFrom(node.GetType()))
            {
                ConfigResourceNode rnode = (ConfigResourceNode)node;
                WriteResourceNode(writer, rnode);
            }
        }

        private void WriteResourceNode(XmlWriter writer, ConfigResourceNode node)
        {
            writer.WriteStartElement(ConstXmlResourceNode.XML_CONFIG_NODE_RESOURCE);
            {
                writer.WriteAttributeString(ConstXmlResourceNode.XML_CONFIG_ATTR_RESOURCE_NAME, node.ResourceName);
                writer.WriteAttributeString(ConstXmlResourceNode.XML_CONFIG_ATTR_RESOURCE_TYPE, node.Type.ToString());
                writer.WriteStartElement(ConstXmlResourceNode.XML_CONFIG_NODE_RESOURCE_URL);
                writer.WriteString(node.Location.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteListElementNode(XmlWriter writer, ConfigElementListNode node, ConfigurationSettings settings)
        {
            writer.WriteStartElement(node.Name);
            {
                List<ConfigElementNode> values = node.GetValues();
                if (values != null && values.Count > 0)
                {
                    foreach (ConfigElementNode vn in values)
                    {
                        WriteNode(writer, vn, settings);
                    }
                }
            }
            writer.WriteEndElement();
        }

        private void WriteListValueNode(XmlWriter writer, ConfigListValueNode node)
        {
            writer.WriteStartElement(node.Name);
            {
                List<ConfigValueNode> values = node.GetValues();
                if (values != null && values.Count > 0)
                {
                    foreach (ConfigValueNode vn in values)
                    {
                        writer.WriteStartElement(vn.Name);
                        writer.WriteString(vn.GetValue());
                        writer.WriteEndElement();
                    }
                }
            }
            writer.WriteEndElement();
        }

        private void WriteKeyValueNode(XmlWriter writer, ConfigKeyValueNode node, string name)
        {
            writer.WriteStartElement(name);
            Dictionary<string, ConfigValueNode> values = node.GetValues();
            if (values != null && values.Count > 0)
            {
                foreach (string key in values.Keys)
                {
                    ConfigValueNode vn = values[key];
                    if (vn != null)
                    {
                        writer.WriteStartElement(vn.Name);
                        writer.WriteString(vn.GetValue());
                        writer.WriteEndElement();
                    }
                }
            }
            writer.WriteEndElement();
        }

        private void WriteHeader(XmlWriter writer, ConfigurationHeader header)
        {
            writer.WriteStartElement(ConstXmlConfigHeader.XML_CONFIG_NODE_HEADER);
            {
                writer.WriteAttributeString(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_ID, header.Id);
                writer.WriteAttributeString(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_GROUP, header.ApplicationGroup);
                writer.WriteAttributeString(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_APP, header.Application);
                writer.WriteAttributeString(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_NAME, header.Name);
                writer.WriteAttributeString(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_VERSION, header.Version.ToString());
                if (!String.IsNullOrWhiteSpace(header.Description))
                {
                    writer.WriteStartElement(ConstXmlConfigHeader.XML_CONFIG_HEADER_DESC);
                    writer.WriteString(header.Description);
                    writer.WriteEndElement();
                }
                if (header.CreatedBy != null)
                {
                    WriteModifiedBy(writer, ConstXmlConfigHeader.XML_CONFIG_HEADER_CREATED_BY, header.CreatedBy);
                }
                if (header.ModifiedBy != null)
                {
                    WriteModifiedBy(writer, ConstXmlConfigHeader.XML_CONFIG_HEADER_UPDATED_BY, header.ModifiedBy);
                }
            }
            writer.WriteEndElement();
        }

        private void WriteModifiedBy(XmlWriter writer, string name, ModifiedBy modifiedBy)
        {
            writer.WriteStartElement(name);
            writer.WriteAttributeString(ConstXmlConfigHeader.XML_CONFIG_HEADER_MB_ATTR_USER, modifiedBy.User);
            writer.WriteAttributeString(ConstXmlConfigHeader.XML_CONFIG_HEADER_MB_ATTR_TIME, modifiedBy.Timestamp.ToString());
            writer.WriteEndElement();
        }
    }
}
