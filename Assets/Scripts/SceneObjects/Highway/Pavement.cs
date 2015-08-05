using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.OpenStreetMap;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.Utils;

namespace Assets.Scripts.SceneObjects
{
    public class Pavement
    {
        public List<Vector3> leftSideVertexes;
        public List<Vector3> rightSideVertexes;
        public string Pavementid; //Takes it from highway that it belongs
        public pavementSide side;

        public GameObject PavementObj;

        private float extraHeight = 0.20f;
        private float stonesHeight = 0.30f;

        public enum pavementSide
        {
            left,
            right
        }

        public Pavement(Highway highway, myTerrain terrain, pavementSide _side)
        {
            Pavementid = highway.id;
            side = _side;

            if(side == pavementSide.left)
            {
                rightSideVertexes = new List<Vector3>(highway.leftSideVertexes);
                leftSideVertexes = new List<Vector3>();
                generateOtherSide(highway.leftSidewalkSize, terrain);

                PavementObj = new GameObject("LeftPavement" + Pavementid);
                PavementObj.transform.parent = highway.highwayGameObject.transform;
                   


            }
            if(side == pavementSide.right)
            {
                leftSideVertexes = new List<Vector3>(highway.rightSideVertexes);
                rightSideVertexes = new List<Vector3>();
                generateOtherSide(highway.rightSidewalkSize, terrain);

                PavementObj = new GameObject("RightPavement" + Pavementid);
                PavementObj.transform.parent = highway.highwayGameObject.transform;
            }

        }

        private void generateOtherSide(float size, myTerrain terrain)
        {
            List<Vector3> otherside;
            List<Vector3> generatedside;

            TerrainInfo ti = terrain.terrainInfo;

            if (side == pavementSide.left)
            {
                otherside = rightSideVertexes;
                generatedside = leftSideVertexes;
            }
            else
            {
                otherside = leftSideVertexes;
                generatedside = rightSideVertexes;
            }

            if (otherside.Count == 2)
            {
                Vector3 up = Vector3.up;
                Vector3 forward = otherside[1] - otherside[0];
                forward.y = 0.0f;
                Vector3 sideVec;
                //if (side == pavementSide.left)*******************************************************
                //    sideVec =  Vector3.Cross(up, forward).normalized;
                //else
                //    sideVec =  Vector3.Cross(forward, up).normalized;
                if (side == pavementSide.left)
                    sideVec = Vector3.Cross(forward, up).normalized;
                else
                    sideVec = Vector3.Cross(up, forward).normalized;


             
                Vector2 point1 = new Vector2(otherside[0].x, otherside[0].z) + new Vector2(sideVec.x, sideVec.z) * size;
                Vector2 point2 = new Vector2(otherside[1].x, otherside[1].z) + new Vector2(sideVec.x, sideVec.z) * size;

                if ((point1 - point2).magnitude < 6.0f)
                    return;

                generatedside.Add(new Vector3(point1.x, terrain.getTerrainHeight2(point1.y + ti.shiftZ, point1.x + ti.shiftX), point1.y));
                generatedside.Add(new Vector3(point2.x, terrain.getTerrainHeight2(point2.y + ti.shiftZ, point2.x + ti.shiftX), point2.y));

             }
             
            else
            {
                //Checking if side has enough length to draw a pavement
                float totalsize = 0.0f;
                for (int k = 0; k < otherside.Count - 1; k++)
                {
                    totalsize += (otherside[k + 1] - otherside[k]).magnitude;
                }
                if (totalsize <= 6.0f)
                    return;


                for (int i = 0; i < otherside.Count; i++)
                {
                    Vector3 up = Vector3.up;
                    Vector3 forward1 = otherside[i + 1] - otherside[i];
                    forward1.y = 0.0f;
                    Vector3 sideVec1;
                    //if (side == pavementSide.left)***************************************************************
                    //    sideVec1 = Vector3.Cross(up, forward1).normalized;
                    //else
                    //    sideVec1 = Vector3.Cross(forward1, up).normalized;
                    if (side == pavementSide.left)
                        sideVec1 = Vector3.Cross(forward1, up).normalized;
                    else
                        sideVec1 = Vector3.Cross(up, forward1).normalized;

                    Vector3 forward2 = otherside[i + 2] - otherside[i + 1];
                    forward2.y = 0.0f;
                    Vector3 sideVec2;
                    if (side == pavementSide.left)
                        sideVec2 = Vector3.Cross(forward2, up).normalized;
                    else
                        sideVec2 = Vector3.Cross(up, forward2).normalized;

                    //START POINT WAS ADDED TO GENERATED SIDE
                    if (i == 0)
                    {
                        Vector2 point1 = new Vector2(otherside[i].x, otherside[i].z) + new Vector2(sideVec1.x, sideVec1.z) * size;
                        generatedside.Add(new Vector3(point1.x, terrain.getTerrainHeight2(point1.y + ti.shiftZ, point1.x + ti.shiftX), point1.y));
                    }

                    //MID POINTS WERE ADDED TO GENERATED SIDE

                    //1ST LINE 
                    Vector2 p0 = new Vector2(otherside[i].x, otherside[i].z) + new Vector2(sideVec1.x, sideVec1.z) * size;
                    Vector2 p1 = new Vector2(otherside[i + 1].x, otherside[i + 1].z) + new Vector2(sideVec1.x, sideVec1.z) * size;

                    //2ND LINE 
                    Vector2 p2 = new Vector2(otherside[i + 1].x, otherside[i + 1].z) + new Vector2(sideVec2.x, sideVec2.z) * size;
                    Vector2 p3 = new Vector2(otherside[i + 2].x, otherside[i + 2].z) + new Vector2(sideVec2.x, sideVec2.z) * size;

                    Vector2 iL = new Vector2();
                    //INTERSECTION                       
                    if (!Geometry.getInfiniteLineIntersection(ref iL, p0, p1, p2, p3))
                        generatedside.Add(new Vector3(p1.x, terrain.getTerrainHeight2(p1.y + ti.shiftZ, p1.x + ti.shiftX), p1.y));
                    else
                        generatedside.Add(new Vector3(iL.x, terrain.getTerrainHeight2(iL.y + ti.shiftZ, iL.x + ti.shiftX), iL.y));
              

                    //ENDING POINT WAS ADDEDD TO GENERATED SIDE
                    if (i == otherside.Count - 3)
                    {
                        Vector2 point1 = new Vector2(otherside[i + 2].x, otherside[i + 2].z) + new Vector2(sideVec2.x, sideVec2.z) * size;
                        generatedside.Add(new Vector3(point1.x, terrain.getTerrainHeight2(point1.y + ti.shiftZ, point1.x + ti.shiftX), point1.y));
                        return;
                    }


                }


            }



        }

        public void renderPavement()
        {
            renderPavementSurface();
            renderPavementStones();
        }

        //Create Pavement GameObject to Render
        private void renderPavementSurface()
        {
            GameObject PavementSurfaceObj = new GameObject("PavementSurface", typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider));
            PavementSurfaceObj.transform.parent = PavementObj.transform;

            float extra = extraHeight + stonesHeight;

            if (leftSideVertexes.Count == 0 || rightSideVertexes.Count == 0)
                return;

            Vector3[] vertices = new Vector3[leftSideVertexes.Count + rightSideVertexes.Count];
            for (int k = 0, m = 0; k < vertices.Length; k += 2, m++)
            {
                vertices[k] = leftSideVertexes[m] + new Vector3(0, extra, 0);
                vertices[k + 1] = rightSideVertexes[m] + new Vector3(0, extra, 0);
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

            MeshFilter meshFilter;
            MeshRenderer meshRenderer;


            meshFilter = PavementSurfaceObj.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            meshRenderer = PavementSurfaceObj.GetComponent<MeshRenderer>();
            Material mat = new Material((Material)Resources.Load("Materials/Highway/Mat_Pavement"));


            meshRenderer.material = mat;

            MeshCollider meshCollider = PavementSurfaceObj.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            PavementSurfaceObj.transform.parent = PavementObj.transform;



        }

        private void renderPavementStones()
        {
            GameObject PavementStonesLeftObj = new GameObject("PavementStonesLeft", typeof(MeshRenderer), typeof(MeshFilter));
            GameObject PavementStonesRightObj = new GameObject("PavementStonesRight", typeof(MeshRenderer), typeof(MeshFilter));

            PavementStonesLeftObj.transform.parent = PavementObj.transform;
            PavementStonesRightObj.transform.parent = PavementObj.transform;


            if (leftSideVertexes.Count == 0 || rightSideVertexes.Count == 0)
                return;


            Vector3[] verticesLeftStone = new Vector3[leftSideVertexes.Count*2];
            Vector3[] verticesRightStone = new Vector3[rightSideVertexes.Count*2];

            
            for (int k = 0, itr=0; k < leftSideVertexes.Count; k++)
            {
                verticesLeftStone[itr++] = leftSideVertexes[k] + new Vector3(0, extraHeight, 0);
                verticesLeftStone[itr++] = leftSideVertexes[k] + new Vector3(0, extraHeight + stonesHeight, 0);
            }
            for (int k = 0, itr= 0; k < rightSideVertexes.Count; k++)
            {
                verticesRightStone[itr++] = rightSideVertexes[k] + new Vector3(0, extraHeight, 0);
                verticesRightStone[itr++] = rightSideVertexes[k] + new Vector3(0, extraHeight + stonesHeight, 0);
            }

            int[] trianglesLeftStone = new int[(verticesLeftStone.Length -2) *3];
            int[] trianglesRightStone = new int[(verticesRightStone.Length -2) *3]; 

            for (int k = 0,itr=0; k < verticesLeftStone.Length - 2; k+=2)
            {
                trianglesLeftStone[itr++] = k;
                trianglesLeftStone[itr++] = k + 2;
                trianglesLeftStone[itr++] = k + 1;

                trianglesLeftStone[itr++] = k + 1;
                trianglesLeftStone[itr++] = k + 2;
                trianglesLeftStone[itr++] = k + 3;
            }

            for (int k = 0, itr = 0; k < verticesRightStone.Length - 2; k += 2)
            {
                trianglesRightStone[itr++] = k;
                trianglesRightStone[itr++] = k + 1;
                trianglesRightStone[itr++] = k + 2;

                trianglesRightStone[itr++] = k + 1;
                trianglesRightStone[itr++] = k + 3;
                trianglesRightStone[itr++] = k + 2;
            }

            Vector2[] UVcoords = new Vector2[verticesLeftStone.Length];
            float pos = 0.0f;
            for(int k = 0 ; k < verticesLeftStone.Length-1; k+=2)
            {
                if (k != 0)
                    pos +=(verticesLeftStone[k] - verticesLeftStone[k - 2]).magnitude;
                UVcoords[k] = new Vector2(pos, 0.0f);
                UVcoords[k + 1] = new Vector2(pos, 1.0f);
            }

            Mesh meshLeft = new Mesh();
            meshLeft.vertices = verticesLeftStone;
            meshLeft.triangles = trianglesLeftStone;
            meshLeft.uv = UVcoords;
            MeshFilter meshFilterLeft = PavementStonesLeftObj.GetComponent<MeshFilter>();
            meshFilterLeft.mesh = meshLeft;

            Mesh meshRight = new Mesh();
            meshRight.vertices = verticesRightStone;
            meshRight.triangles = trianglesRightStone;
            meshRight.uv = UVcoords;
            MeshFilter meshFilterRight = PavementStonesRightObj.GetComponent<MeshFilter>();
            meshFilterRight.mesh = meshRight;

            MeshRenderer meshRendererLeft = PavementStonesLeftObj.GetComponent<MeshRenderer>();
            meshRendererLeft.material = (Material)Resources.Load("Materials/Highway/Mat_PavementStone");

            MeshRenderer meshRendererRight = PavementStonesRightObj.GetComponent<MeshRenderer>();
            meshRendererRight.material = (Material)Resources.Load("Materials/Highway/Mat_PavementStone");

            PavementStonesLeftObj.transform.parent = PavementObj.transform;
            PavementStonesRightObj.transform.parent = PavementObj.transform;
        }

        public void updateMesh(Vector3 newVertice, pavementSide side, int verticeIndex)
        {
            //Update Pavement Surface Mesh and Stones Mesh
            MeshFilter SurfaceMeshFilter = PavementObj.transform.Find("PavementSurface").GetComponent<MeshFilter>();         
            MeshFilter StoneMeshFilter;
            Vector3[] surfaceMeshV, stoneMeshV;

            surfaceMeshV = SurfaceMeshFilter.mesh.vertices;

            if(side == pavementSide.left)
            {
                StoneMeshFilter = PavementObj.transform.Find("PavementStonesLeft").GetComponent<MeshFilter>();
                stoneMeshV = StoneMeshFilter.mesh.vertices;
                if(verticeIndex == 0)
                {
                     surfaceMeshV[0] = newVertice + new Vector3(0,extraHeight + stonesHeight,0);
                     stoneMeshV[0] = newVertice + new Vector3(0,extraHeight,0);
                     stoneMeshV[1] = newVertice + new Vector3(0,extraHeight + stonesHeight,0);
                }
                else
                {
                    surfaceMeshV[surfaceMeshV.Length-2] = newVertice + new Vector3(0,extraHeight + stonesHeight,0);
                    stoneMeshV[stoneMeshV.Length-2] = newVertice + new Vector3(0,extraHeight,0);
                    stoneMeshV[stoneMeshV.Length-1] = newVertice + new Vector3(0,extraHeight + stonesHeight,0);                    
                }
            }                              
            else
            {
                StoneMeshFilter = PavementObj.transform.Find("PavementStonesRight").GetComponent<MeshFilter>();
                stoneMeshV = StoneMeshFilter.mesh.vertices;
                if (verticeIndex == 1)
                {
                    surfaceMeshV[1] = newVertice + new Vector3(0, extraHeight + stonesHeight, 0);
                    stoneMeshV[0] = newVertice + new Vector3(0, extraHeight, 0);
                    stoneMeshV[1] = newVertice + new Vector3(0, extraHeight + stonesHeight, 0);
                }
                else
                {
                    surfaceMeshV[surfaceMeshV.Length - 1] = newVertice + new Vector3(0, extraHeight + stonesHeight, 0);
                    stoneMeshV[stoneMeshV.Length - 2] = newVertice + new Vector3(0, extraHeight, 0);
                    stoneMeshV[stoneMeshV.Length - 1] = newVertice + new Vector3(0, extraHeight + stonesHeight, 0);
                }           
            }

            SurfaceMeshFilter.mesh.vertices = surfaceMeshV;
            StoneMeshFilter.mesh.vertices = stoneMeshV;
            SurfaceMeshFilter.mesh.RecalculateBounds();
            StoneMeshFilter.mesh.RecalculateBounds();

        }

    }
}
