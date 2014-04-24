
using System;
using System.Collections;
using System.Xml;
using System.Drawing;
using Geometry;

namespace MazeEditor
{
    public enum MazeSpaceType
    {
        room, staricase, lift, junction, corridor, hall
    }



    public class MazeSpace : MazeIdentifiable
	{

		public ArrayList Walls;

        protected Polygon2D roomPolygon;
        public Polygon2D RoomPolygon
        {
            get
            {
                return roomPolygon;
            }
        }


        public MazeSpaceType MazeRoomType = MazeSpaceType.room;

        public ArrayList BorderNodes;
        public MazeNode CetralNode;
        public string Function;
        public int ExpectedPersonCount;


        protected static Random random = new Random();
        public string Name;
        public MazeSpace(ArrayList walls) : base ()
		{
            this.Walls = (ArrayList)walls.Clone();
            BorderNodes = new ArrayList();


            BuildPolygon();

            ExpectedPersonCount = (int)(Area * random.NextDouble());

            int doorCount = 0;
            foreach (MazeWall wall in Walls)
                if (wall.MazeWallType == MazeWallType.gate)
                    doorCount++;

            if (doorCount > 2)
            {
                MazeRoomType = MazeEditor.MazeSpaceType.corridor;
                ExpectedPersonCount = 0;
            }

		}

        /// <summary>
        ///
        /// </summary>
        private void BuildPolygon()
        {
            Point2D[] points = new Point2D[Walls.Count];
            points[0] =  (Walls[0] as MazeWall).points[0];

            ArrayList wallsCopy = (ArrayList) Walls.Clone();
            wallsCopy.RemoveAt(0);

            for (int i = 1; i < points.Length; i++)
            {
                //find adjacent wall... the closest
                foreach (MazeWall wall in wallsCopy)
                {
                    if (Point2D.GetDistance(wall.points[0], points[i - 1]) < 0.01)
                    {
                        points[i] = wall.points[1];
                        wallsCopy.Remove(wall);
                        break;
                    }
                    else if (Point2D.GetDistance(wall.points[1], points[i - 1]) < 0.01)
                    {
                        points[i] = wall.points[0];
                        wallsCopy.Remove(wall);
                        break;
                    }
                }
            }
            roomPolygon = new Polygon2D(points);
        }
        /*
                        //find adjacent wall... the closest
                double minDistance = Double.MaxValue;
                Point2D pointToAdd = new Point2D(0, 0);
                MazeWall wallToAdd = null;
                foreach (MazeWall wall in wallsCopy)
                {
                    if (Point2D.GetDistance(wall.points[0], points[i - 1]) < minDistance)
                    {
                        minDistance = Point2D.GetDistance(wall.points[0], points[i - 1]);
                        pointToAdd = wall.points[1];
                        wallToAdd = wall; 
                    }
                    else if (Point2D.GetDistance(wall.points[1], points[i - 1]) < minDistance)
                    {
                        minDistance = Point2D.GetDistance(wall.points[1], points[i - 1]);
                        pointToAdd = wall.points[0];
                        wallToAdd = wall; 
                    }
                }
                if (wallToAdd != null)
                {
                    points[i] = pointToAdd;
                    wallsCopy.Remove(wallToAdd);
                }
        */
        public Point2D CenterPoint
        {
            get { return RoomPolygon.GetCentroid(); }
        }

        public Point2D[] Points
        {
            get { return RoomPolygon.points; }
        }


        public override bool Equals(object obj)
        {
            MazeSpace w = obj as MazeSpace;
            if (w == null)
                return false;
            foreach (MazeWall wall in w.Walls)
                if (!Walls.Contains(wall))
                    return false;

            return true;
        }



        public bool ContainsPoint(Point2D p)
        {
            int i;
            bool value;
            double x1;
            double x2;
            double y1;
            double y2;

            value = false;

            for (i = 0; i <  RoomPolygon.points.Length; i++)
            {
                x1 = RoomPolygon.points[i].x;
                y1 = RoomPolygon.points[i].y;

                if (i <  RoomPolygon.points.Length - 1)
                {
                    x2 = RoomPolygon.points[i + 1].x; 
                    y2 = RoomPolygon.points[i + 1].y; 
                }
                else
                {
                    x2 = RoomPolygon.points[0].x;
                    y2 = RoomPolygon.points[0].y; 
                }

                if ((y1 < p.y && p.y <= y2) ||
                     (p.y <= y1 && y2 < p.y))
                {
                    if ((p.x - x1) - (p.y - y1) * (x2 - x1) / (y2 - y1) < 0)
                    {
                        value = !value;
                    }
                }
            }

            return value;

        }

        public double Diameter
        {
            get
            {

                double diameter;
                int i;
                int j;

                diameter = 0.0;

                for (i = 0; i < RoomPolygon.points.Length; i++)
                {
                    for (j = i + 1; j < RoomPolygon.points.Length; j++)
                    {
                        diameter = Math.Max(diameter,
                          Math.Sqrt((RoomPolygon.points[i].x - RoomPolygon.points[j].x) * (RoomPolygon.points[i].x - RoomPolygon.points[j].x)
                               + (RoomPolygon.points[i].y - RoomPolygon.points[j].y) * (RoomPolygon.points[i].y - RoomPolygon.points[j].y)));
                    }
                }

                return diameter/100;
            }
        }

        public double Area
        {
            get {

                double area;
                int i;
                int im1;
                int ip1;

                area = 0.0;

                for (i = 0; i < RoomPolygon.points.Length; i++)
                {
                    im1 = i - 1;
                    ip1 = i + 1;

                    if (im1 < 0)
                    {
                        im1 = RoomPolygon.points.Length -1;
                    }

                    if (RoomPolygon.points.Length <= ip1)
                    {
                        ip1 = 0;
                    }

                    area = area + RoomPolygon.points[i].x * (RoomPolygon.points[ip1].y - RoomPolygon.points[im1].y);
                }

                area = 0.5 * area;

                return Math.Abs(area/10000);

            
            }
        }

    }
}
