using System;
using Xunit;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config.Readers;
using LibZConfig.Common.Config.Nodes;

namespace LibZConfig.Common.Config.Parsers
{
    public class Test_ZConfigCommonXMLParser
    {
        private const string CONFIG_BASIC_PROPS_FILE = @"..\..\..\Resources\XML\test-config.properties";
        private const string CONFIG_INCLUDE_PROPS_FILE = @"..\..\..\Resources\XML\test-config-include.properties";
        private const string CONFIG_PROP_NAME = "config.name";
        private const string CONFIG_PROP_FILENAME = "config.file";
        private const string CONFIG_PROP_VERSION = "config.version";

        [Fact]
        public void Parse()
        {
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
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void ParseInlcude()
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
                    parser.Parse(cname, reader, Version.Parse(version), null);
                }
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void Search()
        {
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

                Configuration configuration = null;
                using (FileReader reader = new FileReader(cfile))
                {
                    reader.Open();
                    XmlConfigParser parser = new XmlConfigParser();
                    ConfigurationSettings settings = new ConfigurationSettings();
                    settings.DownloadOptions = EDownloadOptions.LoadRemoteResourcesOnStartup;

                    parser.Parse(cname, reader, Version.Parse(version), settings);
                    configuration = parser.GetConfiguration();
                }
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
                Properties properties = new Properties();
                properties.Load(CONFIG_BASIC_PROPS_FILE);

                string cname = properties.GetProperty(CONFIG_PROP_NAME);
                Assert.False(String.IsNullOrWhiteSpace(cname));
                string cfile = properties.GetProperty(CONFIG_PROP_FILENAME);
                Assert.False(String.IsNullOrWhiteSpace(cfile));
                string version = properties.GetProperty(CONFIG_PROP_VERSION);
                Assert.False(String.IsNullOrWhiteSpace(version));

                LogUtils.Info(String.Format("Reading Configuration: [file={0}][version={1}]", cfile, version));

                Configuration configuration = null;
                using (FileReader reader = new FileReader(cfile))
                {
                    reader.Open();
                    XmlConfigParser parser = new XmlConfigParser();
                    ConfigurationSettings settings = new ConfigurationSettings();
                    settings.DownloadOptions = EDownloadOptions.LoadRemoteResourcesOnStartup;

                    parser.Parse(cname, reader, Version.Parse(version), settings);
                    configuration = parser.GetConfiguration();
                }
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
        public void SearchWildcar()
        {
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

                Configuration configuration = null;
                using (FileReader reader = new FileReader(cfile))
                {
                    reader.Open();
                    XmlConfigParser parser = new XmlConfigParser();
                    ConfigurationSettings settings = new ConfigurationSettings();
                    settings.DownloadOptions = EDownloadOptions.LoadRemoteResourcesOnStartup;

                    parser.Parse(cname, reader, Version.Parse(version), settings);
                    configuration = parser.GetConfiguration();
                }
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
                Assert.Equal(path, node.Name);
                LogUtils.Debug(node.GetAbsolutePath());

            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }
    }
}
