using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.OpenStreetMap;
using Assets.Scripts.Utils;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.UnitySideScripts.MouseScripts;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Assets.Scripts.SceneObjects
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

    public struct FacadeSkin
    {
        [XmlAttribute("facadeID")]
        public int facadeID;
        //UV coordinates for corners
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
        //Texture paths
        public string colorTexturePath;
        public string normalTexturePath;
        public string specularTexturePath;
    }

    public class Building
    {
        public string id;
        public bool isClockwise;

        public Way way;
        public BuildingRelation relation;
        public buildingType type;

        BuildingConfigurations buildingConfig;

        //Height of Building
        public float buildingHeight;
        //In order to keep building up vector (0,1,0), roof level equalized for all vertices 
        private float equalizedBuildingHeight;
        
        public Material defaultMaterial;
        public int defaultMaterialID;
        float defaultTextureWidth;
        
        //Necessary parameters for saving Project
        public string modelPath;
        public Vector3 GOtransform, GOrotate, GOscale;
        public List<FacadeSkin> facadeSkins;


        public GameObject building;
        public List<GameObject> facades; 
        GameObject roof;

        

        public Building(Way _way, BuildingConfigurations config, Material mat, int materialID, float texWidth)
        {
            type = buildingType.standard;
            way = _way;

            id = way.id;

            buildingConfig = config;
            buildingHeight = getBuildingHeight(way.tags);
            equalizedBuildingHeight = assingBuildingTopLevel(way);

            defaultMaterial = mat;
            defaultMaterialID = materialID;
            defaultTextureWidth = texWidth;
            facadeSkins = new List<FacadeSkin>();

            isClockwise = getWayOrientation(way);

            building = new GameObject("building" + id);
            building.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

            GOtransform = new Vector3(0, 0, 0);
            GOrotate = new Vector3(0, 0, 0);
            GOscale = new Vector3(1, 1, 1);
        }

        public Building(BuildingRelation _relation, BuildingConfigurations config, Material mat,int materialID, float texWidth)
        {
            type = buildingType.withholeInside;
            relation = _relation;

            id = relation.outerWall.id;

            buildingConfig = config;
            buildingHeight = getBuildingHeight(relation.tags);
            equalizedBuildingHeight = assingBuildingTopLevel(relation.outerWall);

            isClockwise = getWayOrientation(relation.outerWall);

            defaultMaterial = mat;
            defaultMaterialID = materialID;
            defaultTextureWidth = texWidth;
            facadeSkins = new List<FacadeSkin>();

            building = new GameObject("building" + id);
            building.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

            GOtransform = new Vector3(0, 0, 0);
            GOrotate = new Vector3(0, 0, 0);
            GOscale = new Vector3(1, 1, 1);

        }

        public void RenderBuilding()
        {
            facades =  new List<GameObject>();            
            roof = new GameObject("roof", typeof(MeshRenderer), typeof(MeshFilter));
            roof.transform.parent = building.transform;

            if (type == buildingType.standard)
            {
       
                for (int i = 0; i < way.nodes.Count - 1; i++)
                    createFacade(i,way.nodes[i], way.nodes[i + 1],building);

                createRoof(way,roof);
            }
            else if(type == buildingType.withholeInside)
            {
                for (int i = 0; i < relation.outerWall.nodes.Count - 1; i++)
                    createFacade(i,relation.outerWall.nodes[i], relation.outerWall.nodes[i + 1],building);
                for (int i = 0; i < relation.innerHoles.Count; i++)
                    for (int j = 0; j < relation.innerHoles[i].nodes.Count - 1; j++)
                        createFacade(-1,relation.innerHoles[i].nodes[j], relation.innerHoles[i].nodes[j + 1],building);

                createRoof(relation,roof);
            }


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

        private void createFacade(int facadeID, Node node1, Node node2,GameObject building)
        {
            GameObject facade = new GameObject("facade" +facadeID, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            facade.tag = "Building";
            facade.transform.parent = building.transform;
            facade.AddComponent<BuildingMouseHandler>();

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

            //Texture Size Horizontal
            float aa;
            //Texture Size Vertical
            float bb;

            bb = equalizedBuildingHeight - buildingHeight; // Min Height

            try
            {
               aa = (vertices[1] - vertices[0]).magnitude / defaultTextureWidth;
            }
            catch (Exception)
            {
                aa = 1.0f;
            }


            Vector2[] UVcoords = new Vector2[4];
            if (isClockwise)
            {
                UVcoords[0] = new Vector2(0, (vertices[0].y - bb) / buildingHeight);
                UVcoords[1] = new Vector2(aa, (vertices[1].y - bb) / buildingHeight);
                UVcoords[2] = new Vector2(aa, 1);
                UVcoords[3] = new Vector2(0, 1);
            }
            else
            {
                UVcoords[0] = new Vector2(aa, (vertices[0].y - bb) / buildingHeight);
                UVcoords[1] = new Vector2(0, (vertices[1].y - bb) / buildingHeight);
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
            int skinIndex = facadeSkins.FindIndex(o=>o.facadeID == facadeID);
            if (skinIndex == -1)
                meshRenderer.material = defaultMaterial;
            else
                meshRenderer.material = InGameTextureHandler.createMaterial(facadeSkins[skinIndex]);

            MeshCollider meshCollider = facade.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            facades.Add(facade);
        }

        private void createRoof(Way way,GameObject roof)
        {

            Vector2[] vertices2D = new Vector2[way.nodes.Count-1];

            for(int k=0 ; k < way.nodes.Count-1; k++) 
                vertices2D[k] = new Vector2(way.nodes[k].meterPosition.x, way.nodes[k].meterPosition.z);

            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(vertices2D);
            int[] indices = tr.Triangulate();

            //Create the Vector3 vertices
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

        private void createRoof(BuildingRelation relation, GameObject roof)
        {
            createRoof(relation.outerWall,roof);
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

        private float getBuildingHeight(List<Tag> tagList)
        {
            float _buildingHeight = UnityEngine.Random.Range(buildingConfig.minheight, buildingConfig.maxheight);

            for(int i =0 ; i < tagList.Count ; i++)
            {
                if (tagList[i].k == "building:level")
                    _buildingHeight = float.Parse(tagList[i].v) * 3.0f;
                else if (tagList[i].k == "building:height")
                    _buildingHeight = float.Parse(tagList[i].v);
                else if (tagList[i].k == "man_made" && tagList[i].v == "tower")
                    _buildingHeight = 25.0f;

                else if (tagList[i].k == "shop" && tagList[i].v == "kiosk")
                    _buildingHeight = 3.0f;
            }

            return _buildingHeight;
        }

        public void updateHeightMesh(float newHeight)
        {
            equalizedBuildingHeight = newHeight + equalizedBuildingHeight - buildingHeight;
            buildingHeight = newHeight;

            if (type == buildingType.standard)
            {

                for (int i = 0; i < way.nodes.Count - 1; i++)
                {
                    MeshFilter meshFilter = facades[i].GetComponent<MeshFilter>();
                    meshFilter.mesh.vertices = getFacadeVertices(way.nodes[i], way.nodes[i + 1]);
                }

                createRoof(way, roof);
            }
            else if (type == buildingType.withholeInside)
            {
                for (int i = 0; i < relation.outerWall.nodes.Count - 1; i++)
                {
                    MeshFilter meshFilter = facades[i].GetComponent<MeshFilter>();
                    meshFilter.mesh.vertices = getFacadeVertices(relation.outerWall.nodes[i], relation.outerWall.nodes[i + 1]);
                }
                for (int i = 0; i < relation.innerHoles.Count; i++)
                    for (int j = 0; j < relation.innerHoles[i].nodes.Count - 1; j++)
                    {
                        MeshFilter meshFilter = facades[i].GetComponent<MeshFilter>();
                        meshFilter.mesh.vertices = getFacadeVertices(relation.innerHoles[i].nodes[j], relation.innerHoles[i].nodes[j + 1]);                  
                    }

                createRoof(relation, roof);
            }

        }

        private Vector3[] getFacadeVertices(Node node1, Node node2)
        {
            Vector3[] vertices = {
                                     node1.meterPosition,
                                     node2.meterPosition,
                                     new Vector3(node2.meterPosition.x,equalizedBuildingHeight,node2.meterPosition.z),
                                     new Vector3(node1.meterPosition.x,equalizedBuildingHeight,node1.meterPosition.z)
                                 };

            return vertices;
        }


        public void setMaterial(int facadeID, Texture2D colorTexture, Texture2D normalTexture, Texture2D specularTexture, string colorTex, string normalTex, string specularTex)
        {
            Material mat = facades[facadeID].GetComponent<MeshRenderer>().material;
            FacadeSkin fs = new FacadeSkin();
            fs.facadeID = facadeID;
            fs.normalTexturePath = normalTex;
            fs.specularTexturePath = specularTex;
            fs.colorTexturePath = colorTex;
            fs.bottomLeft = new Vector2(0, 0);
            fs.bottomRight = new Vector2(0, 1);
            fs.topLeft = new Vector2(1, 0);
            fs.topRight = new Vector2(1, 1);
            int facadeIndex = facadeSkins.FindIndex(item => item.facadeID == facadeID);
            if (facadeIndex == -1)
                facadeSkins.Add(fs);
            else
                facadeSkins[facadeIndex] = fs;

            if (colorTexture != null)
            {
                mat.SetTexture("_MainTex", colorTexture);
                mat.SetTexture("_BumpMap", normalTexture);
                mat.SetTexture("_SpecGlossMap", specularTexture);
            }
        }

        public void setTextureCoordinate(int facadeID, Vector2 bottomLeft, Vector2 bottomRight, Vector2 topLeft, Vector2 topRight)
        {
            int facadeIndex = facadeSkins.FindIndex(item => item.facadeID == facadeID);
            FacadeSkin fs = facadeSkins[facadeIndex];
            fs.bottomLeft = bottomLeft;
            fs.bottomRight = bottomRight;
            fs.topLeft = topLeft;
            fs.topRight = topRight;
            facadeSkins[facadeIndex] = fs;


            MeshFilter meshFilter = facades[facadeID].GetComponent<MeshFilter>();
            if (isClockwise)
                meshFilter.mesh.uv = new Vector2[] { bottomLeft, bottomRight, topRight, topLeft };
            else
                meshFilter.mesh.uv = new Vector2[] { bottomRight, bottomLeft, topLeft, topRight };
        }

    }
}
