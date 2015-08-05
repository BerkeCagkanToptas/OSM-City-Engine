using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using Assets.Scripts.UnitySideScripts.Menus;

namespace Assets.Scripts.UnitySideScripts.EditingScripts
{
    class CameraVanEdit : MonoBehaviour
    {
        CameraController controller;
        CameraMode cameraMode = CameraMode.None;
        public List<CameraVanCameraSettings.CameraSettingItem> cameraList;

        public List<Vector3> translateList;
        public List<Vector3> rotateList;        

        private float checkingTime = 0.0f;
        private float frameTime = 1.0f/24.0f;
        private enum CameraMode
        {
            Recording,
            Processing,
            None
        }


        void FixedUpdate()
        {
            if (cameraMode == CameraMode.Recording)
            {
                checkingTime += Time.fixedDeltaTime;

                if (checkingTime > frameTime)
                {
                    checkingTime -= frameTime;

                    Vector3 translate = controller.target.transform.position;
                    Vector3 rotation = controller.target.transform.rotation.eulerAngles;
                    translateList.Add(translate);
                    rotateList.Add(rotation);
                }
            }
 
            if (cameraMode == CameraMode.Processing)
            {
                Debug.Log("Processing Coordinates");
                Application.runInBackground = true;
                cameraMode = CameraMode.None;
            }
    
        }

        void Start()
        {
            controller = GameObject.Find("Main Camera").GetComponent<CameraController>();
            translateList = new List<Vector3>();
            rotateList = new List<Vector3>();
        }

        public void clickSetting(GameObject menu)
        {
            menu.SetActive(true);
        }

        public void clickDestroy()
        {
            GameObject.Destroy(controller.target.gameObject);
            controller.target = null;
            this.gameObject.SetActive(false);
        }

        public void clickClose()
        {
            this.gameObject.SetActive(false);
        }

        public void onFrameRateChange(InputField frameRateBox)
        {
            frameTime = 1.0f / float.Parse(frameRateBox.text);
        }

        public void clickPlayPause()
        {
            Button cameraButton = this.transform.Find("Panel").Find("ButtonCameraMode").GetComponent<Button>();

           if(cameraMode == CameraMode.None)
           {
               cameraMode = CameraMode.Recording;
               cameraButton.image.sprite = Resources.Load<Sprite>("Textures/Menu/CameraVan/stopIcon");               
           }
           else if(cameraMode == CameraMode.Recording)
           {
               cameraMode = CameraMode.Processing;
               cameraButton.image.sprite = Resources.Load<Sprite>("Textures/Menu/CameraVan/playIcon");
               cameraButton.interactable = false;
               
               //Buraya process ediyoz gibi bir menu cikart

           }

        }

    }
}
