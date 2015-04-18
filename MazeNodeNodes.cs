using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeEditor
{
    public class MazeNodeNodes
    {
        public string NodeFromId { get; set; }
        public string NodeToId { get; set; }
        public double Cost { get; set; }
        public double Blocked { get; set; }

        public MazeNodeNodes(string sNodeFromId, string sNodeToId, double dCost, double dBlocked)
        {
            NodeFromId = sNodeFromId;
            NodeToId = sNodeToId;
            Cost = dCost;
            Blocked = dBlocked;
        }
    }
}
