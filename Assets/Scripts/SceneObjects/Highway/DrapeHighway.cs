using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.OpenStreetMap;
using Assets.Scripts.Utils;

namespace Assets.Scripts.SceneObjects
{
    class DrapeHighway
    {

        //Drape highway to the Terrain
        public void DrapeRoad(myTerrain _terrain, List<Vector3> leftSideVertexes, List<Vector3> rightSideVertexes)
        {
            //------ DO Not Change Order------
            HorizontalDrape(_terrain,leftSideVertexes,rightSideVertexes);
            VerticalDrape(_terrain, leftSideVertexes,rightSideVertexes);
            DiagonalDrape(_terrain, leftSideVertexes,rightSideVertexes);
            //--------------------------------
        }

        //This will be used to drape Highway to Terrain Tiles Horizontally
        private void HorizontalDrape(myTerrain terrain, List<Vector3> leftSideVertexes, List<Vector3> rightSideVertexes)
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
                        if ((intersectleft - pointLeft1).magnitude > (previousIntersectLeft - pointLeft1).magnitude)
                        {
                            leftSideVertexes.Insert(leftit, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                            leftit++;
                        }
                        else
                        {
                            leftSideVertexes.Insert(i + 1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                        }

                        if ((intersectright - pointRight1).magnitude > (previousIntersectRight - pointRight1).magnitude)
                        {
                            rightSideVertexes.Insert(rightit, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightit++;
                        }
                        else
                        {
                            rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
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
                            leftit++;
                        }
                        else
                        {
                            leftSideVertexes.Insert(i + 1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                        }

                        float ratio = (pointLeft1 - intersectleft).magnitude / (pointLeft1 - pointLeft2).magnitude;
                        Vector2 rightPointnew = pointRight1 + (pointRight2 - pointRight1) * ratio;
                        rightSideVertexes.Insert(rightit, new Vector3(rightPointnew.x, terrain.getTerrainHeight2(rightPointnew.y + ti.shiftZ, rightPointnew.x + ti.shiftX), rightPointnew.y));

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
                            rightit++;
                        }
                        else
                        {
                            rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        }

                        float ratio = (pointRight1 - intersectright).magnitude / (pointRight1 - pointRight2).magnitude;
                        Vector2 leftPointnew = pointLeft1 + (pointLeft2 - pointLeft1) * ratio;
                        leftSideVertexes.Insert(leftit, new Vector3(leftPointnew.x, terrain.getTerrainHeight2(leftPointnew.y + ti.shiftZ, leftPointnew.x + ti.shiftX), leftPointnew.y));

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
        private void VerticalDrape(myTerrain terrain, List<Vector3> leftSideVertexes, List<Vector3> rightSideVertexes)
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
                            leftiterator++;
                        }
                        else
                        {
                            leftSideVertexes.Insert(i + 1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                        }

                        if ((intersectright - pointRight1).magnitude > (previousIntersectRight - pointRight1).magnitude)
                        {
                            rightSideVertexes.Insert(rightiterator, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                            rightiterator++;
                        }
                        else
                        {
                            rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
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
                            leftiterator++;
                        }
                        else
                        {
                            leftSideVertexes.Insert(i + 1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));
                        }

                        float ratio = (pointLeft1 - intersectleft).magnitude / (pointLeft1 - pointLeft2).magnitude;
                        Vector2 rightPointnew = pointRight1 + (pointRight2 - pointRight1) * ratio;

                        rightSideVertexes.Insert(rightiterator, new Vector3(rightPointnew.x, terrain.getTerrainHeight2(rightPointnew.y + ti.shiftZ, rightPointnew.x + ti.shiftX), rightPointnew.y));
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
                            rightiterator++;
                        }
                        else
                        {
                            rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        }

                        float ratio = (pointRight1 - intersectright).magnitude / (pointRight1 - pointRight2).magnitude;
                        Vector2 leftPointnew = pointLeft1 + (pointLeft2 - pointLeft1) * ratio;

                        leftSideVertexes.Insert(leftiterator, new Vector3(leftPointnew.x, terrain.getTerrainHeight2(leftPointnew.y + ti.shiftZ, leftPointnew.x + ti.shiftX), leftPointnew.y));
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
        private void DiagonalDrape(myTerrain terrain, List<Vector3> leftSideVertexes, List<Vector3> rightSideVertexes)
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
                        pTop = new Vector2(ti.meterPositions[1 + t - ti.ColumnCount, ti.ColumnCount - 1].y - ti.shiftX, ti.meterPositions[1 + t - ti.ColumnCount, ti.ColumnCount - 1].x - ti.shiftZ);

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

                        rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));
                        cnt++;
                    }
                    else if (isLeft)
                    {
                        leftSideVertexes.Insert(i + 1, new Vector3(intersectleft.x, terrain.getTerrainHeight2(intersectleft.y + ti.shiftZ, intersectleft.x + ti.shiftX), intersectleft.y));

                        float ratio = (pointLeft1 - intersectleft).magnitude / (pointLeft1 - pointLeft2).magnitude;
                        Vector2 rightPointnew = pointRight1 + (pointRight2 - pointRight1) * ratio;
                        rightSideVertexes.Insert(i + 1, new Vector3(rightPointnew.x, terrain.getTerrainHeight2(rightPointnew.y + ti.shiftZ, rightPointnew.x + ti.shiftX), rightPointnew.y));
                        cnt++;
                    }

                    else if (isRight)
                    {
                        rightSideVertexes.Insert(i + 1, new Vector3(intersectright.x, terrain.getTerrainHeight2(intersectright.y + ti.shiftZ, intersectright.x + ti.shiftX), intersectright.y));

                        float ratio = (pointRight1 - intersectright).magnitude / (pointRight1 - pointRight2).magnitude;
                        Vector2 leftPointnew = pointLeft1 + (pointLeft2 - pointLeft1) * ratio;
                        leftSideVertexes.Insert(i + 1, new Vector3(leftPointnew.x, terrain.getTerrainHeight2(leftPointnew.y + ti.shiftZ, leftPointnew.x + ti.shiftX), leftPointnew.y));
                        cnt++;
                    }
                }
                i += cnt;
            }



        }

    }
}
