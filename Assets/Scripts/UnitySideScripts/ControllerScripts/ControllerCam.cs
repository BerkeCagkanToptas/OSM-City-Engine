using Assets.Scripts.UnitySideScripts.EditingScripts;
using Assets.Scripts.UnitySideScripts.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.UnitySideScripts.ControllerScripts
{
    class ControllerCam : MonoBehaviour
    {
        private Camera camera;
        private CameraSetting camSetting;
        private RenderTexture rt;
        private int resolutionWidth, resolutionHeight;
        public string saveFolder;

        private LogEntry currentLogEntry;

        void OnPostRender()
        {
            Texture2D screenShot = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(camera.pixelRect), 0, 0);
            RenderTexture.active = null;
            byte[] bytes = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(generateScreenShotName(saveFolder, camera.name, currentLogEntry.id), bytes);
            camera.enabled = false;
        }


        private string generateScreenShotName(string saveFolder, string cameraID, int frameNo)
        {
            string Folder = saveFolder + "/" + camera.name;
            return Folder + "/Capture_" + frameNo + ".png";
        }


        public void loadLogEntry(LogEntry entry)
        {
            currentLogEntry = entry;
        }


        public void loadSetting(CameraSetting _camSetting)
        {
            if (rt == null)
            {
                camera = this.GetComponent<Camera>();
                Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
                camera.CopyFrom(mainCamera);

                resolutionWidth = (int)camera.pixelWidth;
                resolutionHeight = (int)camera.pixelHeight;

                rt = new RenderTexture(resolutionWidth, resolutionHeight, 24);
                camera.targetTexture = rt;
            }

            camSetting = _camSetting;
            camera.transform.position = _camSetting.position;
            Quaternion quat = new Quaternion();
            quat.eulerAngles = new Vector3(-_camSetting.pitch,-_camSetting.yaw,-_camSetting.roll);
            camera.transform.localRotation = quat;
            camera.name = _camSetting.id;
            camera.fieldOfView = _camSetting.fieldOfView;



        }

    }
}
