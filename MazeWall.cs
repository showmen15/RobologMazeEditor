/*************************************************************************
 *                                                                       *
 * This file is part of RoBOSS Simulation System,                        *
 * Copyright (C) 2004,2005 Dariusz Czyrnek, Wojciech Turek               *
 * All rights reserved.  Email: soofka@icslab.agh.edu.pl                 *
 *                                                                       *
 * RoBOSS Simulation System is free software; you can redistribute it    *
 * and/or modify it under the terms of The GNU General Public License    *
 * version 2.0 as published by the Free Software Foundation;             *
 * The text of the GNU General Public License is included with this      *
 * program in the file LICENSE.TXT.                                      *
 *                                                                       *
 * This program is distributed in the hope that it will be useful,       *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of        *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the file     *
 * LICENSE.TXT for more details.                                         *
 *                                                                       *
 *************************************************************************/
using System;
using System.Xml;
using System.Drawing;
using Geometry;

namespace MazeEditor
{
    public enum MazeWallType
    {
        wall, gate
    }


    public enum MazeGateType
    {
        door, passage, doorOneWayFromTo, doorOneWayToFrom
    }


    public class MazeWall : MazeIdentifiable
	{
 		public PointF [] points;
		private Color color;
		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				color = value;
			}
		}

        public Segment2D Segment
        {
            get { return new Segment2D(points[0], points[1]); }
        }


		private float width;
		public float Width
		{
			get
			{
				return width;
			}
			set
			{
				width = value;
			}
		}
		private float height;
		public float Height
		{
			get
			{
				return height;
			}
			set
			{
				height = value;
			}
		}

        public MazeWallType MazeWallType;
        public MazeGateType MazeDoorType;

        public MazeSpace RoomFrom, RoomTo;
        public double blocked = 0.0; 


        public Point2D Center { get { return new Point2D((points[1].X + points[0].X) /2, (points[1].Y + points[0].Y)/2); } } 


        public MazeWall(PointF point1, PointF point2, float width, float height, Color color) : base()
        {
            this.Width = width;
            this.Height = height;
            this.Color = color;
            points = new PointF[2];
            points[0] = point1;
            points[1] = point2;

            MazeWallType = MazeEditor.MazeWallType.wall;

        }


        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            MazeWall w = obj as MazeWall;
            if (w == null)
                return false;
            if (w.points[0].Equals(points[0]) && w.points[1].Equals(points[1]) ||
                w.points[0].Equals(points[1]) && w.points[1].Equals(points[0]))
                return true;

            return false;
        }



		public void WriteXMLDefinitionNode(XmlTextWriter writer)
		{
			
			writer.WriteStartElement("Geom");
			writer.WriteAttributeString("type","box");
			writer.WriteAttributeString("position_x",((this.points[0].X + this.points[1].X) / 200).ToString(MazeEditorForm.numberFormatInfo));
			writer.WriteAttributeString("position_y",((this.points[0].Y + this.points[1].Y) / 200).ToString(MazeEditorForm.numberFormatInfo));
			writer.WriteAttributeString("position_z",(this.height / 200).ToString(MazeEditorForm.numberFormatInfo));
			writer.WriteAttributeString("size_x",(Length/100).ToString(MazeEditorForm.numberFormatInfo));
			writer.WriteAttributeString("size_y",(this.width/100).ToString(MazeEditorForm.numberFormatInfo));
			writer.WriteAttributeString("size_z",(this.height/100).ToString(MazeEditorForm.numberFormatInfo));
			writer.WriteAttributeString("rotation_z",(Math.Atan((this.points[1].Y - this.points[0].Y)/(this.points[1].X - this.points[0].X))+Math.PI).ToString(MazeEditorForm.numberFormatInfo));
			writer.WriteAttributeString("color",color.ToArgb().ToString("x"));


			writer.WriteEndElement();
		}

		public static MazeWall BuildFromXmlNode(XmlNode wallNode)
		{
			double position_x = XmlHelper.GetDoubleAttributeFromNode(wallNode, "position_x");
			double position_y = XmlHelper.GetDoubleAttributeFromNode(wallNode, "position_y");
			double position_z = XmlHelper.GetDoubleAttributeFromNode(wallNode, "position_z");
			double size_x = XmlHelper.GetDoubleAttributeFromNode(wallNode, "size_x");
			double size_y = XmlHelper.GetDoubleAttributeFromNode(wallNode, "size_y");
			double size_z = XmlHelper.GetDoubleAttributeFromNode(wallNode, "size_z");
			double rotation_z = XmlHelper.GetDoubleAttributeFromNode(wallNode, "rotation_z");
			string color = XmlHelper.GetStringAttributeFromNode(wallNode, "color");

			if (size_x < size_y)
			{
				double smaller_size = size_x;
				size_x = size_y;
				size_y = smaller_size;
			}

			return new MazeWall(
				new PointF(	(float)( position_x - size_x * Math.Cos(rotation_z) * 0.50)*100, 
							(float)( position_y - size_x * Math.Sin(rotation_z) * 0.50)*100),
				new PointF(	(float)( position_x + size_x * Math.Cos(rotation_z) * 0.50)*100, 
							(float)( position_y + size_x * Math.Sin(rotation_z) * 0.50)*100),
				(float)size_y *100,
				(float)size_z *100,
				Color.FromArgb(Convert.ToInt32(color, 16)));
		}

		private float Length
		{
			get
			{
                return (float)Segment.Length;
 			}
		}

        public override string ToString()
        {
            return MazeWallType.ToString() + ID;// +" " + points[0].ToString() + "-" + points[1].ToString();
        }

    }
}
