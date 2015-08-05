using Assets.Scripts.UnitySideScripts.MouseScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.Menus.AddObjectMenu
{
    class ObjectTypeSelector : MonoBehaviour
    {
        public GameObject listObject = null;
        ObjectList objectList;

        void Start()
        {
            objectList = listObject.GetComponent<ObjectList>();
           
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
            throw new NotImplementedException();
        }

        public void cameraVanClick()
        {
            Transform mainCamera = GameObject.Find("Main Camera").transform;
            CameraController controller = mainCamera.GetComponent<CameraController>();
            
            
            GameObject cameraVan = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Car/PolimiCameraCar/CameraVan"));
            cameraVan.AddComponent<CameraVanMouseHandler>();
            cameraVan.tag = "CameraVan";
            cameraVan.name = "Camera Van";
            cameraVan.transform.position = mainCamera.position + mainCamera.forward * 10.0f;
            cameraVan.GetComponent<Rigidbody>().useGravity = false;
            controller.target = cameraVan.transform;

        }

        public void thirdPersonClick()
        {
            Transform mainCamera = GameObject.Find("Main Camera").transform;
            CameraController controller = mainCamera.GetComponent<CameraController>();


            GameObject thirdPerson = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Ethan/ThirdPersonController"));
            thirdPerson.AddComponent<CameraVanMouseHandler>();
            thirdPerson.tag = "CameraVan";
            thirdPerson.name = "Third Person (Ethan)";
            thirdPerson.transform.position = mainCamera.position + mainCamera.forward * 10.0f;
            controller.target = thirdPerson.transform;

        }
    }
}
