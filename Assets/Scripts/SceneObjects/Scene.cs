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

namespace Assets.Scripts.SceneObjects
{

    enum wayType
    {
        building,
        highway,
        area,
        barrier,
        river,
        none
    }


    public class Scene
    {
        public BBox scenebbox;
        
        public List<Highway> highwayList;
        public List<Pavement> pavementList;      
        public List<Building> buildingList;       
        public List<Barrier> barrierList;
        private List<Object3D> defaultTreeList;
        public List<Object3D> object3DList;

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
            defaultTreeList = new List<Object3D>();
            object3DList = new List<Object3D>();
        }

        /// <summary>
        ///  Constructs the scene from given parameters
        /// </summary>
        /// <param name="OSMfilename"></param>
        /// Full path of OSM file
        /// <param name="continent"></param>
        /// Specify Continent to download correct Heightmap from Nasa Srtm Data
        /// <param name="provider"></param>
        /// Choose mapProvider to select Texture of Terrain
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

            OSMparser parser = new OSMparser();
            scenebbox = parser.readBBox(OSMfilename);
            scenebbox = editbbox(scenebbox);
            config = configloader.loadInitialConfig();

            HeightmapLoader heightMap = new HeightmapLoader(scenebbox, continent);
            terrain = new myTerrain(heightMap, scenebbox, OSMfilename, provider);
            osmxml = parser.parseOSM(OSMfilename);
            assignNodePositions();


            drawTrees(osmxml.treeList);


            for (int k = 0; k < osmxml.wayList.Count; k++)
            {
                Way w = osmxml.wayList[k];

                switch (getWayTpe(w))
                {
                    case wayType.building:
                        WayListforBuilding.Add(w);     
                        break;
                    case wayType.highway:
                        WayListforHighway.Add(w);
                        break;
                    case wayType.area:
                        break;
                    case wayType.barrier:
                        barrierList.Add(new Barrier(w, config.barrierConfig));
                        break;
                    case wayType.river:
                        highwayList.Add(new Highway(w, config.highwayConfig, terrain));
                        break;
                    case wayType.none:
                        break;
                }
            }
            
            highwayModeller = new HighwayModeller(WayListforHighway, terrain, config.highwayConfig);
            highwayModeller.renderHighwayList();
            highwayModeller.renderPavementList();
            highwayList = highwayModeller.highwayList;
            pavementList = highwayModeller.pavementList;

            BuildingListModeller buildingListModeller = new BuildingListModeller(WayListforBuilding, osmxml.buildingRelations, config.buildingConfig);
            buildingListModeller.renderBuildingList();
            buildingList = buildingListModeller.buildingList;

           
            Debug.Log("<color=red>Scene Info:</color> BuildingCount:" + buildingList.Count.ToString() + " HighwayCount:" + highwayList.Count);

        }

        public void loadProject(SceneSave save)
        {
            List<Way> WayListforHighway = new List<Way>();
            List<Way> WayListforBuilding = new List<Way>();

            InitialConfigLoader configloader = new InitialConfigLoader();

            OSMparser parser = new OSMparser();
            scenebbox = parser.readBBox(save.osmPath);
            scenebbox = editbbox(scenebbox);
            config = configloader.loadInitialConfig(); //--> Maybe it is better to include config to SaveProject file

            HeightmapLoader heightMap = new HeightmapLoader(scenebbox, save.continent);
            terrain = new myTerrain(heightMap, scenebbox, save.osmPath, save.provider);
            osmxml = parser.parseOSM(save.osmPath);
            assignNodePositions();

            drawTrees(osmxml.treeList);

            //3D OBJECT LOAD
            for(int i = 0 ; i < save.objectSaveList.Count ; i++)
            {
                Object3D obj = new Object3D();
                obj.name = save.objectSaveList[i].name;
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

                switch (getWayTpe(w))
                {
                    case wayType.building:
                        WayListforBuilding.Add(w);
                        break;
                    case wayType.highway:
                        WayListforHighway.Add(w);
                        break;
                    case wayType.area:
                        break;
                    case wayType.barrier:
                        barrierList.Add(new Barrier(w, config.barrierConfig));
                        break;
                    case wayType.river:
                        highwayList.Add(new Highway(w, config.highwayConfig, terrain));
                        break;
                    case wayType.none:
                        break;
                }
            }

            highwayModeller = new HighwayModeller(WayListforHighway, terrain, config.highwayConfig);
            highwayModeller.renderHighwayList();
            highwayModeller.renderPavementList();
            highwayList = highwayModeller.highwayList;
            pavementList = highwayModeller.pavementList;

            BuildingListModeller buildingListModeller = new BuildingListModeller(WayListforBuilding, osmxml.buildingRelations, config.buildingConfig);
            buildingListModeller.renderBuildingList();
            buildingList = buildingListModeller.buildingList;

        }

        //Seperate WayType of OSM into our City Engine object types
        private wayType getWayTpe(Way way)
        {
            if (way.tags == null)
                return wayType.none;

            foreach(Tag t in way.tags)
            {
                if (t.k == "building")
                    return wayType.building;
                else if (t.k == "barrier")
                    return wayType.barrier;
                else if (t.k == "highway" || t.k == "railway")
                {
                    if (t.v == "footway")
                        return wayType.none;

                    return wayType.highway;
                }
                else if (t.k == "landuse" || t.k == "leisure")
                    return wayType.area;
                else if (t.k == "amenity")
                {
                    for (int k = 0; k < way.tags.Count; k++)
                    {
                        if (way.tags[k].k == "building")
                            return wayType.building;
                    }
                    return wayType.area;
                }
                else if (t.k == "waterway" && t.v == "river")
                    return wayType.highway; //return wayType.river;

                else if (t.k == "historic" && t.v == "monument")
                    return wayType.area;
                else if (t.k == "historic" && (t.v == "citywalls" || t.v == "city_gate"))
                    return wayType.barrier;

            }
      

            return wayType.none;
        }

        //Converts Spherical Mercator Coordinates to Unity Coordinates
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

            for(int i = 0 ; i < osmxml.treeList.Count ; i++)
            {
                Node nd = osmxml.treeList[i];
                Vector2 meterCoord = proj.LatLontoMeters(nd.lat, nd.lon);
                Vector2 shift = new Vector2(scenebbox.meterBottom, scenebbox.meterLeft);
                meterCoord = meterCoord - shift;
                float height = terrain.getTerrainHeight(nd.lat, nd.lon);
                nd.meterPosition = new Vector3(meterCoord.y, height, meterCoord.x);
                osmxml.treeList[i] = nd;
            }

        }

        //Apply Correction to the Osm bbox
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

        //Draw Trees in the scene
        private void drawTrees(List<Node> _treeList)
        {
            for (int i = 0; i < _treeList.Count; i++)
            {
                Object3D obj = new Object3D();
                obj.name = "Broad Leaf Tree";
                obj.type = ObjectType.Tree;
                obj.resourcePath = "Prefabs/Environment/SpeedTree/BroadLeaf/BroadLeafDesktopPrefab";
                obj.object3D = (GameObject) MonoBehaviour.Instantiate(Resources.Load("Prefabs/Environment/SpeedTree/BroadLeaf/BroadLeafDesktopPrefab"));
                obj.object3D.AddComponent<Object3dMouseHandler>();                
                obj.object3D.transform.position = _treeList[i].meterPosition;
                obj.object3D.tag = "3DObject";
                obj.object3D.name = "Broad Leaf Tree";
                
                defaultTreeList.Add(obj);            
            }
        }



    }
}
