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
using System.Diagnostics.Contracts;

namespace LibZConfig.Common.Utils
{
    public static class General
    {
        public static Dictionary<K, T> Clone<K, T>(Dictionary<K, T> map)
        {
            Dictionary<K, T> dict = new Dictionary<K, T>();
            if (map != null && map.Count > 0)
            {
                foreach(K key in map.Keys)
                {
                    dict.Add(key, map[key]);
                }
            }
            return dict;
        }

        public static string Capitalize(this string value)
        {
            Preconditions.CheckArgument(value);
            return String.Format("{0}{1}", Char.ToUpper(value[0]), value.Substring(1));
        }
    }
}
