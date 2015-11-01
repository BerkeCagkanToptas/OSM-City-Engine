using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.HeightMap;
using Assets.Scripts.Utils;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.OpenStreetMap;
using UnityEngine;
using System.IO;
using Assets.Scripts.UnitySideScripts.MouseScripts;
using Assets.Scripts.UnitySideScripts;
using Assets.Scripts.UnitySideScripts.EditingScripts;

namespace Assets.Scripts.SceneObjects
{

    public class Scene
    {
        public BBox scenebbox;
        
        public List<Highway> highwayList;
        public List<Pavement> pavementList;      
        public List<Building> buildingList;       
        public List<Barrier> barrierList;
        public List<Object3D> defaultObject3DList;
        public List<Object3D> object3DList;
        public GameObject controller;

        public myTerrain terrain;
        public InitialConfigurations config;

        public HighwayModeller highwayModeller;
        private OSMXml osmxml;
        public HeightmapContinent continent;
        public MapProvider provider;
        public string sceneName;
        public string OSMPath;
       

        public Scene()
        {
            buildingList = new List<Building>();          
            barrierList = new List<Barrier>();
            defaultObject3DList = new List<Object3D>();
            object3DList = new List<Object3D>();
        }

        /// <summary>
        ///  Constructs the scene from given parameters
        /// </summary>
        /// <param name="OSMfilename">Full path of OSM file</param> 
        /// <param name="continent">Specify Continent to download correct Heightmap from Nasa Srtm Data</param>
        /// <param name="provider">Choose mapProvider to select Texture of Terrain</param> 
        public void initializeScene(string OSMfilename, HeightmapContinent _continent, MapProvider _provider)
        {
            string[] subStr = OSMfilename.Split(new char[] { '/', '\\' });
            sceneName = subStr[subStr.Length - 1];
            OSMPath = OSMfilename;

            continent = _continent;
            provider = _provider;

            List<Way> WayListforHighway = new List<Way>();
            List<Way> WayListforBuilding = new List<Way>();

            InitialConfigLoader configloader = new InitialConfigLoader();

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            OSMparser parser = new OSMparser();
            scenebbox = parser.readBBox(OSMfilename);
            scenebbox = editbbox(scenebbox);
            config = configloader.loadInitialConfig();


            HeightmapLoader heightMap = new HeightmapLoader(scenebbox, continent);
            terrain = new myTerrain(heightMap, scenebbox, OSMfilename, provider);

            stopwatch.Stop();
            Debug.Log("<color=blue>TERRAIN RENDER TIME:</color>" + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();       
            stopwatch.Start();

            osmxml = parser.parseOSM(OSMfilename);
            assignNodePositions();

            stopwatch.Stop();
            Debug.Log("<color=blue>OSM PARSING TIME:</color>" + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            stopwatch.Start();


            defaultObject3DList = DefaultObject3DHandler.drawDefaultObjects(osmxml.defaultobject3DList);

            stopwatch.Stop();
            Debug.Log("<color=blue>3D OBJECT RENDER TIME:</color>" + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            stopwatch.Start();


            for (int k = 0; k < osmxml.wayList.Count; k++)
            {
                Way w = osmxml.wayList[k];

                switch (w.type)
                {
                    case ItemEnumerator.wayType.building:
                        WayListforBuilding.Add(w);     
                        break;
                    case ItemEnumerator.wayType.highway:
                        WayListforHighway.Add(w);
                        break;
                    case ItemEnumerator.wayType.area:
                        break;
                    case ItemEnumerator.wayType.barrier:
                        barrierList.Add(new Barrier(w, config.barrierConfig));
                        break;
                    case ItemEnumerator.wayType.river:
                        highwayList.Add(new Highway(w, config.highwayConfig, terrain));
                        break;
                    case ItemEnumerator.wayType.none:
                        break;
                }
            }

            stopwatch.Stop();
            Debug.Log("<color=blue>ITEM ENUMERATING TIME:</color>" + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            stopwatch.Start();

            highwayModeller = new HighwayModeller(WayListforHighway, terrain, config.highwayConfig);
            highwayModeller.renderHighwayList();
            highwayModeller.renderPavementList();
            highwayList = highwayModeller.highwayList;
            pavementList = highwayModeller.pavementList;

            stopwatch.Stop();
            Debug.Log("<color=blue>HIGHWAY RENDERING TIME:</color>" + stopwatch.ElapsedMilliseconds);
            stopwatch.Reset();
            stopwatch.Start();
            
            BuildingListModeller buildingListModeller = new BuildingListModeller(WayListforBuilding, osmxml.buildingRelations, config.buildingConfig);
            buildingListModeller.renderBuildingList();
            buildingList = buildingListModeller.buildingList;

            stopwatch.Stop();
            Debug.Log("<color=blue>BUILDING RENDERING TIME:</color>" + stopwatch.ElapsedMilliseconds);
           
            Debug.Log("<color=red>Scene Info:</color> BuildingCount:" + buildingList.Count.ToString() + " HighwayCount:" + highwayList.Count);

        }

        /// <summary>
        /// Load urban scene using a save file
        /// </summary>
        /// <param name="save"> Save file object </param>
        public void loadProject(SceneSave save)
        {
            List<Way> WayListforHighway = new List<Way>();
            List<Way> WayListforBuilding = new List<Way>();

            InitialConfigLoader configloader = new InitialConfigLoader();
            OSMPath = save.osmPath;
            OSMparser parser = new OSMparser();
            scenebbox = parser.readBBox(save.osmPath);
            scenebbox = editbbox(scenebbox);
            config = configloader.loadInitialConfig(); //--> Maybe it is better to include config to SaveProject file

            HeightmapLoader heightMap = new HeightmapLoader(scenebbox, save.continent);
            terrain = new myTerrain(heightMap, scenebbox, save.osmPath, save.provider);
            osmxml = parser.parseOSM(save.osmPath);
            assignNodePositions();

            defaultObject3DList = DefaultObject3DHandler.drawDefaultObjects(osmxml.defaultobject3DList);

            LoadExternalOBJ objloader = new LoadExternalOBJ();

            //3D OBJECT LOAD
            for(int i = 0 ; i < save.objectSaveList.Count ; i++)
            {
                Object3D obj = new Object3D();
                obj.name = save.objectSaveList[i].name;

                if (save.objectSaveList[i].type == ObjectType.External)
                    obj.object3D = objloader.loadOBJ(save.objectSaveList[i].resourcePath);
                else
                    obj.object3D = (GameObject)MonoBehaviour.Instantiate(Resources.Load(save.objectSaveList[i].resourcePath));
                obj.object3D.AddComponent<Object3dMouseHandler>();
                obj.resourcePath = save.objectSaveList[i].resourcePath;
                obj.object3D.transform.position = save.objectSaveList[i].translate;
                obj.object3D.transform.localScale = save.objectSaveList[i].scale;
                Quaternion quat = new Quaternion();
                quat.eulerAngles = save.objectSaveList[i].rotate;
                obj.object3D.transform.rotation = quat;
                obj.object3D.name = obj.name;
                obj.object3D.tag = "3DObject";
                object3DList.Add(obj);
                
            }

            for (int k = 0; k < osmxml.wayList.Count; k++)
            {
                Way w = osmxml.wayList[k];

                switch (w.type)
                {
                    case ItemEnumerator.wayType.building:
                        WayListforBuilding.Add(w);
                        break;
                    case ItemEnumerator.wayType.highway:
                        WayListforHighway.Add(w);
                        break;
                    case ItemEnumerator.wayType.area:
                        break;
                    case ItemEnumerator.wayType.barrier:
                        barrierList.Add(new Barrier(w, config.barrierConfig));
                        break;
                    case ItemEnumerator.wayType.river:
                        highwayList.Add(new Highway(w, config.highwayConfig, terrain));
                        break;
                    case ItemEnumerator.wayType.none:
                        break;
                }
            }

            highwayModeller = new HighwayModeller(WayListforHighway, terrain, config.highwayConfig, save.highwaySaveList);
            highwayModeller.renderHighwayList();
            highwayModeller.renderPavementList();
            highwayList = highwayModeller.highwayList;
            pavementList = highwayModeller.pavementList;

            BuildingListModeller buildingListModeller = new BuildingListModeller(WayListforBuilding, osmxml.buildingRelations, config.buildingConfig,save.buildingSaveList);
            buildingListModeller.renderBuildingList();
            buildingList = buildingListModeller.buildingList;

            if (save.controller == null)
                return;

            if(save.controller.controllerType == ControllerSave.ControllerType.CameraVan)
            {
                Transform mainCamera = GameObject.Find("Main Camera").transform;
                CameraController camController = mainCamera.GetComponent<CameraController>();

                controller = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Car/PolimiCameraCar/CameraVan"));
                controller.AddComponent<CameraVanMouseHandler>();
                controller.tag = "CameraVan";
                controller.name = "Camera Van";
                controller.transform.position = mainCamera.position + mainCamera.forward * 10.0f;
                controller.GetComponent<Rigidbody>().useGravity = false;
                camController.target = controller.transform;
            }

            else
            {
                Transform mainCamera = GameObject.Find("Main Camera").transform;
                CameraController camController = mainCamera.GetComponent<CameraController>();

                controller = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Ethan/ThirdPersonController"));
                controller.AddComponent<CameraVanMouseHandler>();
                controller.tag = "CameraVan";
                controller.name = "Third Person (Ethan)";
                controller.transform.position = mainCamera.position + mainCamera.forward * 10.0f;
                camController.target = controller.transform;
            }

            controller.transform.position = save.controller.controllerPosition;
            Quaternion controllerQuat = new Quaternion();
            controllerQuat.eulerAngles = save.controller.controllerRotation;
            controller.transform.rotation = controllerQuat;

            CameraVanEdit cve = GameObject.Find("Canvas").transform.Find("CameraVanEdit").GetComponent<CameraVanEdit>();
            cve.cameraList = save.controller.convertBackToCamList(save.controller.cameraSettings);
            cve.laserScanner = save.controller.convertBackToLaser(save.controller.laserSetting);

        }

        
        /// <summary>
        /// Converts Spherical Mercator Coordinates of all nodes to Unity Coordinates
        /// </summary>      
        private void assignNodePositions()
        {
            Geography proj = new Geography();
           
            for(int k = 0; k < osmxml.nodeList.Count ; k++)
            {
                Node nd = osmxml.nodeList[k];
                Vector2 meterCoord = proj.LatLontoMeters(nd.lat,nd.lon);
                Vector2 shift = new Vector2(scenebbox.meterBottom, scenebbox.meterLeft);
                meterCoord = meterCoord - shift;
                float height = terrain.getTerrainHeight(nd.lat,nd.lon);
                nd.meterPosition = new Vector3(meterCoord.y, height, meterCoord.x);          
                osmxml.nodeList[k] = nd;
            }

            for(int i=0 ; i < osmxml.wayList.Count ; i++)
            {
                for(int k = 0; k < osmxml.wayList[i].nodes.Count ; k++)
                {
                    Node nd = osmxml.wayList[i].nodes[k];
                    Vector2 meterCoord = proj.LatLontoMeters(nd.lat, nd.lon);
                    Vector2 shift = new Vector2(scenebbox.meterBottom, scenebbox.meterLeft);
                    meterCoord = meterCoord - shift;
                    float height = terrain.getTerrainHeight(nd.lat, nd.lon);
                    nd.meterPosition = new Vector3(meterCoord.y, height, meterCoord.x);
                    osmxml.wayList[i].nodes[k] = nd;
                }


            }

            for(int i = 0 ; i < osmxml.defaultobject3DList.Count ; i++)
            {
                Node nd = osmxml.defaultobject3DList[i];
                Vector2 meterCoord = proj.LatLontoMeters(nd.lat, nd.lon);
                Vector2 shift = new Vector2(scenebbox.meterBottom, scenebbox.meterLeft);
                meterCoord = meterCoord - shift;
                float height = terrain.getTerrainHeight(nd.lat, nd.lon);
                nd.meterPosition = new Vector3(meterCoord.y, height, meterCoord.x);
                osmxml.defaultobject3DList[i] = nd;
            }

        }

        /// <summary>
        /// Convert Spherical Mercator Coordinates of scene BBox to Unity Coordinates
        /// </summary>
        /// <param name="bbox"> bbox obtained from OSM file </param>
        /// <returns></returns>
        private BBox editbbox(BBox bbox)
        {
            Geography proj = new Geography();
            Vector2 bottomleft = proj.LatLontoMeters(bbox.bottom, bbox.left);
            Vector2 topright = proj.LatLontoMeters(bbox.top, bbox.right);
            bbox.meterLeft = bottomleft.y;
            bbox.meterBottom = bottomleft.x;
            bbox.meterTop = topright.x;
            bbox.meterRight = topright.y;
            return bbox;
        }



    }
}
