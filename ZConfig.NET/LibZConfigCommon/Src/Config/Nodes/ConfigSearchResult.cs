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
    public class ConfigSearchResult : ConfigListNode<AbstractConfigNode>
    {
        public override AbstractConfigNode Find(string path)
        {
            path = ConfigUtils.CheckSearchPath(path, this);
            if (path == ".")
            {
                return this;
            }
            else if (path.StartsWith(ConfigurationSettings.NODE_SEARCH_SEPERATOR))
            {
                return Configuration.Find(path);
            }
            path = ConfigUtils.MaskSearchPath(path);
            string[] parts = path.Split(ConfigurationSettings.NODE_SEARCH_SEPERATOR);
            if (parts != null && parts.Length > 0)
            {
                List<string> pList = new List<string>();
                foreach (string part in parts)
                {
                    if (String.IsNullOrWhiteSpace(part))
                    {
                        continue;
                    }
                    string npart = ConfigUtils.UnmaskSearchPath(part);
                    pList.Add(npart);
                }
                ConfigUtils.CheckSearchRoot(pList, Name, Configuration.Settings);
                List<AbstractConfigNode> nodes = new List<AbstractConfigNode>();
                foreach (AbstractConfigNode node in GetValues())
                {
                    AbstractConfigNode sn = node.Find(pList, 0);
                    if (sn != null)
                    {
                        nodes.Add(sn);
                    }
                }
                if (nodes.Count > 0)
                {
                    if (nodes.Count == 1)
                    {
                        return nodes[0];
                    }
                    else
                    {
                        ConfigSearchResult result = new ConfigSearchResult();
                        result.Configuration = Configuration;
                        result.AddAll(nodes);
                        return result;
                    }
                }
            }
            return null;
        }

        public override AbstractConfigNode Find(List<string> path, int index)
        {
            throw new NotImplementedException();
        }

        public override void PostLoad()
        {
            throw new NotImplementedException();
        }

        public override void UpdateState(ENodeState state)
        {
            throw new NotImplementedException();
        }
    }
}
