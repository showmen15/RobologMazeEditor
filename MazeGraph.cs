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
	
	public class MazeGraph
	{

        private ArrayList mazeGraphNodes = null;
        public ArrayList MazeGraphNodes
        {
            get{ return mazeGraphNodes; }
        }

        private ArrayList mazeGraphArcs = null;
        public ArrayList MazeGraphArcs
        {
            get { return mazeGraphArcs; }
        }

        public MazeGraph()
		{
            mazeGraphNodes = new ArrayList();
            mazeGraphArcs = new ArrayList();
		}

        public void AddNode(MazeNode node)
        {
            if (!mazeGraphNodes.Contains(node))
                mazeGraphNodes.Add(node);
        }

        public void AddArc(MazeNode from, MazeNode to)
        {
            MazeArc newArc = new MazeArc(from, to);
            if (!mazeGraphArcs.Contains(newArc))
            {
                mazeGraphArcs.Add(newArc);
                from.addOutgoingGraphArc(newArc);
                to.addIncommingGraphArc(newArc);
            }
        }

        public void RemoveArc(MazeArc arc)
        {
            mazeGraphArcs.Remove(arc);
            arc.from.IncommingGraphArcs.Remove(arc);
            arc.from.OutgoingGraphArcs.Remove(arc);
            arc.to.IncommingGraphArcs.Remove(arc);
            arc.to.OutgoingGraphArcs.Remove(arc);
        }

        public void RemoveNode(MazeNode node)
        {
            mazeGraphNodes.Remove(node);

            ArrayList arcsToRemove = new ArrayList();
            foreach (MazeArc arc in mazeGraphArcs)
                if (arc.from == node || arc.to == node)
                    arcsToRemove.Add(arc);
            foreach (MazeArc arc in arcsToRemove)
                this.RemoveArc(arc);

        }
    
    
    }
}
