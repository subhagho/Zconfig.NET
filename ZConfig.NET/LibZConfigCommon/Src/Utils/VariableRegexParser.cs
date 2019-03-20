using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LibZConfig.Common.Utils
{
    /// <summary>
    /// Utility method to extract variable replacements from strings.
    /// </summary>
    public static class VariableRegexParser
    {
        private const string VAR_REGEX = "\\$\\{(.*?)\\}";

        /// <summary>
        /// Check if the string has variables.
        /// </summary>
        /// <param name="input">Input String</param>
        /// <returns>Has Variables?</returns>
        public static bool HasVariable(string input)
        {
            return Regex.IsMatch(input, VAR_REGEX);
        }

        /// <summary>
        /// Get the list of variables in the input string.
        /// </summary>
        /// <param name="input">Input String</param>
        /// <returns>List of Variables</returns>
        public static List<string> GetVariables(string input)
        {
            if (!HasVariable(input))
                return null;
            List<string> vars = new List<string>();
            MatchCollection mc = Regex.Matches(input, VAR_REGEX);
            if (mc != null && mc.Count > 0)
            {
                foreach (Match m in mc)
                {
                    string var = m.Groups[1].Value;
                    if (!String.IsNullOrWhiteSpace(var))
                    {
                        vars.Add(var);
                    }
                }
            }
            if (vars.Count > 0)
                return vars;
            return null;
        }
    }
}
