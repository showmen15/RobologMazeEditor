using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dijkstras
{
    class Graph
    {
        Dictionary<string, Dictionary<string, double>> vertices = new Dictionary<string, Dictionary<string, double>>();

        public void add_vertex(string name, string key_edges, double value_edges)
        {
            if (!vertices.ContainsKey(name))
                vertices.Add(name, new System.Collections.Generic.Dictionary<string, double>());

            vertices[name].Add(key_edges, value_edges);
        }

        public void add_vertex(string name, Dictionary<string, double> edges)
        {
            vertices[name] = edges;
        }

        public double SumShortestPathConst(string start, string finish)
        {
            List<string> tempPath = new List<string>();
            
            tempPath.AddRange(shortest_path(start, finish));
            tempPath.Add(start);
            double sumPath = 0.0;

            for (int i = 0; i < tempPath.Count() - 1; i++)
            {
                sumPath += getValueForEdge(tempPath[i], tempPath[i + 1]);
            }

            return sumPath;
        }

        public List<string> shortest_path(string start, string finish)
        {
            var previous = new Dictionary<string, string>();
            var distances = new Dictionary<string, double>();
            var nodes = new List<string>();

            List<string> path = null;

            foreach (var vertex in vertices)
            {
                if (vertex.Key == start)
                {
                    distances[vertex.Key] = 0;
                }
                else
                {
                    distances[vertex.Key] = double.MaxValue;
                }

                nodes.Add(vertex.Key);
            }

            while (nodes.Count != 0)
            {
                nodes.Sort(delegate(string x, string y)
                {
                    if (distances[x] == distances[y])
                        return 0;
                    else if (distances[x] > distances[y])
                        return 1;
                    else
                        return -1;
                });

                var smallest = nodes[0];
                nodes.Remove(smallest);

                if (smallest == finish)
                {
                    path = new List<string>();
                    while (previous.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = previous[smallest];
                    }

                    break;
                }

                if (distances[smallest] == double.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in vertices[smallest])
                {
                    var alt = distances[smallest] + neighbor.Value;
                    if (alt < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = alt;
                        previous[neighbor.Key] = smallest;
                    }
                }
            }

            return path;
        }

        private double getValueForEdge(string verticFrom, string verticTo)
        {
            return vertices[verticFrom][verticTo];
        }
    }

}
