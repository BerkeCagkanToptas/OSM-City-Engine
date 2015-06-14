using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.HeightMap;
using Assets.Scripts.Utils;
using Assets.Scripts.OpenStreetMap;
using UnityEngine;

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
        Material highwayMaterial;

        public bool hasLeftSidewalk;
        public bool hasRightSideWalk;
        public float leftSidewalkSize;
        public float rightSidewalkSize;

        //External Parameters
        public Way way;
        //myTerrain terrain;
        BBox bbox;
        List<HighwayConfigurations> highwayConfig;

        //Vertex Data
        public List<Vector3> leftSideVertexes;
        public List<Vector3> rightSideVertexes;
        List<Vector3> leftSideIntersections;
        List<Vector3> rightSideIntersections;

        GameObject gameObject;

        public Highway(Way w, List<HighwayConfigurations> config, myTerrain _terrain)
        {

            highwayConfig = config;
            way = w;
            type = getHighwayType(w.tags);

            if (type == highwayType.HighwayFootway)
                return;

            getConfiguration(config);
            bbox = _terrain.scenebbox;

            leftSideVertexes = new List<Vector3>();
            rightSideVertexes = new List<Vector3>();
            leftSideIntersections = new List<Vector3>();
            rightSideIntersections = new List<Vector3>();

            generateInitial3Dway(way, _terrain);      
                    
        }

        //Create Highway GameObject to Render
        public void renderHighway()
        {
            gameObject = new GameObject("Highway" + way.id, typeof(MeshRenderer), typeof(MeshFilter));

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
                triangles[k + 1] = m + 1;
                triangles[k + 2] = m + 3;

                triangles[k + 3] = m + 3;
                triangles[k + 4] = m + 2;
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

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            meshRenderer.material = highwayMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        }

        //Drape highway to the Terrain
        public void DrapeRoad(myTerrain _terrain)
        {
            //------ DO Not Change Order------
            HorizontalDrape(_terrain);
            VerticalDrape(_terrain);
            DiagonalDrape(_terrain);
            //--------------------------------
        }

        //Display Draping pins
        public void createDebugPins()
        {
            GameObject debugSphereContainer = new GameObject("debugSphereContainer");
            debugSphereContainer.transform.position = new Vector3(0, 0, 0);

            for (int k = 0; k < leftSideIntersections.Count; k++)
            {
                GameObject debugSphereLeft = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereTerrain"));
                debugSphereLeft.transform.parent = debugSphereContainer.transform;
                debugSphereLeft.transform.position = leftSideIntersections[k];

                GameObject debugSphereRight = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereTerrain"));
                debugSphereRight.transform.parent = debugSphereContainer.transform;
                debugSphereRight.transform.position = rightSideIntersections[k];

            }


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

        //Gets the ExtraHeight for Compensate floating Point Precision Error while draping highway into terrain
        private float getExtraHeight(highwayType type)
        {
            switch(type)
            {
                case highwayType.River :
                    return 0.20f;
                case highwayType.Railway :
                    return 0.30f;
                case highwayType.HighwayService:
                    return 0.24f;
                default:
                    return 0.25f;
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
        private void generateInitial3Dway(Way way, myTerrain terrain)
        {
	       
	        if (way.nodes.Count == 2)
	        {
		        Vector3 up = new Vector3(0, 1, 0);
		        Vector3 forward = way.nodes[1].meterPosition - way.nodes[0].meterPosition;
                        forward.y = 0.0f;
		        Vector3 right = Vector3.Cross(forward ,up);
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
		            Vector3 right1 = Vector3.Cross(forward1 ,up);
		            right1 = right1.normalized;
		            Vector3 left1 = -1 * right1;

                    Vector3 forward2 = way.nodes[i+2].meterPosition - way.nodes[i+1].meterPosition;
                            forward2.y = 0.0f;
		            Vector3 right2 = Vector3.Cross(forward2 ,up);
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

        //This will be used to drape Highway to Terrain Tiles Horizontally
        private void HorizontalDrape(myTerrain terrain)
        {
            TerrainInfo ti = terrain.terrainInfo;

            for (int i = 0; i < leftSideVertexes.Count - 1; i++)
            {

                //LEFT SIDE
                Vector2 pointLeft1 = new Vector2(leftSideVertexes[i].x, leftSideVertexes[i].z);
                Vector2 pointLeft2 = new Vector2(leftSideVertexes[i + 1].x, leftSideVertexes[i + 1].z);
                //RIGHT SIDE
                Vector2 pointRight1 = new Vector2(rightSideVertexes[i].x, rightSideVertexes[i].z);
                Vector2 pointRight2 = new Vector2(rightSideVertexes[i + 1].x, rightSideVertexes[i + 1].z);

                Vector2 previousIntersectLeft = pointLeft1;
                Vector2 previousIntersectRight = pointRight1;

                int leftit = i + 1;
                int rightit = i + 1;
                int count = 0;

                for (int k = ti.bottomIndex, t = 0; k >= ti.topIndex; k--, t++)
                {
                    //Horizontal Line
                    Vector2 pLeft = new Vector2(ti.meterPositions[t, 0].y - ti.shiftX, ti.meterPositions[t, 0].x - ti.shiftZ);
                    Vector2 pRight = new Vector2(ti.meterPositions[t, ti.ColumnCount - 1].y - ti.shiftX, ti.meterPositions[t, ti.ColumnCount - 1].x - ti.shiftZ);


                    Vector2 intersectleft = new Vector2();
                    Vector2 intersectright = new Vector2();
                    bool isleft = Geometry.getLineIntersection(ref intersectleft, pLeft, pRight, pointLeft1, pointLeft2);
                    bool isright = Geometry.getLineIntersection(ref intersectright, pLeft, pRight, pointRight1, pointRight2);

                    if (isleft && isright)
                    {
                        if((intersectleft - pointLeft1).magnitude > (previousIntersectLeft - pointLeft1).magnitude)
                        {
                            leftSideVertexes.Insert(leftit, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftit++;
                        }
                        else
                        {
                            leftSideVertexes.Insert(i+1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));   
                        }

                        if ((intersectright - pointRight1).magnitude > (previousIntersectRight - pointRight1).magnitude)
                        {
                            rightSideVertexes.Insert(rightit, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightit++;
                        }
                        else
                        {
                            rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        }

                        previousIntersectLeft = intersectleft;
                        previousIntersectRight = intersectright;
                        count++;
                    }

                    else if (isleft)
                    {
                        if ((intersectleft - pointLeft1).magnitude > (previousIntersectLeft - pointLeft1).magnitude)
                        {
                            leftSideVertexes.Insert(leftit, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftit++;
                        }
                        else
                        {
                            leftSideVertexes.Insert(i + 1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                        }

                        float ratio = (pointLeft1 - intersectleft).magnitude / (pointLeft1 - pointLeft2).magnitude;
                        Vector2 rightPointnew = pointRight1 + (pointRight2 - pointRight1) * ratio;
                        rightSideVertexes.Insert(rightit, new Vector3(rightPointnew.x, terrain.getTerrainHeight2(rightPointnew.y + ti.shiftZ, rightPointnew.x + ti.shiftX), rightPointnew.y));
                        rightSideIntersections.Add(new Vector3(rightPointnew.x, terrain.getTerrainHeight2(rightPointnew.y + ti.shiftZ, rightPointnew.x + ti.shiftX), rightPointnew.y));

                        count++;
                        rightit++;
                        previousIntersectLeft = intersectleft;
                        previousIntersectRight = rightPointnew;
 
                    }
                    else if (isright)
                    {

                        if ((intersectright - pointRight1).magnitude > (previousIntersectRight - pointRight1).magnitude)
                        {
                            rightSideVertexes.Insert(rightit, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightit++;
                        }
                        else
                        {
                            rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        }

                        float ratio = (pointRight1 - intersectright).magnitude / (pointRight1 - pointRight2).magnitude;
                        Vector2 leftPointnew = pointLeft1 + (pointLeft2 - pointLeft1) * ratio;
                        leftSideVertexes.Insert(leftit, new Vector3(leftPointnew.x, terrain.getTerrainHeight2(leftPointnew.y + ti.shiftZ, leftPointnew.x + ti.shiftX), leftPointnew.y));
                        leftSideIntersections.Add(new Vector3(leftPointnew.x, terrain.getTerrainHeight2(leftPointnew.y + ti.shiftZ, leftPointnew.x + ti.shiftX), leftPointnew.y));

                        previousIntersectLeft = leftPointnew;
                        previousIntersectRight = intersectright;
                        count++;
                        leftit++;
                    }
                    else
                        continue;


                }

                i += count;

            }



        }

        //This will be used to drape Highway to Terrain Tiles Vertically
        private void VerticalDrape(myTerrain terrain)
        {
            TerrainInfo ti = terrain.terrainInfo;

            for (int i = 0; i < (int)leftSideVertexes.Count - 1; i++)
            {
                Vector2 pointLeft1 = new Vector2(leftSideVertexes[i].x, leftSideVertexes[i].z);
                Vector2 pointLeft2 = new Vector2(leftSideVertexes[i + 1].x, leftSideVertexes[i + 1].z);

                Vector2 pointRight1 = new Vector2(rightSideVertexes[i].x, rightSideVertexes[i].z);
                Vector2 pointRight2 = new Vector2(rightSideVertexes[i + 1].x, rightSideVertexes[i + 1].z);

                int cnt = 0;
                int leftiterator = i + 1;
                int rightiterator = i + 1;

                Vector2 previousIntersectLeft = pointLeft1;
                Vector2 previousIntersectRight = pointRight1;


                for (int k = ti.leftIndex, z = 0; k <= ti.rightIndex; k++, z++)
                {
                    //Vertical Terrain Line
                    Vector2 pTop = new Vector2(ti.meterPositions[0, z].y - ti.shiftX, ti.meterPositions[0, z].x - ti.shiftZ);
                    Vector2 pBottom = new Vector2(ti.meterPositions[ti.RowCount - 1, z].y - ti.shiftX, ti.meterPositions[ti.RowCount - 1, z].x - ti.shiftZ);

                    Vector2 intersectleft = new Vector2();
                    Vector2 intersectright = new Vector2();
                    bool isleft = Geometry.getLineIntersection(ref intersectleft, pTop, pBottom, pointLeft1, pointLeft2);
                    bool isright = Geometry.getLineIntersection(ref intersectright, pTop, pBottom, pointRight1, pointRight2);

                    if (isleft && isright)
                    {

                        if ((intersectleft - pointLeft1).magnitude > (previousIntersectLeft - pointLeft1).magnitude)
                        {
                            leftSideVertexes.Insert(leftiterator, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftiterator++;
                        }
                        else
                        {
                            leftSideVertexes.Insert(i+1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                        }

                        if ((intersectright - pointRight1).magnitude > (previousIntersectRight - pointRight1).magnitude)
                        {
                            rightSideVertexes.Insert(rightiterator, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightiterator++;
                        }
                        else
                        {
                            rightSideVertexes.Insert(i+1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        }

                        previousIntersectLeft = intersectleft;
                        previousIntersectRight = intersectright;
                        cnt++;
                    }
                    else if (isleft)
                    {

                        if ((intersectleft - pointLeft1).magnitude > (previousIntersectLeft - pointLeft1).magnitude)
                        {
                            leftSideVertexes.Insert(leftiterator, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftiterator++;
                        }
                        else
                        {
                            leftSideVertexes.Insert(i+1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                        }

                        float ratio = (pointLeft1 - intersectleft).magnitude / (pointLeft1 - pointLeft2).magnitude;
                        Vector2 rightPointnew = pointRight1 + (pointRight2 - pointRight1) * ratio;

                        rightSideVertexes.Insert(rightiterator, new Vector3(rightPointnew.x, terrain.getTerrainHeight2(rightPointnew.y + ti.shiftZ, rightPointnew.x + ti.shiftX), rightPointnew.y));
                        rightSideIntersections.Add(new Vector3(rightPointnew.x, terrain.getTerrainHeight2(rightPointnew.y + ti.shiftZ, rightPointnew.x + ti.shiftX), rightPointnew.y));

                        previousIntersectLeft = intersectleft;
                        previousIntersectRight = rightPointnew;
                        rightiterator++;
                        cnt++;

                    }
                    else if (isright)
                    {

                        if ((intersectright - pointRight1).magnitude > (previousIntersectRight - pointRight1).magnitude)
                        {
                            rightSideVertexes.Insert(rightiterator, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightiterator++;
                        }
                        else
                        {
                            rightSideVertexes.Insert(i+1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        }

                        float ratio = (pointRight1 - intersectright).magnitude / (pointRight1 - pointRight2).magnitude;
                        Vector2 leftPointnew = pointLeft1 + (pointLeft2 - pointLeft1) * ratio;

                        leftSideVertexes.Insert(leftiterator, new Vector3(leftPointnew.x, terrain.getTerrainHeight2(leftPointnew.y + ti.shiftZ, leftPointnew.x + ti.shiftX), leftPointnew.y));
                        leftSideIntersections.Add(new Vector3(leftPointnew.x, terrain.getTerrainHeight2(leftPointnew.y + ti.shiftZ, leftPointnew.x + ti.shiftX), leftPointnew.y));

                        previousIntersectRight = intersectright;
                        previousIntersectLeft = leftPointnew;
                        leftiterator++;
                        cnt++;
                    }


                }

                i += cnt;

            }




        }

        //This will be used to drape Highway to Terrain Tiles Diagonally
        private void DiagonalDrape(myTerrain terrain)
        {
            TerrainInfo ti = terrain.terrainInfo;

            for (int i = 0; i < (int)leftSideVertexes.Count - 1; i++)
            {

                //LEFT SIDE
                Vector2 pointLeft1 = new Vector2(leftSideVertexes[i].x, leftSideVertexes[i].z);
                Vector2 pointLeft2 = new Vector2(leftSideVertexes[i + 1].x, leftSideVertexes[i + 1].z);

                //RIGHT SIDE
                Vector2 pointRight1 = new Vector2(rightSideVertexes[i].x, rightSideVertexes[i].z);
                Vector2 pointRight2 = new Vector2(rightSideVertexes[i + 1].x, rightSideVertexes[i + 1].z);

                int cnt = 0;

                for (int k = 0, t = 1; k < ti.ColumnCount + ti.RowCount - 3; k++, t++)
                {
                    //Diagonal Line
                    Vector2 pBottom;
                    Vector2 pTop;
                    if (t < ti.ColumnCount)
                        pTop = new Vector2(ti.meterPositions[0, t].y - ti.shiftX, ti.meterPositions[0, t].x - ti.shiftZ);
                    else
                        pTop = new Vector2(ti.meterPositions[1 + t - ti.ColumnCount, ti.ColumnCount - 1].y -ti.shiftX, ti.meterPositions[1 + t - ti.ColumnCount, ti.ColumnCount - 1].x - ti.shiftZ);

                    if (t < ti.RowCount)
                        pBottom = new Vector2(ti.meterPositions[t, 0].y - ti.shiftX, ti.meterPositions[t, 0].x - ti.shiftZ);
                    else
                        pBottom = new Vector2(ti.meterPositions[ti.RowCount - 1, 1 + t - ti.RowCount].y - ti.shiftX, ti.meterPositions[ti.RowCount - 1, t + 1 - ti.RowCount].x - ti.shiftZ);

                    //LEFT SIDE INTERSECTION
                    Vector2 intersectleft = new Vector2();
                    //RIGHT SIDE INTERSECTION
                    Vector2 intersectright = new Vector2();

                    bool isLeft = Geometry.getLineIntersection(ref intersectleft, pTop, pBottom, pointLeft1, pointLeft2);
                    bool isRight = Geometry.getLineIntersection(ref intersectright, pTop, pBottom, pointRight1, pointRight2);

                    if (isLeft && isRight)
                    {
                        leftSideVertexes.Insert(i + 1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                        leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));

                        rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        cnt++;
                    }
                    else if (isLeft)
                    {
                        leftSideVertexes.Insert(i + 1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                        leftSideIntersections.Add(new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));

                        float ratio = (pointLeft1 - intersectleft).magnitude / (pointLeft1 - pointLeft2).magnitude;
                        Vector2 rightPointnew = pointRight1 + (pointRight2 - pointRight1) * ratio;
                        rightSideVertexes.Insert(i + 1, new Vector3(rightPointnew.x, terrain.getTerrainHeight2(rightPointnew.y + ti.shiftZ, rightPointnew.x + ti.shiftX), rightPointnew.y));
                        rightSideIntersections.Add(new Vector3(rightPointnew.x, terrain.getTerrainHeight2(rightPointnew.y + ti.shiftZ, rightPointnew.x + ti.shiftX), rightPointnew.y));
                        cnt++;
                    }

                    else if (isRight)
                    {
                        rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        rightSideIntersections.Add(new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));

                        float ratio = (pointRight1 - intersectright).magnitude / (pointRight1 - pointRight2).magnitude;
                        Vector2 leftPointnew = pointLeft1 + (pointLeft2 - pointLeft1) * ratio;
                        leftSideVertexes.Insert(i + 1, new Vector3(leftPointnew.x, terrain.getTerrainHeight2(leftPointnew.y + ti.shiftZ, leftPointnew.x + ti.shiftX), leftPointnew.y));
                        leftSideIntersections.Add(new Vector3(leftPointnew.x, terrain.getTerrainHeight2(leftPointnew.y + ti.shiftZ, leftPointnew.x + ti.shiftX), leftPointnew.y));
                        cnt++;
                    }
                }
                i += cnt;
            }



        }
    }
}
