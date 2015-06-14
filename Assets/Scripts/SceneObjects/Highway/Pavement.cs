using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.OpenStreetMap;
using Assets.Scripts.ConfigHandler;

namespace Assets.Scripts.SceneObjects
{
    class Pavement
    {
        public List<Vector3> leftSideVertexes;
        public List<Vector3> rightSideVertexes;


        public Pavement(Highway highway)
        {
            if(highway.hasLeftSidewalk)
            {



            }
            if(highway.hasRightSideWalk)
            {


            }




        }




    }
}
