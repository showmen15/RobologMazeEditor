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
	/// <summary>
	/// 
	/// </summary>
    public class MazeRobot : MazeIdentifiable
	{
         public readonly string Type;
		private string name;
		public PointF position;
		public readonly float Height;

		public MazeRobot(string type, string name, PointF position, float height) : base()
		{
			this.Height = height;
			this.name = name;
			this.position = position;
			this.Type = type;

        }


		public void WriteXMLDefinitionNode(XmlTextWriter writer)
		{
			
			writer.WriteStartElement("Robot");
			writer.WriteAttributeString("type",Type);
			writer.WriteAttributeString("name",name);
			writer.WriteAttributeString("position_x",((this.position.X) / 100).ToString(MazeEditorForm.numberFormatInfo));
			writer.WriteAttributeString("position_y",((this.position.Y) / 100).ToString(MazeEditorForm.numberFormatInfo));
			writer.WriteAttributeString("position_z",(this.Height / 100).ToString(MazeEditorForm.numberFormatInfo));

			writer.WriteEndElement();
		}

		public static MazeRobot BuildFromXmlNode(XmlNode robotNode)
		{
			double position_x = XmlHelper.GetDoubleAttributeFromNode(robotNode, "position_x");
			double position_y = XmlHelper.GetDoubleAttributeFromNode(robotNode, "position_y");
			double position_z = XmlHelper.GetDoubleAttributeFromNode(robotNode, "position_z");
			string type = XmlHelper.GetStringAttributeFromNode(robotNode, "type");
			string name = XmlHelper.GetStringAttributeFromNode(robotNode, "name");

			return new MazeRobot(type, name, new PointF((float)position_x*100,(float)position_y*100), (float)position_z*100);
		}

	}
}
