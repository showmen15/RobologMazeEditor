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
	
	public class MazeArc
	{
  
        public MazeNode from;
        public MazeNode to;

        public double Weight; 

        

        public MazeArc(MazeNode from, MazeNode to)
		{
            this.from = from;
            this.to = to;

            Weight = new Segment2D(from.position, to.position).Length;


		}

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;
            MazeArc w = obj as MazeArc;
            if (w == null)
                return false;
            if (w.from == this.from && w.to == this.to)
                return true;
            return false;
        }



	}
}
