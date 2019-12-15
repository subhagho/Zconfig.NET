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

namespace LibZConfig.Common.Config
{
    /// <summary>
    /// Class to define configuration version.
    /// </summary>
    public class Version
    {
        /// <summary>
        /// Major Version number
        /// </summary>
        public int MajorVersion { get; set; }
        /// <summary>
        /// Minor Version number
        /// </summary>
        public int MinorVersion { get; set; }

        /// <summary>
        /// Default Empty Constructor
        /// </summary>
        public Version()
        {
            MajorVersion = -1;
            MinorVersion = -1;
        }

        /// <summary>
        /// Constructor with Version data.
        /// </summary>
        /// <param name="majorVersion">Major Version number</param>
        /// <param name="minorVersion">Minor Version number</param>
        public Version(int majorVersion, int minorVersion)
        {
            this.MajorVersion = majorVersion;
            this.MinorVersion = minorVersion;
        }

        /// <summary>
        /// Check if the target version is compatible.
        /// Default compatibility is defined by Major version being equal.
        /// </summary>
        /// <param name="target">Target Version</param>
        /// <returns>Is Compatible?</returns>
        public bool IsCompatible(Version target)
        {
            if (target != null && MajorVersion == target.MajorVersion)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Override default ToString
        /// </summary>
        /// <returns>Version String</returns>
        public override string ToString()
        {
            return String.Format("{0}.{1}", MajorVersion, MinorVersion);
        }

        /// <summary>
        /// Override default Equals: Checks compatibility and then minor version 
        /// or if minor version is Match All.
        /// </summary>
        /// <param name="obj">Target object to compare</param>
        /// <returns>Is Equal?</returns>
        public override bool Equals(object obj)
        {
            if (obj != null && typeof(Version).IsAssignableFrom(obj.GetType()))
            {
                Version version = (Version)obj;
                if (IsCompatible(version))
                {
                    if (MinorVersion == version.MinorVersion || version.MinorVersion == Int32.MinValue || MinorVersion == Int32.MinValue)
                        return true;
                }
            }
            return base.Equals(obj);
        }

        /// <summary>
        /// Override default Hash Code.
        /// </summary>
        /// <returns>Hash Code</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(MajorVersion, MinorVersion);
        }

        /// <summary>
        /// Parse the Version string to a Version instance.
        /// </summary>
        /// <param name="value">Input String</param>
        /// <returns>Version Instance</returns>
        public static Version Parse(string value)
        {
            if (!String.IsNullOrWhiteSpace(value))
            {
                string[] parts = value.Split('.');
                if (parts != null && parts.Length == 2)
                {
                    string maj = parts[0];
                    string min = parts[1];
                    Version version = new Version();
                    version.MajorVersion = Int32.Parse(maj);
                    if (min.Trim() == "*")
                    {
                        version.MinorVersion = Int32.MinValue;
                    }
                    else
                    {
                        version.MinorVersion = Int32.Parse(min);
                    }
                    return version;
                }
            }
            return null;
        }
    }
}
