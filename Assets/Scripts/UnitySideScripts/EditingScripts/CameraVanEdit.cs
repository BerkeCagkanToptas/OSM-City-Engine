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
using System.Collections;
using Assets.Scripts.UnitySideScripts.Menus.Alert;

namespace Assets.Scripts.UnitySideScripts.EditingScripts
{
    [XmlRoot("RecordLog")]
    public struct RecordLog
    {
        [XmlArray("CameraSettings")]
        [XmlArrayItem("Cam")]
        public List<CameraSetting> cameraSettings;
        [XmlElement("LaserSettings")]
        public LaserSetting laserSettings;
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
        [XmlAttribute("Pos")]
        public Vector3 position;
        [XmlAttribute("Rot")]
        public Vector3 rotation;
        [XmlAttribute("MinimumDist")]
        public float minDistance;
        [XmlAttribute("MaximumDist")]
        public float maxDistance;
        [XmlAttribute("verticalFOV")]
        public float verticalFOV;
        [XmlAttribute("horizontalFOV")]
        public float horizontalFOV;
        [XmlAttribute("verticalRes")]
        public float verticalResolution;
        [XmlAttribute("horizontalRes")]
        public float horizontalResolution;
        [XmlAttribute("FPS")]
        public int frameRate;
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

    public struct PCDrow
    {
        public Vector3 position;
        public float distance;
    }

    class CameraVanEdit : MonoBehaviour
    {
        myFileBrowserDialog fbd;
        GameObject fileBrowser;
        Alert alert;

        List<GameObject> cameraSet;
        GameObject cameraSetOrigin;
        GameObject controllerObject;
        CarController carController;
        CameraMode cameraMode = CameraMode.None;
        public List<CameraSetting> cameraList;
        public LaserSetting laserScanner;
        GameObject liDAROrigin;

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

        System.Diagnostics.Stopwatch stopwatch;
            

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

            LoadSaveMenu lsm = GameObject.Find("Canvas").transform.Find("LoadSave Menu").GetComponent<LoadSaveMenu>();
            lsm.scene.controller = null;

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
               if (laserScanner.frameRate != 0)
               {
                   laserFrameTime = 1.0f / laserScanner.frameRate;
                   hasLaserScanner = true;
               }
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
                entry.id = laserFrameID++;
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
            StartCoroutine(processHelper());

            Directory.CreateDirectory(saveFolder);

            if (cameraList == null)
            {                
                recordLog.laserSettings = laserScanner;
                recordLog.laserLog.frameRate = laserScanner.frameRate;
                generateLog(saveFolder);
                cameraMode = CameraMode.GeneratingPCD;
                return;
            }
            else
            {
                recordLog.cameraLog.frameRate = int.Parse(transform.Find("Panel").Find("IFframeRate").GetComponent<InputField>().text);
                recordLog.cameraSettings = new List<CameraSetting>();

                cameraSet = new List<GameObject>();
                cameraSetOrigin = new GameObject();

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

                    GameObject camObj = new GameObject("Cam" + cameraList[c].id, typeof(Camera), typeof(ControllerCam));
                    ControllerCam camScript = camObj.GetComponent<ControllerCam>();
                    camScript.saveFolder = saveFolder;
                    camScript.loadSetting(cameraList[c]);
                    camObj.transform.SetParent(cameraSetOrigin.transform);
                    Directory.CreateDirectory(saveFolder + "/" + camObj.name);
                    cameraSet.Add(camObj);

                }

            }

            generateLog(saveFolder);
            controllerObject.SetActive(false);
            cameraMode = CameraMode.GeneratingScreenShots;

            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

        }

        private void generateScreenShotOperations()
        {
            if (updateIterators())
            {
                stopwatch.Stop();
                Debug.Log("<color=green>SCREEN SHOT GENERATE TIME:</color>" + stopwatch.ElapsedMilliseconds);
                cameraMode = CameraMode.GeneratingPCD;
                for (int k = 0; k < cameraSet.Count; k++)               
                    cameraSet[k].GetComponent<Camera>().enabled = false;                
                return;
            }

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
            Button cameraButton = this.transform.Find("Panel").Find("ButtonCameraMode").GetComponent<Button>();

            if (!hasLaserScanner)
            {
                cameraMode = CameraMode.None;
                alert.closeAlertDialog();
                cameraButton = this.transform.Find("Panel").Find("ButtonCameraMode").GetComponent<Button>();
                cameraButton.interactable = true;

                if (carController != null)
                {
                    carController.wheelColliders[0].brakeTorque = 0;
                    carController.wheelColliders[1].brakeTorque = 0;
                }
                controllerObject.SetActive(true);
                return;
            }

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();


            //This is the Game Object contains transform of the camera Van
            liDAROrigin = new GameObject();

            //This is the Game Object contains transform information of the LiDAR device wrt Camera Van
            GameObject liDAR = new GameObject();
            liDAR.transform.position = laserScanner.position;
            Quaternion liDARquat = new Quaternion();
            liDARquat.eulerAngles = laserScanner.rotation;
            liDAR.transform.rotation = liDARquat;

            //liDAR and Camera Van is Linked
            liDAR.transform.SetParent(liDAROrigin.transform);            
                       
            Directory.CreateDirectory(saveFolder + "\\PCD");

            int verticalBeamCount =  (int)(laserScanner.verticalFOV / laserScanner.verticalResolution) +1;
            int horizontalBeamCount = (int)(laserScanner.horizontalFOV / laserScanner.horizontalResolution) +1;

            for (int k = 0 ; k < recordLog.laserLog.laserEntries.Count ; k++)
            {
                liDAROrigin.transform.position = recordLog.laserLog.laserEntries[k].position;
                Quaternion quat = new Quaternion();
                quat.eulerAngles = recordLog.laserLog.laserEntries[k].rotation;
                liDAROrigin.transform.localRotation = quat;

                RaycastHit hit = new RaycastHit();
                List<PCDrow> PCDList = new List<PCDrow>();

                Vector3 laserDirection;

                for(int i = 0 ; i < verticalBeamCount ; i++)
                {
                    Vector3 vertlaserDirection = Quaternion.AngleAxis(laserScanner.verticalFOV / 2 - i * laserScanner.verticalResolution, -liDAR.transform.right) * liDAR.transform.forward;
 
                    for(int j = 0 ; j < horizontalBeamCount ; j++)
                    {
                        laserDirection = Quaternion.AngleAxis(-laserScanner.horizontalFOV / 2 + j * laserScanner.horizontalResolution, liDAR.transform.up) * vertlaserDirection;

                        //FOR DEBUG
                        //Debug.DrawRay(liDAR.transform.position, laserDirection, new Color(255, 0, 0), 120);
                        
                        bool isHit = Physics.Raycast(liDAR.transform.position, laserDirection, out hit,laserScanner.maxDistance);
                                             
                        PCDrow row = new PCDrow();
                        if (isHit && (hit.distance > laserScanner.minDistance))
                        {
                            row.position = hit.point;
                            row.distance = hit.distance;
                        }
                        else
                        {
                            row.position = new Vector3();
                            row.distance = 0.0f;
                        }
                            PCDList.Add(row);
                  
                    }
                }

                PCDGenerator(saveFolder + "\\PCD\\frame_" + k.ToString() + ".pcd" ,PCDList, horizontalBeamCount,verticalBeamCount);
               
                //FOR DEBUG
                //cameraMode = CameraMode.None;
                //alert.closeAlertDialog();
                //return;
            }

            stopwatch.Stop();
            Debug.Log("<color=blue>PCD FILE GENERATION TIME</color>" + stopwatch.ElapsedMilliseconds);
         
            cameraButton.interactable = true;
            if (carController != null)
            {
                carController.wheelColliders[0].brakeTorque = 0;
                carController.wheelColliders[1].brakeTorque = 0;
            }
            controllerObject.SetActive(true);
            cameraMode = CameraMode.None;
            alert.closeAlertDialog();
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

        private void PCDGenerator(string savePath, List<PCDrow> pointList, int horizontalBeamCount, int verticalBeamCount)
        {
            string content;

            string header = "#POLIMI OSM CITY ENGINE PCD DATA" + "\n" +
                            "VERSION .7" + "\n" +
                            "FIELDS x y z distance" + "\n" +
                            "SIZE 4 4 4 4" + "\n" +
                            "TYPE F F F F" + "\n" +
                            "COUNT 1 1 1 1" + "\n" +
                            "WIDTH " + horizontalBeamCount.ToString() + "\n" +
                            "HEIGHT " + verticalBeamCount.ToString() + "\n" +
                            "VIEWPOINT 0 0 0 1 0 0 0" + "\n" +
                            "POINTS " + (horizontalBeamCount * verticalBeamCount).ToString() + "\n" +
                            "DATA ascii" + "\n";

            string data = "";

            for (int k = 0; k < pointList.Count; k++)
            {
                data = data + pointList[k].position.x.ToString() + " " + pointList[k].position.y.ToString() + " " + pointList[k].position.z.ToString() +
                       " " + pointList[k].distance.ToString() + "\n";
            }

            content = header + data;
            System.IO.File.WriteAllText(savePath, content);
        }

        private IEnumerator processHelper()
        {
            alert = new Alert();
            alert.openAlertDialog("Generating Output Data might take several minutes depends on settings. Please wait...");
            yield return new WaitForSeconds(0.2f);
        }
    }


}
