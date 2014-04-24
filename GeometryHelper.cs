using System;
using System.Collections.Generic;
using System.Text;
using Geometry;

namespace MazeEditor
{
    class GeometryHelper
    {
        public static Point2D GetClosestPointOnSegment(Segment2D segment, Point2D point, out double distance)
        {

           double bot;
            int i;
            double t;  //the relative position of the point Pn to the SEGMENT

            //
            //  If the line segment is actually a point, then the answer is easy.
            //
            if (segment.Length == 0)
            {
                t = 0.0;
            }
            else
            {
                bot = Math.Pow(segment.x2 - segment.x1, 2) + Math.Pow(segment.y2 - segment.y1, 2);

                t = (point.x - segment.x1) * (segment.x2 - segment.x1) + (point.y - segment.y1) * (segment.y2 - segment.y1);

                t = t / bot;
                t = Math.Min( Math.Max(t, 0.0), 1.0 );
            }

            Point2D closestPoint;
            closestPoint.x = segment.x1 + t * (segment.x2 - segment.x1);
            closestPoint.y = segment.y1 + t * (segment.y2 - segment.y1);

            distance = point.GetDistance(closestPoint);

            return closestPoint;
        }



    }
}
