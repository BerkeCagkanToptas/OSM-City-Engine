//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Assets.Scripts.HeightMap;
//using Assets.Scripts.ConfigHandler;
//using Assets.Scripts.Utils;
//using Assets.Scripts.OpenStreetMap;
//using UnityEngine;

//namespace Assets.Scripts.SceneObjects
//{

//    class HighwayListModeller
//    {
//        private enum IntersectionType
//        {
//            front,
//            end,
//            middle
//        }
//        private struct IntersectionNode
//        {
//            public Node node;
//            public List<Vector3> pointList; //from node to outside
//            public List<int> intersectionIndex;
//            public List<IntersectionType> intersectionTypes;
//            public List<string> wayIds;

//            public int count;
//            public bool hasRoadDivision;
//        }

//        public List<Highway> highwayList;
//        private List<IntersectionNode> intersections;

//        protected myTerrain terrain;
//        protected List<HighwayConfigurations> configurations;

//        public HighwayListModeller(List<Way> _wayList, myTerrain _terrain, List<HighwayConfigurations> _configurations)
//        {
//            highwayList = new List<Highway>();
//            terrain = _terrain;
//            configurations = _configurations;
//            generateHighwayList(_wayList);
//        }

//        public void renderHighwayList()
//        {
//            for (int i = 0; i < highwayList.Count; i++)
//                highwayList[i].renderHighway();
//        }

//        private void generateHighwayList(List<Way> wayList)
//        {

//            for (int i = 0; i < wayList.Count; i++)
//            {
//                Highway newHighway = new Highway(wayList[i], configurations, terrain);
//                highwayList.Add(newHighway);
//            }

//            intersections = generateIntersectionList(wayList);

//            for (int k = 0; k < intersections.Count; k++)
//                correctIntersectionPoint(intersections[k]);

//            for (int i = 0; i < highwayList.Count; i++)
//                highwayList[i].DrapeRoad(terrain);
//        }

//        private void correctIntersectionPoint(IntersectionNode intersection)
//        {
//            if (intersection.count == 2)
//                correct2wayIntersection(intersection);
//            else if (intersection.count == 3)
//            {
//                correct3wayIntersection(intersection);
//                correct3wayIntersection2(intersection);
//            }
//            else if (intersection.count == 4)
//                correct4wayIntersection(intersection);
//        }

//        //Helper for correctIntersectionPoint()
//        private void correct2wayIntersection(IntersectionNode intersection)
//        {
//            Vector3 up = new Vector3(0, 1, 0);

//            int index1 = getHighwayIndex(intersection.wayIds[0]);
//            Vector3 forward1 = intersection.node.meterPosition - intersection.pointList[0];
//            Vector3 right1 = Vector3.Cross(forward1, up);
//            right1 = right1.normalized;
//            Vector3 left1 = -right1;
//            float waySize1 = highwayList[index1].waySize;

//            int index2 = getHighwayIndex(intersection.wayIds[1]);
//            Vector3 forward2 = intersection.pointList[1] - intersection.node.meterPosition;
//            Vector3 right2 = Vector3.Cross(forward2, up);
//            right2 = right2.normalized;
//            Vector3 left2 = -right2;
//            float waySize2 = highwayList[index2].waySize;

//            int intersectionType = -1;
//            if (intersection.intersectionIndex[0] == 0 && intersection.intersectionIndex[1] == 0)
//                intersectionType = 0;
//            else if (intersection.intersectionIndex[0] == 0 && intersection.intersectionIndex[1] != 0)
//                intersectionType = 1;
//            else if (intersection.intersectionIndex[0] == 0 && intersection.intersectionIndex[1] != 0)
//                intersectionType = 2;
//            else if (intersection.intersectionIndex[0] != 0 && intersection.intersectionIndex[1] == 0)
//                intersectionType = 3;

//            float waySize = (waySize1 + waySize2) / 2.0f;

//            //Left Side
//            Vector2 p0 = new Vector2(intersection.pointList[0].x, intersection.pointList[0].z) + new Vector2(left1.x, left1.z) * (waySize / 2.0f);
//            Vector2 p1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(left1.x, left1.z) * (waySize / 2.0f);
//            Vector2 p2 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(left2.x, left2.z) * (waySize / 2.0f);
//            Vector2 p3 = new Vector2(intersection.pointList[1].x, intersection.pointList[1].z) + new Vector2(left2.x, left2.z) * (waySize / 2.0f);

//            Vector3 newVertexLeft, newVertexRight;

//            //INTERSECTION LEFT
//            Vector2 iL = new Vector2();
//            if (!Geometry.getInfiniteLineIntersection(ref iL, p0, p1, p2, p3))
//                newVertexLeft = new Vector3(p1.x, terrain.getTerrainHeight2(p1.y + terrain.terrainInfo.shiftZ, p1.x + terrain.terrainInfo.shiftX), p1.y);
//            else
//                newVertexLeft = new Vector3(iL.x, terrain.getTerrainHeight2(iL.y + terrain.terrainInfo.shiftZ, iL.x + terrain.terrainInfo.shiftX), iL.y);

//            //Right Side
//            p0 = new Vector2(intersection.pointList[0].x, intersection.pointList[0].z) + new Vector2(right1.x, right1.z) * (waySize / 2.0f);
//            p1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(right1.x, right1.z) * (waySize / 2.0f);
//            p2 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(right2.x, right2.z) * (waySize / 2.0f);
//            p3 = new Vector2(intersection.pointList[1].x, intersection.pointList[1].z) + new Vector2(right2.x, right2.z) * (waySize / 2.0f);


//            //INTERSECTION RIGHT
//            Vector2 iR = new Vector2();
//            if (!Geometry.getInfiniteLineIntersection(ref iR, p0, p1, p2, p3))
//                newVertexRight = new Vector3(p1.x, terrain.getTerrainHeight2(p1.y + terrain.terrainInfo.shiftZ, p1.x + terrain.terrainInfo.shiftX), p1.y);
//            else
//                newVertexRight = new Vector3(iR.x, terrain.getTerrainHeight2(iR.y + terrain.terrainInfo.shiftZ, iR.x + terrain.terrainInfo.shiftX), iR.y);


//            if (intersectionType == 0)
//            {
//                highwayList[index1].rightSideVertexes[intersection.intersectionIndex[0]] = newVertexLeft;
//                highwayList[index2].leftSideVertexes[intersection.intersectionIndex[1]] = newVertexLeft;
//                highwayList[index1].leftSideVertexes[intersection.intersectionIndex[0]] = newVertexRight;
//                highwayList[index2].rightSideVertexes[intersection.intersectionIndex[1]] = newVertexRight;
//            }
//            else if (intersectionType == 1)
//            {
//                highwayList[index1].rightSideVertexes[intersection.intersectionIndex[0]] = newVertexLeft;
//                highwayList[index2].rightSideVertexes[intersection.intersectionIndex[1]] = newVertexLeft;
//                highwayList[index1].leftSideVertexes[intersection.intersectionIndex[0]] = newVertexRight;
//                highwayList[index2].leftSideVertexes[intersection.intersectionIndex[1]] = newVertexRight;
//            }
//            else if (intersectionType == 2)
//            {
//                highwayList[index1].leftSideVertexes[intersection.intersectionIndex[0]] = newVertexLeft;
//                highwayList[index2].rightSideVertexes[intersection.intersectionIndex[1]] = newVertexLeft;
//                highwayList[index1].rightSideVertexes[intersection.intersectionIndex[0]] = newVertexRight;
//                highwayList[index2].leftSideVertexes[intersection.intersectionIndex[1]] = newVertexRight;
//            }
//            else if (intersectionType == 3)
//            {
//                highwayList[index1].leftSideVertexes[intersection.intersectionIndex[0]] = newVertexLeft;
//                highwayList[index2].leftSideVertexes[intersection.intersectionIndex[1]] = newVertexLeft;
//                highwayList[index1].rightSideVertexes[intersection.intersectionIndex[0]] = newVertexRight;
//                highwayList[index2].rightSideVertexes[intersection.intersectionIndex[1]] = newVertexRight;
//            }




//        }

//        //Helper for correctIntersectionPoint()
//        private void correct3wayIntersection(IntersectionNode intersection)
//        {
//            Vector3 up = new Vector3(0, 1, 0);

//            if (!intersection.hasRoadDivision)
//                return;

//            string dividedWwayid, intersectWayid;
//            int dividedPointIndex, intersectPointIndex;

//            if (intersection.wayIds[0] == intersection.wayIds[1])
//            {
//                dividedWwayid = intersection.wayIds[0];
//                intersectWayid = intersection.wayIds[2];
//                dividedPointIndex = 0;
//                intersectPointIndex = 2;
//            }
//            else
//            {
//                intersectWayid = intersection.wayIds[0];
//                dividedWwayid = intersection.wayIds[2];
//                dividedPointIndex = 1;
//                intersectPointIndex = 0;
//            }

//            int indexDivided = getHighwayIndex(dividedWwayid);
//            int indexIntersect = getHighwayIndex(intersectWayid);

//            //LEFT SIDE OF DIVIDED WAY
//            Vector3 pL0 = highwayList[indexDivided].leftSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1]];
//            Vector3 pL1 = highwayList[indexDivided].leftSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1];
//            Vector3 pL2 = highwayList[indexDivided].leftSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 2];
//            //RIGHT SIDE OF DIVIDED WAY
//            Vector3 pR0 = highwayList[indexDivided].rightSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1]];
//            Vector3 pR1 = highwayList[indexDivided].rightSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1];
//            Vector3 pR2 = highwayList[indexDivided].rightSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 2];

//            Vector3 ppL0, ppL1, ppR0, ppR1;

//            if (intersection.intersectionTypes[intersectPointIndex] == IntersectionType.front)
//            {
//                //LEFT SIDE OF INTERSECTED WAY
//                ppL0 = highwayList[indexIntersect].leftSideVertexes[0];
//                ppL1 = highwayList[indexIntersect].leftSideVertexes[1];
//                //RIGHT SIDE OF INTERSECTED WAY
//                ppR0 = highwayList[indexIntersect].rightSideVertexes[0];
//                ppR1 = highwayList[indexIntersect].rightSideVertexes[1];

//                Vector2 newIntersection = new Vector2();
//                if (Geometry.getLineIntersection(ref newIntersection, pL0, pL1, ppL0, ppL1))
//                {

//                    Debug.Log("INTERSECTION  LEFT SIDE FROM CONTINUOUS ROAD FROM BEGINNING");
//                    //Intersect from left Side - Intersected road Connect From the Beginning
//                    Vector3 vertexLeft1 = new Vector3(newIntersection.x, terrain.getTerrainHeight2(newIntersection.y + terrain.terrainInfo.shiftZ, newIntersection.x + terrain.terrainInfo.shiftX), newIntersection.y);
//                    Vector2 newIntersection2 = new Vector2();
//                    bool hasIntersect = Geometry.getLineIntersection(ref newIntersection2, pL1, pL2, ppR0, ppR1);

//                    if (!hasIntersect)
//                    {
//                        float ratio = (vertexLeft1 - pL0).magnitude / (pL1 - pL0).magnitude;
//                        Vector3 vertexRight = pR0 + (pR1 - pR0) * ratio;

//                        highwayList[indexDivided].leftSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 1, vertexLeft1);
//                        highwayList[indexDivided].rightSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 1, vertexRight);
//                        highwayList[indexIntersect].leftSideVertexes[0] = vertexLeft1;
//                        return;
//                    }

//                    Vector3 vertexLeft2 = new Vector3(newIntersection2.x, terrain.getTerrainHeight2(newIntersection2.y + terrain.terrainInfo.shiftZ, newIntersection2.x + terrain.terrainInfo.shiftX), newIntersection2.y);

//                    float ratio1 = (vertexLeft1 - pL0).magnitude / (pL1 - pL0).magnitude;
//                    float ratio2 = (vertexLeft2 - pL1).magnitude / (pL2 - pL1).magnitude;

//                    Vector3 vertexRight1 = pR0 + (pR1 - pR0) * ratio1;
//                    Vector3 vertexRight2 = pR1 + (pR2 - pR1) * ratio2;

//                    //GameObject debugSphereLeft1 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereTerrain"));
//                    //debugSphereLeft1.transform.position = vertexLeft1;
//                    //debugSphereLeft1.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);
//                    //GameObject debugSphereLeft2 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereVertice"));
//                    //debugSphereLeft2.transform.position = vertexLeft2;
//                    //debugSphereLeft2.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);

//                    highwayList[indexIntersect].leftSideVertexes[0] = vertexLeft1;
//                    highwayList[indexIntersect].rightSideVertexes[0] = vertexLeft2;

//                    highwayList[indexDivided].rightSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1] = vertexRight1;
//                    highwayList[indexDivided].rightSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 2, vertexRight2);

//                    highwayList[indexDivided].leftSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1] = vertexLeft1;
//                    highwayList[indexDivided].leftSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 2, vertexLeft2);
//                }
//                else if (Geometry.getLineIntersection(ref newIntersection, pR0, pR1, ppR0, ppR1))
//                {
//                    Debug.Log("INTERSECTION  RIGHT SIDE FROM CONTINUOUS ROAD FROM BEGINNING");
//                    //Intersect from right Side - Intersected road Connect From the Beginning
//                    Vector3 vertexRight1 = new Vector3(newIntersection.x, terrain.getTerrainHeight2(newIntersection.y + terrain.terrainInfo.shiftZ, newIntersection.x + terrain.terrainInfo.shiftX), newIntersection.y);
//                    Vector2 newIntersection2 = new Vector2();
//                    bool hasIntersect = Geometry.getLineIntersection(ref newIntersection2, pR1, pR2, ppL0, ppL1);

//                    if (!hasIntersect)
//                    {
//                        float ratio = (vertexRight1 - pR0).magnitude / (pR1 - pR0).magnitude;
//                        Vector3 vertexLeft = pL0 + (pL1 - pL0) * ratio;

//                        highwayList[indexDivided].rightSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 1, vertexRight1);
//                        highwayList[indexDivided].leftSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 1, vertexLeft);
//                        highwayList[indexIntersect].rightSideVertexes[0] = vertexRight1;
//                        return;
//                    }

//                    Vector3 vertexRight2 = new Vector3(newIntersection2.x, terrain.getTerrainHeight2(newIntersection2.y + terrain.terrainInfo.shiftZ, newIntersection2.x + terrain.terrainInfo.shiftX), newIntersection2.y);

//                    float ratio1 = (vertexRight1 - pR0).magnitude / (pR1 - pR0).magnitude;
//                    float ratio2 = (vertexRight2 - pR1).magnitude / (pR2 - pR1).magnitude;

//                    Vector3 vertexLeft1 = pL0 + (pL1 - pL0) * ratio1;
//                    Vector3 vertexLeft2 = pL1 + (pL2 - pL1) * ratio2;

//                    //GameObject debugSphereRight1 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereTerrain"));
//                    //debugSphereRight1.transform.position = vertexRight1;
//                    //debugSphereRight1.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);
//                    //GameObject debugSphereRight2 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereVertice"));
//                    //debugSphereRight2.transform.position = vertexRight2;
//                    //debugSphereRight2.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);

//                    highwayList[indexIntersect].leftSideVertexes[0] = vertexRight2;
//                    highwayList[indexIntersect].rightSideVertexes[0] = vertexRight1;

//                    highwayList[indexDivided].rightSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1] = vertexRight1;
//                    highwayList[indexDivided].rightSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 2, vertexRight2);

//                    highwayList[indexDivided].leftSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1] = vertexLeft1;
//                    highwayList[indexDivided].leftSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 2, vertexLeft2);

//                }

//            }
//            else
//            {
//                //LEFT SIDE OF INTERSECTED WAY
//                ppL0 = highwayList[indexIntersect].leftSideVertexes[highwayList[indexIntersect].leftSideVertexes.Count - 1];
//                ppL1 = highwayList[indexIntersect].leftSideVertexes[highwayList[indexIntersect].leftSideVertexes.Count - 2];
//                //RIGHT SIDE OF INTERSECTED WAY
//                ppR0 = highwayList[indexIntersect].rightSideVertexes[highwayList[indexIntersect].rightSideVertexes.Count - 1];
//                ppR1 = highwayList[indexIntersect].rightSideVertexes[highwayList[indexIntersect].rightSideVertexes.Count - 2];

//                Vector2 newIntersection = new Vector2();
//                if (Geometry.getLineIntersection(ref newIntersection, pL0, pL1, ppR1, ppR0))
//                {
//                    //Intersect from left Side - Intersected road Connect From the End
//                    Debug.Log("INTERSECTION  LEFT SIDE FROM CONTINUOUS ROAD FROM ENDING");
//                    Vector3 vertexLeft1 = new Vector3(newIntersection.x, terrain.getTerrainHeight2(newIntersection.y + terrain.terrainInfo.shiftZ, newIntersection.x + terrain.terrainInfo.shiftX), newIntersection.y);
//                    Vector2 newIntersection2 = new Vector2();
//                    bool hasIntersect = Geometry.getLineIntersection(ref newIntersection2, pL1, pL2, ppL0, ppL1);

//                    if (!hasIntersect)
//                    {
//                        float ratio = (vertexLeft1 - pL0).magnitude / (pL1 - pL0).magnitude;
//                        Vector3 vertexRight = pR0 + (pR1 - pR0) * ratio;

//                        highwayList[indexDivided].leftSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 1, vertexLeft1);
//                        highwayList[indexDivided].rightSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 1, vertexRight);
//                        highwayList[indexIntersect].rightSideVertexes[highwayList[indexIntersect].leftSideVertexes.Count - 1] = vertexLeft1;
//                        return;
//                    }

//                    Vector3 vertexLeft2 = new Vector3(newIntersection2.x, terrain.getTerrainHeight2(newIntersection2.y + terrain.terrainInfo.shiftZ, newIntersection2.x + terrain.terrainInfo.shiftX), newIntersection2.y);

//                    float ratio1 = (vertexLeft1 - pL0).magnitude / (pL1 - pL0).magnitude;
//                    float ratio2 = (vertexLeft2 - pL1).magnitude / (pL2 - pL1).magnitude;

//                    Vector3 vertexRight1 = pR0 + (pR1 - pR0) * ratio1;
//                    Vector3 vertexRight2 = pR1 + (pR2 - pR1) * ratio2;

//                    //GameObject debugSphereLeft1 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereVertice"));
//                    //debugSphereLeft1.transform.position = vertexLeft1;
//                    //debugSphereLeft1.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);
//                    //GameObject debugSphereLeft2 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereVertice"));
//                    //debugSphereLeft2.transform.position = vertexLeft2;
//                    //debugSphereLeft2.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);

//                    highwayList[indexIntersect].leftSideVertexes[highwayList[indexIntersect].leftSideVertexes.Count - 1] = vertexLeft2;
//                    highwayList[indexIntersect].rightSideVertexes[highwayList[indexIntersect].rightSideVertexes.Count - 1] = vertexLeft1;

//                    highwayList[indexDivided].rightSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1] = vertexRight1;
//                    highwayList[indexDivided].rightSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 2, vertexRight2);

//                    highwayList[indexDivided].leftSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1] = vertexLeft1;
//                    highwayList[indexDivided].leftSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 2, vertexLeft2);



//                }

//                else if (Geometry.getLineIntersection(ref newIntersection, pR0, pR1, ppL1, ppL0))
//                {
//                    //Intersect from right Side - Intersected road Connect From the End
//                    Debug.Log("INTERSECTION  RIGHT SIDE FROM CONTINUOUS ROAD FROM ENDING");

//                    Vector3 vertexRight1 = new Vector3(newIntersection.x, terrain.getTerrainHeight2(newIntersection.y + terrain.terrainInfo.shiftZ, newIntersection.x + terrain.terrainInfo.shiftX), newIntersection.y);
//                    Vector2 newIntersection2 = new Vector2();
//                    bool hasIntersect = Geometry.getLineIntersection(ref newIntersection2, pR1, pR2, ppR0, ppR1);

//                    if (!hasIntersect)
//                    {
//                        float ratio = (vertexRight1 - pR0).magnitude / (pR1 - pR0).magnitude;
//                        Vector3 vertexLeft = pL0 + (pL1 - pL0) * ratio;

//                        highwayList[indexDivided].rightSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 1, vertexRight1);
//                        highwayList[indexDivided].leftSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 1, vertexLeft);
//                        highwayList[indexIntersect].leftSideVertexes[highwayList[indexIntersect].leftSideVertexes.Count - 1] = vertexRight1;
//                        return;
//                    }

//                    Vector3 vertexRight2 = new Vector3(newIntersection2.x, terrain.getTerrainHeight2(newIntersection2.y + terrain.terrainInfo.shiftZ, newIntersection2.x + terrain.terrainInfo.shiftX), newIntersection2.y);

//                    float ratio1 = (vertexRight1 - pR0).magnitude / (pR1 - pR0).magnitude;
//                    float ratio2 = (vertexRight2 - pR1).magnitude / (pR2 - pR1).magnitude;

//                    Vector3 vertexLeft1 = pL0 + (pL1 - pL0) * ratio1;
//                    Vector3 vertexLeft2 = pL1 + (pL2 - pL1) * ratio2;

//                    //GameObject debugSphereRight1 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereTerrain"));
//                    //debugSphereRight1.transform.position = vertexRight1;
//                    //debugSphereRight1.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);
//                    //GameObject debugSphereRight2 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereTerrain"));
//                    //debugSphereRight2.transform.position = vertexRight2;
//                    //debugSphereRight2.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);

//                    highwayList[indexIntersect].leftSideVertexes[highwayList[indexIntersect].leftSideVertexes.Count - 1] = vertexRight1;
//                    highwayList[indexIntersect].rightSideVertexes[highwayList[indexIntersect].leftSideVertexes.Count - 1] = vertexRight2;

//                    highwayList[indexDivided].rightSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1] = vertexRight1;
//                    highwayList[indexDivided].rightSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 2, vertexRight2);

//                    highwayList[indexDivided].leftSideVertexes[intersection.intersectionIndex[dividedPointIndex + 1] + 1] = vertexLeft1;
//                    highwayList[indexDivided].leftSideVertexes.Insert(intersection.intersectionIndex[dividedPointIndex + 1] + 2, vertexLeft2);

//                }
//            }






//        }

//        private void correct3wayIntersection2(IntersectionNode intersection)
//        {

//            if (intersection.hasRoadDivision)
//                return;

//            int index0 = getHighwayIndex(intersection.wayIds[0]);
//            int index1 = getHighwayIndex(intersection.wayIds[1]);
//            int index2 = getHighwayIndex(intersection.wayIds[2]);

//            // FIND OUTDOMINATED HIGHWAY IN THE INTERSECTION
//            float waySize0 = highwayList[index0].waySize;
//            float waySize1 = highwayList[index1].waySize;
//            float waySize2 = highwayList[index2].waySize;
//            float waySize = (waySize0 + waySize1 + waySize2) / 3.0f;

//            //DEFINING VECTORS
//            Vector3 v0 = intersection.pointList[0] - intersection.node.meterPosition;
//            Vector3 v1 = intersection.pointList[1] - intersection.node.meterPosition;
//            Vector3 v2 = intersection.pointList[2] - intersection.node.meterPosition;

//            Vector3 up = Vector3.up;
//            Vector3 v0Right = Vector3.Cross(v0, up).normalized;
//            Vector3 v0Left = -v0Right;
//            Vector3 v1Right = Vector3.Cross(v1, up).normalized;
//            Vector3 v1Left = -v1Right;
//            Vector3 v2Right = Vector3.Cross(v2, up).normalized;
//            Vector3 v2Left = -v2Right;

//            Vector2 v0PL1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(v0Left.x, v0Left.z) * (waySize / 2.0f);
//            Vector2 v0PL2 = new Vector2(intersection.pointList[0].x, intersection.pointList[0].z) + new Vector2(v0Left.x, v0Left.z) * (waySize0 / 2.0f);
//            Vector2 v1PL1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(v1Left.x, v1Left.z) * (waySize / 2.0f);
//            Vector2 v1PL2 = new Vector2(intersection.pointList[1].x, intersection.pointList[1].z) + new Vector2(v1Left.x, v1Left.z) * (waySize1 / 2.0f);
//            Vector2 v2PL1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(v2Left.x, v2Left.z) * (waySize / 2.0f);
//            Vector2 v2PL2 = new Vector2(intersection.pointList[2].x, intersection.pointList[2].z) + new Vector2(v2Left.x, v2Left.z) * (waySize2 / 2.0f);

//            Vector2 v0PR1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(v0Right.x, v0Right.z) * (waySize / 2.0f);
//            Vector2 v0PR2 = new Vector2(intersection.pointList[0].x, intersection.pointList[0].z) + new Vector2(v0Right.x, v0Right.z) * (waySize0 / 2.0f);
//            Vector2 v1PR1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(v1Right.x, v1Right.z) * (waySize / 2.0f);
//            Vector2 v1PR2 = new Vector2(intersection.pointList[1].x, intersection.pointList[1].z) + new Vector2(v1Right.x, v1Right.z) * (waySize1 / 2.0f);
//            Vector2 v2PR1 = new Vector2(intersection.node.meterPosition.x, intersection.node.meterPosition.z) + new Vector2(v2Right.x, v2Right.z) * (waySize / 2.0f);
//            Vector2 v2PR2 = new Vector2(intersection.pointList[2].x, intersection.pointList[2].z) + new Vector2(v2Right.x, v2Right.z) * (waySize2 / 2.0f);


//            GameObject ds1 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereVertice"));
//            ds1.transform.position = new Vector3(v0PL1.x, terrain.getTerrainHeight2(v0PL1.y + terrain.terrainInfo.shiftZ, v0PL1.x + terrain.terrainInfo.shiftX), v0PL1.y);
//            ds1.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
//            GameObject ds2 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereTerrain"));
//            ds2.transform.position = new Vector3(v1PR1.x, terrain.getTerrainHeight2(v1PR1.y + terrain.terrainInfo.shiftZ, v1PR1.x + terrain.terrainInfo.shiftX), v1PR1.y);
//            ds2.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);

//            GameObject ds3 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereGreen"));
//            ds3.transform.position = new Vector3(intersection.node.meterPosition.x, terrain.getTerrainHeight2(intersection.node.meterPosition.z + terrain.terrainInfo.shiftZ, intersection.node.meterPosition.x + terrain.terrainInfo.shiftX), intersection.node.meterPosition.z);
//            ds3.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);

//            GameObject ds4 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSpherePink"));
//            ds4.transform.position = new Vector3(intersection.pointList[0].x, terrain.getTerrainHeight2(intersection.pointList[0].z + terrain.terrainInfo.shiftZ, intersection.pointList[0].x + terrain.terrainInfo.shiftX), intersection.pointList[0].z);
//            ds4.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            


//            float dot1 = Vector3.Dot(v0.normalized, v1.normalized);
//            float dot2 = Vector3.Dot(v0.normalized, v2.normalized);
//            float dot3 = Vector3.Dot(v1.normalized, v2.normalized);

//            double angle1 = Math.Acos(dot1) * 180.0 / Math.PI;
//            double angle2 = Math.Acos(dot2) * 180.0 / Math.PI;
//            double angle3 = Math.Acos(dot3) * 180.0 / Math.PI;


//            int conditionNumber = -1;

//            Vector2 intersectionA = new Vector2();
//            Vector2 intersectionB = new Vector2();
//            Vector2 intersectionC = new Vector2();
//            Vector3 newVerticeA = new Vector3();
//            Vector3 newVerticeB = new Vector3();
//            Vector3 newVerticeC = new Vector3();


//            if (angle1 <= angle2 && angle1 <= angle3)
//            {

//                if (Geometry.getHalfVectorIntersection(ref intersectionA, v0PL1, v0PL2, v0, v1PR1, v1PR2, v1))
//                    conditionNumber = 1;
//                else if (Geometry.getHalfVectorIntersection(ref intersectionA, v0PR1, v0PR2, v0, v1PL1, v1PL2, v1))
//                    conditionNumber = 2;
//                else
//                    Debug.Log("<color=green> !!! RIOOOOT angle1 has problem !!!</color>");
//            }
//            else if (angle2 <= angle3 && angle2 <= angle1)
//            {
//                if (Geometry.getHalfVectorIntersection(ref intersectionA, v0PL1, v0PL2, v0, v2PR1, v2PR2, v2))
//                    conditionNumber = 2;
//                else if (Geometry.getHalfVectorIntersection(ref intersectionA, v0PR1, v0PR2, v0, v2PL1, v2PL2, v2))
//                    conditionNumber = 1;
//                else
//                    Debug.Log("<color=green> !!! RIOOOOT angle2 has problem !!!</color>");

//            }
//            else if (angle3 <= angle2 && angle3 <= angle1)
//            {
//                if (Geometry.getHalfVectorIntersection(ref intersectionA, v1PL1, v1PL2, v1, v2PR1, v2PR2, v2))
//                    conditionNumber = 1;
//                else if (Geometry.getHalfVectorIntersection(ref intersectionA, v1PR1, v1PR2, v1, v2PL1, v2PL2, v2))
//                    conditionNumber = 2;
//                else
//                    Debug.Log("<color=green> !!! RIOOOOT angle3 has problem !!!</color>");
//            }
//            else
//                Debug.Log(" I WANT TO CRY :(  ");


//            switch (conditionNumber)
//            {
//                case -1:
//                    return;
//                case 1: //counterclockwise
//                    Geometry.getVectorIntersection(ref intersectionA, v0PL1, v0PL2, v0, v1PR1, v1PR2, v1);
//                    Geometry.getVectorIntersection(ref intersectionB, v1PL1, v1PL2, v1, v2PR1, v2PR2, v2);
//                    Geometry.getVectorIntersection(ref intersectionC, v2PL1, v2PL2, v2, v0PR1, v0PR2, v0);
//                    break;
//                case 2: //clockwise
//                    Geometry.getVectorIntersection(ref intersectionA, v0PR1, v0PR2, v0, v1PL1, v1PL2, v1);
//                    Geometry.getVectorIntersection(ref intersectionB, v1PR1, v1PR2, v1, v2PL1, v2PL2, v2);
//                    Geometry.getVectorIntersection(ref intersectionC, v2PR1, v2PR2, v2, v0PL1, v0PL2, v0);
//                    break;
//            }

//            newVerticeA = new Vector3(intersectionA.x, terrain.getTerrainHeight2(intersectionA.y + terrain.terrainInfo.shiftZ, intersectionA.x + terrain.terrainInfo.shiftX), intersectionA.y);
//            newVerticeB = new Vector3(intersectionB.x, terrain.getTerrainHeight2(intersectionB.y + terrain.terrainInfo.shiftZ, intersectionB.x + terrain.terrainInfo.shiftX), intersectionB.y);
//            newVerticeC = new Vector3(intersectionC.x, terrain.getTerrainHeight2(intersectionC.y + terrain.terrainInfo.shiftZ, intersectionC.x + terrain.terrainInfo.shiftX), intersectionC.y);

//            double maxangle = Math.Max(angle1, Math.Max(angle2, angle3));
//            if (maxangle == angle1)
//            {
//                return;
//                float ratio1, ratio2;
//                if (conditionNumber == 1)
//                {
//                    ratio1 = (intersectionC - v0PR1).magnitude / (v0PR2 - v0PR1).magnitude;
//                    ratio2 = (intersectionB - v1PL1).magnitude / (v1PL2 - v1PL1).magnitude;
//                }
//                else
//                {
//                    ratio1 = (intersectionC - v0PL1).magnitude / (v0PL2 - v0PL1).magnitude;
//                    ratio2 = (intersectionB - v1PR1).magnitude / (v1PR2 - v1PR1).magnitude;
//                }

//                //for v0
//                Vector3 newVerticeA1 = new Vector3((newVerticeA + v0 * ratio1).x,
//                                        terrain.getTerrainHeight2((newVerticeA + v0 * ratio1).z + terrain.terrainInfo.shiftZ, (newVerticeA + v0 * ratio1).x + terrain.terrainInfo.shiftX),
//                                        (newVerticeA + v0 * ratio1).z);
//                //for v1
//                Vector3 newVerticeA2 = new Vector3((newVerticeA + v1 * ratio2).x,
//                                        terrain.getTerrainHeight2((newVerticeA + v1 * ratio2).z + terrain.terrainInfo.shiftZ, (newVerticeA + v1 * ratio2).x + terrain.terrainInfo.shiftX),
//                                        (newVerticeA + v1 * ratio2).z);

//                if ((intersection.intersectionTypes[0] == IntersectionType.front && conditionNumber == 1) || (intersection.intersectionTypes[0] == IntersectionType.end && conditionNumber == 2))
//                {

//                    highwayList[index0].leftSideVertexes[intersection.intersectionIndex[0]] = newVerticeA1;
//                    highwayList[index0].rightSideVertexes[intersection.intersectionIndex[0]] = newVerticeC;

//                    if (intersection.intersectionTypes[0] == IntersectionType.front)
//                    {
//                        highwayList[index0].leftSideVertexes.Insert(intersection.intersectionIndex[0], newVerticeA2);
//                        highwayList[index0].rightSideVertexes.Insert(intersection.intersectionIndex[0], newVerticeB);
//                    }
//                    else
//                    {
//                        highwayList[index0].leftSideVertexes.Add(newVerticeA2);
//                        highwayList[index0].rightSideVertexes.Add(newVerticeB);
//                    }
//                }
//                else
//                {
//                    highwayList[index0].rightSideVertexes[intersection.intersectionIndex[0]] = newVerticeA1;
//                    highwayList[index0].leftSideVertexes[intersection.intersectionIndex[0]] = newVerticeC;

//                    if (intersection.intersectionTypes[0] == IntersectionType.front)
//                    {
//                        highwayList[index0].rightSideVertexes.Insert(intersection.intersectionIndex[0], newVerticeA2);
//                        highwayList[index0].leftSideVertexes.Insert(intersection.intersectionIndex[0], newVerticeB);
//                    }
//                    else
//                    {
//                        highwayList[index0].rightSideVertexes.Add(newVerticeA2);
//                        highwayList[index0].leftSideVertexes.Add(newVerticeB);
//                    }
//                }

//                if ((intersection.intersectionTypes[1] == IntersectionType.front && conditionNumber == 1) || (intersection.intersectionTypes[1] == IntersectionType.end && conditionNumber == 2))
//                {
//                    highwayList[index1].rightSideVertexes[intersection.intersectionIndex[1]] = newVerticeA2;
//                    highwayList[index1].leftSideVertexes[intersection.intersectionIndex[1]] = newVerticeB;
//                }
//                else
//                {
//                    highwayList[index1].leftSideVertexes[intersection.intersectionIndex[1]] = newVerticeA2;
//                    highwayList[index1].rightSideVertexes[intersection.intersectionIndex[1]] = newVerticeB;
//                }

//                if ((intersection.intersectionTypes[2] == IntersectionType.front && conditionNumber == 1) || (intersection.intersectionTypes[2] == IntersectionType.end && conditionNumber == 2))
//                {
//                    highwayList[index2].rightSideVertexes[intersection.intersectionIndex[2]] = newVerticeB;
//                    highwayList[index2].leftSideVertexes[intersection.intersectionIndex[2]] = newVerticeC;
//                }
//                else
//                {
//                    highwayList[index2].leftSideVertexes[intersection.intersectionIndex[2]] = newVerticeB;
//                    highwayList[index2].rightSideVertexes[intersection.intersectionIndex[2]] = newVerticeC;
//                }

//            }
//            else if (maxangle == angle2)
//            {
//                //Intersection C will be divided into 2
//                //0 and 2 intersection
//                float ratio1, ratio2;
//                if (conditionNumber == 1)
//                {
//                    ratio1 = (intersectionA - v0PL1).magnitude / (v0PL2 - v0PL1).magnitude;
//                    ratio2 = (intersectionB - v2PR1).magnitude / (v2PR2 - v2PR1).magnitude;
//                }
//                else
//                {
//                    ratio1 = (intersectionA - v0PR1).magnitude / (v0PR2 - v0PR1).magnitude;
//                    ratio2 = (intersectionB - v2PL1).magnitude / (v2PL2 - v2PL1).magnitude;
//                }

//                Vector3 newVerticeC1 = new Vector3((newVerticeC + v0 * ratio1).x,
//                                       terrain.getTerrainHeight2((newVerticeC + v0 * ratio1).z + terrain.terrainInfo.shiftZ, (newVerticeC + v0 * ratio1).x + terrain.terrainInfo.shiftX),
//                                       (newVerticeC + v0 * ratio1).z);

//                Vector3 newVerticeC2 = new Vector3((newVerticeC + v2 * ratio2).x,
//                                       terrain.getTerrainHeight2((newVerticeC + v2 * ratio2).z + terrain.terrainInfo.shiftZ, (newVerticeC + v2 * ratio2).x + terrain.terrainInfo.shiftX),
//                                       (newVerticeC + v2 * ratio2).z);

//                if ((intersection.intersectionTypes[0] == IntersectionType.front && conditionNumber == 1) || (intersection.intersectionTypes[0] == IntersectionType.end && conditionNumber == 2))
//                {
//                    highwayList[index0].leftSideVertexes[intersection.intersectionIndex[0]] = newVerticeA;
//                    highwayList[index0].rightSideVertexes[intersection.intersectionIndex[0]] = newVerticeC1;

//                    if (intersection.intersectionTypes[0] == IntersectionType.front)
//                    {
//                        highwayList[index0].leftSideVertexes.Insert(intersection.intersectionIndex[0], newVerticeB);
//                        highwayList[index0].rightSideVertexes.Insert(intersection.intersectionIndex[0], newVerticeC2);
//                    }
//                    else
//                    {
//                        highwayList[index0].leftSideVertexes.Add(newVerticeB);
//                        highwayList[index0].rightSideVertexes.Add(newVerticeC2);
//                    }

//                }
//                else
//                {
//                    highwayList[index0].rightSideVertexes[intersection.intersectionIndex[0]] = newVerticeA;
//                    highwayList[index0].leftSideVertexes[intersection.intersectionIndex[0]] = newVerticeC1;

//                    if (intersection.intersectionTypes[0] == IntersectionType.front)
//                    {
//                        highwayList[index0].rightSideVertexes.Insert(intersection.intersectionIndex[0], newVerticeB);
//                        highwayList[index0].leftSideVertexes.Insert(intersection.intersectionIndex[0], newVerticeC2);
//                    }
//                    else
//                    {
//                        highwayList[index0].rightSideVertexes.Add(newVerticeB);
//                        highwayList[index0].leftSideVertexes.Add(newVerticeC2);
//                    }
//                }

//                if ((intersection.intersectionTypes[1] == IntersectionType.front && conditionNumber == 1) || (intersection.intersectionTypes[1] == IntersectionType.end && conditionNumber == 2))
//                {
//                    highwayList[index1].rightSideVertexes[intersection.intersectionIndex[1]] = newVerticeA;
//                    highwayList[index1].leftSideVertexes[intersection.intersectionIndex[1]] = newVerticeB;
//                }
//                else
//                {
//                    highwayList[index1].leftSideVertexes[intersection.intersectionIndex[1]] = newVerticeA;
//                    highwayList[index1].rightSideVertexes[intersection.intersectionIndex[1]] = newVerticeB;
//                }

//                if ((intersection.intersectionTypes[2] == IntersectionType.front && conditionNumber == 1) || (intersection.intersectionTypes[2] == IntersectionType.end && conditionNumber == 2))
//                {
//                    highwayList[index2].leftSideVertexes[intersection.intersectionIndex[2]] = newVerticeC2;
//                    highwayList[index2].rightSideVertexes[intersection.intersectionIndex[2]] = newVerticeB;
//                }
//                else
//                {
//                    highwayList[index2].rightSideVertexes[intersection.intersectionIndex[2]] = newVerticeC2;
//                    highwayList[index2].leftSideVertexes[intersection.intersectionIndex[2]] = newVerticeB;
//                }

//            }
//            else
//            {
//                return;
//            }


//            GameObject debugSphere1 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereVertice"));
//            debugSphere1.transform.position = newVerticeA;
//            debugSphere1.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);
//            GameObject debugSphere2 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereTerrain"));
//            debugSphere2.transform.position = newVerticeB;
//            debugSphere2.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);
//            GameObject debugSphere3 = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Highway/DebugSphereGreen"));
//            debugSphere3.transform.position = newVerticeC;
//            debugSphere3.transform.localScale = new Vector3(4.0f, 4.0f, 4.0f);

//        }

//        //Helper for correctIntersectionPoint()
//        private void correct4wayIntersection(IntersectionNode intersection)
//        {


//        }


//        private List<IntersectionNode> generateIntersectionList(List<Way> _wayList)
//        {
//            List<Way> wayList = _wayList;
//            List<IntersectionNode> intersectionList = new List<IntersectionNode>();

//            for (int i = 0; i < wayList.Count; i++)
//            {
//                for (int j = 0; j < wayList[i].nodes.Count; j++)
//                {
//                    highwayType type = Highway.getHighwayType(wayList[i].tags);
//                    if (type == highwayType.Railway || type == highwayType.River || type == highwayType.none || type == highwayType.HighwayFootway || type == highwayType.HighwayPedestrian)
//                        continue;

//                    int index = intersectionList.FindIndex(item => item.node.id == wayList[i].nodes[j].id);
//                    if (index == -1) // if node not exist, add it to the intersectionList
//                    {
//                        IntersectionNode nd = new IntersectionNode();
//                        nd.node = wayList[i].nodes[j];
//                        nd.pointList = new List<Vector3>();
//                        nd.wayIds = new List<string>();
//                        nd.intersectionIndex = new List<int>();
//                        nd.intersectionTypes = new List<IntersectionType>();
//                        nd.count = 0;
//                        nd.hasRoadDivision = false;
//                        intersectionList.Add(nd);
//                        index = intersectionList.Count - 1;
//                    }

//                    if (j == 0) //connect from beginning
//                    {
//                        Vector3 point = wayList[i].nodes[1].meterPosition;
//                        IntersectionNode nd = intersectionList[index];
//                        nd.pointList.Add(point);
//                        nd.wayIds.Add(wayList[i].id);
//                        nd.intersectionTypes.Add(IntersectionType.front);
//                        nd.count++;
//                        nd.intersectionIndex.Add(j);
//                        intersectionList[index] = nd;
//                    }

//                    else if (j == wayList[i].nodes.Count - 1) //connect from ending
//                    {
//                        Vector3 point = wayList[i].nodes[j - 1].meterPosition;
//                        IntersectionNode nd = intersectionList[index];
//                        nd.intersectionTypes.Add(IntersectionType.end);
//                        nd.pointList.Add(point);
//                        nd.wayIds.Add(wayList[i].id);
//                        nd.count++;
//                        nd.intersectionIndex.Add(j);
//                        intersectionList[index] = nd;
//                    }

//                    else // connect from the middle (split way into 2)
//                    {
//                        IntersectionNode nd = intersectionList[index];

//                        Vector3 point1 = wayList[i].nodes[j + 1].meterPosition;
//                        nd.pointList.Add(point1);
//                        nd.wayIds.Add(wayList[i].id);
//                        nd.intersectionIndex.Add(j + 1);
//                        nd.intersectionTypes.Add(IntersectionType.middle);

//                        Vector3 point2 = wayList[i].nodes[j - 1].meterPosition;
//                        nd.pointList.Add(point2);
//                        nd.wayIds.Add(wayList[i].id);
//                        nd.intersectionIndex.Add(j - 1);
//                        nd.intersectionTypes.Add(IntersectionType.middle);

//                        nd.count += 2;
//                        nd.hasRoadDivision = true;

//                        intersectionList[index] = nd;
//                    }

//                }
//            }

//            intersectionList.RemoveAll(item => (item.count < 2 || (item.count == 2 && item.hasRoadDivision == true)));
//            return intersectionList;
//        }

//        private int getHighwayIndex(String wayID)
//        {
//            return highwayList.FindIndex(item => item.way.id == wayID);
//        }




//    }

//}
