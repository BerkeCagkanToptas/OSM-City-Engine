using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine.UI;
using UnityEngine;
using Assets.Scripts.UnitySideScripts.Menus;
using System.IO;
using Assets.Scripts.UnitySideScripts.ControllerScripts;

namespace Assets.Scripts.UnitySideScripts.EditingScripts
{
    [XmlRoot("RecordLog")]
    public struct RecordLog
    {
        [XmlArray("CameraSettings")]
        [XmlArrayItem("Cam")]
        public List<CameraSetting> cameraSettings;
        [XmlElement("CameraLog")]
        public CameraLog cameraLog;
        [XmlElement("LaserLog")]
        public LaserLog laserLog;
    }

    public struct CameraSetting
    {
        [XmlAttribute("CameraID")]
        public string id;
        [XmlAttribute("Pitch")]
        public float pitch;
        [XmlAttribute("Yaw")]
        public float yaw;
        [XmlAttribute("Roll")]
        public float roll;
        [XmlAttribute("Position")]
        public Vector3 position;
        [XmlAttribute("FOV")]
        public float fieldOfView;
    }

    public struct LaserSetting
    {
        [XmlElement("MaximumDistance")]
        public Vector3 position;
        [XmlElement("MinimumDistance")]
        public float minDistance;
        [XmlElement("MaximumDistance")]
        public float maxDistance;
        [XmlElement("verticalFOV")]
        public float verticalFOV;
        [XmlElement("horizontalFOV")]
        public float horizontalFOV;
        [XmlElement("verticalResolution")]
        public float verticalResolution;
        [XmlElement("horizontalResolution(azimuth)")]
        public float horizontalResolution;
        [XmlElement("frameRate")]
        public float frameRate;
    }

    public struct CameraLog
    {
        [XmlAttribute("FrameRate")]
        public int frameRate;
        [XmlArray("CameraEntries")]
        [XmlArrayItem("Entry")]
        public List<LogEntry> cameraEntries;
    }

    public struct LaserLog
    {
        [XmlAttribute("FrameRate")]
        public int frameRate;
        [XmlArray("LaserEntries")]
        [XmlArrayItem("Entry")]
        public List<LogEntry> laserEntries;
    }

    public struct LogEntry
    {
        [XmlAttribute("id")]
        public int id;
        [XmlAttribute("Rotation")]
        public Vector3 rotation;
        [XmlAttribute("Position")]
        public Vector3 position;
        [XmlAttribute("Steer")]
        public float steer;
        [XmlAttribute("Velocity")]
        public float velocity;
    }

    class CameraVanEdit : MonoBehaviour
    {
        myFileBrowserDialog fbd;
        GameObject fileBrowser;

        List<GameObject> cameraSet;
        GameObject cameraSetOrigin;
        GameObject controllerObject;
        CarController carController;
        CameraMode cameraMode = CameraMode.None;
        public List<CameraVanCameraSettings.CameraSettingItem> cameraList;
        public LaserSetting laserScanner;

        RecordLog recordLog;
        private int cameraIDIterator = -1;
        private int cameraLogEntryIterator = 0;

        private float cameraCheckingTime = 0.0f;
        private float laserCheckingTime = 0.0f;
        private float cameraFrameTime = 1.0f/24.0f;
        private float laserFrameTime = 1.0f/10.0f;
        private int cameraFrameID = 0;
        private int laserFrameID = 0;
        private string saveFolder;

        Text steerText;
        Text velocityText;

        private bool hasCamera = false, hasLaserScanner = false;


        private enum CameraMode
        {
            Recording,
            Processing,
            GeneratingScreenShots,
            GeneratingPCD,
            None
        }


        void FixedUpdate()
        {
            switch (cameraMode)
            {
                case CameraMode.Recording:
                    recordModeOperations();
                    break;
                case CameraMode.Processing:
                    processModeOperations();
                    break;
                case CameraMode.None:
                    break;
            }    
        }

        void LateUpdate()
        {
            if (cameraMode == CameraMode.GeneratingScreenShots)
                generateScreenShotOperations();
            else if (cameraMode == CameraMode.GeneratingPCD)
                generatePCDOperations();
        }


        void Start()
        {

            recordLog = new RecordLog();
            recordLog.cameraLog = new CameraLog();
            recordLog.cameraLog.cameraEntries = new List<LogEntry>();
            recordLog.laserLog = new LaserLog();
            recordLog.laserLog.laserEntries = new List<LogEntry>();

            fileBrowser = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Menu/FileBrowser"));
            fileBrowser.transform.SetParent(GameObject.Find("Canvas").transform);
            fileBrowser.SetActive(false);
            RectTransform rt = fileBrowser.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);
            fbd = fileBrowser.GetComponent<myFileBrowserDialog>();

            steerText = transform.Find("Panel").Find("TextSteer").GetComponent<Text>();
            velocityText = transform.Find("Panel").Find("TextVelocity").GetComponent<Text>();
           
        }

        public void clickSetting(GameObject menu)
        {
            menu.SetActive(true);
        }

        public void clickDestroy()
        {
            controllerObject = GameObject.Find("Camera Van");
            if (controllerObject == null)
                controllerObject = GameObject.Find("Third Person (Ethan)");

            GameObject.Destroy(controllerObject);
            carController = null;
            this.gameObject.SetActive(false);
        }

        public void clickClose()
        {
            this.gameObject.SetActive(false);
        }

        public void onFrameRateChange(InputField frameRateBox)
        {
            cameraFrameTime = 1.0f / float.Parse(frameRateBox.text);
        }

        public void clickPlayPause()
        {
           Button cameraButton = this.transform.Find("Panel").Find("ButtonCameraMode").GetComponent<Button>();
           
           if(cameraMode == CameraMode.None)
           {
               controllerObject = GameObject.Find("Camera Van");
               if (controllerObject != null)
                   carController = controllerObject.GetComponent<CarController>();
               else
                   controllerObject = GameObject.Find("Third Person (Ethan)");

               if (cameraList != null && cameraList.Count > 0)
                   hasCamera = true;
               //if (laserList != null && laserList.Count > 0)
               //    hasLaserScanner = true;

               cameraMode = CameraMode.Recording;
               cameraButton.image.sprite = Resources.Load<Sprite>("Textures/Menu/CameraVan/stopIcon");
           }
           else if(cameraMode == CameraMode.Recording)
           {
               if (carController != null)
               {
                   carController.wheelColliders[0].brakeTorque = 1000;
                   carController.wheelColliders[1].brakeTorque = 1000;
               }
               cameraMode = CameraMode.Processing;
               cameraButton.image.sprite = Resources.Load<Sprite>("Textures/Menu/CameraVan/playIcon");
               cameraButton.interactable = false;
               DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
               fbd.draw(myFileBrowserDialog.BrowserMode.FolderSelect, di, new string[] { ".xml" });

           }

        }

        private void generateLog(string path)
        {
            var serializer = new XmlSerializer(typeof(RecordLog));
            var encoding = Encoding.GetEncoding("UTF-8");

            string logPath = path + "/OUTPUTLOG.xml";

            using (var stream = new StreamWriter(logPath, false, encoding))//new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, recordLog);
            }
        }

        private void recordModeOperations()
        {
            if(hasCamera)
                cameraCheckingTime += Time.fixedDeltaTime;
            if(hasLaserScanner)
                laserCheckingTime += Time.fixedDeltaTime;

            if (cameraCheckingTime > cameraFrameTime)
            {
                cameraCheckingTime -= cameraFrameTime;
                Vector3 translate = controllerObject.transform.position;
                Vector3 rotation = controllerObject.transform.rotation.eulerAngles;
                float steer = 0, velocity = 0;
                if (carController != null)
                {
                    
                    steer = carController.wheelColliders[0].steerAngle;
                    velocity = carController.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
                    steerText.text = steer.ToString();
                    velocityText.text = velocity.ToString();
                }

                LogEntry entry = new LogEntry();
                entry.id = cameraFrameID++;
                entry.velocity= velocity;
                entry.position = translate;
                entry.rotation = rotation;
                entry.steer = steer;
                recordLog.cameraLog.cameraEntries.Add(entry);
            }

            if (laserCheckingTime > laserFrameTime)
            {
                laserCheckingTime -= laserFrameTime;
                Vector3 translate = controllerObject.transform.position;
                Vector3 rotation = controllerObject.transform.rotation.eulerAngles;
                float steer = 0, velocity = 0;
                if (carController != null)
                {
                    steer = carController.wheelColliders[0].steerAngle;
                    velocity = carController.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
                }

                LogEntry entry = new LogEntry();
                entry.id = cameraFrameID++;
                entry.velocity = velocity;
                entry.position = translate;
                entry.rotation = rotation;
                entry.steer = steer;
                recordLog.laserLog.laserEntries.Add(entry);
            }

            return;
            
        }

        private void processModeOperations()
        {

            if (fbd.state == myFileBrowserDialog.BrowserState.Processing)
                return;
            else if (fbd.state == myFileBrowserDialog.BrowserState.Cancelled)
            {
                if (carController != null)
                {
                    carController.wheelColliders[0].brakeTorque = 0;
                    carController.wheelColliders[1].brakeTorque = 0;
                }
                cameraMode = CameraMode.None;
                return;
            }
            else
                saveFolder = fbd.selectedPath + "/" + fbd.saveName;
            Debug.Log("Processing Coordinates");
            Directory.CreateDirectory(saveFolder);

            recordLog.cameraLog.frameRate = int.Parse(transform.Find("Panel").Find("IFframeRate").GetComponent<InputField>().text);
            recordLog.cameraSettings = new List<CameraSetting>();

            for (int c = 0; c < cameraList.Count; c++)
            {
                CameraSetting cset = new CameraSetting();
                cset.fieldOfView = cameraList[c].fieldOfView;
                cset.id = cameraList[c].id;
                cset.position = cameraList[c].position;
                cset.pitch = cameraList[c].pitch;
                cset.yaw = cameraList[c].yaw;
                cset.roll = cameraList[c].roll;
                recordLog.cameraSettings.Add(cset);
            }

            generateLog(saveFolder);

            cameraSet = new List<GameObject>();
            cameraSetOrigin = new GameObject();

            for (int k = 0; k < cameraList.Count; k++)
            {
                GameObject camObj = new GameObject("Cam" + cameraList[k].id,typeof(Camera),typeof(ControllerCam));
                ControllerCam camScript = camObj.GetComponent<ControllerCam>();
                camScript.saveFolder = saveFolder;
                camScript.loadSetting(cameraList[k]);
                camObj.transform.SetParent(cameraSetOrigin.transform);
                Directory.CreateDirectory(saveFolder + "/" + camObj.name);
                cameraSet.Add(camObj);
            }
           
            controllerObject.SetActive(false);
            cameraMode = CameraMode.GeneratingScreenShots;
        }

        private void generateScreenShotOperations()
        {
            if (updateIterators())
            {
                Button cameraButton = this.transform.Find("Panel").Find("ButtonCameraMode").GetComponent<Button>();
                cameraButton.interactable = true;

                if (carController != null)
                {
                    carController.wheelColliders[0].brakeTorque = 0;
                    carController.wheelColliders[1].brakeTorque = 0;
                }
                controllerObject.SetActive(true);
                cameraMode = CameraMode.None;
                return;
            }

          //  SSgenerator.TakeScreenShot(cameraSet[cameraIDIterator].gameObject,saveFolder,recordLog.cameraLog.cameraEntries[cameraLogEntryIterator]);

            ControllerCam cameraScript = cameraSet[cameraIDIterator].GetComponent<ControllerCam>();
            cameraScript.loadLogEntry(recordLog.cameraLog.cameraEntries[cameraLogEntryIterator]);

            cameraSetOrigin.transform.position = recordLog.cameraLog.cameraEntries[cameraLogEntryIterator].position;
            Quaternion quat = new Quaternion();
            quat.eulerAngles = recordLog.cameraLog.cameraEntries[cameraLogEntryIterator].rotation;
            cameraSetOrigin.transform.localRotation = quat;
            cameraSet[cameraIDIterator].GetComponent<Camera>().enabled = true;
            cameraSet[cameraIDIterator].GetComponent<Camera>().Render();
        }

        private void generatePCDOperations()
        {


        }


        private bool updateIterators()
        {
            cameraIDIterator++;
            if (cameraIDIterator == cameraList.Count)
            {
                cameraIDIterator = 0;
                cameraLogEntryIterator++;
                if (cameraLogEntryIterator == recordLog.cameraLog.cameraEntries.Count)
                {
                    cameraLogEntryIterator--;
                    return true;
                }
            }


            return false;
        }

    }


}
