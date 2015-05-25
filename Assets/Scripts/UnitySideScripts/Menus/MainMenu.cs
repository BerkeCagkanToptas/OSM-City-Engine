using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour 
{

    private bool[] isSubmenuOpen;
	// Use this for initialization
	void Start () 
    {
        isSubmenuOpen = new bool[5];
        for (int i = 0; i < 5; i++)
            isSubmenuOpen[0] = false;
	}

    public void ClickButton(GameObject menu)
    {
        if (isSubmenuOpen[0] == false)
        {
            Debug.Log("Kapaliydi Aciyo Simdi");
            menu.SetActive(true);

        }
        else
        {
            Debug.Log("Acikti Kapiyo simdi");
            menu.SetActive(false);
        }

        isSubmenuOpen[0] = !isSubmenuOpen[0];
    }

    public void ClickExit()
    {
        Application.Quit();
    }
}
