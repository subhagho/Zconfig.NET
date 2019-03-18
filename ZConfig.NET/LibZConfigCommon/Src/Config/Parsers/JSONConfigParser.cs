using System;
using System.Collections.Generic;
using System.Text;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config;
using LibZConfig.Common.Config.Readers;

namespace LibZConfig.Common.Config.Parsers
{
    public class JSONConfigParser : AbstractConfigParser
    {
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
    }
}
