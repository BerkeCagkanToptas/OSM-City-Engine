using Assets.Scripts.UnitySideScripts.MouseScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Utils;
using System.IO;
using Assets.Scripts.SceneObjects;

namespace Assets.Scripts.UnitySideScripts.Menus.AddObjectMenu
{
    class ObjectTypeSelector : MonoBehaviour
    {
        private GameObject fileBrowser;
        private myFileBrowserDialog fbd;


        public GameObject listObject = null;
        ObjectList objectList;

        void Start()
        {
            objectList = listObject.GetComponent<ObjectList>();

            fileBrowser = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Menu/FileBrowser"));
            fileBrowser.transform.SetParent(GameObject.Find("Canvas").transform);
            fileBrowser.SetActive(false);
            RectTransform rt = fileBrowser.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);
            fbd = fileBrowser.GetComponent<myFileBrowserDialog>();
        }

        void Update()
        {
            if (fbd.state == myFileBrowserDialog.BrowserState.Selected)
            {
                LoadExternalOBJ loader = new LoadExternalOBJ();

                Transform camera = GameObject.Find("Main Camera").transform;
                Vector3 position = camera.position + camera.forward * 10.0f;

                Object3D obj = new Object3D();
                obj.type = ObjectType.External;
                obj.resourcePath = fbd.selectedPath;
                try
                {
                    obj.object3D = loader.loadOBJ(fbd.selectedPath);
                }
                catch (Exception ex)
                {
                    Debug.Log("<color=red>LOADER ERROR</color> " + ex.Message);
                    fbd.state = myFileBrowserDialog.BrowserState.None;
                    return;
                }
                obj.object3D.tag = "3DObject";
                obj.object3D.AddComponent<Object3dMouseHandler>();
                obj.object3D.transform.position = position;

                LoadSaveMenu lsm = GameObject.Find("Canvas").transform.Find("LoadSave Menu").GetComponent<LoadSaveMenu>();
                lsm.scene.object3DList.Add(obj);

                fbd.state = myFileBrowserDialog.BrowserState.None;
            }

        }

        public void carClick()
        {
            objectList.fillCarList();
        }

        public void treeClick()
        {
            objectList.fillTreeList();
        }

        public void barrierClick()
        {
            objectList.fillBarrierList();
        }

        public void trafficSignClick()
        {
            objectList.fillTrafficSignList();
        }

        public void newObjectClick()
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            fbd.draw(myFileBrowserDialog.BrowserMode.FileSelect, di, new string[] { ".obj", ".txt", ".mtl" });
        }

        public void cameraVanClick()
        {
           
            Transform mainCamera = GameObject.Find("Main Camera").transform;
            CameraController controller = mainCamera.GetComponent<CameraController>();
            LoadSaveMenu lsm = GameObject.Find("Canvas").transform.Find("LoadSave Menu").GetComponent<LoadSaveMenu>();

            if(lsm.scene.controller != null)
            {
                Alert.Alert alertdialog = new Alert.Alert();
                alertdialog.openInteractableAlertDialog("ERROR", "A Controller already exists. Please remove the other controller first...");
                return;
            }

            lsm.scene.controller = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Car/PolimiCameraCar/CameraVan"));
            lsm.scene.controller.AddComponent<CameraVanMouseHandler>();
            lsm.scene.controller.tag = "CameraVan";
            lsm.scene.controller.name = "Camera Van";
            lsm.scene.controller.transform.position = mainCamera.position + mainCamera.forward * 10.0f;
            lsm.scene.controller.GetComponent<Rigidbody>().useGravity = false;
            controller.target = lsm.scene.controller.transform;

        }

        public void thirdPersonClick()
        {
            Transform mainCamera = GameObject.Find("Main Camera").transform;
            CameraController controller = mainCamera.GetComponent<CameraController>();
            LoadSaveMenu lsm = GameObject.Find("Canvas").transform.Find("LoadSave Menu").GetComponent<LoadSaveMenu>();

            if (lsm.scene.controller != null)
            {
                Alert.Alert alertdialog = new Alert.Alert();
                alertdialog.openInteractableAlertDialog("ERROR", "A Controller already exists. Please remove the other controller first...");
                return;
            }
            
            lsm.scene.controller = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Ethan/ThirdPersonController"));
            lsm.scene.controller.AddComponent<CameraVanMouseHandler>();
            lsm.scene.controller.tag = "CameraVan";
            lsm.scene.controller.name = "Third Person (Ethan)";
            lsm.scene.controller.transform.position = mainCamera.position + mainCamera.forward * 10.0f;
            controller.target = lsm.scene.controller.transform;

        }
    }
}
