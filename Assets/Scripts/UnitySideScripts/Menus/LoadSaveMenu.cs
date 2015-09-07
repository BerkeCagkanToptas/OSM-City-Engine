using UnityEngine;
using System.Runtime.InteropServices;
using Assets.Scripts.Utils;
using UnityEngine.UI;
using Assets.Scripts.SceneObjects;
using Assets.Scripts.HeightMap;
using System.IO;
using Assets.Scripts.UnitySideScripts.MouseScripts;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.UnitySideScripts.Menus;

public class LoadSaveMenu : MonoBehaviour
{

    public Scene scene;

    private GameObject fileBrowser, fileBrowserSave;
    private myFileBrowserDialog fbd, fbdSave;
    private bool isNewProject, isLoadProject, isSaveProject;

    void Start()
    {
        fileBrowser = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Menu/FileBrowser"));
        fileBrowser.transform.SetParent(GameObject.Find("Canvas").transform);
        fileBrowser.SetActive(false);
        RectTransform rt = fileBrowser.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, 0);
        fbd = fileBrowser.GetComponent<myFileBrowserDialog>();

        isNewProject = false;
        isLoadProject = false;
        isSaveProject = false;
    }

    void Update()
    {

        if(fbd.state == myFileBrowserDialog.BrowserState.Selected)
        {
            if(isNewProject)
            {
                InputField IFnewProject = transform.Find("Panel_LoadSaveMenu").Find("InputField_SelectOSM").GetComponent<InputField>();
                IFnewProject.text = fbd.selectedPath;
                isNewProject = false;
            }
            else if(isLoadProject)
            {
                InputField IFloadProject = transform.Find("Panel_LoadSaveMenu").Find("InputField_SelectProject").GetComponent<InputField>();
                IFloadProject.text = fbd.selectedPath;
                isLoadProject = false;
            }

            else if(isSaveProject)
            {
                string path = fbd.selectedPath + "/" + fbd.saveName;
                string osmPath = scene.OSMPath;
                SaveConfig save = new SaveConfig(path, scene);
                save.saveConfigurations();
                Debug.Log("Successfully Saved");
                isSaveProject = false;
            }

        }


     
    }

    public void ClickOSMBrowse()
    {
        isNewProject = true;
        DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
        fbd.draw(myFileBrowserDialog.BrowserMode.FileSelect, di, new string[]{".osm",".xml"});
    }

    public void ClickOSMSelectRender()
    {
        InputField _if = transform.Find("Panel_LoadSaveMenu").Find("InputField_SelectOSM").GetComponent<InputField>();
           
        Toggle osmStreetToggle = transform.Find("Panel_LoadSaveMenu").Find("Toggle_OSMStreet").GetComponent<Toggle>();
        Toggle osmStreet2Toggle = transform.Find("Panel_LoadSaveMenu").Find("Toggle_OSMStreet2").GetComponent<Toggle>();
        Toggle bingStreetToggle = transform.Find("Panel_LoadSaveMenu").Find("Toggle_BingStreet").GetComponent<Toggle>();
        //Toggle bingAerialToggle = GameObject.Find("Toggle_BingAerial").GetComponent<Toggle>();

        Toggle euroAsiaToggle = transform.Find("Panel_LoadSaveMenu").Find("ToggleEuroAsia").GetComponent<Toggle>();
        Toggle australiaToggle = transform.Find("Panel_LoadSaveMenu").Find("ToggleAustralia").GetComponent<Toggle>();
        Toggle africaToggle = transform.Find("Panel_LoadSaveMenu").Find("ToggleAfrica").GetComponent<Toggle>();


        MapProvider provider;
        HeightmapContinent continent;

        if (osmStreetToggle.isOn)
            provider = MapProvider.OpenStreetMap;
        else if (osmStreet2Toggle.isOn)
            provider = MapProvider.MapQuest;
        else if (bingStreetToggle.isOn)
            provider = MapProvider.BingMapStreet;
        else
            provider = MapProvider.BingMapAerial;

        if (euroAsiaToggle.isOn)
            continent = HeightmapContinent.Eurasia;
        else if (australiaToggle.isOn)
            continent = HeightmapContinent.Australia;
        else if (africaToggle.isOn)
            continent = HeightmapContinent.Africa;
        else
            continent = HeightmapContinent.North_America;

        scene = new Scene();
        scene.initializeScene(_if.text, continent, provider);

    }

    public void ClickLoadProjectRender()
    {
        InputField _if = transform.Find("Panel_LoadSaveMenu").Find("InputField_SelectProject").GetComponent<InputField>();         
        LoadConfig loadConfig = new LoadConfig(_if.text);
        scene = new Scene();
        scene.loadProject(loadConfig.sceneSave);
    }

    public void ClickLoadProjectBrowse()
    {
        isLoadProject = true;
        DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
        fbd.draw(myFileBrowserDialog.BrowserMode.FileSelect, di, new string[] { ".xml" });
    }

    public void ClickSaveProject()
    {
        fbd.saveName = scene.sceneName;
        isSaveProject = true;
        DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
        fbd.draw(myFileBrowserDialog.BrowserMode.FolderSelect, di, new string[] { ".xml" });
    }

}