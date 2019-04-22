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
using System.Text;
using LibZConfig.Common.Utils;
using LibZConfig.Common.Config;
using LibZConfig.Common.Config.Readers;

namespace LibZConfig.Common.Config.Parsers
{
    public class JSONConfigParser : AbstractConfigParser
    {
        public override void Parse(string name, AbstractReader reader, Version version, ConfigurationSettings settings, string passwrod = null)
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
