using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeEditor
{
    public class MazeSpaceNodes
    {
        public string Type { get; set; }
        public string SpaceId { get; set; }
        public string NodeId { get; set; }

        public MazeSpaceNodes()
        {
        }

        public MazeSpaceNodes(string sType, string sSpaceId, string sNodeId)
        {
            Type = sType;
            SpaceId = sSpaceId;
            NodeId = sNodeId;
        }
    }
}
