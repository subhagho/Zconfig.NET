﻿#region copyright
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
using System.IO;
using System.Xml;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config;
using LibZConfig.Common.Config.Nodes;
using LibZConfig.Common.Config.Readers;

namespace LibZConfig.Common.Config.Parsers
{
    /// <summary>
    /// XML Constant Definitions for Include Node.
    /// </summary>
    public static class ConstXmlConfigIncludeNode
    {
        /// <summary>
        /// XML Node: Include Node
        /// </summary>
        public const string XML_CONFIG_INCLUDE = "include";
        /// <summary>
        /// XML Node: Include Node - Config name
        /// </summary>
        public const string XML_CONFIG_INCLUDE_NAME = "configName";
        /// <summary>
        /// XML Node: Include Node - Config Path
        /// </summary>
        public const string XML_CONFIG_INCLUDE_PATH = "path";
        /// <summary>
        /// XML Node: Include Node - Config Reader Type
        /// </summary>
        public const string XML_CONFIG_INCLUDE_TYPE = "type";
        /// <summary>
        /// XML Node: Include Node - Config Version
        /// </summary>
        public const string XML_CONFIG_INCLUDE_VERSION = "version";

    }

    /// <summary>
    /// XML Constant Definitions for Configuration Header.
    /// </summary>
    public static class ConstXmlConfigHeader
    {
        /// <summary>
        /// XML Node: Header
        /// </summary>
        public const string XML_CONFIG_NODE_HEADER = "header";
        /// <summary>
        /// XML Attribute: ID
        /// </summary>
        public const string XML_CONFIG_HEADER_ATTR_ID = "ID";
        /// <summary>
        /// XML Attribute: Name
        /// </summary>
        public const string XML_CONFIG_HEADER_ATTR_NAME = "name";
        /// <summary>
        /// XML Attribute: Appication Group
        /// </summary>
        public const string XML_CONFIG_HEADER_ATTR_GROUP = "group";
        /// <summary>
        /// XML Attribute: Application name.
        /// </summary>
        public const string XML_CONFIG_HEADER_ATTR_APP = "application";
        /// <summary>
        /// XML Attribute: Configuration Version
        /// </summary>
        public const string XML_CONFIG_HEADER_ATTR_VERSION = "version";
        /// <summary>
        /// XML Attribute: Password Hash
        /// </summary>
        public const string XML_CONFIG_HEADER_ATTR_HASH = "passwordHash";
        /// <summary>
        /// XML Node: Created By
        /// </summary>
        public const string XML_CONFIG_HEADER_CREATED_BY = "createdBy";
        /// <summary>
        /// XML Node: Updated By
        /// </summary>
        public const string XML_CONFIG_HEADER_UPDATED_BY = "updatedBy";
        /// <summary>
        /// XML Attribute: Modified By User
        /// </summary>
        public const string XML_CONFIG_HEADER_MB_ATTR_USER = "user";
        /// <summary>
        /// XML Attribute: Modified At Timestamp
        /// </summary>
        public const string XML_CONFIG_HEADER_MB_ATTR_TIME = "timestamp";
        /// <summary>
        /// XML Node: Description
        /// </summary>
        public const string XML_CONFIG_HEADER_DESC = "description";
    }

    /// <summary>
    /// XML Constant Definitions for Resource Node.
    /// </summary>
    public static class ConstXmlResourceNode
    {
        /// <summary>
        /// XML Node: Resource
        /// </summary>
        public const string XML_CONFIG_NODE_RESOURCE = "remoteResource";
        /// <summary>
        /// XML Attribute: Resource Type
        /// </summary>
        public const string XML_CONFIG_ATTR_RESOURCE_TYPE = "type";
        /// <summary>
        /// XML Attribute: Resource URL
        /// </summary>
        public const string XML_CONFIG_NODE_RESOURCE_URL = "url";
        /// <summary>
        /// XML Attribute: Resource name.
        /// </summary>
        public const string XML_CONFIG_ATTR_RESOURCE_NAME = "resourceName";
    }

    /// <summary>
    /// Configuration Parser implementation for reading XML configurations.
    /// </summary>
    public class XmlConfigParser : AbstractConfigParser
    {
        public const string XML_VALUE_ENCRYPTED = "encrypted";

        /// <summary>
        /// Parse a new configuration instance.
        /// </summary>
        /// <param name="name">Configuration name.</param>
        /// <param name="reader">Configuration Data reader</param>
        /// <param name="version">Expected configuration version</param>
        /// <param name="settings">Configuration Settings</param>
        /// <param name="password">Decryption Password (if required)</param>
        public override void Parse(string name, AbstractReader reader, Version version, ConfigurationSettings settings, string password = null)
        {
            Preconditions.CheckArgument(name);
            Preconditions.CheckArgument(reader);
            Preconditions.CheckArgument(version);

            if (settings == null)
            {
                settings = new ConfigurationSettings();
            }
            configuration = new Configuration(settings);
            this.settings = settings;
            LogUtils.Info(String.Format("Loading Configuration: [name={0}]", name));
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(reader.GetStream());

                XmlElement root = doc.DocumentElement;
                ParseRootNode(name, version, root, password);

                PostLoad(settings.ReplaceProperties);

                if (!String.IsNullOrWhiteSpace(password))
                {
                    ZConfigEnv.Vault.AddPasscode(configuration, password);
                    LogUtils.Info(String.Format("Added passcode to vault. [configuration={0}]", configuration.Header.Name));
                }
                LogUtils.Debug(String.Format("Configuration Loaded: [name={0}]", configuration.Header.Name), configuration);
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                configuration.SetError(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Parse the configuration root node and beyond.
        /// </summary>
        /// <param name="name">Configuration Name</param>
        /// <param name="version">Expected Configuration Version</param>
        /// <param name="root">XML Document node</param>
        private void ParseRootNode(string name, Version version, XmlElement root, string password)
        {
            Stack<AbstractConfigNode> nodeStack = new Stack<AbstractConfigNode>();
            configuration.RootConfigNode = new ConfigPathNode(configuration, null);
            configuration.RootConfigNode.Name = root.Name;
            nodeStack.Push(configuration.RootConfigNode);
            if (root.HasChildNodes)
            {
                foreach (XmlNode elem in root.ChildNodes)
                {
                    if (elem.Name == ConstXmlConfigHeader.XML_CONFIG_NODE_HEADER && elem.NodeType == XmlNodeType.Element)
                    {
                        ParseHeader(name, version, (XmlElement)elem, password);
                    }
                    else if (elem.NodeType == XmlNodeType.Element)
                    {
                        ParseBodyNode(elem.Name, (XmlElement)elem, nodeStack);
                    }
                }
            }
            nodeStack.Pop();
            if (nodeStack.Count > 0)
            {
                throw new ConfigurationException(String.Format("Error parsing configuration: Node stack is not empty. [name={0}]", name));
            }
        }

        /// <summary>
        /// Parse a configuration node.
        /// </summary>
        /// <param name="name">Config node name</param>
        /// <param name="elem">XML Element</param>
        /// <param name="nodeStack">Current Node Stack</param>
        private void ParseBodyNode(string name, XmlElement elem, Stack<AbstractConfigNode> nodeStack)
        {
            bool popStack = false;
            bool processed = false;
            AbstractConfigNode parent = nodeStack.Peek();
            if (IsTextNode(elem))
            {
                bool encrypted = false;
                if (elem.HasAttributes)
                {
                    string attr = elem.Attributes[XML_VALUE_ENCRYPTED].Value;
                    if (!String.IsNullOrWhiteSpace(attr))
                    {
                        if (attr.CompareTo("true") == 0)
                        {
                            encrypted = true;
                        }
                    }
                }
                AddValueNode(parent, elem.Name, elem.FirstChild.Value, encrypted);
            }
            else
            {
                XmlNodeType nt = IsListNode(elem);
                if (nt == XmlNodeType.Element)
                {
                    if (parent.GetType() != typeof(ConfigPathNode))
                    {
                        throw new ConfigurationException(String.Format("Invalid Stack State: Cannot add List node to parent. [parent={0}]", parent.GetType().FullName));
                    }
                    ConfigPathNode pnode = (ConfigPathNode)parent;

                    ConfigElementListNode nodeList = new ConfigElementListNode(parent.Configuration, parent);
                    nodeList.Name = name;
                    pnode.AddChildNode(nodeList);

                    nodeStack.Push(nodeList);
                    popStack = true;
                }
                else if (nt == XmlNodeType.Text)
                {
                    if (parent.GetType() != typeof(ConfigPathNode))
                    {
                        throw new ConfigurationException(String.Format("Invalid Stack State: Cannot add List node to parent. [parent={0}]", parent.GetType().FullName));
                    }
                    ConfigPathNode pnode = (ConfigPathNode)parent;

                    ConfigListValueNode nodeList = new ConfigListValueNode(parent.Configuration, parent);
                    nodeList.Name = name;
                    pnode.AddChildNode(nodeList);

                    nodeStack.Push(nodeList);
                    popStack = true;
                }
                else
                {
                    if (elem.Name == ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE)
                    {
                        if (parent.GetType() != typeof(ConfigPathNode))
                        {
                            throw new ConfigurationException(String.Format("Invalid Stack State: Cannot add List node to parent. [parent={0}]", parent.GetType().FullName));
                        }
                        ConfigPathNode pnode = (ConfigPathNode)parent;
                        AddIncludeNode(pnode, elem);
                        processed = true;
                    }
                    else if (elem.Name == ConstXmlResourceNode.XML_CONFIG_NODE_RESOURCE)
                    {
                        if (parent.GetType() != typeof(ConfigPathNode))
                        {
                            throw new ConfigurationException(String.Format("Invalid Stack State: Cannot add List node to parent. [parent={0}]", parent.GetType().FullName));
                        }
                        ConfigPathNode pnode = (ConfigPathNode)parent;
                        AddResourceNode(pnode, elem);
                        processed = true;
                    }
                    else if (elem.Name == settings.ParametersNodeName)
                    {
                        if (parent.GetType() != typeof(ConfigPathNode))
                        {
                            throw new ConfigurationException(String.Format("Invalid Stack State: Cannot add List node to parent. [parent={0}]", parent.GetType().FullName));
                        }
                        ConfigPathNode pnode = (ConfigPathNode)parent;
                        ConfigParametersNode paramNode = new ConfigParametersNode(parent.Configuration, parent);
                        paramNode.Name = name;
                        pnode.AddChildNode(paramNode);

                        nodeStack.Push(paramNode);
                        popStack = true;
                    }
                    else if (elem.Name == settings.PropertiesNodeName)
                    {
                        if (parent.GetType() != typeof(ConfigPathNode))
                        {
                            throw new ConfigurationException(String.Format("Invalid Stack State: Cannot add List node to parent. [parent={0}]", parent.GetType().FullName));
                        }
                        ConfigPathNode pnode = (ConfigPathNode)parent;
                        ConfigPropertiesNode propNode = new ConfigPropertiesNode(parent.Configuration, parent);
                        propNode.Name = name;
                        pnode.AddChildNode(propNode);

                        nodeStack.Push(propNode);
                        popStack = true;
                    }
                    else
                    {
                        ConfigPathNode cnode = new ConfigPathNode(parent.Configuration, parent);
                        cnode.Name = name;
                        if (parent.GetType() == typeof(ConfigPathNode))
                        {
                            ConfigPathNode pnode = (ConfigPathNode)parent;
                            pnode.AddChildNode(cnode);
                        }
                        else if (parent.GetType() == typeof(ConfigElementListNode))
                        {
                            ConfigElementListNode nodeList = (ConfigElementListNode)parent;
                            nodeList.Add(cnode);
                        }
                        else
                        {
                            throw new ConfigurationException(String.Format("Invalid Stack State: Cannot add path node to parent. [parent={0}]", parent.GetType().FullName));
                        }
                        nodeStack.Push(cnode);
                        popStack = true;
                    }
                }
                if (!processed)
                {
                    if (elem.HasAttributes)
                    {
                        AbstractConfigNode pp = nodeStack.Peek();
                        if (pp.GetType() == typeof(ConfigPathNode))
                        {
                            ConfigPathNode cp = (ConfigPathNode)pp;
                            ConfigAttributesNode attrs = new ConfigAttributesNode(cp.Configuration, cp);
                            cp.AddChildNode(attrs);
                            foreach (XmlAttribute attr in elem.Attributes)
                            {
                                ConfigValueNode vn = new ConfigValueNode(attrs.Configuration, attrs);
                                vn.Name = attr.Name;
                                vn.SetValue(attr.Value);

                                attrs.Add(vn);
                            }
                        }
                    }
                    if (elem.HasChildNodes)
                    {
                        foreach (XmlNode cnode in elem.ChildNodes)
                        {
                            if (cnode.NodeType == XmlNodeType.Element)
                            {
                                ParseBodyNode(cnode.Name, (XmlElement)cnode, nodeStack);
                            }
                        }
                    }
                }
                if (popStack)
                {
                    nodeStack.Pop();
                }
            }
        }

        /// <summary>
        /// Create a Resource node instance.
        /// </summary>
        /// <param name="parent">Parent Config node</param>
        /// <param name="elem">XML Element</param>
        private void AddResourceNode(ConfigPathNode parent, XmlElement elem)
        {
            string resourceName = elem.GetAttribute(ConstXmlResourceNode.XML_CONFIG_ATTR_RESOURCE_NAME);
            if (String.IsNullOrWhiteSpace(resourceName))
            {
                throw ConfigurationException.PropertyMissingException(ConstXmlResourceNode.XML_CONFIG_ATTR_RESOURCE_NAME);
            }
            string st = elem.GetAttribute(ConstXmlResourceNode.XML_CONFIG_ATTR_RESOURCE_TYPE);
            if (String.IsNullOrWhiteSpace(st))
            {
                throw ConfigurationException.PropertyMissingException(ConstXmlResourceNode.XML_CONFIG_ATTR_RESOURCE_TYPE);
            }
            EResourceType type = Enum.Parse<EResourceType>(st);
            if (type == EResourceType.NONE)
            {
                throw ConfigurationException.PropertyMissingException(ConstXmlResourceNode.XML_CONFIG_ATTR_RESOURCE_TYPE);
            }
            Uri uri = null;
            if (elem.HasChildNodes)
            {
                foreach (XmlNode nn in elem.ChildNodes)
                {
                    if (nn.NodeType == XmlNodeType.Element && nn.Name == ConstXmlResourceNode.XML_CONFIG_NODE_RESOURCE_URL)
                    {
                        string su = nn.InnerText;
                        if (String.IsNullOrWhiteSpace(su))
                        {
                            throw ConfigurationException.PropertyMissingException(ConstXmlResourceNode.XML_CONFIG_NODE_RESOURCE_URL);
                        }
                        uri = new Uri(su);
                        break;
                    }
                }
            }

            if (uri == null)
            {
                throw ConfigurationException.PropertyMissingException(ConstXmlResourceNode.XML_CONFIG_NODE_RESOURCE_URL);
            }
            ConfigResourceNode node = null;
            switch (type)
            {
                case EResourceType.FILE:
                    node = new ConfigResourceFile(configuration, parent);
                    break;
                case EResourceType.DIRECTORY:
                    node = new ConfigDirectoryResource(configuration, parent);
                    break;
                case EResourceType.ZIP:
                    node = new ConfigResourceZip(configuration, parent);
                    break;
            }
            node.Name = elem.Name;
            node.Type = type;
            node.Location = uri;
            node.ResourceName = resourceName;
            if (settings.DownloadOptions == EDownloadOptions.LoadRemoteResourcesOnStartup)
            {
                if (type == EResourceType.ZIP || type == EResourceType.FILE)
                {
                    ConfigResourceFile fnode = (ConfigResourceFile)node;
                    ConfigResourceHelper.DownloadResource(fnode);
                }
            }

            if (type == EResourceType.DIRECTORY)
            {
                ConfigDirectoryResource dnode = (ConfigDirectoryResource)node;
                if (!dnode.Location.IsFile)
                {
                    throw new ConfigurationException(String.Format("Invalid URI: Must be a local/mounted directory. [uri={0}]", dnode.Location.ToString()));
                }
                string dir = dnode.Location.LocalPath;
                DirectoryInfo di = new DirectoryInfo(dir);
                if (di.Exists)
                {
                    throw new ConfigurationException(String.Format("Invalid URI: Directory not found. [uri={0}]", dnode.Location.ToString()));
                }
                dnode.Downloaded = true;
                dnode.Directory = di;
            }
            parent.AddChildNode(node);
        }

        /// <summary>
        /// Parse a included configuration reference.
        /// </summary>
        /// <param name="parent">Parent Config Node</param>
        /// <param name="elem">XML Element</param>
        private void AddIncludeNode(ConfigPathNode parent, XmlElement elem)
        {
            string configName = elem.GetAttribute(ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE_NAME);
            if (String.IsNullOrWhiteSpace(configName))
            {
                throw ConfigurationException.PropertyMissingException(ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE_NAME);
            }
            string path = elem.GetAttribute(ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE_PATH);
            if (String.IsNullOrWhiteSpace(path))
            {
                throw ConfigurationException.PropertyMissingException(ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE_PATH);
            }
            string st = elem.GetAttribute(ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE_TYPE);
            if (String.IsNullOrWhiteSpace(st))
            {
                throw ConfigurationException.PropertyMissingException(ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE_TYPE);
            }
            EUriScheme type = EUriScheme.none.ParseScheme(st);
            if (type == EUriScheme.none)
            {
                throw ConfigurationException.PropertyMissingException(ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE_TYPE);
            }
            string vs = elem.GetAttribute(ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE_VERSION);
            if (String.IsNullOrWhiteSpace(vs))
            {
                throw ConfigurationException.PropertyMissingException(ConstXmlConfigIncludeNode.XML_CONFIG_INCLUDE_VERSION);
            }
            Version version = Version.Parse(vs);

            ConfigIncludeNode node = new ConfigIncludeNode(configuration, parent);
            node.Name = elem.Name;
            node.ConfigName = configName;
            node.Path = ReaderTypeHelper.ParseUri(path);
            node.ReaderType = type;
            node.Version = version;

            using (AbstractReader reader = ReaderTypeHelper.GetReader(type, node.Path))
            {
                reader.Open();
                XmlConfigParser parser = new XmlConfigParser();
                parser.Parse(node.ConfigName, reader, node.Version, settings);
                Configuration config = parser.GetConfiguration();
                node.Reference = config;
                node.Node = config.RootConfigNode;
                node.UpdateConfiguration(configuration);
            }
            parent.AddChildNode(node);
        }

        /// <summary>
        /// Add a configuration value node.
        /// </summary>
        /// <param name="parent">Parent Configuration node</param>
        /// <param name="name">Value node name</param>
        /// <param name="value">Value</param>
        private void AddValueNode(AbstractConfigNode parent, string name, string value, bool encrypted)
        {
            ConfigValueNode vn = new ConfigValueNode(parent.Configuration, parent);
            vn.Encrypted = encrypted;
            vn.Name = name;
            vn.SetValue(value);

            if (parent.GetType() == typeof(ConfigParametersNode))
            {
                ConfigParametersNode node = (ConfigParametersNode)parent;
                node.Add(vn);
            }
            else if (parent.GetType() == typeof(ConfigPropertiesNode))
            {
                ConfigPropertiesNode node = (ConfigPropertiesNode)parent;
                node.Add(vn);
            }
            else if (parent.GetType() == typeof(ConfigListValueNode))
            {
                ConfigListValueNode node = (ConfigListValueNode)parent;
                node.Add(vn);
            }
            else if (parent.GetType() == typeof(ConfigPathNode))
            {
                ConfigPathNode node = (ConfigPathNode)parent;
                node.AddChildNode(vn);
            }
            else
            {
                throw new ConfigurationException(String.Format("Invalid Stack State: Cannot add value node to parent. [parent={0}]", parent.GetType().FullName));
            }
        }

        /// <summary>
        /// Parse the configuration header information.
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <param name="version">Expected configuration version</param>
        /// <param name="elem">Header XML element</param>
        private void ParseHeader(string name, Version version, XmlElement elem, string passwrod)
        {
            if (elem.HasAttributes)
            {
                ConfigurationHeader header = new ConfigurationHeader();
                string attr = elem.GetAttribute(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_GROUP);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    header.ApplicationGroup = attr;
                }
                attr = elem.GetAttribute(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_ID);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    header.Id = attr;
                }
                attr = elem.GetAttribute(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_APP);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    header.Application = attr;
                }
                attr = elem.GetAttribute(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_NAME);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    header.Name = attr;
                }
                attr = elem.GetAttribute(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_VERSION);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    Version ver = Version.Parse(attr);
                    if (!ver.Equals(version))
                    {
                        throw new ConfigurationException(String.Format("Configuration Version mis-match: [expected={0}][actual={1}]", version.ToString(), ver.ToString()));
                    }
                    header.Version = ver;
                }
                if (name != header.Name)
                {
                    throw new ConfigurationException(String.Format("Invalid Configuration: Name mis-match. [expected={0}][actual={1}]", name, header.Name));
                }
                attr = elem.GetAttribute(ConstXmlConfigHeader.XML_CONFIG_HEADER_ATTR_HASH);
                if (!String.IsNullOrWhiteSpace(attr))
                {
                    header.EncryptionHash = attr;
                    if (String.IsNullOrWhiteSpace(passwrod))
                    {
                        throw new ConfigurationException(String.Format("Configuration has encryption, but no password specified. [name={0}]", header.Name));
                    }
                    if (!CryptoUtils.CompareHash(header.EncryptionHash, passwrod))
                    {
                        throw new ConfigurationException(String.Format("Password doesn't match the hash specified in the configuration. [name={0}]", header.Name));
                    }
                }
                if (elem.HasChildNodes)
                {
                    foreach (XmlNode node in elem.ChildNodes)
                    {
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            if (node.Name == ConstXmlConfigHeader.XML_CONFIG_HEADER_CREATED_BY)
                            {
                                ModifiedBy mb = ParseModifiedBy((XmlElement)node);
                                if (mb != null)
                                {
                                    header.CreatedBy = mb;
                                }
                            }
                            else if (node.Name == ConstXmlConfigHeader.XML_CONFIG_HEADER_UPDATED_BY)
                            {
                                ModifiedBy mb = ParseModifiedBy((XmlElement)node);
                                if (mb != null)
                                {
                                    header.ModifiedBy = mb;
                                }
                            }
                            else if (IsTextNode((XmlElement)node))
                            {
                                if (node.Name == ConstXmlConfigHeader.XML_CONFIG_HEADER_DESC)
                                {
                                    header.Description = node.InnerText;
                                }
                            }
                        }
                    }
                }
                header.Timestamp = DateTime.Now.Ticks;
                LogUtils.Debug(String.Format("Loaded Header: [name={0}]", header.Name), header);
                configuration.Header = header;
            }
            else
            {
                throw new ConfigurationException(String.Format("Error loading configuration header: No attributes defined. [name={0}]", name));
            }
        }

        /// <summary>
        /// Parse a modification info node.
        /// </summary>
        /// <param name="elem">XML Element</param>
        /// <returns>Modification info</returns>
        private ModifiedBy ParseModifiedBy(XmlElement elem)
        {
            if (elem.HasAttributes)
            {
                ModifiedBy mb = new ModifiedBy();
                XmlAttribute attr = elem.Attributes[ConstXmlConfigHeader.XML_CONFIG_HEADER_MB_ATTR_USER];
                if (attr != null)
                {
                    mb.User = attr.Value;
                }
                attr = elem.Attributes[ConstXmlConfigHeader.XML_CONFIG_HEADER_MB_ATTR_TIME];
                if (attr != null)
                {
                    string ts = attr.Value;
                    if (!String.IsNullOrWhiteSpace(ts))
                    {
                        mb.Timestamp = Int64.Parse(ts);
                    }
                }
                return mb;
            }
            return null;
        }

        /// <summary>
        /// Check if the specified XML Element is a Text Node.
        /// </summary>
        /// <param name="elem">XML Element</param>
        /// <returns>Is Text node?</returns>
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

        /// <summary>
        /// Check if the specified XML Element is a List node.
        /// </summary>
        /// <param name="elem">XML Element</param>
        /// <returns>List type or None</returns>
        private XmlNodeType IsListNode(XmlElement elem)
        {
            if (elem.HasChildNodes)
            {
                int count = 0;
                string name = null;
                XmlNodeType type = XmlNodeType.None;
                foreach (XmlNode node in elem.ChildNodes)
                {
                    if (node.NodeType != XmlNodeType.Element)
                    {
                        return XmlNodeType.None;
                    }
                    if (name == null)
                    {
                        name = node.Name;
                        type = node.NodeType;
                        if (IsTextNode((XmlElement)node))
                        {
                            type = XmlNodeType.Text;
                        }
                        else
                        {
                            type = XmlNodeType.Element;
                        }
                        count++;
                        continue;
                    }
                    XmlNodeType ntype = node.NodeType;
                    if (IsTextNode((XmlElement)node))
                    {
                        ntype = XmlNodeType.Text;
                    }
                    else
                    {
                        ntype = XmlNodeType.Element;
                    }
                    if (name != node.Name || type != ntype)
                    {
                        return XmlNodeType.None;
                    }
                    count++;
                }
                if (count > 1)
                {
                    return type;
                }
            }
            return XmlNodeType.None;
        }
    }
}
