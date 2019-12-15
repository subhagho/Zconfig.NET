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
using LibZConfig.Common.Config;

namespace LibZConfig.Common.Config.Writers
{
    /// <summary>
    /// Abstract base class for defining configuration writers.
    ///Writers will serialize a configuration instance if to selected serialization format.
    /// </summary>
    public abstract class AbstractConfigWriter
    {
        /// <summary>
        /// Write this instance of the configuration to the specified output location.
        /// </summary>
        /// <param name="configuration">Configuration instance to write.</param>
        /// <param name="path">Output location to write to.</param>
        /// <returns>Return the path of the output file created.</returns>
        public abstract string Write(Configuration configuration, string path);

        /// <summary>
        /// Write this instance of the configuration to a string buffer.
        /// </summary>
        /// <param name="configuration">Configuration instance to write.</param>
        /// <returns>Return the string buffer</returns>
        public abstract string Write(Configuration configuration);
    }
}
