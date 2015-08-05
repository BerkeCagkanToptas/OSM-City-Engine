using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.HeightMap;
using Assets.Scripts.Utils;
using Assets.Scripts.OpenStreetMap;
using UnityEngine;
using Assets.Scripts.UnitySideScripts.MouseScripts;

namespace Assets.Scripts.SceneObjects
{

    public enum highwayType
    {
        HighwayResidential,
        HighwayUnclassified,
        HighwayPrimary,
        HighwaySecondary,
        HighwayTertiary,
        HighwayTertiaryLink,
        HighwayService,
        HighwayPedestrian,
        HighwayFootway,
        HighwayPath,
        Railway,
        River,
        none
    }

    public class Highway
    {
        //Initial Configurations
        public highwayType type;
        public float waySize;
        public string id;
        public string name;
        

        public bool hasLeftSidewalk;
        public bool hasRightSideWalk;
        public float leftSidewalkSize;
        public float rightSidewalkSize;

        //External Parameters
        public Way way;
        private BBox bbox;

        //Vertex Data
        public List<Vector3> leftSideVertexes;
        public List<Vector3> rightSideVertexes;

        public GameObject highwayGameObject;
        public Material highwayMaterial;
        private Mesh colliderMesh;


        public Highway(Way w, List<HighwayConfigurations> config, myTerrain _terrain)
        {
            id = w.id;
            way = w;
            type = getHighwayType(w.tags);
            name = getHighwayName(w.tags);

            if (type == highwayType.HighwayFootway)
                return;

            getConfiguration(config);
            bbox = _terrain.scenebbox;

            generateInitial3Dway(_terrain);
            //colliderMesh = generateColliderMesh();

            highwayGameObject = new GameObject("Highway" + way.id, typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            highwayGameObject.transform.localScale = new Vector3(1, 1, 1);
            

            if(type != highwayType.River)
                highwayGameObject.tag = "Highway";
                    
        }

        //Create Highway GameObject to Render
        public void renderHighway()
        {
            float extraHeight = getExtraHeight(type);

            Vector3[] vertices = new Vector3[leftSideVertexes.Count + rightSideVertexes.Count];
            for (int k = 0, m = 0; k < vertices.Length; k += 2, m++)
            {
                vertices[k] = leftSideVertexes[m] + new Vector3(0, extraHeight, 0);
                vertices[k + 1] = rightSideVertexes[m] + new Vector3(0, extraHeight, 0);
            }

            int[] triangles = new int[(vertices.Length - 2) * 3];
            for (int k = 0, m = 0; k < triangles.Length; k += 6, m += 2)
            {
                triangles[k] = m;
                triangles[k + 1] = m + 3;
                triangles[k + 2] = m + 1;

                triangles[k + 3] = m + 2;
                triangles[k + 4] = m + 3;
                triangles[k + 5] = m;
            }

            Vector2[] UVcoords = new Vector2[vertices.Length];
            float texPosition1 = 0.0f;
            float texPosition2 = 0.0f;
            float texPosition = 0.0f;
            const float texSize = 4.0f;

            UVcoords[0] = new Vector2(0.0f, texPosition1);
            UVcoords[1] = new Vector2(1.0f, texPosition2);

            for (int k = 2, m = 0; k < vertices.Length; k += 2, m++)
            {
                Vector3 dif1 = leftSideVertexes[m + 1] - leftSideVertexes[m];
                Vector3 dif2 = rightSideVertexes[m + 1] - rightSideVertexes[m];

                           
                texPosition1 += dif1.magnitude / texSize;
                texPosition2 += dif2.magnitude / texSize;
                texPosition += (dif1.magnitude + dif2.magnitude) / (2 * texSize);

                UVcoords[k] = new Vector2(0.0f, texPosition);
                UVcoords[k + 1] = new Vector2(1.0f, texPosition);
            }

            Vector3[] Normals = new Vector3[vertices.Length];
            for (int k = 0; k < Normals.Length; k++)
                Normals[k] = new Vector3(0.0f, 1.0f, 0.0f);


            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = UVcoords;
            mesh.triangles = triangles;
            mesh.normals = Normals;

            highwayGameObject.AddComponent<HighwayMouseHandler>();
            highwayGameObject.transform.localScale = new Vector3(1, 1, 1);
            
            MeshFilter meshFilter = highwayGameObject.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = highwayGameObject.GetComponent<MeshRenderer>();
            meshRenderer.material = highwayMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            MeshCollider meshCollider = highwayGameObject.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;

        }

        //Gets highwayType
        public static highwayType getHighwayType(List<Tag> tags)
        {
        

            for(int i =0 ; i < tags.Count ; i++)
            {
              
                if(tags[i].k == "highway")
                {
                    if (tags[i].v == "residential")
                        return highwayType.HighwayResidential;
                    else if (tags[i].v == "service")
                        return highwayType.HighwayService;
                    else if (tags[i].v == "primary")
                        return highwayType.HighwayPrimary;
                    else if (tags[i].v == "secondary")
                        return highwayType.HighwaySecondary;
                    else if (tags[i].v == "tertiary")
                        return highwayType.HighwayTertiary;
                    else if (tags[i].v == "tertiary_link")
                        return highwayType.HighwayTertiaryLink;
                    else if (tags[i].v == "unclassified")
                        return highwayType.HighwayUnclassified;
                    else if (tags[i].v == "pedestrian")
                        return highwayType.HighwayPedestrian;
                    else if (tags[i].v == "footway")
                        return highwayType.HighwayFootway;
                    else if (tags[i].v == "path")
                        return highwayType.HighwayPath;            
                }

                if (tags[i].k == "railway")
                    return highwayType.Railway;
                if (tags[i].k == "waterway")
                {
                    for(int x=0 ; x < tags.Count ; x++)
                    {
                        if (tags[x].k == "tunnel" && tags[x].v == "yes")
                            return highwayType.none;
                    }

                    return highwayType.River;
                }
                    
            }
       
            return highwayType.none;

        }

        //Gets highwayName
        public static string getHighwayName(List<Tag> tags)
        {
            int ind = tags.FindIndex(o => o.k == "name");
            if (ind == -1)
                return "Unknown";
            else
                return tags[ind].v;

        }

        //Gets the ExtraHeight for Compensate floating Point Precision Error while draping highway into terrain
        private float getExtraHeight(highwayType type)
        {
            switch(type)
            {
                case highwayType.River :
                    return 0.18f;
                case highwayType.Railway :
                    return 0.25f;
                case highwayType.HighwayService:
                    return 0.20f;
                default:
                    return 0.19f;
            }
        }

        //Sets appropriate configuration according to highway type
        private void getConfiguration(List<HighwayConfigurations> config)
        {
         
            for(int k =0 ; k < config.Count ; k++)
            {
              
                if(config[k].type == type.ToString("G"))
                {
                    //Debug.Log("Entered mat !!");    
                    waySize = config[k].size;
                    highwayMaterial = (Material)Resources.Load(config[k].materialPath);

                    hasLeftSidewalk = config[k].leftSidewalk;
                    hasRightSideWalk = config[k].rightSidewalk;
                    leftSidewalkSize = config[k].leftSidewalkSize;
                    rightSidewalkSize = config[k].rightSidewalkSize;
                    break;
                }                
            }
        }

        //This will be used for generating initial vertexes
        public void generateInitial3Dway(myTerrain terrain)
        {
            leftSideVertexes = new List<Vector3>();
            rightSideVertexes = new List<Vector3>();
	       
	        if (way.nodes.Count == 2)
	        {
		        Vector3 up = new Vector3(0, 1, 0);
		        Vector3 forward = way.nodes[1].meterPosition - way.nodes[0].meterPosition;
                        forward.y = 0.0f;
		        //Vector3 right = Vector3.Cross(forward ,up);**********************************************
                Vector3 right = Vector3.Cross(up ,forward);
                right = right.normalized;
		        Vector3 left = -1 * right;

                
                //LEFT SIDE
                Vector2 pointLeft1 = new Vector2(way.nodes[0].meterPosition.x,way.nodes[0].meterPosition.z) + new Vector2(left.x,left.z) * (waySize /2.0f);
                Vector2 pointLeft2 = new Vector2(way.nodes[1].meterPosition.x,way.nodes[1].meterPosition.z) + new Vector2(left.x,left.z) * (waySize /2.0f);
                leftSideVertexes.Add(new Vector3(pointLeft1.x,terrain.getTerrainHeight2(pointLeft1.y + bbox.meterBottom, pointLeft1.x + bbox.meterLeft),pointLeft1.y));
                leftSideVertexes.Add(new Vector3(pointLeft2.x,terrain.getTerrainHeight2(pointLeft2.y + bbox.meterBottom, pointLeft2.x + bbox.meterLeft),pointLeft2.y));

                //RIGHT SIDE
                Vector2 pointRight1 = new Vector2(way.nodes[0].meterPosition.x,way.nodes[0].meterPosition.z) + new Vector2(right.x,right.z) * (waySize /2.0f);
                Vector2 pointRight2 = new Vector2(way.nodes[1].meterPosition.x,way.nodes[1].meterPosition.z) + new Vector2(right.x,right.z) * (waySize /2.0f);
                rightSideVertexes.Add(new Vector3(pointRight1.x,terrain.getTerrainHeight2(pointRight1.y + bbox.meterBottom, pointRight1.x + bbox.meterLeft),pointRight1.y));
                rightSideVertexes.Add(new Vector3(pointRight2.x,terrain.getTerrainHeight2(pointRight2.y + bbox.meterBottom, pointRight2.x + bbox.meterLeft),pointRight2.y));
	        }
	        else
	        {
		        for (int i = 0; i < way.nodes.Count - 2; i++)
		        {

                    Vector3 up = new Vector3(0, 1, 0);
		            Vector3 forward1 = way.nodes[i+1].meterPosition - way.nodes[i].meterPosition;
                            forward1.y = 0.0f;
		            //Vector3 right1 = Vector3.Cross(forward1 ,up);**************************************************
                    Vector3 right1 = Vector3.Cross(up, forward1);
		            right1 = right1.normalized;
		            Vector3 left1 = -1 * right1;

                    Vector3 forward2 = way.nodes[i+2].meterPosition - way.nodes[i+1].meterPosition;
                            forward2.y = 0.0f;
		            //Vector3 right2 = Vector3.Cross(forward2 ,up);**************************************************
                    Vector3 right2 = Vector3.Cross(up, forward2);
		            right2 = right2.normalized;
		            Vector3 left2 = -1 * right2;

                    //INITIAL POINTS ARE ADDED TO NODES3D
                    if(i == 0)
                    {
                        Vector2 pointLeft1 = new Vector2(way.nodes[i].meterPosition.x, way.nodes[i].meterPosition.z) + new Vector2(left1.x, left1.z) * (waySize / 2.0f);
                        leftSideVertexes.Add(new Vector3(pointLeft1.x, terrain.getTerrainHeight2(pointLeft1.y + bbox.meterBottom, pointLeft1.x + bbox.meterLeft), pointLeft1.y));

                        Vector2 pointRight1 = new Vector2(way.nodes[i].meterPosition.x, way.nodes[i].meterPosition.z) + new Vector2(right1.x, right1.z) * (waySize / 2.0f);
                        rightSideVertexes.Add(new Vector3(pointRight1.x, terrain.getTerrainHeight2(pointRight1.y + bbox.meterBottom, pointRight1.x + bbox.meterLeft), pointRight1.y));
                

                    }

                    //1ST LINE LEFT
                    Vector2 p0 = new Vector2(way.nodes[i].meterPosition.x, way.nodes[i].meterPosition.z) + new Vector2(left1.x, left1.z) * (waySize / 2.0f);
                    Vector2 p1 = new Vector2(way.nodes[i + 1].meterPosition.x, way.nodes[i + 1].meterPosition.z) + new Vector2(left1.x, left1.z) * (waySize / 2.0f);

                    //2ND LINE LEFT
                    Vector2 p2 = new Vector2(way.nodes[i + 1].meterPosition.x, way.nodes[i + 1].meterPosition.z) + new Vector2(left2.x, left2.z) * (waySize / 2.0f);
                    Vector2 p3 = new Vector2(way.nodes[i + 2].meterPosition.x, way.nodes[i + 2].meterPosition.z) + new Vector2(left2.x, left2.z) * (waySize / 2.0f);

                    //INTERSECTION LEFT
                    Vector2 iL = new Vector2();
                    if(!Geometry.getInfiniteLineIntersection(ref iL,p0,p1,p2,p3))
                        leftSideVertexes.Add(new Vector3(p1.x, terrain.getTerrainHeight2(p1.y + bbox.meterBottom,p1.x + bbox.meterLeft) ,p1.y));
                    else
                        leftSideVertexes.Add(new Vector3(iL.x, terrain.getTerrainHeight2(iL.y + bbox.meterBottom, iL.x + bbox.meterLeft), iL.y));


                    //1ST LINE RIGHT
                     p0 = new Vector2(way.nodes[i].meterPosition.x, way.nodes[i].meterPosition.z) + new Vector2(right1.x, right1.z) * (waySize / 2.0f);
                     p1 = new Vector2(way.nodes[i + 1].meterPosition.x, way.nodes[i + 1].meterPosition.z) + new Vector2(right1.x, right1.z) * (waySize / 2.0f);

                    //2ND LINE RIGHT
                     p2 = new Vector2(way.nodes[i + 1].meterPosition.x, way.nodes[i + 1].meterPosition.z) + new Vector2(right2.x, right2.z) * (waySize / 2.0f);
                     p3 = new Vector2(way.nodes[i + 2].meterPosition.x, way.nodes[i + 2].meterPosition.z) + new Vector2(right2.x, right2.z) * (waySize / 2.0f);


                    //INTERSECTION RIGHT
                     Vector2 iR = new Vector2();
                    if (!Geometry.getInfiniteLineIntersection(ref iR, p0, p1, p2, p3))
                        rightSideVertexes.Add(new Vector3(p1.x, terrain.getTerrainHeight2(p1.y + bbox.meterBottom, p1.x + bbox.meterLeft), p1.y));
                    else
                        rightSideVertexes.Add(new Vector3(iR.x, terrain.getTerrainHeight2(iR.y + bbox.meterBottom, iR.x + bbox.meterLeft), iR.y));
            

			        //ENDING POINTS ARE ADDEDD TO NODES3D
			        if (i == way.nodes.Count - 3)
			        {
                        Vector2 pointLeft1 = new Vector2(way.nodes[i + 2].meterPosition.x, way.nodes[i + 2].meterPosition.z) + new Vector2(left2.x, left2.z) * (waySize / 2.0f);
                        leftSideVertexes.Add(new Vector3(pointLeft1.x, terrain.getTerrainHeight2(pointLeft1.y + bbox.meterBottom, pointLeft1.x + bbox.meterLeft), pointLeft1.y));

                        Vector2 pointRight1 = new Vector2(way.nodes[i + 2].meterPosition.x, way.nodes[i + 2].meterPosition.z) + new Vector2(right2.x, right2.z) * (waySize / 2.0f);
                        rightSideVertexes.Add(new Vector3(pointRight1.x, terrain.getTerrainHeight2(pointRight1.y + bbox.meterBottom, pointRight1.x + bbox.meterLeft), pointRight1.y));
                        return;
			        }


		        }
	        }



        }

        //Generate Mesh for Collider Excludes Points Come From Road Draping
        private Mesh generateColliderMesh()
        {
            Mesh mesh = new Mesh();

            float extraHeight = getExtraHeight(type);
            Vector3[] vertices = new Vector3[leftSideVertexes.Count + rightSideVertexes.Count];
            for (int k = 0, m = 0; k < vertices.Length; k += 2, m++)
            {
                vertices[k] = leftSideVertexes[m] + new Vector3(0, extraHeight, 0);
                vertices[k + 1] = rightSideVertexes[m] + new Vector3(0, extraHeight, 0);
            }

            int[] triangles = new int[(vertices.Length - 2) * 3];
            for (int k = 0, m = 0; k < triangles.Length; k += 6, m += 2)
            {
                triangles[k] = m;
                triangles[k + 1] = m + 3;
                triangles[k + 2] = m + 1;

                triangles[k + 3] = m + 2;
                triangles[k + 4] = m + 3;
                triangles[k + 5] = m;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;


            return mesh;
        }

        public void updateHighwayMesh()
        {
            float extraHeight = getExtraHeight(type);

            Vector3[] vertices = new Vector3[leftSideVertexes.Count + rightSideVertexes.Count];
            for (int k = 0, m = 0; k < vertices.Length; k += 2, m++)
            {
                vertices[k] = leftSideVertexes[m] + new Vector3(0, extraHeight, 0);
                vertices[k + 1] = rightSideVertexes[m] + new Vector3(0, extraHeight, 0);
            }

            int[] triangles = new int[(vertices.Length - 2) * 3];
            for (int k = 0, m = 0; k < triangles.Length; k += 6, m += 2)
            {
                triangles[k] = m;
                triangles[k + 1] = m + 3;
                triangles[k + 2] = m + 1;

                triangles[k + 3] = m + 2;
                triangles[k + 4] = m + 3;
                triangles[k + 5] = m;
            }

            Vector2[] UVcoords = new Vector2[vertices.Length];
            float texPosition1 = 0.0f;
            float texPosition2 = 0.0f;
            const float texSize = 4.0f;

            UVcoords[0] = new Vector2(0.0f, texPosition1);
            UVcoords[1] = new Vector2(1.0f, texPosition2);

            for (int k = 2, m = 0; k < vertices.Length; k += 2, m++)
            {
                Vector3 dif1 = leftSideVertexes[m + 1] - leftSideVertexes[m];
                Vector3 dif2 = rightSideVertexes[m + 1] - rightSideVertexes[m];

                texPosition1 += dif1.magnitude / texSize;
                texPosition2 += dif2.magnitude / texSize;

                UVcoords[k] = new Vector2(0.0f, texPosition1);
                UVcoords[k + 1] = new Vector2(1.0f, texPosition2);
            }

            Vector3[] Normals = new Vector3[vertices.Length];
            for (int k = 0; k < Normals.Length; k++)
                Normals[k] = new Vector3(0.0f, 1.0f, 0.0f);


            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = UVcoords;
            mesh.triangles = triangles;
            mesh.normals = Normals;

            MeshFilter meshFilter = highwayGameObject.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;


        }
    }
}
