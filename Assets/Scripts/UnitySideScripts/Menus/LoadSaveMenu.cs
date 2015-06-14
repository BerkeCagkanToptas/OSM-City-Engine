using UnityEngine;
using System.Runtime.InteropServices;
using Assets.Scripts.Utils;
using UnityEngine.UI;
using Assets.Scripts.SceneObjects;
using Assets.Scripts.HeightMap;
using System.IO;

public class LoadSaveMenu : MonoBehaviour
{
    private bool OsmSelectBrowse = false;
    private bool ExistingProjectSelectBrowse = false;

    //initialize file browser
    FileBrowser fb;
    string output = "no file";
    // Use this for initialization
    void Start()
    {
        fb = new FileBrowser(0);
        GUISkin skin = (GUISkin)Resources.Load("FileBrowser/DefaultSkin");

        fb.setGUIRect(new Rect(400, 50, 800, 500));
        //setup file browser style
        fb.guiSkin = skin; //set the starting skin
        //set the various textures
        fb.fileTexture = (Texture2D)Resources.Load("FileBrowser/Images/file");
        fb.directoryTexture = (Texture2D)Resources.Load("FileBrowser/Images/folder");
        fb.backTexture = (Texture2D)Resources.Load("FileBrowser/Images/back2");
        fb.driveTexture = (Texture2D)Resources.Load("FileBrowser/Images/drive");
        //show the search bar
        fb.showSearch = true;
        //search recursively (setting recursive search may cause a long delay)
        fb.searchRecursively = true;
    }

    void OnGUI()
    {
        if (OsmSelectBrowse == false && ExistingProjectSelectBrowse == false)
            return;

        if (fb.draw())
        { 
            //true is returned when a file has been selected
            //the output file is a member if the FileInfo class, if cancel was selected the value is null
            
            if (fb.outputFile == null)
            {
                Debug.Log("<color=red>Path:</color>" + "No Path Selected");
            }
            else
            {
                if (OsmSelectBrowse)
                {
                    GameObject IF_osmselectFile = GameObject.Find("InputField_SelectOSM");
                    InputField IF_osmSelect = IF_osmselectFile.GetComponent<InputField>();
                    IF_osmSelect.text = fb.outputFile.FullName;
                }
                else if(ExistingProjectSelectBrowse)
                {
                    GameObject loadProject = GameObject.Find("InputField_SelectProject");
                    InputField IF_loadProject = loadProject.GetComponent<InputField>();
                    IF_loadProject.text = fb.outputFile.ToString();
                }

                Debug.Log("<color=red>FileName:</color>" + fb.outputFile.Name);
                Debug.Log("<color=red>Path:</color>" + fb.outputFile.ToString());
            }

            OsmSelectBrowse = false;
            ExistingProjectSelectBrowse = false;

        }
    }


    public void ClickOSMBrowse()
    {
        OsmSelectBrowse = true;
    }

    public void ClickOSMSelectRender(InputField _if)
    {
        Scene scene = new Scene();

        Toggle osmStreetToggle = GameObject.Find("Toggle_OSMStreet").GetComponent<Toggle>();
        Toggle osmStreet2Toggle = GameObject.Find("Toggle_OSMStreet2").GetComponent<Toggle>();
        Toggle bingStreetToggle = GameObject.Find("Toggle_BingStreet").GetComponent<Toggle>();
        Toggle bingAerialToggle = GameObject.Find("Toggle_BingAerial").GetComponent<Toggle>();


        MapProvider provider;

        if (osmStreetToggle.isOn)
            provider = MapProvider.OpenStreetMap;
        else if (osmStreet2Toggle.isOn)
            provider = MapProvider.OpenStreetMap2;
        else if (bingStreetToggle.isOn)
            provider = MapProvider.BingMapStreet;
        else
            provider = MapProvider.BingMapAerial;

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        string extension = Path.GetExtension(_if.text);
        if((extension == ".xml" || extension == ".osm") && File.Exists(_if.text))
            scene.initializeScene(_if.text, HeightmapContinent.Eurasia, provider);
        stopwatch.Stop();
        Debug.Log("<color=blue>TOTAL TIME:</color>" + stopwatch.ElapsedMilliseconds);
    }

    public void ClickLoadProjectRender()
    {

    }

    public void ClickLoadProjectBrowse()
    {
        ExistingProjectSelectBrowse = true;
    }


}