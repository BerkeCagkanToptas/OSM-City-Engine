using Assets.Scripts.UnitySideScripts.EditingScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.UnitySideScripts
{
    class ScreenshotGenerator : MonoBehaviour
    {
        
        //*****************************
        private int resWidth;
        private int resHeight;
        private int namingIndex;
        public string saveFolder;
        //******************************
       
        public int frameRate = 24;
        private List<Vector3> transformList;
        private List<Vector3> rotateList;
        private Transform cameraTransform;
        private int index = 0;

        void Start()
        {
            Cursor.visible = false;
            Application.runInBackground = true;

            // Set the playback framerate!
            Time.captureFramerate = frameRate;

            transformList = this.GetComponent<CameraVanEdit>().translateList;
            rotateList = this.GetComponent<CameraVanEdit>().rotateList;
            cameraTransform = GameObject.Find("Main Camera").transform;


            namingIndex = 0;
        }



        void Update()
        {

        }

        private string ScreenShotName(string camID,string saveFolder)
        {
            return saveFolder + "/" + camID + "/" + "image" + namingIndex.ToString() + ".png";         
        }

        void TakeScreenShot(GameObject cameraVanCenter, string savePath)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("picCam"))
            {
                Camera cam = go.GetComponent<Camera>();
                resWidth = (int)cam.pixelWidth;
                resHeight = (int)cam.pixelHeight;
                cam.Render();
                RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
                Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(cam.pixelRect), 0, 0);
                byte[] bytes = screenShot.EncodeToPNG();
              
 //               System.IO.File.WriteAllBytes(ScreenShotName(camID,saveFolder), bytes);
            }



        }


    }
}
