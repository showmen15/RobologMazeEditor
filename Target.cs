using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MazeEditor
{
    public class Target
    {
        public int X;
        public int Y;
        public bool TaskDone;

        public Target(double x, double y,bool isEnd)
        {
            X = Convert.ToInt32(x);
            Y = Convert.ToInt32(y);
            TaskDone = isEnd;
        }
    }
}
