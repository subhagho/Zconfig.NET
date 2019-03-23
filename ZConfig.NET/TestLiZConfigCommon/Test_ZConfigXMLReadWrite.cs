using System;
using System.IO;
using Xunit;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config.Readers;
using LibZConfig.Common.Config.Nodes;
using LibZConfig.Common.Config.Parsers;
using LibZConfig.Common.Config.Writers;

namespace LibZConfig.Common.Config
{
    public class Test_ZConfigXMLReadWrite
    {
        private const string CONFIG_BASIC_PROPS_FILE = @"../../../Resources/XML/test-config.properties";
        private const string CONFIG_INCLUDE_PROPS_FILE = @"../../../Resources/XML/test-config-include.properties";
        private const string CONFIG_PROP_NAME = "config.name";
        private const string CONFIG_PROP_FILENAME = "config.file";
        private const string CONFIG_PROP_VERSION = "config.version";

        private static Configuration configuration = null;

        private Configuration ReadConfiguration()
        {
            if (configuration != null)
            {
                return configuration;
            }
            try
            {
                Properties properties = new Properties();
                properties.Load(CONFIG_BASIC_PROPS_FILE);

                string cname = properties.GetProperty(CONFIG_PROP_NAME);
                Assert.False(String.IsNullOrWhiteSpace(cname));
                string cfile = properties.GetProperty(CONFIG_PROP_FILENAME);
                Assert.False(String.IsNullOrWhiteSpace(cfile));
                string version = properties.GetProperty(CONFIG_PROP_VERSION);
                Assert.False(String.IsNullOrWhiteSpace(version));

                LogUtils.Info(String.Format("Reading Configuration: [file={0}][version={1}]", cfile, version));

                using (FileReader reader = new FileReader(cfile))
                {
                    reader.Open();
                    XmlConfigParser parser = new XmlConfigParser();
                    ConfigurationSettings settings = new ConfigurationSettings();
                    
                    settings.DownloadOptions = EDownloadOptions.LoadRemoteResourcesOnStartup;

                    parser.Parse(cname, reader, Version.Parse(version), settings);

                    configuration = parser.GetConfiguration();

                    return configuration;
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        private Configuration ReadIncludeConfiguration(bool replace)
        {
            try
            {
                Properties properties = new Properties();
                properties.Load(CONFIG_INCLUDE_PROPS_FILE);

                string cname = properties.GetProperty(CONFIG_PROP_NAME);
                Assert.False(String.IsNullOrWhiteSpace(cname));
                string cfile = properties.GetProperty(CONFIG_PROP_FILENAME);
                Assert.False(String.IsNullOrWhiteSpace(cfile));
                string version = properties.GetProperty(CONFIG_PROP_VERSION);
                Assert.False(String.IsNullOrWhiteSpace(version));

                LogUtils.Info(String.Format("Reading Configuration: [file={0}][version={1}]", cfile, version));

                using (FileReader reader = new FileReader(cfile))
                {
                    reader.Open();
                    XmlConfigParser parser = new XmlConfigParser();
                    ConfigurationSettings settings = new ConfigurationSettings();
                    settings.ReplaceProperties = replace;

                    parser.Parse(cname, reader, Version.Parse(version), settings);

                    return parser.GetConfiguration();
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void Parse()
        {
            Configuration config = ReadConfiguration();
            Assert.NotNull(config);
        }

        [Fact]
        public void ParseInlcude()
        {
            Configuration config = ReadIncludeConfiguration(true);
            Assert.NotNull(config);
        }

        [Fact]
        public void Search()
        {
            try
            {

                Configuration configuration = ReadConfiguration();

                Assert.NotNull(configuration);
                string path = "root.configuration.node_1";
                AbstractConfigNode node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.Equal(path, node.GetSearchPath());
                path = "VALUE_LIST";
                node = node.Find(path);
                Assert.True(node.GetType() == typeof(ConfigListValueNode));
                Assert.Equal(8, ((ConfigListValueNode)node).Count());
                Assert.Equal(path, node.Name);
                LogUtils.Debug(node.GetAbsolutePath());

            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void SearchParameters()
        {
            try
            {
                Configuration configuration = ReadConfiguration();

                Assert.NotNull(configuration);
                String path = "root.configuration.node_1#";
                AbstractConfigNode node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(node.GetType() == typeof(ConfigParametersNode));

                path = "#PARAM_1";
                node = configuration.Find(node, path);
                Assert.True(node.GetType() == typeof(ConfigValueNode));
                String param = ((ConfigValueNode)node).GetValue();
                Assert.False(String.IsNullOrEmpty(param));
                LogUtils.Debug(
                      String.Format("[path={0}] parameter value = {1}", path, param));

                path = "root.configuration.node_1.node_2#PARAM_1";
                node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(node.GetType() == typeof(ConfigValueNode));
                LogUtils.Debug("NODE>>", node);

            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void SearchAttributes()
        {
            try
            {
                Configuration configuration = ReadConfiguration();

                Assert.NotNull(configuration);
                String path = "root.configuration.node_1.node_2";
                AbstractConfigNode node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(node.GetType() == typeof(ConfigPathNode));

                path = "@ATTRIBUTE_2";
                node = configuration.Find(node, path);
                Assert.NotNull(node);
                Assert.True(node.GetType() == typeof(ConfigValueNode));
                String param = ((ConfigValueNode)node).GetValue();
                Assert.False(String.IsNullOrEmpty(param));
                LogUtils.Debug(
                      String.Format("[path={0}] attribute value = {1}", path, param));

                path = "root.configuration.node_1.node_2.node_3@ATTR_2";
                node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(node.GetType() == typeof(ConfigValueNode));

                path = "root.configuration.node_1.node_2.node_3@";
                node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(node.GetType() == typeof(ConfigAttributesNode));
                LogUtils.Debug("NODE>>", node);
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }


        [Fact]
        public void SearchWildcar()
        {
            try
            {
                Configuration configuration = ReadConfiguration();

                Assert.NotNull(configuration);
                string path = "root.configuration.node_1.node_2.node_3.*";
                AbstractConfigNode node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(node.GetType() == typeof(ConfigSearchResult));
                path = "configuration.node_1.node_2.node_3.*.LONG_VALUE_LIST";
                node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(node.GetType() == typeof(ConfigListValueNode));
                Assert.Equal(8, ((ConfigListValueNode)node).Count());
                LogUtils.Debug(node.GetAbsolutePath());

            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void SearchIndex()
        {
            try
            {
                Configuration configuration = ReadConfiguration();

                Assert.NotNull(configuration);
                string path = "root.configuration.node_1.ELEMENT_LIST";
                AbstractConfigNode node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.Equal(path, node.GetSearchPath());
                path = "ELEMENT_LIST%2.string_2";
                node = node.Find(path);
                Assert.NotNull(node);
                Assert.True(node.GetType() == typeof(ConfigValueNode));
                LogUtils.Debug(node.GetAbsolutePath());
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void ReadResource()
        {
            try
            {
                Configuration configuration = ReadConfiguration();

                Assert.NotNull(configuration);
                string path = "root.configuration.node_1.node_2.node_3.[data/LICENSE.txt]";
                AbstractConfigNode node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(typeof(ConfigResourceNode).IsAssignableFrom(node.GetType()));

                StreamReader reader = ConfigResourceHelper.GetResourceStream(configuration, path);
                Assert.NotNull(reader);
                reader.Dispose();
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void Write()
        {
            try
            {
                Configuration configuration = ReadConfiguration();

                Assert.NotNull(configuration);
                string path = FileUtils.GetTempDirectory();
                XmlConfigWriter writer = new XmlConfigWriter();
                string filename = writer.Write(configuration, path);

                FileInfo fi = new FileInfo(filename);
                if (!fi.Exists)
                {
                    throw new Exception(String.Format("Error getting created file: [file={0}]", fi.FullName));
                }
                LogUtils.Info(String.Format("Configuration written to file. [file={0}]", fi.FullName));

                Properties properties = new Properties();
                properties.Load(CONFIG_BASIC_PROPS_FILE);

                string cname = properties.GetProperty(CONFIG_PROP_NAME);
                Assert.False(String.IsNullOrWhiteSpace(cname));
                string version = properties.GetProperty(CONFIG_PROP_VERSION);
                Assert.False(String.IsNullOrWhiteSpace(version));

                LogUtils.Info(String.Format("Reading Configuration: [file={0}][version={1}]", filename, version));

                using (FileReader reader = new FileReader(filename))
                {
                    reader.Open();
                    XmlConfigParser parser = new XmlConfigParser();
                    ConfigurationSettings settings = new ConfigurationSettings();
                    settings.DownloadOptions = EDownloadOptions.LoadRemoteResourcesOnStartup;

                    parser.Parse(cname, reader, Version.Parse(version), settings);

                    Configuration nconfig = parser.GetConfiguration();

                    LogUtils.Debug("New Configuration:", nconfig);
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void WriteToString()
        {
            try
            {
                Configuration configuration = ReadIncludeConfiguration(false);

                Assert.NotNull(configuration);
                XmlConfigWriter writer = new XmlConfigWriter();
                string config = writer.Write(configuration);

                if (String.IsNullOrWhiteSpace(config))
                {
                    throw new Exception("Error getting configuration as String");
                }
                LogUtils.Info(String.Format("Configuration written to String: [config={0}]", config));
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }
    }
}
