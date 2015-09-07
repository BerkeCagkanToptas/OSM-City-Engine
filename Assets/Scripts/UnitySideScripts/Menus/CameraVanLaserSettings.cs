using Assets.Scripts.UnitySideScripts.EditingScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.Menus
{
 
    class CameraVanLaserSettings : MonoBehaviour
    {
        LaserSetting laserScanner;

        public void statusChange()
        {
            GameObject laserSettingMenu = GameObject.Find("CameraVan LaserScanner Menu");
            Transform menuPanel = laserSettingMenu.transform.Find("Panel");
            Toggle isEnabled = menuPanel.Find("ToggleIsActive").GetComponent<Toggle>();
            if (isEnabled.isOn == false)
            {

                foreach (Transform item in menuPanel)
                {
                    InputField infield = item.GetComponent<InputField>();
                    if (infield != null)
                        infield.interactable = false;
                }

                GameObject.Find("CameraVanEdit").GetComponent<CameraVanEdit>().laserScanner = new LaserSetting();
            }
            else
            {
                foreach (Transform item in menuPanel)
                {
                    InputField infield = item.GetComponent<InputField>();
                    if (infield != null)
                        infield.interactable = true;
                }
            }

        }

        public void okayPressed()
        {
            GameObject laserSettingMenu = GameObject.Find("CameraVan LaserScanner Menu");
            Transform menuPanel = laserSettingMenu.transform.Find("Panel");
            laserScanner = new LaserSetting();

            laserScanner.frameRate = float.Parse(menuPanel.Find("IFframeRate").GetComponent<InputField>().text);
            laserScanner.minDistance = float.Parse(menuPanel.Find("IFminDistance").GetComponent<InputField>().text);
            laserScanner.maxDistance = float.Parse(menuPanel.Find("IFmaxDistance").GetComponent<InputField>().text);
            laserScanner.verticalFOV = float.Parse(menuPanel.Find("IFverticalFOV").GetComponent<InputField>().text);
            laserScanner.horizontalFOV = float.Parse(menuPanel.Find("IFhorizontalFOV").GetComponent<InputField>().text);
            laserScanner.position = new Vector3();
            laserScanner.position.x = float.Parse(menuPanel.Find("IFposX").GetComponent<InputField>().text);
            laserScanner.position.y = float.Parse(menuPanel.Find("IFposY").GetComponent<InputField>().text);
            laserScanner.position.z = float.Parse(menuPanel.Find("IfposZ").GetComponent<InputField>().text);

            laserScanner.verticalResolution = float.Parse(menuPanel.Find("IFverticalResolution").GetComponent<InputField>().text);
            laserScanner.horizontalResolution = float.Parse(menuPanel.Find("IFhorizontalResolution").GetComponent<InputField>().text);

            GameObject.Find("CameraVanEdit").GetComponent<CameraVanEdit>().laserScanner = laserScanner;
            GameObject.Find("CameraVan LaserScanner Menu").gameObject.SetActive(false);
        }




    }
}
