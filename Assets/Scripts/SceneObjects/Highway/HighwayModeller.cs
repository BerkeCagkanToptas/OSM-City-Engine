using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.HeightMap;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.Utils;
using Assets.Scripts.OpenStreetMap;
using UnityEngine;
using Assets.Scripts.UnitySideScripts.MouseScripts;


namespace Assets.Scripts.SceneObjects
{


    public class HighwayModeller
    {
        private struct intersectionInfo
        {
            public int wayNo1;
            public int wayNo2;
            public bool isleftright;
            public Vector3 vertice;
        }

        private struct LineSegment
        {
            public Vector2 Left1,  Left2;
            public Vector2 Right1, Right2;
        }

        private struct intersectValidator
        {
           public bool left;
           public bool right;
        }

        private enum IntersectionType
        {
            front,
            end,
            middle_to,
            middle_from,
        }

        private struct IntersectionNode
        {
            public Node node;
            public List<Vector3> pointList; //from node to outside
            public List<int> intersectionIndex;
            public List<IntersectionType> intersectionTypes;
            public List<string> wayIds;

            public List<Object3D> trafficLights;
            public GameObject intersectionHole;

            public int count;
            public bool hasRoadDivision;
        }

        private struct IntersectionAngle
        {
            public int way1No, way2No;
            public double angle;
        }

        public List<Highway> highwayList;
        public List<Pavement> pavementList;        
        private List<IntersectionNode> intersections;

        protected myTerrain terrain;
        protected List<HighwayConfigurations> configurations;

        private List<Way> wayList;

        public HighwayModeller(List<Way> _wayList, myTerrain _terrain, List<HighwayConfigurations> _configurations)
        {
            wayList = _wayList;
            highwayList = new List<Highway>();
            pavementList = new List<Pavement>();
            terrain = _terrain;
            configurations = _configurations;
            generateHighwayList();
            generatePavementList();
        }
        
        
        public HighwayModeller(List<Way> _wayList, myTerrain _terrain, List<HighwayConfigurations> _configurations, List<HighwaySave> highwaySaveList)
        {
            wayList = _wayList;
            highwayList = new List<Highway>();
            pavementList = new List<Pavement>();
            terrain = _terrain;
            configurations = _configurations;
            generateHighwayList(highwaySaveList);
            generatePavementList();
        }
        
        public void renderHighwayList()
        {
            for (int i = 0; i < highwayList.Count; i++)
            {
                try
                {
                    highwayList[i].renderHighway();
                }
                catch (Exception ex)
                {
                    Debug.Log("<color=red>ERROR:</color>" + ex.Message);
                    continue;
                }
            }
        }
        public void renderPavementList()
        {
            for (int i = 0; i < pavementList.Count; i++)
                pavementList[i].renderPavement();

            for (int i = 0; i < pavementList.Count; i++)
            {
                correctPavement(pavementList[i].Pavementid, Pavement.pavementSide.left);
                correctPavement(pavementList[i].Pavementid, Pavement.pavementSide.right);
            }
        }

        private void generateHighwayList()
        {
            //Eliminate small highways that cause problems
            for (int i = 0; i < wayList.Count; i++)
            {
                float length = 0;
                bool isNormal = false;
                for (int k = 0; k < wayList[i].nodes.Count - 1; k++)
                {
                    length += (wayList[i].nodes[k + 1].meterPosition - wayList[i].nodes[k].meterPosition).magnitude;
                    if (length >= 5.0f)
                    {
                        isNormal = true;
                        break;
                    }
                }

                if (!isNormal)
                {
                    wayList.RemoveAt(i);
                    i--;
                }
            }

            intersections = generateIntersectionList();


            divideNecessaryWays();

            for(int i = 0 ; i < wayList.Count ; i++)
                highwayList.Add(new Highway(wayList[i], configurations, terrain));

            for (int i = 0; i < intersections.Count; i++)
            {
                if (intersections[i].wayIds.Count == 2)
                    correct2wayIntersection(intersections[i]);
                else
                {
                    try
                    {
                        correctIntersection(intersections[i]);
                    }
                    catch (Exception ex)
                    {
                        resetIntersectionNode(intersections[i]);
                        continue;
                    }
                }
            }

            DrapeHighway draper = new DrapeHighway();
            for (int i = 0; i < highwayList.Count; i++)
                draper.DrapeRoad(terrain, highwayList[i].leftSideVertexes, highwayList[i].rightSideVertexes);

        }
        private void generatePavementList()
        {
            DrapeHighway draper = new DrapeHighway();

            for (int i = 0; i < highwayList.Count; i++)
            {
                if (highwayList[i].hasLeftSidewalk)
                {
                    Pavement newPavement = new Pavement(highwayList[i], terrain,Pavement.pavementSide.left);
                    if (newPavement.leftSideVertexes.Count == 0 || newPavement.rightSideVertexes.Count == 0)
                        continue;
                    draper.DrapeRoad(terrain, newPavement.leftSideVertexes, newPavement.rightSideVertexes);
                    pavementList.Add(newPavement);
                }
                if(highwayList[i].hasRightSideWalk)
                {
                    Pavement newPavement = new Pavement(highwayList[i], terrain, Pavement.pavementSide.right);
                    if (newPavement.leftSideVertexes.Count == 0 || newPavement.rightSideVertexes.Count == 0)
                        continue;
                    draper.DrapeRoad(terrain, newPavement.leftSideVertexes, newPavement.rightSideVertexes);
                    pavementList.Add(newPavement);
                }
            }



        }

        private void generateHighwayList(List<HighwaySave> saveList)
        {
            //Eliminate small highways that cause problems
            for (int i = 0; i < wayList.Count; i++)
            {
                float length = 0;
                bool isNormal = false;
                for (int k = 0; k < wayList[i].nodes.Count - 1; k++)
                {
                    length += (wayList[i].nodes[k + 1].meterPosition - wayList[i].nodes[k].meterPosition).magnitude;
                    if (length >= 5.0f)
                    {
                        isNormal = true;
                        break;
                    }
                }

                if (!isNormal)
                {
                    wayList.RemoveAt(i);
                    i--;
                }
            }

            intersections = generateIntersectionList();


            divideNecessaryWays();

            for (int i = 0; i < wayList.Count; i++)
            {               
                highwayList.Add(new Highway(wayList[i], configurations, terrain,saveList[i]));
            }

            for (int i = 0; i < intersections.Count; i++)
            {
                if (intersections[i].wayIds.Count == 2)
                    correct2wayIntersection(intersections[i]);
                else
                    correctIntersection(intersections[i]);
            }

            DrapeHighway draper = new DrapeHighway();
            for (int i = 0; i < highwayList.Count; i++)
                draper.DrapeRoad(terrain, highwayList[i].leftSideVertexes, highwayList[i].rightSideVertexes);

        }


        /// <summary>
        /// Divide necessary ways into multiple pieces for easier intersection handling
        /// </summary>
        private void divideNecessaryWays()
        {


            for(int i = 0 ; i < intersections.Count ; i++)
            {
                if(intersections[i].hasRoadDivision)
                {
                   
                    for(int k = 0 ; k < intersections[i].intersectionTypes.Count ; k++)
                    {
                        if (intersections[i].intersectionTypes[k] == IntersectionType.middle_from)
                        {
                           string wayId = intersections[i].wayIds[k];
                           int intersectionIndex = intersections[i].intersectionIndex[k] -1;
                           divideWay(wayId,intersectionIndex);
                           IntersectionNode thisnode = intersections[i];
                           thisnode.intersectionTypes[k] = IntersectionType.front;
                           thisnode.intersectionTypes[k + 1] = IntersectionType.end;
                        }

                    }                 
                }
            }
        }

        /// <summary>
        /// Divide the way into 2 given intersection node
        /// </summary>
        private void divideWay(string wayID, int intersectionIndex)
        {
            int wayIndex = getWayIndex(wayID);

            Way w1 =  new Way(wayList[wayIndex]);
            w1.id = w1.id + "i1";
            w1.nodes = w1.nodes.GetRange(0, intersectionIndex+1);


            Way w2 = new Way(wayList[wayIndex]);
            try
            {
                w2.id = w2.id + "i2";
                w2.nodes = w2.nodes.GetRange(intersectionIndex, w2.nodes.Count - intersectionIndex);
            }
            catch(ArgumentOutOfRangeException)
            {
                //Debug.Log("<color=red>" + wayID + "</color>");
                return;
            }

            wayList.RemoveAt(wayIndex);
            wayList.Add(w1);
            wayList.Add(w2);

            //UPDATE REST OF INTERSECTIONS
            for(int i = 0 ; i < intersections.Count ; i++)
            {
                if(intersections[i].wayIds.Exists(item => item == wayID))
                {
                    IntersectionNode node = intersections[i];
                    
                    for(int k = 0 ; k < node.wayIds.Count ; k++)
                    {
                        if(node.wayIds[k] == wayID)
                        {
                            if (node.intersectionIndex[k] < intersectionIndex)
                                node.wayIds[k] = w1.id;
                            else
                            {
                                node.wayIds[k] = w2.id;
                                node.intersectionIndex[k] -= intersectionIndex;
                            }
                        }
                    }
                    


                }
            }


        }

        private void fillIntersectionHole(List<intersectionInfo> intersectionInfos, bool hasTrafficLight, string intersectionID)
        {
            Vector3[] vertices = orderIntersectionVertices(intersectionInfos);
            if (vertices == null)
                return;

            int[] triangles = new int[(intersections.Count-2)*3];
            Vector3[] normals = new Vector3[intersectionInfos.Count];
            Vector2[] uv = new Vector2[intersectionInfos.Count];
            int intersectionIndex = intersections.FindIndex(item => item.node.id == intersectionID);

            if (hasTrafficLight)
            {
                if (intersections[intersectionIndex].trafficLights != null)
                {
                    GameObject.Destroy(intersections[intersectionIndex].trafficLights[0].object3D);
                    intersections[intersectionIndex].trafficLights.RemoveAt(0);
                }

                IntersectionNode Node = intersections[intersectionIndex];
                Node.trafficLights = new List<Object3D>();
                Node.trafficLights.Add(DefaultObject3DHandler.drawTrafficSign(vertices[UnityEngine.Random.Range(0, vertices.Length)]));
                intersections[intersectionIndex] = Node;
            }



            int itr = 0;
            for(int i = 1 ; i < intersectionInfos.Count-1 ; i++)
            {
                triangles[itr++] = 0;
                triangles[itr++] = i;
                triangles[itr++] = i+1;
            }
            for(int i = 0 ; i < intersectionInfos.Count ; i++)
                normals[i] = Vector3.up;

            //UV CALCULATION
            float lowestX, lowestZ, highestX, highestZ;

            lowestX = intersectionInfos[0].vertice.x;
            lowestZ = intersectionInfos[0].vertice.z;
            highestX = intersectionInfos[0].vertice.x;
            highestZ = intersectionInfos[0].vertice.z;


            for (int i = 1; i < intersectionInfos.Count; i++)
            {
                if (intersectionInfos[i].vertice.x < lowestX)
                    lowestX = intersectionInfos[i].vertice.x;
                
                if (intersectionInfos[i].vertice.z < lowestZ)
                    lowestZ = intersectionInfos[i].vertice.z;

                if (intersectionInfos[i].vertice.x > highestX)
                    highestX = intersectionInfos[i].vertice.x;

                if (intersectionInfos[i].vertice.z > highestZ)
                    highestZ = intersectionInfos[i].vertice.z;

            }

            float rangeX = highestX - lowestX;
            float rangeZ = highestZ - lowestZ;

            float offsetX = 0.0f - lowestX;
            float offsetZ = 0.0f - lowestZ;

            for (int i = 0; i < intersectionInfos.Count; i++)
            {
                Vector2 vec = new Vector2(intersectionInfos[i].vertice.x,intersectionInfos[i].vertice.z);
                uv[i] = new Vector2((vec.x + offsetX) / rangeX, (vec.y + offsetZ) / rangeZ);
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;

            IntersectionNode interNode = intersections[intersectionIndex];
            if (interNode.intersectionHole == null)
            {

                interNode.intersectionHole = new GameObject("intersectArea", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

                MeshFilter objectmesh = interNode.intersectionHole.GetComponent<MeshFilter>();
                objectmesh.mesh = mesh;

                MeshRenderer objectrenderer = interNode.intersectionHole.GetComponent<MeshRenderer>();
                objectrenderer.material = (Material)Resources.Load("Materials/Highway/Mat_RoadIntersection");

                MeshCollider objectcollider = interNode.intersectionHole.GetComponent<MeshCollider>();
                objectcollider.sharedMesh = mesh;

                objectrenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            else
            {
                MeshFilter objectmesh = interNode.intersectionHole.GetComponent<MeshFilter>();
                objectmesh.mesh = mesh;

                MeshCollider objectcollider = interNode.intersectionHole.GetComponent<MeshCollider>();
                objectcollider.sharedMesh = mesh;
            }
            intersections[intersectionIndex] = interNode;

        }

        /// <summary>
        /// Order vertices counter-Clock Wise
        /// </summary>
        private Vector3[] orderIntersectionVertices(List<intersectionInfo> intersections)
        {
            int verticeCount = intersections.Count;
            List<intersectionInfo> tmpintersections = new List<intersectionInfo>(intersections);

            Vector3[] vertices = new Vector3[tmpintersections.Count];
            
            //add first vertex
            vertices[0] = tmpintersections[0].vertice + new Vector3(0.0f, 0.2f, 0.0f);
            int nextTofind;
            int count = 1;

            if (tmpintersections[0].isleftright) 
                nextTofind = tmpintersections[0].wayNo1;
            else
                nextTofind = tmpintersections[0].wayNo2;

            tmpintersections.RemoveAt(0);

            while(count != verticeCount)
            {

                int index = tmpintersections.FindIndex(item => (item.wayNo2 == nextTofind || item.wayNo1 == nextTofind));
                if (index == -1)
                    return null;
                vertices[count++] = tmpintersections[index].vertice + new Vector3(0.0f,0.2f,0.0f);


                if (tmpintersections[index].wayNo1 == nextTofind)
                    nextTofind = tmpintersections[index].wayNo2;
                else
                    nextTofind = tmpintersections[index].wayNo1;

                tmpintersections.RemoveAt(index);
            }

            return vertices;
        }


        private void correctPavementIntersection(IntersectionNode intersection,string wayID, Pavement.pavementSide side)
        {
            Highway highwayOrg, highwayFound; 
            Vector3 intersectionPoint;
            Vector2 calculatedIntersection = new Vector2();
            


            int intersectCondition = -1; //0:start-left, 1:start-right, 2:end-left,3:end-right
            int intersectCondition2 = -1; // 0: start , 1 : left;

            //1. Find the point that given highway intersect with the other highway from the given side
            highwayOrg = highwayList.Find(item => item.id == wayID);
            highwayFound = null;
            int indexOrg = intersection.wayIds.FindIndex(item => item == wayID);
            if (intersection.intersectionTypes[indexOrg] == IntersectionType.front)
            {

                if (side == Pavement.pavementSide.left)
                {
                    intersectionPoint = highwayOrg.leftSideVertexes[0]; //start-left
                    intersectCondition = 0;
                }
                else
                {
                    intersectionPoint = highwayOrg.rightSideVertexes[0]; //start-right
                    intersectCondition = 1;
                }
            }
            else
            {

                if (side == Pavement.pavementSide.left)
                {
                    intersectionPoint = highwayOrg.leftSideVertexes[highwayOrg.leftSideVertexes.Count - 1]; //end - left
                    intersectCondition = 2;
                }
                else
                {
                    intersectionPoint = highwayOrg.rightSideVertexes[highwayOrg.rightSideVertexes.Count - 1]; // end - right
                    intersectCondition = 3;
                }
            }

            //2. For the intersection point compare all other highways in the intersection to Detect the highway we are looking for
            
            for(int i = 0 ; i < intersection.wayIds.Count ;i++)
            {
                if (intersection.wayIds[i] == wayID)
                    continue;

                highwayFound = highwayList.Find(item => item.id == intersection.wayIds[i]);

                if(intersection.intersectionTypes[i] == IntersectionType.front) //start
                {

                    if (highwayFound.leftSideVertexes[0] == intersectionPoint || highwayFound.rightSideVertexes[0] == intersectionPoint)
                    {
                        intersectCondition2 = 0;
                        break;
                    }
                }
                
                else //end
                {
                    
                    if (highwayFound.leftSideVertexes[highwayFound.leftSideVertexes.Count - 1] == intersectionPoint ||
                        highwayFound.rightSideVertexes[highwayFound.rightSideVertexes.Count - 1] == intersectionPoint)
                    {
                        intersectCondition2 = 1;
                        break;
                    }                      
                }
            }

            //3. We found the 2 highways that are intersecting from the side stated. Lets Get appropriate pavement obj and calculate
            //intersection points

            Vector2 p1, p2, p3, p4;
            int pavOrgIndex=-1, pavFoundIndex=-1;

            pavOrgIndex = pavementList.FindIndex(item => item.Pavementid == wayID && item.side == side);
            
            
            if(intersectCondition == 1 || intersectCondition == 2)
            {
                if (intersectCondition2 == 0)
                    pavFoundIndex = pavementList.FindIndex(item => item.Pavementid == highwayFound.id && item.side == Pavement.pavementSide.left);
                else if (intersectCondition2 == 1)
                    pavFoundIndex = pavementList.FindIndex(item => item.Pavementid == highwayFound.id && item.side == Pavement.pavementSide.right);
                else
                {
                    Debug.Log("PAVEMENT NOT FOUND EXCEPTION");
                    return;
                }
            }

            if (intersectCondition == 0 || intersectCondition == 3)
            {
                if (intersectCondition2 == 0)
                    pavFoundIndex = pavementList.FindIndex(item => item.Pavementid == highwayFound.id && item.side == Pavement.pavementSide.right);
                else if (intersectCondition2 == 1)
                    pavFoundIndex = pavementList.FindIndex(item => item.Pavementid == highwayFound.id && item.side == Pavement.pavementSide.left);
                else
                {
                    Debug.Log("PAVEMENT NOT FOUND EXCEPTION");
                    return;
                }
            }

            //3.1 Check if other highway has sidewalk at the given side
            if (pavFoundIndex == -1)
            {
                //Debug.Log("No Pavement to Intersect");
                return;
            }

            //3.2 Calculate Points for intersection
            if (intersectCondition == 0) //start-left
            {
                p1 = new Vector2(pavementList[pavOrgIndex].leftSideVertexes[0].x, pavementList[pavOrgIndex].leftSideVertexes[0].z);
                p2 = new Vector2(pavementList[pavOrgIndex].leftSideVertexes[1].x, pavementList[pavOrgIndex].leftSideVertexes[1].z);

                if(intersectCondition2 == 0) // start-right
                {
                    p3 = new Vector2(pavementList[pavFoundIndex].rightSideVertexes[0].x, pavementList[pavFoundIndex].rightSideVertexes[0].z);
                    p4 = new Vector2(pavementList[pavFoundIndex].rightSideVertexes[1].x, pavementList[pavFoundIndex].rightSideVertexes[1].z);            

                    Geometry.getInfiniteLineIntersection(ref calculatedIntersection, p1, p2, p3, p4);
                    Vector3 newVertice = new Vector3(calculatedIntersection.x,
                                                 terrain.getTerrainHeight2(calculatedIntersection.y + terrain.terrainInfo.shiftZ, calculatedIntersection.x + terrain.terrainInfo.shiftX),
                                                 calculatedIntersection.y);
                    pavementList[pavFoundIndex].updateMesh(newVertice, Pavement.pavementSide.right,1);
                    pavementList[pavOrgIndex].updateMesh(newVertice, Pavement.pavementSide.left, 0);

                }
                else //end-left
                {
                    int ind = pavementList[pavFoundIndex].leftSideVertexes.Count;
                    p3 = new Vector2(pavementList[pavFoundIndex].leftSideVertexes[ind-2].x, pavementList[pavFoundIndex].leftSideVertexes[ind-2].z);
                    p4 = new Vector2(pavementList[pavFoundIndex].leftSideVertexes[ind-1].x, pavementList[pavFoundIndex].leftSideVertexes[ind-1].z);

                    Geometry.getInfiniteLineIntersection(ref calculatedIntersection, p1, p2, p3, p4);
                    Vector3 newVertice = new Vector3(calculatedIntersection.x,
                                                 terrain.getTerrainHeight2(calculatedIntersection.y + terrain.terrainInfo.shiftZ, calculatedIntersection.x + terrain.terrainInfo.shiftX),
                                                 calculatedIntersection.y);
                    pavementList[pavFoundIndex].updateMesh(newVertice, Pavement.pavementSide.left,ind-2);
                    pavementList[pavOrgIndex].updateMesh(newVertice, Pavement.pavementSide.left, 0);       
                }

            }
            else if (intersectCondition == 1) //start-right
            {
                p1 = new Vector2(pavementList[pavOrgIndex].rightSideVertexes[0].x, pavementList[pavOrgIndex].rightSideVertexes[0].z);
                p2 = new Vector2(pavementList[pavOrgIndex].rightSideVertexes[1].x, pavementList[pavOrgIndex].rightSideVertexes[1].z);

                if (intersectCondition2 == 0) // start-left
                {
                    p3 = new Vector2(pavementList[pavFoundIndex].leftSideVertexes[0].x, pavementList[pavFoundIndex].leftSideVertexes[0].z);
                    p4 = new Vector2(pavementList[pavFoundIndex].leftSideVertexes[1].x, pavementList[pavFoundIndex].leftSideVertexes[1].z);

                    Geometry.getInfiniteLineIntersection(ref calculatedIntersection, p1, p2, p3, p4);
                    Vector3 newVertice = new Vector3(calculatedIntersection.x,
                                                 terrain.getTerrainHeight2(calculatedIntersection.y + terrain.terrainInfo.shiftZ, calculatedIntersection.x + terrain.terrainInfo.shiftX),
                                                 calculatedIntersection.y);
                    pavementList[pavFoundIndex].updateMesh(newVertice, Pavement.pavementSide.left, 0);
                    pavementList[pavOrgIndex].updateMesh(newVertice, Pavement.pavementSide.right, 1);
                
                }
                else //end-right
                {
                    int ind = pavementList[pavFoundIndex].leftSideVertexes.Count;
                    p3 = new Vector2(pavementList[pavFoundIndex].rightSideVertexes[ind - 2].x, pavementList[pavFoundIndex].rightSideVertexes[ind - 2].z);
                    p4 = new Vector2(pavementList[pavFoundIndex].rightSideVertexes[ind - 1].x, pavementList[pavFoundIndex].rightSideVertexes[ind - 1].z);

                    Geometry.getInfiniteLineIntersection(ref calculatedIntersection, p1, p2, p3, p4);
                    Vector3 newVertice = new Vector3(calculatedIntersection.x,
                                                 terrain.getTerrainHeight2(calculatedIntersection.y + terrain.terrainInfo.shiftZ, calculatedIntersection.x + terrain.terrainInfo.shiftX),
                                                 calculatedIntersection.y);
                    pavementList[pavFoundIndex].updateMesh(newVertice, Pavement.pavementSide.right, ind-1);
                    pavementList[pavOrgIndex].updateMesh(newVertice, Pavement.pavementSide.right, 1);
               
                }
            
            }
            else if (intersectCondition == 2) //end-left
            {
                int ind = pavementList[pavOrgIndex].leftSideVertexes.Count;
                p1 = new Vector2(pavementList[pavOrgIndex].leftSideVertexes[ind-2].x, pavementList[pavOrgIndex].leftSideVertexes[ind-2].z);
                p2 = new Vector2(pavementList[pavOrgIndex].leftSideVertexes[ind-1].x, pavementList[pavOrgIndex].leftSideVertexes[ind-1].z);

                if (intersectCondition2 == 0) // start-left
                {
                    p3 = new Vector2(pavementList[pavFoundIndex].leftSideVertexes[0].x, pavementList[pavFoundIndex].leftSideVertexes[0].z);
                    p4 = new Vector2(pavementList[pavFoundIndex].leftSideVertexes[1].x, pavementList[pavFoundIndex].leftSideVertexes[1].z);

                    Geometry.getInfiniteLineIntersection(ref calculatedIntersection, p1, p2, p3, p4);
                    Vector3 newVertice = new Vector3(calculatedIntersection.x,
                                                 terrain.getTerrainHeight2(calculatedIntersection.y + terrain.terrainInfo.shiftZ, calculatedIntersection.x + terrain.terrainInfo.shiftX),
                                                 calculatedIntersection.y);
                    pavementList[pavFoundIndex].updateMesh(newVertice, Pavement.pavementSide.left, 0);
                    pavementList[pavOrgIndex].updateMesh(newVertice, Pavement.pavementSide.left, ind-2);
                }
                else //end-right
                {
                    int ind2 = pavementList[pavFoundIndex].rightSideVertexes.Count;
                    p3 = new Vector2(pavementList[pavFoundIndex].rightSideVertexes[ind2 - 2].x, pavementList[pavFoundIndex].rightSideVertexes[ind2 - 2].z);
                    p4 = new Vector2(pavementList[pavFoundIndex].rightSideVertexes[ind2 - 1].x, pavementList[pavFoundIndex].rightSideVertexes[ind2 - 1].z);

                    Geometry.getInfiniteLineIntersection(ref calculatedIntersection, p1, p2, p3, p4);
                    Vector3 newVertice = new Vector3(calculatedIntersection.x,
                                                 terrain.getTerrainHeight2(calculatedIntersection.y + terrain.terrainInfo.shiftZ, calculatedIntersection.x + terrain.terrainInfo.shiftX),
                                                 calculatedIntersection.y);
                    pavementList[pavFoundIndex].updateMesh(newVertice, Pavement.pavementSide.right, ind2-1);
                    pavementList[pavOrgIndex].updateMesh(newVertice, Pavement.pavementSide.left, ind-2);
                }
            
            }
            else if (intersectCondition == 3) //end-right
            {
                int ind = pavementList[pavOrgIndex].rightSideVertexes.Count;
                p1 = new Vector2(pavementList[pavOrgIndex].rightSideVertexes[ind-2].x, pavementList[pavOrgIndex].rightSideVertexes[ind-2].z);
                p2 = new Vector2(pavementList[pavOrgIndex].rightSideVertexes[ind-1].x, pavementList[pavOrgIndex].rightSideVertexes[ind-1].z);

                if (intersectCondition2 == 0) // start-right
                {
                    p3 = new Vector2(pavementList[pavFoundIndex].rightSideVertexes[0].x, pavementList[pavFoundIndex].rightSideVertexes[0].z);
                    p4 = new Vector2(pavementList[pavFoundIndex].rightSideVertexes[1].x, pavementList[pavFoundIndex].rightSideVertexes[1].z);

                    Geometry.getInfiniteLineIntersection(ref calculatedIntersection, p1, p2, p3, p4);
                    Vector3 newVertice = new Vector3(calculatedIntersection.x,
                                                 terrain.getTerrainHeight2(calculatedIntersection.y + terrain.terrainInfo.shiftZ, calculatedIntersection.x + terrain.terrainInfo.shiftX),
                                                 calculatedIntersection.y);
                    pavementList[pavFoundIndex].updateMesh(newVertice, Pavement.pavementSide.right, 1);
                    pavementList[pavOrgIndex].updateMesh(newVertice, Pavement.pavementSide.right, ind-1);
                
                }
                else //end-left
                {
                    int ind2 = pavementList[pavFoundIndex].leftSideVertexes.Count;
                    p3 = new Vector2(pavementList[pavFoundIndex].leftSideVertexes[ind2 - 2].x, pavementList[pavFoundIndex].leftSideVertexes[ind2 - 2].z);
                    p4 = new Vector2(pavementList[pavFoundIndex].leftSideVertexes[ind2 - 1].x, pavementList[pavFoundIndex].leftSideVertexes[ind2 - 1].z);

                    Geometry.getInfiniteLineIntersection(ref calculatedIntersection, p1, p2, p3, p4);
                    Vector3 newVertice = new Vector3(calculatedIntersection.x,
                                                 terrain.getTerrainHeight2(calculatedIntersection.y + terrain.terrainInfo.shiftZ, calculatedIntersection.x + terrain.terrainInfo.shiftX),
                                                 calculatedIntersection.y);
                    pavementList[pavFoundIndex].updateMesh(newVertice, Pavement.pavementSide.left, ind2-2);
                    pavementList[pavOrgIndex].updateMesh(newVertice, Pavement.pavementSide.right, ind-1);
                }            
            
            }



        }


        private void correctIntersection(IntersectionNode intersection)
        {

            int roadCount = intersection.pointList.Count;
            List<int> highwayIndexes = new List<int>();

            float waySize = 0;
            for(int i = 0 ; i < roadCount ; i++)
            {
                int index = getHighwayIndex(intersection.wayIds[i]);
                highwayIndexes.Add(index);
                waySize += highwayList[index].waySize;
            }
            
            //Shared waySize at the intersection
            waySize = waySize / (float)roadCount;

            List<Vector3> forwardVectors = new List<Vector3>();
            List<Vector3> leftVectors = new List<Vector3>();
            List<Vector3> rightVectors = new List<Vector3>();
            List<LineSegment> lineSegments = new List<LineSegment>();

            Vector3 up = Vector3.up;

            for(int i = 0 ; i < roadCount ; i++)
            {
                Vector3 v = intersection.pointList[i] - intersection.node.meterPosition;
                forwardVectors.Add(v);
                Vector3 right = Vector3.Cross(up, v).normalized; 
                rightVectors.Add(right);
                leftVectors.Add(-right);

                LineSegment ls = new LineSegment();
                ls.Left1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(-right.x, -right.z) * (waySize / 2.0f);
                ls.Left2 = new Vector2(intersection.pointList[i].x, intersection.pointList[i].z) + new Vector2(-right.x, -right.z) * (waySize/2.0f);

                ls.Right1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(right.x, right.z) * (waySize / 2.0f);
                ls.Right2 = new Vector2(intersection.pointList[i].x, intersection.pointList[i].z) + new Vector2(right.x, right.z) * (waySize / 2.0f);
                lineSegments.Add(ls);
            }


            List<IntersectionAngle> angles = new List<IntersectionAngle>();

            for (int i = 0; i < roadCount - 1; i++)
            {
                for (int j = i+1; j < roadCount; j++)
                {
                    float dot = Vector3.Dot(forwardVectors[i].normalized, forwardVectors[j].normalized);
                    double angle = Math.Acos(dot) * 180.0 / Math.PI;
                    IntersectionAngle ia = new IntersectionAngle();
                    ia.way1No = i;
                    ia.way2No = j;
                    ia.angle = angle;
                    angles.Add(ia);
                }
            }

            List<IntersectionAngle> sortedAngles = angles.OrderBy(o => o.angle).ToList();

            intersectValidator[] truthtable = new intersectValidator[roadCount];
            for(int k = 0 ; k < truthtable.Length ; k++)
            {
                truthtable[k].left = false;
                truthtable[k].right = false;
            }
            List<intersectionInfo> intersectList = new List<intersectionInfo>();

            int iterationCount = 0;

            while (sortedAngles.Count != 0)
            {
                if (iterationCount == 10)
                {
                    throw new System.InvalidOperationException("Sorted Angles entered to a loop");
                    //Debug.Log("ERRROR IN THE WHILE LOOP !!!");
                    //break;
                }
                else
                    iterationCount++;

                //intersect these two
                int way1No = sortedAngles[0].way1No;
                int way2No = sortedAngles[0].way2No;

                intersectionside iside;

                //Normal intersect
                if ((truthtable[way1No].left == false && truthtable[way1No].right == false) || (truthtable[way2No].left == false && truthtable[way2No].right == false))
                {
                    iside = intersectWay(way1No, way2No, intersection, lineSegments[way1No], lineSegments[way2No],
                                                      forwardVectors[way1No], forwardVectors[way2No],sortedAngles[0].angle);
                }
                else //Forced intersect
                {
                    if (truthtable[way1No].left || truthtable[way2No].right)
                        iside = forcedIntersectWay(way1No, way2No, intersection, lineSegments[way1No], lineSegments[way2No],
                                                      forwardVectors[way1No], forwardVectors[way2No], false);
                    else if (truthtable[way1No].right || truthtable[way2No].left)
                        iside = forcedIntersectWay(way1No, way2No, intersection, lineSegments[way1No], lineSegments[way2No],
                                                      forwardVectors[way1No], forwardVectors[way2No], true);
                    else
                    {
                        Debug.Log("SOMETHING SHOULD BE REALLY WRONG ABOUT HIGHWAYS :(  !!!");
                        throw new System.InvalidOperationException("Forced Intersection somehow does not work!");
                        //iside = new intersectionside();
                    }
                }

                if (iside.success)
                {
                    intersectionInfo ii = new intersectionInfo();
                    ii.vertice = iside.vertex;
                    ii.wayNo1 = way1No;
                    ii.wayNo2 = way2No;
                    ii.isleftright = iside.leftright;
                    intersectList.Add(ii);

                    if (iside.leftright == true)
                    {
                        truthtable[way1No].left = true;
                        truthtable[way2No].right = true;
                    }
                    else
                    {
                        truthtable[way1No].right = true;
                        truthtable[way2No].left = true;
                    }

                    //Update AngleList

                    sortedAngles.RemoveAt(0);
                    if (truthtable[way1No].left && truthtable[way1No].right)
                        sortedAngles.RemoveAll(item => (item.way1No == way1No || item.way2No == way1No));
                    if (truthtable[way2No].left && truthtable[way2No].right)
                        sortedAngles.RemoveAll(item => (item.way1No == way2No || item.way2No == way2No));

                }
                else
                {
                    IntersectionAngle ang = sortedAngles[0];
                    ang.angle = 360.0 - ang.angle;
                    sortedAngles.RemoveAt(0);
                    sortedAngles.Add(ang);
                }


            }


            fillIntersectionHole(intersectList, isTrafficLight(intersection.node), intersection.node.id);

        }

        //Helper for correctIntersectionPoint()
        private void correct2wayIntersection(IntersectionNode intersection)
        {
            Vector3 up = new Vector3(0, 1, 0);

            int index1 = getHighwayIndex(intersection.wayIds[0]);
            Vector3 forward1 = intersection.node.meterPosition - intersection.pointList[0];
            //****************************************************************************
            Vector3 right1 = Vector3.Cross(up, forward1);
            right1 = right1.normalized;
            Vector3 left1 = -right1;
            float waySize1 = highwayList[index1].waySize;

            int index2 = getHighwayIndex(intersection.wayIds[1]);
            Vector3 forward2 = intersection.pointList[1] - intersection.node.meterPosition;
            //*****************************************************************************
            Vector3 right2 = Vector3.Cross(up, forward2);
            right2 = right2.normalized;
            Vector3 left2 = -right2;
            float waySize2 = highwayList[index2].waySize;

            int intersectionType = -1;
            if (intersection.intersectionIndex[0] == 0 && intersection.intersectionIndex[1] == 0)
                intersectionType = 0;
            else if (intersection.intersectionIndex[0] == 0 && intersection.intersectionIndex[1] != 0)
                intersectionType = 1;
            else if (intersection.intersectionIndex[0] != 0 && intersection.intersectionIndex[1] == 0)
                intersectionType = 2;
            else if (intersection.intersectionIndex[0] != 0 && intersection.intersectionIndex[1] != 0)
                intersectionType = 3;

            float waySize = (waySize1 + waySize2) / 2.0f;

            //Left Side
            Vector2 p0 = new Vector2(intersection.pointList[0].x, intersection.pointList[0].z) + new Vector2(left1.x, left1.z) * (waySize / 2.0f);
            Vector2 p1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(left1.x, left1.z) * (waySize / 2.0f);
            Vector2 p2 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(left2.x, left2.z) * (waySize / 2.0f);
            Vector2 p3 = new Vector2(intersection.pointList[1].x, intersection.pointList[1].z) + new Vector2(left2.x, left2.z) * (waySize / 2.0f);

            Vector3 newVertexLeft, newVertexRight;

            //INTERSECTION LEFT
            Vector2 iL = new Vector2();
            if (!Geometry.getInfiniteLineIntersection(ref iL, p0, p1, p2, p3))
                newVertexLeft = new Vector3(p1.x, terrain.getTerrainHeight2(p1.y + terrain.terrainInfo.shiftZ, p1.x + terrain.terrainInfo.shiftX), p1.y);
            else
                newVertexLeft = new Vector3(iL.x, terrain.getTerrainHeight2(iL.y + terrain.terrainInfo.shiftZ, iL.x + terrain.terrainInfo.shiftX), iL.y);

            //Right Side
            p0 = new Vector2(intersection.pointList[0].x, intersection.pointList[0].z) + new Vector2(right1.x, right1.z) * (waySize / 2.0f);
            p1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(right1.x, right1.z) * (waySize / 2.0f);
            p2 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(right2.x, right2.z) * (waySize / 2.0f);
            p3 = new Vector2(intersection.pointList[1].x, intersection.pointList[1].z) + new Vector2(right2.x, right2.z) * (waySize / 2.0f);


            //INTERSECTION RIGHT
            Vector2 iR = new Vector2();
            if (!Geometry.getInfiniteLineIntersection(ref iR, p0, p1, p2, p3))
                newVertexRight = new Vector3(p1.x, terrain.getTerrainHeight2(p1.y + terrain.terrainInfo.shiftZ, p1.x + terrain.terrainInfo.shiftX), p1.y);
            else
                newVertexRight = new Vector3(iR.x, terrain.getTerrainHeight2(iR.y + terrain.terrainInfo.shiftZ, iR.x + terrain.terrainInfo.shiftX), iR.y);

            if(isTrafficLight(intersection.node))
            {
                int intersectionIndex = intersections.FindIndex(item => item.node.id == intersection.node.id);
                if (intersections[intersectionIndex].trafficLights != null)
                {
                    for (int ind = 0; ind < intersections[intersectionIndex].trafficLights.Count; ind++)                    
                        GameObject.Destroy(intersections[intersectionIndex].trafficLights[ind].object3D);                   
                }

                IntersectionNode node = intersections[intersectionIndex];
                node.trafficLights = new List<Object3D>();
                node.trafficLights.Add(DefaultObject3DHandler.drawTrafficSign(newVertexLeft));
                node.trafficLights.Add(DefaultObject3DHandler.drawTrafficSign(newVertexRight));
                intersections[intersectionIndex] = node;    
            }


            if (intersectionType == 0) //start - start
            {
                highwayList[index1].rightSideVertexes[intersection.intersectionIndex[0]] = newVertexLeft;
                highwayList[index2].leftSideVertexes[intersection.intersectionIndex[1]] = newVertexLeft;
                highwayList[index1].leftSideVertexes[intersection.intersectionIndex[0]] = newVertexRight;
                highwayList[index2].rightSideVertexes[intersection.intersectionIndex[1]] = newVertexRight;
            }
            else if (intersectionType == 1) //start - end
            {
                highwayList[index1].rightSideVertexes[intersection.intersectionIndex[0]] = newVertexLeft;
                highwayList[index2].rightSideVertexes[intersection.intersectionIndex[1]] = newVertexLeft;
                highwayList[index1].leftSideVertexes[intersection.intersectionIndex[0]] = newVertexRight;
                highwayList[index2].leftSideVertexes[intersection.intersectionIndex[1]] = newVertexRight;
            }
            else if (intersectionType == 2) // end - start
            {
                highwayList[index1].leftSideVertexes[intersection.intersectionIndex[0]] = newVertexLeft;
                highwayList[index2].rightSideVertexes[intersection.intersectionIndex[1]] = newVertexRight;
                highwayList[index1].rightSideVertexes[intersection.intersectionIndex[0]] = newVertexRight;
                highwayList[index2].leftSideVertexes[intersection.intersectionIndex[1]] = newVertexLeft;
            }
            else if (intersectionType == 3) // end - end
            {
                highwayList[index1].leftSideVertexes[intersection.intersectionIndex[0]] = newVertexLeft;
                highwayList[index2].rightSideVertexes[intersection.intersectionIndex[1]] = newVertexLeft;
                highwayList[index1].rightSideVertexes[intersection.intersectionIndex[0]] = newVertexRight;
                highwayList[index2].leftSideVertexes[intersection.intersectionIndex[1]] = newVertexRight;
            }




        }

        private struct intersectionside
        {
            public bool success;
            public Vector3 vertex;
            public bool leftright; //true = leftright  false = rightleft
        }

        /// <summary>
        /// Tries to intersect way1 and way2 (if the angle <180) 
        /// </summary>
        private intersectionside intersectWay(int way1No, int way2No, IntersectionNode intersection, LineSegment segment1, LineSegment segment2,Vector3 fwd1, Vector3 fwd2,double angle)
        {
            int highwayIndex1 = getHighwayIndex(intersection.wayIds[way1No]);
            int highwayIndex2 = getHighwayIndex(intersection.wayIds[way2No]);

            Vector2 intersect = new Vector2();
            if (Geometry.getHalfVectorIntersection(ref intersect, segment1.Left1, segment1.Left2, fwd1, segment2.Right1, segment2.Right2, fwd2))
            {

                Vector3 newVertex = new Vector3(intersect.x, terrain.getTerrainHeight2(intersect.y + terrain.terrainInfo.shiftZ, intersect.x + terrain.terrainInfo.shiftX), intersect.y);  
                applyIntersection(way1No, way2No,highwayIndex1,highwayIndex2, newVertex, intersection, true);
                
                intersectionside s = new intersectionside();
                s.vertex = newVertex;
                s.leftright = true;
                s.success = true;
                return s;
            }

            else if (Geometry.getHalfVectorIntersection(ref intersect, segment1.Right1, segment1.Right2, fwd1, segment2.Left1, segment2.Left2, fwd2))
            {
                Vector3 newVertex = new Vector3(intersect.x, terrain.getTerrainHeight2(intersect.y + terrain.terrainInfo.shiftZ, intersect.x + terrain.terrainInfo.shiftX), intersect.y); 
                applyIntersection(way1No, way2No,highwayIndex1,highwayIndex2, newVertex, intersection, false);

                intersectionside s = new intersectionside();
                s.vertex = newVertex;
                s.leftright = false;
                s.success = true;
                return s;
            }

            else
            {
                throw new System.InvalidOperationException("Bormal Intersection Failed!!");
                Debug.Log("------------------------------");
                intersectionside s = new intersectionside();
                s.vertex = new Vector3(0, 0, 0);
                s.leftright = false;
                s.success = false;
                return s;
            }


        }

        /// <summary>
        /// Force function to intersect way1 and way2 according to leftright or rightleft orientation
        /// </summary>
        private intersectionside forcedIntersectWay(int way1No, int way2No, IntersectionNode intersection, LineSegment segment1, LineSegment segment2, Vector3 fwd1, Vector3 fwd2, bool leftright)
        {

            int highwayIndex1 = getHighwayIndex(intersection.wayIds[way1No]);
            int highwayIndex2 = getHighwayIndex(intersection.wayIds[way2No]);

            Vector2 intersect = new Vector2();

            if(leftright)
            {
                Geometry.getVectorIntersection(ref intersect, segment1.Left1, segment1.Left2, fwd1, segment2.Right1, segment2.Right2, fwd2);

                if (intersect.magnitude == 0)
                    throw new System.InvalidOperationException("Intersection Failed!");
                
                Vector3 newVertex = new Vector3(intersect.x, terrain.getTerrainHeight2(intersect.y + terrain.terrainInfo.shiftZ, intersect.x + terrain.terrainInfo.shiftX), intersect.y);
                applyIntersection(way1No, way2No, highwayIndex1, highwayIndex2, newVertex, intersection, true);

                intersectionside s = new intersectionside();
                s.vertex = newVertex;
                s.leftright = leftright;
                s.success = true;
                return s;

            }

            else
            {
                Geometry.getVectorIntersection(ref intersect, segment1.Right1, segment1.Right2, fwd1, segment2.Left1, segment2.Left2, fwd2);

                if (intersect.magnitude == 0)
                    throw new System.InvalidOperationException("Intersection Failed!");
                
                Vector3 newVertex = new Vector3(intersect.x, terrain.getTerrainHeight2(intersect.y + terrain.terrainInfo.shiftZ, intersect.x + terrain.terrainInfo.shiftX), intersect.y);
                applyIntersection(way1No, way2No, highwayIndex1, highwayIndex2, newVertex, intersection, false);

                intersectionside s = new intersectionside();
                s.vertex = newVertex;
                s.leftright = leftright;
                s.success = true;
                return s;
            }


        }

        /// <summary>
        /// Updates the highway nodes of way1 and way2 for the detected newVertex
        /// </summary>
        private void applyIntersection(int way1No, int way2No,int highwayIndex1,int highwayIndex2, Vector3 newVertex, IntersectionNode intersection, bool leftRight)
        {

            if(leftRight)
            {
                //Update Way1
                if (intersection.intersectionTypes[way1No] == IntersectionType.front)
                    highwayList[highwayIndex1].leftSideVertexes[0] = newVertex;
                else
                    highwayList[highwayIndex1].rightSideVertexes[highwayList[highwayIndex1].rightSideVertexes.Count - 1] = newVertex;
                //Update Way2
                if (intersection.intersectionTypes[way2No] == IntersectionType.front)
                    highwayList[highwayIndex2].rightSideVertexes[0] = newVertex;
                else
                    highwayList[highwayIndex2].leftSideVertexes[highwayList[highwayIndex2].leftSideVertexes.Count - 1] = newVertex;
            }

            else
            {
                //Update Way1
                if (intersection.intersectionTypes[way1No] == IntersectionType.front)
                    highwayList[highwayIndex1].rightSideVertexes[0] = newVertex;
                else
                    highwayList[highwayIndex1].leftSideVertexes[highwayList[highwayIndex1].leftSideVertexes.Count - 1] = newVertex;
                //Update Way2
                if (intersection.intersectionTypes[way2No] == IntersectionType.front)
                    highwayList[highwayIndex2].leftSideVertexes[0] = newVertex;
                else
                    highwayList[highwayIndex2].rightSideVertexes[highwayList[highwayIndex2].rightSideVertexes.Count - 1] = newVertex;
            }



        }


        /// <summary>
        /// Search for wayList and detects shared nodes, return the result as IntersectionNodeList
        /// </summary>
        /// <returns></returns>
        private List<IntersectionNode> generateIntersectionList()
        {
            List<IntersectionNode> intersectionList = new List<IntersectionNode>();

            for (int i = 0; i < wayList.Count; i++)
            {
                for (int j = 0; j < wayList[i].nodes.Count; j++)
                {
                    highwayType type = Highway.getHighwayType(wayList[i].tags);
                    if (type == highwayType.Railway || type == highwayType.River || type == highwayType.none || type == highwayType.HighwayFootway || type == highwayType.HighwayPedestrian)
                        continue;

                    int index = intersectionList.FindIndex(item => item.node.id == wayList[i].nodes[j].id);
                    if (index == -1) // if node not exist, add it to the intersectionList
                    {
                        IntersectionNode nd = new IntersectionNode();
                        nd.node = wayList[i].nodes[j];
                        nd.pointList = new List<Vector3>();
                        nd.wayIds = new List<string>();
                        nd.intersectionIndex = new List<int>();
                        nd.intersectionTypes = new List<IntersectionType>();
                        nd.count = 0;
                        nd.hasRoadDivision = false;
                        intersectionList.Add(nd);
                        index = intersectionList.Count - 1;
                    }

                    if (j == 0) //connect from beginning
                    {
                        Vector3 point = wayList[i].nodes[1].meterPosition;
                        IntersectionNode nd = intersectionList[index];
                        nd.pointList.Add(point);
                        nd.wayIds.Add(wayList[i].id);
                        nd.intersectionTypes.Add(IntersectionType.front);
                        nd.count++;
                        nd.intersectionIndex.Add(j);
                        intersectionList[index] = nd;
                    }

                    else if (j == wayList[i].nodes.Count - 1) //connect from ending
                    {
                        Vector3 point = wayList[i].nodes[j - 1].meterPosition;
                        IntersectionNode nd = intersectionList[index];
                        nd.intersectionTypes.Add(IntersectionType.end);
                        nd.pointList.Add(point);
                        nd.wayIds.Add(wayList[i].id);
                        nd.count++;
                        nd.intersectionIndex.Add(j);
                        intersectionList[index] = nd;
                    }

                    else // connect from the middle (split way into 2)
                    {
                        IntersectionNode nd = intersectionList[index];

                        Vector3 point1 = wayList[i].nodes[j + 1].meterPosition;
                        nd.pointList.Add(point1);
                        nd.wayIds.Add(wayList[i].id);
                        nd.intersectionIndex.Add(j + 1);
                        nd.intersectionTypes.Add(IntersectionType.middle_from);

                        Vector3 point2 = wayList[i].nodes[j - 1].meterPosition;
                        nd.pointList.Add(point2);
                        nd.wayIds.Add(wayList[i].id);
                        nd.intersectionIndex.Add(j - 1);
                        nd.intersectionTypes.Add(IntersectionType.middle_to);

                        nd.count += 2;
                        nd.hasRoadDivision = true;

                        intersectionList[index] = nd;
                    }

                }
            }

            intersectionList.RemoveAll(item => (item.count < 2 || (item.count == 2 && item.hasRoadDivision == true)));
            return intersectionList;
        }

        /// <summary>
        /// Gets the highwayIndex in the List given wayID
        /// </summary>
        private int getHighwayIndex(string wayID)
        {
            return highwayList.FindIndex(item => item.id == wayID);
        }

        /// <summary>
        /// Gets the way Index in the list given wayID
        /// </summary>
        private int getWayIndex(string wayID)
        {
            return wayList.FindIndex(item => item.id == wayID);
        }

        private bool isTrafficLight(Node nd)
        {
            return (nd.type == ItemEnumerator.nodeType.TrafficSign);
        }

        public void addNewPavement(string pavementID, Pavement.pavementSide side, float size)
        {
            int highwayIndex = getHighwayIndex(pavementID);
            if (side == Pavement.pavementSide.left)
            {
                highwayList[highwayIndex].hasLeftSidewalk = true;
                highwayList[highwayIndex].leftSidewalkSize = size;
            }
            else
            {
                highwayList[highwayIndex].hasRightSideWalk = true;
                highwayList[highwayIndex].rightSidewalkSize = size;
            }

            Pavement newPavement = new Pavement(highwayList[getHighwayIndex(pavementID)], terrain, side);
            pavementList.Add(newPavement);
            newPavement.renderPavement();
        }

        public void deletePavement(string pavementID, Pavement.pavementSide side)
        {
            int highwayIndex = getHighwayIndex(pavementID);
            if (side == Pavement.pavementSide.left)
            {
                highwayList[highwayIndex].hasLeftSidewalk = false;
                highwayList[highwayIndex].leftSidewalkSize = 0;
            }
            else
            {
                highwayList[highwayIndex].hasRightSideWalk = false;
                highwayList[highwayIndex].rightSidewalkSize = 0;
            }

            int pavementIndex = pavementList.FindIndex(item => item.Pavementid == pavementID &&
                                               item.side == side);

            UnityEngine.Object.Destroy(pavementList[pavementIndex].PavementObj);
            pavementList.RemoveAt(pavementIndex);
        }

        public void correctPavement(string pavementID, Pavement.pavementSide side)
        {
            List<IntersectionNode> nodes = intersections.FindAll(item => item.wayIds.Contains(pavementID));

            for (int i = 0; i < nodes.Count; i++)
                correctPavementIntersection(nodes[i], pavementID, side);
        }

        public void resizeHighway(string highwayID, float newSize)
        {

            int highwayIndex = highwayList.FindIndex(item => item.id == highwayID);

            for (int i = 0; i < highwayList[highwayIndex].streetLampList.Count;i++)
            {
                GameObject.Destroy(highwayList[highwayIndex].streetLampList[i].object3D);
            }

            highwayList[highwayIndex].streetLampList.Clear();



            highwayList[highwayIndex].waySize = newSize;
            highwayList[highwayIndex].generateInitial3Dway(terrain);


            List<IntersectionNode> allIntersections = intersections.FindAll(item => item.wayIds.Contains(highwayID));
            for (int i = 0; i < allIntersections.Count; i++)
                resetIntersectionNode(allIntersections[i]);

            for (int i = 0; i < allIntersections.Count; i++)
            {
                if (allIntersections[i].wayIds.Count == 2)
                    correct2wayIntersection(allIntersections[i]);
                else
                    correctIntersection(allIntersections[i]);
            }

            DrapeHighway draper = new DrapeHighway();
            draper.DrapeRoad(terrain, highwayList[highwayIndex].leftSideVertexes, highwayList[highwayIndex].rightSideVertexes);            

            for (int i = 0; i < allIntersections.Count; i++)
            {
                    for (int k = 0; k < allIntersections[i].wayIds.Count; k++)
                    {
                        int hwIndex = highwayList.FindIndex(item => item.id == allIntersections[i].wayIds[k]);
                        highwayList[hwIndex].updateHighwayMesh();

                        if (highwayList[highwayIndex].hasLeftSidewalk)
                        {
                            float size = highwayList[highwayIndex].leftSidewalkSize;
                            deletePavement(highwayList[highwayIndex].id, Pavement.pavementSide.left);
                            addNewPavement(highwayList[highwayIndex].id, Pavement.pavementSide.left, size);
                            correctPavement(highwayList[highwayIndex].id, Pavement.pavementSide.left);
                        }
                        if (highwayList[highwayIndex].hasRightSideWalk)
                        {
                            float size = highwayList[highwayIndex].rightSidewalkSize;
                            deletePavement(highwayList[highwayIndex].id, Pavement.pavementSide.right);
                            addNewPavement(highwayList[highwayIndex].id, Pavement.pavementSide.right, size);
                            correctPavement(highwayList[highwayIndex].id, Pavement.pavementSide.right);
                        }

                    }
            }


      
        }

        private void resetIntersectionNode(IntersectionNode node)
        {
            for(int i = 0 ; i < node.wayIds.Count ; i++)
            {
                Highway hway = highwayList.Find(item => item.id == node.wayIds[i]);
                Vector3 forward = (node.pointList[i] - node.node.meterPosition).normalized;
                Vector3 right = Vector3.Cross(Vector3.up,forward);
                Vector3 left = -right;

                if(node.intersectionTypes[i] == IntersectionType.front)
                {
                    hway.leftSideVertexes[0] = hway.way.nodes[0].meterPosition + left * (hway.waySize/2.0f);
                    hway.rightSideVertexes[0] = hway.way.nodes[0].meterPosition + right * (hway.waySize / 2.0f);
                }
                else if(node.intersectionTypes[i] == IntersectionType.end)
                {
                    hway.leftSideVertexes[hway.leftSideVertexes.Count - 1] = hway.way.nodes[hway.way.nodes.Count-1].meterPosition + right * (hway.waySize / 2.0f);
                    hway.rightSideVertexes[hway.rightSideVertexes.Count - 1] = hway.way.nodes[hway.way.nodes.Count - 1].meterPosition + left * (hway.waySize / 2.0f);
                }

            }

        }
    }
}
