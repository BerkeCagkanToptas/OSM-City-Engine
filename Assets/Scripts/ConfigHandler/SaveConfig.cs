using Assets.Scripts.HeightMap;
using Assets.Scripts.SceneObjects;
using Assets.Scripts.UnitySideScripts.EditingScripts;
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

        [XmlElement("Controller")]
        public ControllerSave controller;
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


    public class ControllerSave
    {
        [Serializable]
        public struct SaveCameraSetting
        {
            public string id;
            public Vector3 position;
            public float yaw, pitch, roll, FOV;
        }

        [Serializable]
        public struct SaveLaserSetting
        {
            public Vector3 position;
            public Vector3 rotation;
            public float FOVvertical, FOVhorizontal, resVertical, resHorizontal;
            public float minDistance, maxDistance;
            public int frameRate;
        }

        public enum ControllerType { CameraVan, Trekker}
        public SaveLaserSetting laserSetting;

        public List<SaveCameraSetting> cameraSettings;
        public ControllerType controllerType;
        public Vector3 controllerRotation;
        public Vector3 controllerPosition;

        public SaveLaserSetting convertToSaveLaser(LaserSetting laserSetting)
        {
            SaveLaserSetting sls = new SaveLaserSetting();
            sls.FOVhorizontal = laserSetting.horizontalFOV;
            sls.FOVvertical = laserSetting.verticalFOV;
            sls.resHorizontal = laserSetting.horizontalResolution;
            sls.resVertical = laserSetting.verticalResolution;
            sls.position = laserSetting.position;
            sls.rotation = laserSetting.rotation;
            sls.minDistance = laserSetting.minDistance;
            sls.maxDistance = laserSetting.maxDistance;
            sls.frameRate = laserSetting.frameRate;

            return sls;
        }
        public List<SaveCameraSetting> convertToSaveCamList(List<CameraSetting> cameraSetting)
        {
            List<SaveCameraSetting> newCamSettingList = new List<SaveCameraSetting>();

            for (int k = 0; k < cameraSetting.Count; k++)
            {
                SaveCameraSetting scs = new SaveCameraSetting();
                scs.FOV = cameraSetting[k].fieldOfView;
                scs.id = cameraSetting[k].id;
                scs.pitch = cameraSetting[k].pitch;
                scs.roll = cameraSetting[k].roll;
                scs.yaw = cameraSetting[k].yaw;
                scs.position = cameraSetting[k].position;
                newCamSettingList.Add(scs);
            }
            return newCamSettingList;
        }

        public LaserSetting convertBackToLaser(SaveLaserSetting saveLaser)
        {
            LaserSetting ls = new LaserSetting();
            ls.minDistance = saveLaser.minDistance;
            ls.maxDistance = saveLaser.maxDistance;
            ls.horizontalFOV = saveLaser.FOVhorizontal;
            ls.verticalFOV = saveLaser.FOVvertical;
            ls.verticalResolution = saveLaser.resVertical;
            ls.horizontalResolution = saveLaser.resHorizontal;
            ls.position = saveLaser.position;
            ls.rotation = saveLaser.rotation;
            ls.frameRate = saveLaser.frameRate;

            return ls;
        }

        public List<CameraSetting> convertBackToCamList(List<SaveCameraSetting> saveCamList)
        {
            List<CameraSetting> camSettingList = new List<CameraSetting>();
            
            for(int k = 0 ; k < saveCamList.Count ; k++)
            {
                CameraSetting cs = new CameraSetting();
                cs.position = saveCamList[k].position;
                cs.roll = saveCamList[k].roll;
                cs.yaw = saveCamList[k].yaw;
                cs.pitch = saveCamList[k].pitch;
                cs.id = saveCamList[k].id;
                cs.fieldOfView = saveCamList[k].FOV;
                camSettingList.Add(cs);
            }

            return camSettingList;
        }

    }

    class SaveConfig
    {


        private SceneSave sceneSave;
        private Scene scene;
        private string savePath;


        public SaveConfig(string _savePath,Scene _scene)
        {
            sceneSave = new SceneSave();
            scene = _scene;
            savePath = _savePath;
            sceneSave.osmPath = _scene.OSMPath;
            sceneSave.continent = _scene.continent;
            sceneSave.provider = _scene.provider;
        }

        public void saveConfigurations()
        {
            saveBuildingConfig();
            saveHighwayConfig();
            saveObjectConfig();
            saveControllerConfig();

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

        private void saveControllerConfig()
        {
            if (scene.controller == null)
                return;

            CameraVanEdit cve = GameObject.Find("Canvas").transform.Find("CameraVanEdit").GetComponent<CameraVanEdit>();
                      
            sceneSave.controller = new ControllerSave();
            sceneSave.controller.laserSetting = sceneSave.controller.convertToSaveLaser(cve.laserScanner);
            sceneSave.controller.cameraSettings = sceneSave.controller.convertToSaveCamList(cve.cameraList);

            if (scene.controller.name == "Camera Van")
                sceneSave.controller.controllerType = ControllerSave.ControllerType.CameraVan;
            else
                sceneSave.controller.controllerType = ControllerSave.ControllerType.Trekker;

            sceneSave.controller.controllerRotation = scene.controller.transform.rotation.eulerAngles;
            sceneSave.controller.controllerPosition = scene.controller.transform.position;

        }




    }
}
