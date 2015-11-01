using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts.UnitySideScripts;
using Assets.Scripts.UnitySideScripts.Menus.Alert;

public class MainMenu : MonoBehaviour 
{
    private CameraController camControl;
    private Transform cameraVan = null;
    private GameObject EditModeCanvas;
    private GameObject cameraVanEditMenu;

    void Start()
    {
        camControl = GameObject.Find("Main Camera").GetComponent<CameraController>();
        EditModeCanvas = GameObject.Find("AddObjectMenuCanvas");
        cameraVanEditMenu = GameObject.Find("Canvas").transform.Find("CameraVanEdit").gameObject;
    }

    public void ClickButton(GameObject menu)
    {

        if (!menu.activeSelf)
            menu.SetActive(true);
        else
            menu.SetActive(false);
    }

    public void clickCloseError(GameObject message)
    {
        message.SetActive(false);
    }

    public void clickChangeMode(Button btn)
    {
        if (camControl.mode == CameraController.CameraMode.freeFlyMode)
        {
            if (camControl.target == null)
            {
                Alert alertdialog = new Alert();
                alertdialog.openInteractableAlertDialog("ERROR", "No Camera Van was detected. Please use 'Object Bar' at the bottom of screen to add a Camera Van.");
                return;
            }
            else
            {
                btn.image.sprite = Resources.Load<Sprite>("Textures/Menu/AddObjectMenu/RecordModeIcon");
                camControl.mode = CameraController.CameraMode.recordMode;
                camControl.cameraUpdate();
                camControl.target.GetComponent<Rigidbody>().useGravity = true;
                EditModeCanvas.SetActive(false);
                cameraVanEditMenu.SetActive(true);
                cameraVanEditMenu.transform.Find("Panel").Find("ButtonClose").GetComponent<Button>().interactable = false;
                cameraVanEditMenu.transform.Find("Panel").Find("ButtonDestroy").GetComponent<Button>().interactable = false;
                TransformGizmos tg = camControl.target.GetComponent<TransformGizmos>();
                if (tg != null)
                {
                    tg.TurnOffGizmos();
                    Destroy(camControl.target.GetComponent<TransformGizmos>());
                }
            }
        }
        else if(camControl.mode == CameraController.CameraMode.recordMode)
        {
            btn.image.sprite = Resources.Load<Sprite>("Textures/Menu/AddObjectMenu/EditModeIcon");                
            camControl.mode = CameraController.CameraMode.freeFlyMode;
            camControl.cameraUpdate();
            EditModeCanvas.SetActive(true);
            cameraVanEditMenu.SetActive(false);
            cameraVanEditMenu.transform.Find("Panel").Find("ButtonClose").GetComponent<Button>().interactable = true;
            cameraVanEditMenu.transform.Find("Panel").Find("ButtonDestroy").GetComponent<Button>().interactable = true;
        }
    }

    public void ClickExit()
    {
        Application.Quit();
    }

}
