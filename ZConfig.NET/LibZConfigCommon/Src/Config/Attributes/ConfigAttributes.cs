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

namespace LibZConfig.Common.Config.Attributes
{
    /// <summary>
    /// Annotation to be used for auto-wiring configurations.
    /// Annotation specfies the search path for this type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public class ConfigPath : Attribute
    {
        /// <summary>
        /// Search Path to find the configuration node.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Is the search mandatory?
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ConfigPath()
        {
            Path = null;
            Required = true;
        }
    }

    /// <summary>
    /// Annotation to be used to define configuration mapping for auto-wired configuration elements.
    /// Annotation reads values from config value elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ConfigValue : Attribute
    {
        /// <summary>
        /// Name of the node, if null will use the Field/Property name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is the value mandatory?
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Transformer to handle the type conversion if required.
        /// </summary>
        public Type Function { get; set; }

        /// <summary>
        /// Search Path to find the configuration node.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ConfigValue()
        {
            Name = null;
            Required = true;
            Function = null;
            Path = null;
        }
    }

    /// <summary>
    /// Annotation to be used to define configuration mapping for auto-wired configuration elements.
    /// Annotation reads values from config attribute elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class ConfigAttribute : Attribute
    {
        /// <summary>
        /// Name of the node, if null will use the Field/Property name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is the value mandatory?
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Transformer to handle the type conversion if required.
        /// </summary>
        public Type Function { get; set; }

        /// <summary>
        /// Search Path to find the configuration node.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ConfigAttribute()
        {
            Name = null;
            Required = true;
            Function = null;
            Path = null;
        }
    }

    /// <summary>
    /// Annotation to be used to define configuration mapping for auto-wired configuration elements.
    /// Annotation reads values from config parameter elements.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, Inherited = true)]
    public class ConfigParam : Attribute
    {
        /// <summary>
        /// Name of the node, if null will use the Field/Property name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is the value mandatory?
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Transformer to handle the type conversion if required.
        /// </summary>
        public Type Function { get; set; }
        /// <summary>
        /// Search Path to find the configuration node.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public ConfigParam()
        {
            Name = null;
            Required = true;
            Function = null;
            Path = null;
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, Inherited = true)]
    public class MethodInvoke : Attribute
    {
        /// <summary>
        /// Search Path to find the configuration node.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public MethodInvoke()
        {
            Path = String.Empty;
        }
    }
}
