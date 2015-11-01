using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    class LoadExternalOBJ
    {

        ObjReader reader;
        Material Standard;
        Material Transparent;

        public LoadExternalOBJ()
        {
            reader = new ObjReader();
            reader.maxPoints = 60000;
            reader.combineMultipleGroups = false;
            reader.useFileNameAsObjectName = true;
            reader.computeTangents = true;
            reader.useSuppliedNormals = true;
            reader.useMTLFallback = true;

            Standard = new Material((Material)Resources.Load("Materials/ExternalObject/Standard"));
            Transparent = new Material((Material)Resources.Load("Materials/ExternalObject/Transparent"));
        }

        public GameObject loadOBJ(string path)
        {
            GameObject toRet = new GameObject();
            GameObject[] objects = reader.ConvertFile(path,true,Standard,Transparent);
            foreach (GameObject go in objects)
            {
                go.AddComponent(typeof(MeshCollider));
                MeshCollider collider = go.GetComponent<MeshCollider>();
                collider.sharedMesh = go.GetComponent<MeshFilter>().mesh;
                go.transform.SetParent(toRet.transform);
            }
            toRet.name = objects[0].name;
            return toRet;
        }
      
    }
}
