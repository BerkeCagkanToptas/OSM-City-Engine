using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour 
{

    public void ClickButton(GameObject menu)
    {

        if (!menu.activeSelf)
            menu.SetActive(true);
        else
            menu.SetActive(false);
    }

    public void ClickExit()
    {
        Application.Quit();
    }
}
