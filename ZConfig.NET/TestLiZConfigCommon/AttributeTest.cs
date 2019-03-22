using System;
using System.Collections.Generic;
using LibZConfig.Common.Config.Attributes;
using LibZConfig.Common.Utils;

namespace LibZConfig.Common.Config
{
    [ConfigPath(Path ="root.configuration.node_1")]
    public class Entity
    {
        public string Param_1 { get; }
        [ConfigValue(Name = "timestamp", Path = "createdBy", Function = typeof(StringToDateTransformer))]
        public DateTime CreateTime { get; set; }
        [ConfigValue(Name = "LONG_VALUE_LIST", Path = "node_2.node_3.node_4")]
        private HashSet<long> longValues;
        [ConfigAttribute(Name = "ATTR_3", Path ="node_2.node_3")]
        public string Attr_3 { get; set; }
        public double DoubleValue { get; set; }

        public Entity()
        {

        }

        [MethodInvoke()]
        public Entity([ConfigParam(Name = "PARAM_1")]string param_1)
        {
            Param_1 = param_1;
        }

        public void SetLongValues(HashSet<long> values)
        {
            longValues = values;
        }

        public HashSet<long> GetLongValues()
        {
            return longValues;
        }

        [MethodInvoke(Path = "node_2.node_3.node_4")]
        public void SetDoubleValue([ConfigParam(Path = "node_4", Name = "PARAM_3")]double value)
        {
            DoubleValue = value;
            LogUtils.Debug(String.Format("Invoked SetDoubleValue: Value = {0}", DoubleValue));
        }
    }
}