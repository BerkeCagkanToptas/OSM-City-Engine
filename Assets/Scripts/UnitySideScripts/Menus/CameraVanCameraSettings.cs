using Assets.Scripts.UnitySideScripts.EditingScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.Menus
{
    class CameraVanCameraSettings : MonoBehaviour
    {
        int cameraidIterate = 2;

        public void clickAddCamera()
        {
            Transform contentPanel = this.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel");
            GameObject cameraItem = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Menu/CameraSetting"));
            cameraItem.transform.SetParent(contentPanel);
            cameraItem.transform.Find("Panel").Find("TextID").GetComponent<Text>().text = cameraidIterate.ToString();
            cameraidIterate++;
        }

        public void clickSaveChanges()
        {
            Transform contentPanel = this.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel");
            List<CameraSetting> cameraList = new List<CameraSetting>();

            foreach(Transform cameraSetting in contentPanel)
            {
                CameraSetting item = new CameraSetting();
                Transform panel = cameraSetting.Find("Panel");
                if (panel.Find("ToggleIsActive").GetComponent<Toggle>().isOn)
                {
                    item.id = panel.Find("TextID").GetComponent<Text>().text;
                    item.pitch = float.Parse(panel.Find("IFpitch").GetComponent<InputField>().text);
                    item.yaw = float.Parse(panel.Find("IFYaw").GetComponent<InputField>().text);
                    item.roll = float.Parse(panel.Find("IFRoll").GetComponent<InputField>().text);
                    item.fieldOfView = float.Parse(panel.Find("IFfov").GetComponent<InputField>().text);
                    float posX = float.Parse(panel.Find("IFposX").GetComponent<InputField>().text);
                    float posY = float.Parse(panel.Find("IFposY").GetComponent<InputField>().text);
                    float posZ = float.Parse(panel.Find("IFposZ").GetComponent<InputField>().text);
                    item.position = new Vector3(posX, posY, posZ);
                    cameraList.Add(item);
                }
            }

            CameraVanEdit cameraVan = GameObject.Find("Canvas").transform.Find("CameraVanEdit").GetComponent<CameraVanEdit>();
            cameraVan.cameraList = cameraList;
        }

        public void clickClose()
        {
            this.gameObject.SetActive(false);
        }

        public void fillMenu()
        {
            Transform contentPanel = this.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel");
            CameraVanEdit cve = GameObject.Find("Canvas").transform.Find("CameraVanEdit").GetComponent<CameraVanEdit>();
            if(cve.cameraList == null || cve.cameraList.Count < 1 || contentPanel.childCount > 1)
                return;


            for(int k = 0 ; k < cve.cameraList.Count-1; k++)
                clickAddCamera();

            int iterator = 0;

            foreach (Transform cameraSetting in contentPanel)
            {
                CameraSetting item = cve.cameraList[iterator++]; 
                Transform panel = cameraSetting.Find("Panel");
                panel.Find("TextID").GetComponent<Text>().text = item.id;
                panel.Find("IFpitch").GetComponent<InputField>().text = item.pitch.ToString();
                panel.Find("IFYaw").GetComponent<InputField>().text = item.yaw.ToString();
                panel.Find("IFRoll").GetComponent<InputField>().text = item.roll.ToString();
                panel.Find("IFfov").GetComponent<InputField>().text = item.fieldOfView.ToString();
                panel.Find("IFposX").GetComponent<InputField>().text = item.position.x.ToString();
                panel.Find("IFposY").GetComponent<InputField>().text = item.position.y.ToString();
                panel.Find("IFposZ").GetComponent<InputField>().text = item.position.z.ToString();               
            }


        }

    }
}
