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
using System.Collections;
using System.Xml;
using System.Drawing;
using Geometry;

namespace MazeEditor
{
    public enum MazeNodeType
    {
        SpaceNode, GateNode
    }





    public class MazeNode : MazeIdentifiable
	{
         private ArrayList outgoingGraphArcs = null;
        public ArrayList OutgoingGraphArcs
        {
            get { return outgoingGraphArcs; }
        }
        private ArrayList incommingGraphArcs = null;
        public ArrayList IncommingGraphArcs
        {
            get { return incommingGraphArcs; }
        }

        public MazeSpace Room;
        public MazeWall Door;


        public readonly MazeNodeType MazeGraphNodeType;

        public Point2D position;
        public bool blocked = false;


        public MazeNode(Point2D position, MazeSpace Room, MazeWall Door) : base()
		{
            this.position = position;
            this.Room = Room;
            this.Door = Door;
            if (this.Door != null)
                this.MazeGraphNodeType = MazeNodeType.GateNode;
            else
                this.MazeGraphNodeType = MazeNodeType.SpaceNode;

            incommingGraphArcs = new ArrayList();
            outgoingGraphArcs = new ArrayList();

		}

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            MazeNode w = obj as MazeNode;
            if (w == null)
                return false;
 //           if (w.ID == this.ID)
 //               return true;
            //            if (w.position == this.position)
            //               return true;

            return false;
        }

        public void addOutgoingGraphArc(MazeArc arc)
        {
            if (!outgoingGraphArcs.Contains(arc))
                outgoingGraphArcs.Add(arc);
        }

        public void addIncommingGraphArc(MazeArc arc)
        {
            if (!incommingGraphArcs.Contains(arc))
                incommingGraphArcs.Add(arc);
        }



	}
}
