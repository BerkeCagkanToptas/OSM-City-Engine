using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.Utils;
using Assets.Scripts.ConfigHandler;

namespace Assets.Scripts.OpenStreetMap
{

    public enum buildingType
    {
        standard,
        withholeInside, 
        custom  //LOAD FROM EXTERNAL MESH
    }

    public enum materialMode
    {
        matFacadeDefault,
        matRoof,
        matFacadeCustom
    }


    class Building
    {
        string id;
        bool isClockwise;
        List<Node> nodeList;
        List<Tag> tagList;

        BuildingConfigurations buildingConfig;

        //Height of Building
        float buildingHeight;
        //In order to keep building up vector (0,1,0), roof level equalized for all vertices 
        float equalizedBuildingHeight;

        List<GameObject> facades;
        GameObject roof;
        GameObject building;

        Material defaultMaterial;
        int MaterialSkinID;
        materialMode materialMode;
       
        public Building(Way way, BuildingConfigurations config)
        {
            building = new GameObject("building" + way.id);
            building.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            
            id = way.id;
            nodeList = way.nodes;
            tagList = way.tags;

            buildingConfig = config;
            buildingHeight = getBuildingHeight(way.tags);
            equalizedBuildingHeight = assingBuildingTopLevel(way);

            defaultMaterial = (Material)Resources.Load(getMaterialID());

            isClockwise = getWayOrientation(way);
            facades = new List<GameObject>();
            for(int i=0 ; i < way.nodes.Count-1 ; i++)          
                createFacade(way.nodes[i], way.nodes[i + 1]);

            createRoof(way);

        }

        public Building(BuildingRelation relation, BuildingConfigurations config)
        {
            building = new GameObject("RelationBuilding" + relation.id);
            building.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

            id = relation.outerWall.id;
            tagList = relation.tags;

            buildingConfig = config;
            buildingHeight = getBuildingHeight(relation.tags);
            equalizedBuildingHeight = assingBuildingTopLevel(relation.outerWall);

            isClockwise = getWayOrientation(relation.outerWall);

            defaultMaterial = (Material)Resources.Load(getMaterialID());

            facades = new List<GameObject>();
            for (int i = 0; i < relation.outerWall.nodes.Count - 1; i++)
                createFacade(relation.outerWall.nodes[i], relation.outerWall.nodes[i + 1]);
            for (int i = 0; i < relation.innerHoles.Count; i++)
                for (int j = 0; j < relation.innerHoles[i].nodes.Count - 1; j++)
                    createFacade(relation.innerHoles[i].nodes[j], relation.innerHoles[i].nodes[j + 1]);

            createRoof(relation);

        }

        private float assingBuildingTopLevel(Way way)
        {
            float equalized = 100000.0f;

            for(int i=0 ; i < way.nodes.Count;i++)
            {
                if (way.nodes[i].meterPosition.y < equalized)
                    equalized = way.nodes[i].meterPosition.y;
            }

            return equalized + buildingHeight;
        }

        private void createFacade(Node node1, Node node2)
        {
            GameObject facade = new GameObject("facade", typeof(MeshRenderer), typeof(MeshFilter));
            facade.transform.parent = building.transform;
         

            Vector3[] vertices = {
                                     node1.meterPosition,
                                     node2.meterPosition,
                                     new Vector3(node2.meterPosition.x,equalizedBuildingHeight,node2.meterPosition.z),
                                     new Vector3(node1.meterPosition.x,equalizedBuildingHeight,node1.meterPosition.z)
                                 };

            int[] triangles;

            if (!isClockwise)
                triangles = new int[] {1,2,3,3,0,1};
            else
                triangles = new int[] {0,3,2,2,1,0};

            //Texture Size
            float aa;

            try
            {
               aa = (vertices[1] - vertices[0]).magnitude / buildingConfig.defaultSkins[MaterialSkinID - 1].width;
            }
            catch (Exception)
            {
                aa = 1.0f;
            }

            Vector2[] UVcoords = new Vector2[4];
            if(isClockwise)
            {
                UVcoords[0] = new Vector2(0,0);
                UVcoords[1] = new Vector2(aa, 0);
                UVcoords[2] = new Vector2(aa, 1);
                UVcoords[3] = new Vector2(0, 1);
            }
            else
            {
                UVcoords[0] = new Vector2(aa, 0);
                UVcoords[1] = new Vector2(0, 0);
                UVcoords[2] = new Vector2(0, 1);
                UVcoords[3] = new Vector2(aa, 1);
            }
      
            Geometry geo = new Geometry();
            Vector3 facadeNormal;
            if (isClockwise)
                facadeNormal = -1 * geo.findNormal(vertices[0], vertices[1], vertices[2]);
            else
                facadeNormal = geo.findNormal(vertices[0], vertices[1], vertices[2]);

            Vector3[] normals = {
                                    facadeNormal,
                                    facadeNormal,
                                    facadeNormal,
                                    facadeNormal
                                };


            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = UVcoords;
            mesh.triangles = triangles; 
            mesh.normals = normals;

            MeshFilter meshFilter = facade.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = facade.GetComponent<MeshRenderer>();
            meshRenderer.material = defaultMaterial;

            facades.Add(facade);
        }

        private void createRoof(Way way)
        {
            roof = new GameObject("roof", typeof(MeshRenderer), typeof(MeshFilter));
            roof.transform.parent = building.transform;

            Vector2[] vertices2D = new Vector2[way.nodes.Count-1];

            for(int k=0 ; k < way.nodes.Count-1; k++) 
                vertices2D[k] = new Vector2(way.nodes[k].meterPosition.x, way.nodes[k].meterPosition.z);

            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(vertices2D);
            int[] indices = tr.Triangulate();

            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[vertices2D.Length];
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] = new Vector3(vertices2D[i].x,equalizedBuildingHeight,vertices2D[i].y);

            Vector3[] normals = new Vector3[vertices2D.Length];
            for (int i = 0; i < normals.Length; i++)
                normals[i] = new Vector3(0.0f, 1.0f, 0.0f);

            Mesh msh = new Mesh();
            msh.vertices = vertices;
            msh.triangles = indices;
            msh.normals = normals;

            MeshFilter meshFilter = roof.GetComponent<MeshFilter>();
            meshFilter.mesh = msh;

            MeshRenderer meshRenderer = roof.GetComponent<MeshRenderer>();
            meshRenderer.material = (Material)Resources.Load("Materials/Building/Mat_Roof", typeof(Material));

        }

        private void createRoof(BuildingRelation relation)
        {
            createRoof(relation.outerWall);
        }

        private bool getWayOrientation(Way way)
        {
            float result = 0.0f;

            for(int k = 0 ; k < way.nodes.Count-1 ; k++)
            {
                result += (way.nodes[k + 1].lon - way.nodes[k].lon) * (way.nodes[k + 1].lat + way.nodes[k].lat); 
            }
            return (result < 0.0f);
        }

        private String getMaterialID()
        {

            for(int i =0 ; i < tagList.Count ; i++)
            {
                if (tagList[i].k == "man_made" && tagList[i].v == "tower")
                {
                    MaterialSkinID = 6;
                    return "Materials/Building/Mat_BuildingTower";
                }
                if (tagList[i].k == "shop" && tagList[i].v == "kiosk")
                {
                    MaterialSkinID = 7;
                    return "Materials/Building/Mat_BuildingKiosk";
                }
            }

            MaterialSkinID = UnityEngine.Random.Range(1, 6);
            return "Materials/Building/Mat_BuildingDefault" + MaterialSkinID;

        }

        private float getBuildingHeight(List<Tag> tagList)
        {
            float _buildingHeight = buildingConfig.height;

            for(int i =0 ; i < tagList.Count ; i++)
            {
                if (tagList[i].k == "building:level")
                    _buildingHeight = float.Parse(tagList[i].v) * 3.0f;

                else if (tagList[i].k == "man_made" && tagList[i].v == "tower")
                    _buildingHeight = 25.0f;

                else if (tagList[i].k == "shop" && tagList[i].v == "kiosk")
                    _buildingHeight = 3.0f;
            }

            return _buildingHeight;
        }

    }
}
