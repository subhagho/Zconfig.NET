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
    public class Test_ZConfigXMLEncrypted
    {
        private const string CONFIG_BASIC_PROPS_FILE = @"../../../Resources/XML/test-config-encrypted.properties";
        private const string CONFIG_PROP_NAME = "config.name";
        private const string CONFIG_PROP_FILENAME = "config.file";
        private const string CONFIG_PROP_VERSION = "config.version";
        private const string CONFIG_PASSWORD = "21947a50-6755-47";

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

                    parser.Parse(cname, reader, Version.Parse(version), settings, CONFIG_PASSWORD);

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


        [Fact]
        public void Parse()
        {
            Configuration config = ReadConfiguration();
            Assert.NotNull(config);
        }

        [Fact]
        public void Search()
        {
            try
            {
                Configuration configuration = ReadConfiguration();

                Assert.NotNull(configuration);
                string path = "root/configuration/node_1/node_2/node_3/password";
                AbstractConfigNode node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(node is ConfigValueNode);
                string value = ((ConfigValueNode)node).GetDecryptedValue();
                Assert.Equal("wh0c@res", value);
                LogUtils.Debug(String.Format("[{0}:{1}]", node.GetSearchPath(), ((ConfigValueNode)node).GetValue()));
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
                string path = "root/configuration/node_1/node_2#PARAM_3";
                AbstractConfigNode node = configuration.Find(path);
                Assert.NotNull(node);
                Assert.True(node is ConfigValueNode);
                string value = ((ConfigValueNode)node).GetDecryptedValue();
                Assert.Equal("TEST-PARAM-3", value);
                LogUtils.Debug(String.Format("[{0}:{1}]", node.GetSearchPath(), ((ConfigValueNode)node).GetValue()));
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }
    }
}
