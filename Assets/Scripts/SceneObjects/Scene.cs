using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.HeightMap;
using Assets.Scripts.Utils;
using Assets.Scripts.ConfigHandler;
using UnityEngine;

namespace Assets.Scripts.OpenStreetMap
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


    class Scene
    {
        public BBox scenebbox;
        public List<Building> buildingList;
        public List<Area> areaList;
        public List<Highway> highwayList;
        public List<Barrier> barrierList;
        public List<GameObject> treeList;
        public List<GameObject> trafficLightList;
        public myTerrain terrain;
        public InitialConfigurations config;

        private HeighmapLoader heightMap;
        private OSMXml osmxml;

        private InitialConfigLoader configloader;

        public Scene()
        {
            buildingList = new List<Building>();
            areaList = new List<Area>();
            highwayList = new List<Highway>();
            barrierList = new List<Barrier>();
            treeList = new List<GameObject>();
            trafficLightList = new List<GameObject>();

            configloader = new InitialConfigLoader();
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
        public void initializeScene(string OSMfilename, HeightmapContinent continent, MapProvider provider)
        {
            OSMparser parser = new OSMparser();
            scenebbox = parser.readBBox(OSMfilename);
            scenebbox = editbbox(scenebbox);
            config = configloader.loadInitialConfig();
            
            heightMap = new HeighmapLoader(scenebbox,continent);
            terrain = new myTerrain(heightMap, scenebbox, OSMfilename, provider);

            osmxml = parser.parseOSM(OSMfilename);
            assignNodePositions();

            drawTrees(osmxml.treeList);

            for (int k = 0; k < osmxml.buildingRelations.Count;k++)
            {
                //continue;
                buildingList.Add(new Building(osmxml.buildingRelations[k], config.buildingConfig));
            }

            for (int k = 0; k < osmxml.wayList.Count; k++)
            {
                Way w = osmxml.wayList[k];

                switch (getWayTpe(w))
                {
                    case wayType.building:
                        //continue;
                        if (!isDuplicateBuilding(w.id))
                            buildingList.Add(new Building(w, config.buildingConfig));
                        break;
                    case wayType.highway:                     
                            highwayList.Add(new Highway(w, config.highwayConfig,terrain));
                        break;
                    case wayType.area:
                        //continue;
                        areaList.Add(new Area(w, config.areaConfig));
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


             Debug.Log("<color=red>Scene Info:</color> BuildingCount:" + buildingList.Count.ToString() + " HighwayCount:" + highwayList.Count);

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
                    return wayType.highway;
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
                GameObject tree = (GameObject) MonoBehaviour.Instantiate(Resources.Load("Prefabs/Environment/SpeedTree/BroadLeaf/BroadLeaf_Desktop"));
                tree.transform.position = _treeList[i].meterPosition;
                treeList.Add(tree);
                
            }
        }

        //For relation building : check for duplicate with normal building 
        private bool isDuplicateBuilding(string id)
        {
            for (int k = 0; k < osmxml.relationList.Count; k++)
                if (id == osmxml.relationList[k].id)
                    return true;

            return false;

        }
    }
}
