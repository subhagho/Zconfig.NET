using System;
using Xunit;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config.Readers;
using LibZConfig.Common.Config.Parsers;

namespace LibZConfig.Common.Config.Parsers
{
    public class Test_ZConfigCommonXMLParser
    {
        private const string CONFIG_BASIC_PROPS_FILE = @"..\..\..\Resources\XML\test-config.properties";
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

                using(FileReader reader = new FileReader(cfile))
                {
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
    }
}
