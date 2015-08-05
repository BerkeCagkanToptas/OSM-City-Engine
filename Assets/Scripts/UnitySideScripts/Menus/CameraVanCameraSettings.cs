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

        public struct CameraSettingItem
        {
            public string id;
            public float yaw, pitch;
            public Vector3 position;
            public float fieldOfView;
            public int frameRate;
        }

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
            List<CameraSettingItem> cameraList = new List<CameraSettingItem>();

            foreach(Transform cameraSetting in contentPanel)
            {
                CameraSettingItem item = new CameraSettingItem();
                Transform panel = cameraSetting.Find("Panel");
                if (panel.Find("ToggleIsActive").GetComponent<Toggle>().isOn)
                {
                    item.id = panel.Find("TextID").GetComponent<Text>().text;
                    item.pitch = float.Parse(panel.Find("IFpitch").GetComponent<InputField>().text);
                    item.yaw = float.Parse(panel.Find("IFYaw").GetComponent<InputField>().text);
                    item.fieldOfView = panel.Find("Slider").GetComponent<Slider>().value;
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


    }
}
