using System;
using System.Collections;
using System.Text;

namespace MazeEditor
{

 

    public class MazeIdentifiable
    {
        private static int counter = 0;
        private static ArrayList busyIds = new ArrayList();
        public static void ClearBusyIdsCache()
        {
            busyIds.Clear();
        }

        protected string id;
        public string ID
        {
            get { return id; }
            set 
            {
                if (!busyIds.Contains(value))
                {
                    //busyIds.Remove(id);
                    //if (busyIds.Contains(value))
                    //{
                    //    //throw new Exception("ID already taken");
                    //}
                    busyIds.Add(value);
                }

                id = value;
            }
        }

        public MazeIdentifiable()
        {
            while (busyIds.Contains(this.GetType().Name.Replace("Maze", "") + counter))
                counter++;
            
            ID = this.GetType().Name.Replace("Maze", "") + counter++;
        }
       
    
    }
}
