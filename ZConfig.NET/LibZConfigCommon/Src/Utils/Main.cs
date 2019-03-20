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
                string input = "This is a ${variable} match ${test}";
                LogUtils.Debug("INPUT>>" + input);
                if (!VariableRegexParser.HasVariable(input))
                {
                    throw new Exception("Match failed.");
                }
                List<string> vars = VariableRegexParser.GetVariables(input);
                foreach(string var in vars)
                {
                    LogUtils.Debug("VARIABLE:" + var);
                }
            }
            catch (Exception e)
            {
                LogUtils.Error(e);
            }
        }
    }
}
