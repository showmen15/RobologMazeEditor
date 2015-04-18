using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeEditor
{
    public class MazeSpaceNodesArea 
    {
        public double Area { get; set; }
        public string SpaceId { get; set; }
        public string NodeId { get; set; }
        public int Index { get; set; }
        public bool Searched { get; set; }

        public MazeSpaceNodesArea(string sSpaceId, string sNodeId, double dArea,int iIndex) 
        {
            SpaceId = sSpaceId;
            NodeId = sNodeId;
            Area = dArea;
            Index = iIndex;
            Searched = false;
        }
    }
}
