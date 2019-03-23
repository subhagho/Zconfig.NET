using System;
using System.Collections.Generic;
using System.Text;

namespace LibZConfig.Common.Config.Nodes
{
    public class ConfigSearchResult : ConfigListNode<AbstractConfigNode>
    {
        public override AbstractConfigNode Find(string path)
        {
            string[] parts = path.Split('.');
            if (parts != null && parts.Length > 0)
            {
                List<string> stack = new List<string>(parts);
                List<AbstractConfigNode> nodes = new List<AbstractConfigNode>();
                foreach (AbstractConfigNode node in GetValues())
                {
                    AbstractConfigNode sn = node.Find(stack, 0);
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
