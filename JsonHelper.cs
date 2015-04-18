using System;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Drawing;
using Geometry;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MazeEditor
{
	/// <summary>
	/// Summary description for JsonHelper
	/// </summary>
	public class JsonHelper
	{
        public ArrayList mazeWalls = null;
        public ArrayList mazeRobots = null;
        public ArrayList mazeVictims = null;

        public ArrayList mazeSpaces = null;
        public MazeGraph mazeGraph = null;

        public string worldName;
        public int iWorldTimeout;

        public ArrayList mazeNodeNodes = null;
        public ArrayList mazeSpaceNode = null;
        public ArrayList mazeSpaceRobots = null;

        public void LoadAll(String filename) 
        {
            JToken tempJObject;
            StreamReader reader = File.OpenText(filename);
            JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));

            if (o.TryGetValue("name", out tempJObject))
                worldName =  o.GetValue("name").ToString();

            if (o.TryGetValue("timeout", out tempJObject))
                iWorldTimeout = (int)o.GetValue("timeout");

            mazeWalls = new ArrayList();
            Hashtable mazeWallsById = new Hashtable();
            mazeRobots = new ArrayList();
            mazeVictims = new ArrayList();

            mazeSpaces = new ArrayList();
            Hashtable mazeSpacesById = new Hashtable();
            mazeGraph = new MazeGraph();

            mazeNodeNodes = new ArrayList();
            mazeSpaceNode = new ArrayList();
            mazeSpaceRobots = new ArrayList();

            foreach (JObject jObject in o.GetValue("walls").Children())
            {
                MazeWall mazeWall = new MazeWall(
                    new Point2D((double)((JObject)jObject.GetValue("from")).GetValue("x") * 100, (double)((JObject)jObject.GetValue("from")).GetValue("y") * 100),
                    new Point2D((double)((JObject)jObject.GetValue("to")).GetValue("x") * 100, (double)((JObject)jObject.GetValue("to")).GetValue("y") * 100),
                    (float)jObject.GetValue("width") * 100,
                    (float)jObject.GetValue("height") * 100,
                    Color.FromArgb(Convert.ToInt32((string)jObject.GetValue("color"), 16)));
                if (mazeWall.Color.A == 0)
                    mazeWall.Color = Color.LightGray;
                mazeWall.ID = (string)jObject.GetValue("id");
                mazeWalls.Add(mazeWall);
                mazeWallsById.Add((string)jObject.GetValue("id"), mazeWall);
            }
            if (o.GetValue("gates") != null)
            {
                foreach (JObject jObject in o.GetValue("gates").Children())
                {
                    MazeWall mazeWall = new MazeWall(
                        new Point2D((double)((JObject)jObject.GetValue("from")).GetValue("x") * 100, (double)((JObject)jObject.GetValue("from")).GetValue("y") * 100),
                        new Point2D((double)((JObject)jObject.GetValue("to")).GetValue("x") * 100, (double)((JObject)jObject.GetValue("to")).GetValue("y") * 100),
                        0, 0, Color.Black);
                    mazeWall.MazeWallType = MazeWallType.gate;
                    string kind = (string)jObject.GetValue("kind");
                    mazeWall.MazeDoorType = (kind == "door" ? MazeGateType.door : (kind == "passage" ? MazeGateType.passage : MazeGateType.doorOneWayFromTo));      //doorOneWayFromTo will be changed later
                    mazeWall.blocked = (double)jObject.GetValue("blocked");
                    mazeWall.ID = (string)jObject.GetValue("id");
                    mazeWalls.Add(mazeWall);
                    mazeWallsById.Add((string)jObject.GetValue("id"), mazeWall);
                }
            }
            if (o.GetValue("robots") != null)
            {
                JToken tmp;
                Point2D? target;

                foreach (JObject jObject in o.GetValue("robots").Children())
                {
                    if (jObject.TryGetValue("target", out tmp))
                        target = new Point2D((double)((JObject)jObject.GetValue("target")).GetValue("x"), (double)((JObject)jObject.GetValue("target")).GetValue("y"));
                    else
                        target = null;

                    MazeRobot mazeRobot = new MazeRobot(
                        (string)jObject.GetValue("type"),
                        (string)jObject.GetValue("id"),
                        new Point2D((double)((JObject)jObject.GetValue("location")).GetValue("x") * 100, (double)((JObject)jObject.GetValue("location")).GetValue("y") * 100),
                        (float)((JObject)jObject.GetValue("location")).GetValue("z") * 100,
                        //new Point2D((double)((JObject)jObject.GetValue("target")).GetValue("x"),(double)((JObject)jObject.GetValue("target")).GetValue("y"))
                       target

                        );
                    mazeRobot.ID = (string)jObject.GetValue("id");
                    mazeRobots.Add(mazeRobot);
                }
            }
            if (o.GetValue("victims") != null)
            {
                foreach (JObject jObject in o.GetValue("victims").Children())
                {
                    MazeVictim mazeVictim = new MazeVictim(
                        new Point2D((double)((JObject)jObject.GetValue("position")).GetValue("x") * 100, (double)((JObject)jObject.GetValue("position")).GetValue("y") * 100));
                    mazeVictim.ID = (string)jObject.GetValue("id");
                    mazeVictims.Add(mazeVictim);
                }
            }



            Hashtable wallsBySpaceId = new Hashtable();
            if (o.GetValue("space-walls") != null)
            {
                foreach (JObject jObject in o.GetValue("space-walls").Children())
                {
                    string spaceId = (string)jObject.GetValue("spaceId");
                    string wallId = (string)jObject.GetValue("wallId");
                    if (wallsBySpaceId.ContainsKey(spaceId))
                    {
                        ((ArrayList)wallsBySpaceId[spaceId]).Add(mazeWallsById[wallId]);
                    }
                    else
                    {
                        ArrayList newArrayList = new ArrayList();
                        newArrayList.Add(mazeWallsById[wallId]);
                        wallsBySpaceId[spaceId] = newArrayList;
                    }
                }
            }
            if (o.GetValue("space-gates") != null)
            {
                foreach (JObject jObject in o.GetValue("space-gates").Children())
                {
                    string spaceId = (string)jObject.GetValue("spaceId");
                    string wallId = (string)jObject.GetValue("gateId");
                    if (wallsBySpaceId.ContainsKey(spaceId))
                    {
                        ((ArrayList)wallsBySpaceId[spaceId]).Add(mazeWallsById[wallId]);
                    }
                    else
                    {
                        ArrayList newArrayList = new ArrayList();
                        newArrayList.Add(mazeWallsById[wallId]);
                        wallsBySpaceId[spaceId] = newArrayList;
                    }
                }
            }

            if (o.GetValue("spaces") != null)
            {
                foreach (JObject jObject in o.GetValue("spaces").Children())
                {
                    MazeSpace newRoom = new MazeSpace((ArrayList)wallsBySpaceId[(string)jObject.GetValue("id")]);
                    newRoom.ID = (string)jObject.GetValue("id");
                    newRoom.MazeRoomType = (MazeSpaceType)System.Enum.Parse(typeof(MazeSpaceType), (string)jObject.GetValue("kind"));
                    newRoom.Function = (string)jObject.GetValue("function");
                    newRoom.Name = (string)jObject.GetValue("name");
                    newRoom.ExpectedPersonCount = (int)jObject.GetValue("expectedPersonCount");

                    if (jObject.TryGetValue("searched", out tempJObject))
                        newRoom.Searched = (int)jObject.GetValue("searched");
                    
                    mazeSpaces.Add(newRoom);
                    mazeSpacesById[newRoom.ID] = newRoom;

                    foreach (MazeWall roomWall in newRoom.Walls)
                    {
                        if (roomWall.RoomFrom == null)
                            roomWall.RoomFrom = newRoom;
                        else
                            roomWall.RoomTo = newRoom;
                    }
                }
            }

            ///////////////////////////  graph

            Hashtable spacesByNodeId = new Hashtable();
            if (o.GetValue("space-nodes") != null)
            {
                foreach (JObject jObject in o.GetValue("space-nodes").Children())
                {
                    spacesByNodeId[(string)jObject.GetValue("nodeId")] = mazeSpacesById[(string)jObject.GetValue("spaceId")];
                }
            }
            Hashtable gatesByNodeId = new Hashtable();
            if (o.GetValue("gate-nodes") != null)
            {
                foreach (JObject jObject in o.GetValue("gate-nodes").Children())
                {
                    gatesByNodeId[(string)jObject.GetValue("nodeId")] = mazeWallsById[(string)jObject.GetValue("gateId")];
                }
            }

            Hashtable nodesById = new Hashtable();
            if (o.GetValue("nodes") != null)
            {
                foreach (JObject jObject in o.GetValue("nodes").Children())
                {
                    MazeNode node = new MazeNode(new Point2D((double)((JObject)jObject.GetValue("position")).GetValue("x") * 100, (double)((JObject)jObject.GetValue("position")).GetValue("y") * 100),
                        (MazeSpace)spacesByNodeId[(string)jObject.GetValue("id")],
                        gatesByNodeId[(string)jObject.GetValue("id")] as MazeWall);
                    node.ID = (string)jObject.GetValue("id");
                    nodesById[node.ID] = node;
                    mazeGraph.AddNode(node);
                }
            }

            if (o.GetValue("node-nodes") != null)
            {
                foreach (JObject jObject in o.GetValue("node-nodes").Children())
                {
                    MazeNode fromNode = (MazeNode)nodesById[(string)jObject.GetValue("nodeFromId")];
                    MazeNode toNode = (MazeNode)nodesById[(string)jObject.GetValue("nodeToId")];

                    if (fromNode.Door != null && (fromNode.Door.MazeDoorType == MazeGateType.doorOneWayFromTo || fromNode.Door.MazeDoorType == MazeGateType.doorOneWayToFrom))
                    {
                        if (fromNode.Room == fromNode.Door.RoomFrom)
                            fromNode.Door.MazeDoorType = MazeGateType.doorOneWayFromTo;
                        else
                            fromNode.Door.MazeDoorType = MazeGateType.doorOneWayToFrom;
                    }
                    mazeGraph.AddArc(fromNode, toNode);

                    mazeNodeNodes.Add(new MazeNodeNodes(jObject.GetValue("nodeFromId").ToString(), jObject.GetValue("nodeToId").ToString(), double.Parse(jObject.GetValue("cost").ToString()), double.Parse(jObject.GetValue("blocked").ToString())));
                }
            }

            if (o.GetValue("space-nodes") != null)
            {
                foreach (JObject jObject in o.GetValue("space-nodes").Children())
                {
                    mazeSpaceNode.Add(new MazeSpaceNodes(jObject.GetValue("type").ToString(), 
                                                         jObject.GetValue("spaceId").ToString(), 
                                                         jObject.GetValue("nodeId").ToString()));
                }
            }

            if (o.GetValue("space-robots") != null)
            {
                foreach (JObject jObject in o.GetValue("space-robots").Children())
                {
                    mazeSpaceRobots.Add(new MazeSpaceRobots(jObject.GetValue("type").ToString(),
                                                         jObject.GetValue("spaceId").ToString(),
                                                         jObject.GetValue("robotId").ToString()));
                }
            }

            
            reader.Close();
        }

        public static void SaveAll(String filename, ArrayList mazeWalls, ArrayList mazeRobots, ArrayList mazeVictims, ArrayList mazeRooms, MazeGraph mazeGraph,string sWorldName,int iTimeout)
        {

            ArrayList jWalls = new ArrayList();
            ArrayList jGates = new ArrayList(); 
            


            foreach (MazeWall mazeWall in mazeWalls)
            {
                if (mazeWall.MazeWallType == MazeWallType.wall)
                {
                    jWalls.Add(new JObject(
                           new JProperty("type", "wall"),
                           new JProperty("id", mazeWall.ID),
                           new JProperty("width", mazeWall.Width / 100),
                           new JProperty("height", mazeWall.Height / 100),
                           new JProperty("color", mazeWall.Color.ToArgb().ToString("x")),
                           new JProperty("from", new JObject(new JProperty("x", mazeWall.points[0].X / 100), new JProperty("y", mazeWall.points[0].Y / 100))),
                           new JProperty("to", new JObject(new JProperty("x", mazeWall.points[1].X / 100), new JProperty("y", mazeWall.points[1].Y / 100)))));
                }
                else
                {
                     jGates.Add(new JObject(
                          new JProperty("type", "gate"),
                          new JProperty("id", mazeWall.ID),
                          new JProperty("kind", (mazeWall.MazeDoorType == MazeGateType.doorOneWayFromTo || mazeWall.MazeDoorType == MazeGateType.doorOneWayToFrom ? "doorOneWay" : mazeWall.MazeDoorType.ToString())),
                          new JProperty("blocked", mazeWall.blocked),
                          new JProperty("from", new JObject(new JProperty("x", mazeWall.points[0].X / 100), new JProperty("y", mazeWall.points[0].Y / 100))),
                          new JProperty("to", new JObject(new JProperty("x", mazeWall.points[1].X / 100), new JProperty("y", mazeWall.points[1].Y / 100)))));
                }
            }

            ArrayList jRobots = new ArrayList();
            foreach (MazeRobot mazeRobot in mazeRobots)
            {
                jRobots.Add(new JObject(
                           new JProperty("type", "robot"),
                           new JProperty("id", mazeRobot.ID),
                           new JProperty("kind", mazeRobot.Type),
                           new JProperty("location", new JObject(
                               new JProperty("x", mazeRobot.position.X / 100),
                               new JProperty("y", mazeRobot.position.Y / 100),
                               new JProperty("z", mazeRobot.Height / 100),
                               new JProperty("alpha", 0.0001))),
                           new JProperty("target", new JObject(
                               new JProperty("x", mazeRobot.target.X),
                               new JProperty("y", mazeRobot.target.Y)))                           
                              ));
            }

            ArrayList jVictims = new ArrayList();
            foreach (MazeVictim mazeVictim in mazeVictims)
            {
                jVictims.Add(new JObject(
                           new JProperty("type", "victim"),
                           new JProperty("id", mazeVictim.ID),
                           new JProperty("position", new JObject(new JProperty("x", mazeVictim.position.X / 100), new JProperty("y", mazeVictim.position.Y / 100)))));
            }

            ArrayList jSpaces = new ArrayList();
            ArrayList jrSpaceWalls = new ArrayList();
            ArrayList jrSpaceGates = new ArrayList();
            foreach (MazeSpace mazeRoom in mazeRooms )
            {
                jSpaces.Add(new JObject(
                           new JProperty("type", "space"),
                           new JProperty("id", mazeRoom.ID),
                           new JProperty("kind", mazeRoom.MazeRoomType.ToString()),
                           new JProperty("name", mazeRoom.Name),
                           new JProperty("function", mazeRoom.Function),
                           new JProperty("expectedPersonCount", mazeRoom.ExpectedPersonCount),
                           new JProperty("area", mazeRoom.Area),
                           new JProperty("diameter", mazeRoom.Diameter),
                           new JProperty("searched",mazeRoom.Searched)));

                foreach (MazeWall roomWall in mazeRoom.Walls)
                {
                    if (roomWall.MazeWallType == MazeWallType.wall)
                    {
                        jrSpaceWalls.Add(new JObject(
                             new JProperty("type", "space-wall"),
                             new JProperty("spaceId", mazeRoom.ID),
                             new JProperty("wallId", roomWall.ID)));
                    }
                    else
                    {
                        bool passable = true;
                        if (    roomWall.MazeDoorType == MazeGateType.doorOneWayFromTo && roomWall.RoomTo == mazeRoom
                            ||  roomWall.MazeDoorType == MazeGateType.doorOneWayToFrom && roomWall.RoomFrom == mazeRoom)
                            passable = false;

                        jrSpaceGates.Add(new JObject(
                              new JProperty("type", "space-gate"),
                             new JProperty("spaceId", mazeRoom.ID),
                             new JProperty("gateId", roomWall.ID),
                              new JProperty("passable", passable)));
                    }
                }
            }


            ArrayList jNodes = new ArrayList();
            ArrayList jrNodeNodes = new ArrayList();
            ArrayList jrGateNodes = new ArrayList();
            ArrayList jrSpaceNodes = new ArrayList();
            foreach (MazeNode node in mazeGraph.MazeGraphNodes)
            {
                jNodes.Add(new JObject(
                        new JProperty("type", "node"),
                        new JProperty("id", node.ID),
                        new JProperty("kind", (node.MazeGraphNodeType == MazeNodeType.SpaceNode ? "spaceNode" : "gateNode")),
                        new JProperty("position", new JObject(new JProperty("x", node.position.x / 100), new JProperty("y", node.position.y / 100)))));

                foreach (MazeArc arc in node.OutgoingGraphArcs )
                    jrNodeNodes.Add(new JObject(
                         new JProperty("type", "node-nodes"),
                         new JProperty("nodeFromId", node.ID),
                         new JProperty("nodeToId", arc.to.ID),
                         new JProperty("cost", arc.Weight / 100),
                         new JProperty("blocked", (node.Door != null && node.Door == arc.to.Door ? node.Door.blocked : 0.0)) ));


                if (node.Door != null)
                {
                    jrGateNodes.Add(new JObject(
                        new JProperty("type", "gate-node"),
                        new JProperty("nodeId", node.ID),
                        new JProperty("gateId", node.Door.ID)));
                }

                jrSpaceNodes.Add(new JObject(
                    new JProperty("type", "space-node"),
                    new JProperty("spaceId", node.Room.ID),
                    new JProperty("nodeId", node.ID)));
            }




            ArrayList jrSpaceRobots = new ArrayList();
            ArrayList jrSpaceVictims = new ArrayList();
            foreach (MazeSpace mazeRoom in mazeRooms)
            {
 
                foreach (MazeRobot mazeRobot in mazeRobots)
                {
                    if (mazeRoom.ContainsPoint(mazeRobot.position))
                    {
                        jrSpaceRobots.Add(new JObject(
                            new JProperty("type", "space-robot"),
                            new JProperty("spaceId", mazeRoom.ID),
                            new JProperty("robotId", mazeRobot.ID)));
                    }
                }
                foreach (MazeVictim mazeVictim in mazeVictims)
                {
                    if (mazeRoom.ContainsPoint(mazeVictim.position))
                    {
                        jrSpaceVictims.Add(new JObject(
                            new JProperty("type", "space-victim"),
                            new JProperty("spaceId", mazeRoom.ID),
                            new JProperty("robotId", mazeVictim.ID)));
                    }
                }
            }


            ///////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////
            ///////////////////////////////////////////////////////////////////



            JObject data = new JObject(
                new JProperty("name", sWorldName),
                new JProperty("timeout", iTimeout),
                new JProperty("walls", new JArray(jWalls)),
                new JProperty("gates", new JArray(jGates)),
                new JProperty("spaces", new JArray(jSpaces)),
                new JProperty("nodes", new JArray(jNodes)),
                new JProperty("robots", new JArray(jRobots)),
                new JProperty("victims", new JArray(jVictims)),
                new JProperty("space-walls", new JArray(jrSpaceWalls)),
                new JProperty("space-gates", new JArray(jrSpaceGates)),
                new JProperty("space-nodes", new JArray(jrSpaceNodes)),
                new JProperty("space-robots", new JArray(jrSpaceRobots)),
                new JProperty("space-victims", new JArray(jrSpaceVictims)),
                new JProperty("gate-nodes", new JArray(jrGateNodes)),
                new JProperty("node-nodes", new JArray(jrNodeNodes))
              );




            StreamWriter outfile = new StreamWriter(filename);
            outfile.Write(data.ToString());
            outfile.Close();

        }





    }
}
