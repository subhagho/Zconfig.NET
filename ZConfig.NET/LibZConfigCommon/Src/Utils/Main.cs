using System;
using System.Collections.Generic;
using System.Text;

namespace LibZConfig.Common.Utils
{
    public class Program
    {
        public static void Main(String[] args)
        {
            try
            {
                string value = "true";
                bool bv = (bool)ReflectionUtils.ConvertFromString<Boolean>(value);
            }
            catch (Exception e)
            {
                LogUtils.Error(e);
            }
        }
    }
}
