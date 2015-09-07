using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    class LoadExternalOBJ : MonoBehaviour
    {
        bool MTLon = true; /* MTL used if true!Work with: I checked .jpeg and .png 
								if false it uses MaterialDefault 
	                           */
        public Material materialDefault; //optional
        public string objFileName = null;
        public Material MTLwithDminthan1; //optional

        void Start()
        {

            ObjReader reader = new ObjReader();
            reader.ConvertFile(objFileName, true);

        }



    }
}
