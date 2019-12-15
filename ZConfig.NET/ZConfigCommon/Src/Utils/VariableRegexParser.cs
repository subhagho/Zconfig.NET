#region copyright
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2019
// Date: 2019-3-23
// Project: LibZConfigCommon
// Subho Ghosh (subho dot ghosh at outlook.com)
//
//
#endregion
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
