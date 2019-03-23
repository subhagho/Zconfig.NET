using System;
using Xunit;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config.Readers;
using LibZConfig.Common.Config.Nodes;
using LibZConfig.Common.Config.Parsers;

namespace LibZConfig.Common.Config
{
    public class Test_ZConfigAnnotationProcessor
    {
        private const string CONFIG_BASIC_PROPS_FILE = @"../../../Resources/XML/test-config.properties";
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

        [Fact]
        public void Annotate()
        {
            try
            {
                Entity entity = new Entity();
                ConfigurationAnnotationProcessor.Process(ReadConfiguration(), entity);
                Assert.False(String.IsNullOrWhiteSpace(entity.Attr_3));
                Assert.False(entity.CreateTime == default(DateTime));
                Assert.True(entity.DoubleValue > 0);
                Assert.NotEmpty(entity.GetLongValues());
                LogUtils.Debug("Entity:", entity);
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }

        [Fact]
        public void AnnotateInstance()
        {
            try
            {
                Entity entity = ConfigurationAnnotationProcessor.Process<Entity>(ReadConfiguration());
                Assert.False(String.IsNullOrWhiteSpace(entity.Attr_3));
                Assert.False(entity.CreateTime == default(DateTime));
                Assert.True(entity.DoubleValue > 0);
                Assert.NotEmpty(entity.GetLongValues());
                LogUtils.Debug("Entity:", entity);
            }
            catch (Exception ex)
            {
                LogUtils.Error(ex);
                throw ex;
            }
        }
    }
}