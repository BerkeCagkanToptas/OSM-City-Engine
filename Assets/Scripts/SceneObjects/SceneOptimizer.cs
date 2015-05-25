using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.OpenStreetMap;
{
    class SceneOptimizer
    {

        List<Highway> highwayList;

        public SceneOptimizer(List<Highway> _highwayList)
        {
            highwayList = _highwayList;
        }

        
        public List<Highway> OptimizeRoadIntersections()
        {


            return highwayList;
        }


        private void EndtoEndIntersection(ref Highway hw1,ref Highway hw2)
        {
           Way w1 = hw1.way;
           Way w2 = hw2.way;

           if(w1.nodes[0].id == w2.nodes[0].id)
           {


           }
           else if(w1.nodes[0].id == w2.nodes[w2.nodes.Count-1].id)
           {


           }
           else if(w1.nodes[w1.nodes.Count-1].id == w2.nodes[0].id)
           {


           }
           else if(w1.nodes[w1.nodes.Count-1].id == w2.nodes[w2.nodes.Count-1].id)
           {


           }
           
           else 
               return;

        }





    }
}
