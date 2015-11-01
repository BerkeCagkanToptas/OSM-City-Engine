using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.Menus.Alert
{
    public class Alert
    {
        GameObject dialog;

        public void openAlertDialog(string message)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (dialog == null)
            {
                dialog = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Menu/InfoBox"));
                dialog.transform.SetParent(canvas.transform);
                RectTransform rt = dialog.GetComponent<RectTransform>();
                rt.localPosition = new Vector3(0, 0, 0);
            }
            else
                dialog.SetActive(true);

            dialog.transform.Find("Panel").Find("Text").GetComponent<Text>().text = message;
            
        }

        public void closeAlertDialog()
        {
            dialog.SetActive(false);
        }

        public void openInteractableAlertDialog(string title, string message)
        {
            GameObject InteractableAlertDialog = GameObject.Find("Canvas").transform.Find("InteractableAlertDialog").gameObject;
            InteractableAlertDialog.transform.Find("Panel").Find("Title").GetComponent<Text>().text = title;
            InteractableAlertDialog.transform.Find("Panel").Find("Text").GetComponent<Text>().text = message;
            InteractableAlertDialog.SetActive(true);
        }
    }
}
