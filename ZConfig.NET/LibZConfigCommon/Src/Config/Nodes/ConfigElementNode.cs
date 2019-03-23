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

namespace LibZConfig.Common.Config.Nodes
{
    /// <summary>
    /// Element nodes represent path nodes in the configuration.
    /// </summary>
    public abstract class ConfigElementNode : AbstractConfigNode
    {
        /// <summary>
        /// Default Empty constructor
        /// </summary>
        protected ConfigElementNode() : base()
        {

        }

        /// <summary>
        /// Constructor with configuration instance and parent node.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="parent">Parent node.</param>
        protected ConfigElementNode(Configuration configuration, AbstractConfigNode parent) : base(configuration, parent)
        {

        }

        /// <summary>
        /// Find the node relative to this node instance for
        /// the specified search path.
        /// </summary>
        /// <param name="path">Search path relative to this node.</param>
        /// <returns>Config node instance</returns>
        public override AbstractConfigNode Find(string path)
        {
            return base.Find(path);
        }
        
    }
}
