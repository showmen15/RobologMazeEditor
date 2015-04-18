using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeEditor
{
    public class MazeSpaceRobots
    {
        public string Type { get; set; }
        public string SpaceId { get; set; }
        public string RobotId { get; set; }

        public MazeSpaceRobots(string sType, string sSpaceId, string sRobotId)
        {
            Type = sType;
            SpaceId = sSpaceId;
            RobotId = sRobotId;
        }
    }
}
