using Assets.Scripts.HeightMap;
using Assets.Scripts.SceneObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets.Scripts.ConfigHandler
{

    [XmlRoot("ProjectSave")]
    public class SceneSave
    {         
        [XmlElement("Provider")]
        public MapProvider provider;

        [XmlElement("OSMPath")]
        public string osmPath;

        [XmlElement("Continent")]
        public HeightmapContinent continent;

        [XmlArray("BuildingSaveList")]
        [XmlArrayItem("Building")]
        public List<BuildingSave> buildingSaveList;

        [XmlArray("HighwaySaveList")]
        [XmlArrayItem("Highway")]
        public List<HighwaySave> highwaySaveList;

        [XmlArray("ObjectSaveList")]
        [XmlArrayItem("Object")]
        public List<ObjectSave> objectSaveList;
    }


    public class BuildingSave
    {
        [XmlAttribute("ID")]
        public string buildingID;

        //Height of building
        public float height;

        //If Building has replaced with its 3D model, modelPath is assigned
        public bool has3Dmodel;
        public string modelPath;
     //   public Vector3 GOtranslate, GOrotate, GOscale;

        //If Building has Facade Edit FacadeSkin List will be filled otherwise default material id will be assigned
        public int materialID;
        public List<FacadeSkin> skins;
    }


    public class HighwaySave
    {
        [XmlAttribute("ID")]
        public string highwayID;
        public bool hasLeftSidewalk, hasRightSidewalk;
        public float leftSidewalkSize, rightSidewalkSize;
        public float waySize;
    }


    public class ObjectSave
    {
        [XmlAttribute("Name")]
        public string name;
        public ObjectType type;
        public string resourcePath;
        public Vector3 translate, rotate, scale;
    }


    class SaveConfig
    {


        private SceneSave sceneSave;
        private Scene scene;
        private string savePath;


        public SaveConfig(string _savePath,Scene _scene,string _osmPath)
        {
            sceneSave = new SceneSave();
            scene = _scene;
            savePath = _savePath;
            sceneSave.osmPath = _osmPath;
            sceneSave.continent = _scene.continent;
            sceneSave.provider = _scene.provider;
        }

        public void saveConfigurations()
        {
            saveBuildingConfig();
            saveHighwayConfig();
            saveObjectConfig();

            var serializer = new XmlSerializer(typeof(SceneSave));
            var encoding = Encoding.GetEncoding("UTF-8");

            if (File.Exists(savePath))
                File.Delete(savePath);

            using (var stream = new StreamWriter(savePath, false, encoding))
            {
                serializer.Serialize(stream, sceneSave);
            }
            
        }

        private void saveBuildingConfig()
        {
            sceneSave.buildingSaveList = new List<BuildingSave>();

            for (int i = 0; i < scene.buildingList.Count; i++)
            {
                BuildingSave buildingSave = new BuildingSave();
                Building b = scene.buildingList[i];
                buildingSave.buildingID = b.id;
                buildingSave.height = b.buildingHeight;
                buildingSave.has3Dmodel = (b.type == buildingType.custom);
                //if (buildingSave.has3Dmodel)
                //{
                //    buildingSave.modelPath = b.modelPath;
                //    buildingSave.GOrotate = b.GOrotate;
                //    buildingSave.GOtranslate = b.GOtransform;
                //    buildingSave.GOscale = b.GOscale;
                //}
                buildingSave.materialID = b.defaultMaterialID;
                buildingSave.skins = new List<FacadeSkin>(b.facadeSkins);
                sceneSave.buildingSaveList.Add(buildingSave);
            }
        }

        private void saveHighwayConfig()
        {
            sceneSave.highwaySaveList = new List<HighwaySave>();

            for(int i = 0 ; i < scene.highwayList.Count ; i++)
            {
                HighwaySave highwaySave = new HighwaySave();
                Highway h = scene.highwayList[i];
                highwaySave.highwayID = h.id;
                highwaySave.hasLeftSidewalk = h.hasLeftSidewalk;
                highwaySave.hasRightSidewalk = h.hasRightSideWalk;
                highwaySave.leftSidewalkSize = h.leftSidewalkSize;
                highwaySave.rightSidewalkSize = h.rightSidewalkSize;
                highwaySave.waySize = h.waySize;
                sceneSave.highwaySaveList.Add(highwaySave);
            }
        }

        private void saveObjectConfig()
        {
            sceneSave.objectSaveList = new List<ObjectSave>();
            for (int i = 0; i < scene.object3DList.Count; i++)
            {
                Object3D o = scene.object3DList[i];
                ObjectSave objectSave = new ObjectSave();
                objectSave.name = o.name;
                objectSave.type = o.type;
                objectSave.resourcePath = o.resourcePath;
                objectSave.rotate = o.object3D.transform.rotation.eulerAngles;
                objectSave.scale = o.object3D.transform.localScale;
                objectSave.translate = o.object3D.transform.position;
                sceneSave.objectSaveList.Add(objectSave);
            }

        }






    }
}
