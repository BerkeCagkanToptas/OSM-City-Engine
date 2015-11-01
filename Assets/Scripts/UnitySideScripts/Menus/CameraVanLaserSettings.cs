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

            if (!menuPanel.Find("ToggleIsActive").GetComponent<Toggle>().isOn)
            {
                gameObject.SetActive(false);
                return;
            }

            laserScanner = new LaserSetting();
            laserScanner.frameRate = int.Parse(menuPanel.Find("IFframeRate").GetComponent<InputField>().text);
            laserScanner.minDistance = float.Parse(menuPanel.Find("IFminDistance").GetComponent<InputField>().text);
            laserScanner.maxDistance = float.Parse(menuPanel.Find("IFmaxDistance").GetComponent<InputField>().text);
            laserScanner.verticalFOV = float.Parse(menuPanel.Find("IFverticalFOV").GetComponent<InputField>().text);
            laserScanner.horizontalFOV = float.Parse(menuPanel.Find("IFhorizontalFOV").GetComponent<InputField>().text);
            
            laserScanner.position = new Vector3();
            laserScanner.position.x = float.Parse(menuPanel.Find("IFposX").GetComponent<InputField>().text);
            laserScanner.position.y = float.Parse(menuPanel.Find("IFposY").GetComponent<InputField>().text);
            laserScanner.position.z = float.Parse(menuPanel.Find("IFposZ").GetComponent<InputField>().text);

            laserScanner.rotation = new Vector3();
            laserScanner.rotation.x = -float.Parse(menuPanel.Find("IFpitch").GetComponent<InputField>().text);
            laserScanner.rotation.y = -float.Parse(menuPanel.Find("IFyaw").GetComponent<InputField>().text);
            laserScanner.rotation.z = -float.Parse(menuPanel.Find("IFroll").GetComponent<InputField>().text);
           
            laserScanner.verticalResolution = float.Parse(menuPanel.Find("IFverticalResolution").GetComponent<InputField>().text);
            laserScanner.horizontalResolution = float.Parse(menuPanel.Find("IFhorizontalResolution").GetComponent<InputField>().text);

            GameObject.Find("CameraVanEdit").GetComponent<CameraVanEdit>().laserScanner = laserScanner;
            GameObject.Find("CameraVan LaserScanner Menu").gameObject.SetActive(false);
        }

        public void fillMenu()
        {
            CameraVanEdit cve = GameObject.Find("Canvas").transform.Find("CameraVanEdit").GetComponent<CameraVanEdit>();
            if (cve.laserScanner.frameRate < 1)
                return;

            laserScanner = cve.laserScanner;
            Transform menuPanel = gameObject.transform.Find("Panel");

            menuPanel.Find("IFframeRate").GetComponent<InputField>().text = laserScanner.frameRate.ToString();
            menuPanel.Find("IFminDistance").GetComponent<InputField>().text = laserScanner.minDistance.ToString();
            menuPanel.Find("IFmaxDistance").GetComponent<InputField>().text = laserScanner.maxDistance.ToString();
            menuPanel.Find("IFverticalFOV").GetComponent<InputField>().text = laserScanner.verticalFOV.ToString();
            menuPanel.Find("IFhorizontalFOV").GetComponent<InputField>().text = laserScanner.horizontalFOV.ToString();

            menuPanel.Find("IFposX").GetComponent<InputField>().text = laserScanner.position.x.ToString();
            menuPanel.Find("IFposY").GetComponent<InputField>().text = laserScanner.position.y.ToString();
            menuPanel.Find("IFposZ").GetComponent<InputField>().text = laserScanner.position.z.ToString();

            menuPanel.Find("IFpitch").GetComponent<InputField>().text = laserScanner.rotation.x.ToString();
            menuPanel.Find("IFyaw").GetComponent<InputField>().text = laserScanner.rotation.y.ToString();
            menuPanel.Find("IFroll").GetComponent<InputField>().text = laserScanner.rotation.z.ToString();

            menuPanel.Find("IFverticalResolution").GetComponent<InputField>().text = laserScanner.verticalResolution.ToString();
            menuPanel.Find("IFhorizontalResolution").GetComponent<InputField>().text = laserScanner.horizontalResolution.ToString();
           
        }


    }
}
