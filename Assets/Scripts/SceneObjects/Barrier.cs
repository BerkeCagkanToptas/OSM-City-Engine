using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.OpenStreetMap;
using UnityEngine;
using Assets.Scripts.UnitySideScripts.MouseScripts;
using Assets.Scripts.Utils;

namespace Assets.Scripts.SceneObjects
{
    public enum BarrierType
    {
        fence,
        wall,
        cityWall,
        cityGate,
        gate,
        retaining_wall,
        none
    }


    public class Barrier
    {
        public string id;
        public BarrierType type;
        public GameObject BarrierContainer;

        BarrierConfigurations barrierConfig;  
        List<GameObject> BarrierElements;
        Way barrierWay;

        public float height;
        public float thickness;
        public string texturePath;
        public Material barrierMaterial;


        public Barrier(Way w, List<BarrierConfigurations> config)
        {
            id = w.id;
            type = getBarrierType(w.tags);
            barrierConfig = getBarrierConfiguration(config,type);

            height = barrierConfig.height;
            thickness = barrierConfig.width;
            barrierMaterial = (Material)Resources.Load(barrierConfig.Path);

            BarrierContainer = new GameObject("Barrier" + w.id);
            BarrierContainer.transform.position = new Vector3(0, 0, 0);
            BarrierElements = new List<GameObject>();
            barrierWay = w;
            generateBarrier(w);
      
        }

        private BarrierType getBarrierType(List<Tag> tags)
        {

            for(int i = 0 ; i < tags.Count ; i++)
            {
                if (tags[i].k == "historic" && tags[i].v == "citywalls")
                    return BarrierType.cityWall;
                else if (tags[i].k == "historic" && tags[i].v == "city_gate")
                    return BarrierType.cityGate;
                else if (tags[i].k == "barrier" && tags[i].v == "gate")
                    return BarrierType.gate;
                else if (tags[i].k == "barrier" && tags[i].v == "wall")
                    return BarrierType.wall;
                else if (tags[i].k == "barrier" && tags[i].v == "fence")
                    return BarrierType.fence;
                else if (tags[i].k == "barrier" && tags[i].v == "retaining_wall")
                    return BarrierType.retaining_wall;
                


            }

            return BarrierType.none;
        }
        private BarrierConfigurations getBarrierConfiguration(List<BarrierConfigurations> configs, BarrierType type)
        {
            switch(type)
            {
                case BarrierType.fence:
                    return configs[0];
                case BarrierType.wall:
                    return configs[1];
                case BarrierType.cityWall:
                    return configs[2];
                case BarrierType.retaining_wall:
                    return configs[3];
                case BarrierType.none:
                    return configs[4];

            }
            return new BarrierConfigurations();
        }

        private void generateFence(Way way)
        {

            for (int i = 0; i < way.nodes.Count-1; i++)
            {

                Vector3 differVec = way.nodes[i+1].meterPosition - way.nodes[i].meterPosition;
                float fenceWidth = differVec.magnitude;
                int poleCount = (int)Math.Round(fenceWidth / 5.0f) -1;
                float delta = fenceWidth / (float)(poleCount +1);
                for(int k = 1 ; k <= poleCount ; k++)
                {
                    GameObject obj = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Barrier/Fence/FencePole"));
                    obj.transform.parent = BarrierContainer.transform;
                    obj.transform.position = way.nodes[i].meterPosition + differVec * ((k *delta)/fenceWidth) + new Vector3(0.0f, 1.0f, 0.0f);
                    BarrierElements.Add(obj);
                }


                GameObject fencePlane = new GameObject("Plane", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                fencePlane.AddComponent<BarrierMouseHandler>();
                fencePlane.transform.parent = BarrierContainer.transform;

                Vector3[] vertices = new Vector3[] { way.nodes[i].meterPosition,
                                                     way.nodes[i+1].meterPosition,
                                                     way.nodes[i+1].meterPosition + new Vector3(0.0f,height,0.0f),
                                                     way.nodes[i].meterPosition + new Vector3(0.0f, height, 0.0f)};
                int[] triangles = new int[] { 0,1,2,2,3,0,0,3,2,2,1,0};
                Vector2[] UVcoords = new Vector2[] {new Vector2(0.0f,0.0f), new Vector2(fenceWidth,0.0f),
                                                  new Vector2(fenceWidth,2.0f), new Vector2(0.0f,2.0f)};

                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = UVcoords;

                MeshFilter meshFilter = fencePlane.GetComponent<MeshFilter>();
                meshFilter.mesh = mesh;

                MeshCollider meshCollider = fencePlane.GetComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;

                MeshRenderer meshRenderer = fencePlane.GetComponent<MeshRenderer>();
                meshRenderer.material = barrierMaterial;
                BarrierElements.Add(fencePlane);
            }            

            for (int i = 0 ; i < way.nodes.Count; i++)
            {
                GameObject obj = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Barrier/Fence/FencePole"));
                obj.name = "FencePole";
                obj.transform.parent = BarrierContainer.transform;
                obj.transform.position = way.nodes[i].meterPosition + new Vector3(0.0f, height/2.0f, 0.0f);
                BarrierElements.Add(obj);
            }

        }

        private void generateWall(Way way)
        {
            for (int i = 0; i < way.nodes.Count-1; i++)
            {

                GameObject Wall = new GameObject("WallBlock", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                Wall.transform.parent = BarrierContainer.transform;
                Wall.AddComponent<BarrierMouseHandler>();
                Vector3 up = new Vector3(0, 1, 0);
                Vector3 forward = way.nodes[i+1].meterPosition - way.nodes[i].meterPosition;
                forward.y = 0.0f;
                Vector3 right = Vector3.Cross(forward, up);
                right = right.normalized;
                Vector3 left = -1.0f * right;

                Vector3[] vertices = new Vector3[] { way.nodes[i].meterPosition   + left * thickness/2.0f,
                                                     way.nodes[i+1].meterPosition + left * thickness/2.0f,
                                                     way.nodes[i+1].meterPosition + new Vector3(0.0f, height, 0.0f) + left * thickness/2.0f,
                                                     way.nodes[i].meterPosition   + new Vector3(0.0f, height, 0.0f) + left * thickness/2.0f,

                                                     way.nodes[i].meterPosition   + right * thickness/2.0f,
                                                     way.nodes[i+1].meterPosition + right * thickness/2.0f,
                                                     way.nodes[i+1].meterPosition + new Vector3(0.0f, height, 0.0f) + right * thickness/2.0f,
                                                     way.nodes[i].meterPosition   + new Vector3(0.0f, height, 0.0f) + right * thickness/2.0f,

                                                     way.nodes[i].meterPosition   + new Vector3(0.0f, height, 0.0f) + left * thickness/2.0f,
                                                     way.nodes[i+1].meterPosition   + new Vector3(0.0f, height, 0.0f) + left * thickness/2.0f,
                                                     way.nodes[i+1].meterPosition   + new Vector3(0.0f, height, 0.0f) + right * thickness/2.0f,
                                                     way.nodes[i].meterPosition   + new Vector3(0.0f, height, 0.0f) + right * thickness/2.0f,

                                                     way.nodes[i].meterPosition   + left  * thickness/2.0f,
                                                     way.nodes[i].meterPosition   + right * thickness/2.0f,
                                                     way.nodes[i].meterPosition   + new Vector3(0.0f, height, 0.0f) + right * thickness/2.0f,
                                                     way.nodes[i].meterPosition   + new Vector3(0.0f, height, 0.0f) + left  * thickness/2.0f,

                                                     way.nodes[i+1].meterPosition   + left  * thickness/2.0f,
                                                     way.nodes[i+1].meterPosition   + right * thickness/2.0f,
                                                     way.nodes[i+1].meterPosition   + new Vector3(0.0f, height, 0.0f) + right * thickness/2.0f,
                                                     way.nodes[i+1].meterPosition   + new Vector3(0.0f, height, 0.0f) + left  * thickness/2.0f,

                                                   };

                int[] triangles = new int[] {0,3,2,2,1,0, //Front Plane
                                             4,5,6,6,7,4, //Back Plane
                                             8,11,10,10,9,8, //Top Plane
                                             12,13,14,14,15,12, //Left Plane
                                             16,19,18,18,17,16  //Right Right
                                             };
                float magnitude = (way.nodes[i+1].meterPosition - way.nodes[i].meterPosition).magnitude;

                Vector2[] UVcoords = new Vector2[] {new Vector2(0.0f,0.0f), new Vector2(magnitude/20.0f,0.0f), new Vector2(magnitude/20.0f,1.0f), new Vector2(0.0f,1.0f),
                                                    new Vector2(0.0f,0.0f), new Vector2(magnitude/20.0f,0.0f), new Vector2(magnitude/20.0f,1.0f), new Vector2(0.0f,1.0f),
                                                    new Vector2(0.0f,0.0f), new Vector2(magnitude/20.0f,0.0f), new Vector2(magnitude/20.0f,0.1f), new Vector2(0.0f,0.1f),
                                                    new Vector2(0.0f,0.0f), new Vector2(0.1f,0.0f), new Vector2(0.1f,1.0f), new Vector2(0.0f,1.0f),
                                                    new Vector2(0.0f,0.0f), new Vector2(0.1f,0.0f), new Vector2(0.1f,1.0f), new Vector2(0.0f,1.0f),
                                                   };

                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.uv = UVcoords;

                MeshFilter meshFilter = Wall.GetComponent<MeshFilter>();
                meshFilter.mesh = mesh;

                MeshCollider meshCollider = Wall.GetComponent<MeshCollider>();
                meshCollider.sharedMesh = mesh;

                MeshRenderer meshRenderer = Wall.GetComponent<MeshRenderer>();
                meshRenderer.material = barrierMaterial;
                BarrierElements.Add(Wall);
            }           


        }

        private void generateBarrier(Way way)
        {
           

            switch (type)
            {
                case BarrierType.fence:
                    generateFence(way);
                    break;
                case BarrierType.cityWall:
                case BarrierType.retaining_wall:
                case BarrierType.wall:
                    generateWall(way);
                    break;
                default :
                    generateWall(way);
                    break;
            }
        }

        public void updateHeight(float newHeight)
        {
            height = newHeight;

            if (type == BarrierType.fence)
            {
                var parentTransform = BarrierContainer.transform;
                foreach (Transform child in parentTransform)
                {
                    if (child.name == "FencePole")
                    {


                    }
                    else
                    {
                        int itr = 0;
                        MeshFilter meshfilter = child.GetComponent<MeshFilter>();
                        meshfilter.mesh.vertices = getVertices(newHeight, thickness, itr);
                    }
                }
            }

            else
            {
                int itr = 0;
                var parentTransform = BarrierContainer.transform;
                foreach(Transform child in parentTransform)
                {
                    MeshFilter meshfilter = child.GetComponent<MeshFilter>();
                    meshfilter.mesh.vertices = getVertices(newHeight, thickness, itr);
                    itr++;
                }

            }


        }

        public void updateThickness(float newThickness)
        {
            thickness = newThickness;

            if (type == BarrierType.fence)
                return;
            else
            {
                int itr = 0;
                var parentTransform = BarrierContainer.transform;
                foreach (Transform child in parentTransform)
                {
                    MeshFilter meshfilter = child.GetComponent<MeshFilter>();
                    meshfilter.mesh.vertices = getVertices(height, newThickness, itr);
                    itr++;
                }
            }

        }

        public void updateTexture(string texPath)
        {
            texturePath = texPath;
            barrierMaterial = InGameTextureHandler.createMaterial(texPath, "", "");

            if(type == BarrierType.fence)
            {
                var parentTransform = BarrierContainer.transform;
                foreach (Transform child in parentTransform)
                {
                    if (child.name != "FencePole")
                    {
                        MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                        renderer.material = barrierMaterial;
                    }
                }
            }
            else
            {
                var parentTransform = BarrierContainer.transform;
                foreach (Transform child in parentTransform)
                {
                        MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                        renderer.material = barrierMaterial;              
                }
            }


        }

        private Vector3[] getVertices(float _height, float _thickness, int itr)
        {
            if(type == BarrierType.fence)
            {
                return new Vector3[] { barrierWay.nodes[itr].meterPosition,
                                                     barrierWay.nodes[itr+1].meterPosition,
                                                     barrierWay.nodes[itr+1].meterPosition + new Vector3(0.0f,_height,0.0f),
                                                     barrierWay.nodes[itr].meterPosition + new Vector3(0.0f, _height, 0.0f)};
            }

            else
            {

                Vector3 up = new Vector3(0, 1, 0);
                Vector3 forward = barrierWay.nodes[itr + 1].meterPosition - barrierWay.nodes[itr].meterPosition;
                forward.y = 0.0f;
                Vector3 right = Vector3.Cross(forward, up);
                right = right.normalized;
                Vector3 left = -1.0f * right;

                return new Vector3[] { barrierWay.nodes[itr].meterPosition   + left * _thickness/2.0f,
                                                     barrierWay.nodes[itr+1].meterPosition + left * _thickness/2.0f,
                                                     barrierWay.nodes[itr+1].meterPosition + new Vector3(0.0f, _height, 0.0f) + left * _thickness/2.0f,
                                                     barrierWay.nodes[itr].meterPosition   + new Vector3(0.0f, _height, 0.0f) + left * _thickness/2.0f,

                                                     barrierWay.nodes[itr].meterPosition   + right * _thickness/2.0f,
                                                     barrierWay.nodes[itr+1].meterPosition + right * _thickness/2.0f,
                                                     barrierWay.nodes[itr+1].meterPosition + new Vector3(0.0f, _height, 0.0f) + right * _thickness/2.0f,
                                                     barrierWay.nodes[itr].meterPosition   + new Vector3(0.0f, _height, 0.0f) + right * _thickness/2.0f,

                                                     barrierWay.nodes[itr].meterPosition   + new Vector3(0.0f, _height, 0.0f) + left * _thickness/2.0f,
                                                     barrierWay.nodes[itr+1].meterPosition   + new Vector3(0.0f, _height, 0.0f) + left * _thickness/2.0f,
                                                     barrierWay.nodes[itr+1].meterPosition   + new Vector3(0.0f, _height, 0.0f) + right * _thickness/2.0f,
                                                     barrierWay.nodes[itr].meterPosition   + new Vector3(0.0f, _height, 0.0f) + right * _thickness/2.0f,

                                                     barrierWay.nodes[itr].meterPosition   + left  * _thickness/2.0f,
                                                     barrierWay.nodes[itr].meterPosition   + right * _thickness/2.0f,
                                                     barrierWay.nodes[itr].meterPosition   + new Vector3(0.0f, _height, 0.0f) + right * _thickness/2.0f,
                                                     barrierWay.nodes[itr].meterPosition   + new Vector3(0.0f, _height, 0.0f) + left  * _thickness/2.0f,

                                                     barrierWay.nodes[itr+1].meterPosition   + left  * _thickness/2.0f,
                                                     barrierWay.nodes[itr+1].meterPosition   + right * _thickness/2.0f,
                                                     barrierWay.nodes[itr+1].meterPosition   + new Vector3(0.0f, _height, 0.0f) + right * _thickness/2.0f,
                                                     barrierWay.nodes[itr+1].meterPosition   + new Vector3(0.0f, _height, 0.0f) + left  * _thickness/2.0f,

                                                   };




            }


        }

    }
}
