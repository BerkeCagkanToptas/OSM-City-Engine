using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.ConfigHandler;


namespace Assets.Scripts.UnitySideScripts.Menus
{
    class DefaultHighwaySettings : MonoBehaviour
    {
        private GameObject highwayMenu;

        private int TextureSelection;
        private bool[] isTextureChanged;
        private string[] texturePaths;
        private string[] materialPaths;

        //File browser
        FileBrowser fb;

        void Start()
        {
            highwayMenu = GameObject.Find("Highway Menu");
            TextureSelection = -1;
            loadConfig();

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
            fb.searchRecursively = false;
        }

        void OnGUI()
        {
            if (TextureSelection < 0)
                return;

            if (fb.draw())
            {

                if (fb.outputFile == null)
                {
                    Debug.Log("<color=red>Path:</color>" + "No Path Selected");
                    TextureSelection = -1;
                }
                else
                {
                    string GOname = "HighwaySkin " + TextureSelection;
                    texturePaths[TextureSelection] = fb.outputFile.FullName;
                    GameObject skinItem = highwayMenu.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel").Find(GOname).gameObject;

                    RawImage skinTexture = skinItem.transform.Find("Panel").Find("RawImage").GetComponent<RawImage>();

                    byte[] fileData = File.ReadAllBytes(fb.outputFile.FullName);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(fileData);
                    skinTexture.texture = tex;

                    TextureSelection = -1;
                }

                highwayMenu.transform.Find("Panel").gameObject.SetActive(true);

            }

        }

        void Update()
        {
            if (TextureSelection > 0)
                highwayMenu.transform.Find("Panel").gameObject.SetActive(false);
        }

        public void EditTextureClick(GameObject skinItem)
        {
            string objName = skinItem.name;
            string[] substrings = objName.Split(new char[] { ' ' });
            TextureSelection = int.Parse(substrings[substrings.Length - 1]);
        }

        public void ClickSave()
        {
            InitialConfigLoader loader = new InitialConfigLoader();
            InitialConfigurations config = loader.loadInitialConfig();

            List<HighwayConfigurations> newHighwayConfig = new List<HighwayConfigurations>();

            for (int i = 0; i < config.highwayConfig.Count; i++)
            {
                string GOname = "HighwaySkin " + i;
                var skinitem = highwayMenu.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel").Find(GOname).Find("Panel");

                HighwayConfigurations highwayitem = new HighwayConfigurations();

                highwayitem.type = skinitem.Find("Text_Type").GetComponent<Text>().text;
                highwayitem.size = float.Parse(skinitem.Find("InputField_Width").GetComponent<InputField>().text); 
                highwayitem.leftSidewalk = skinitem.Find("ToggleLeftSidewalk").GetComponent<Toggle>().isOn;
                highwayitem.rightSidewalk = skinitem.Find("ToggleRightSidewalk").GetComponent<Toggle>().isOn;
                highwayitem.leftSidewalkSize = float.Parse(skinitem.Find("InputField_LeftSize").GetComponent<InputField>().text);
                highwayitem.rightSidewalkSize = float.Parse(skinitem.Find("InputField_RightSize").GetComponent<InputField>().text);
                highwayitem.materialPath = materialPaths[i]; 

                if (isTextureChanged[i])
                {
                    Material mat = (Material)Resources.Load(highwayitem.materialPath);
                    byte[] fileData = File.ReadAllBytes(texturePaths[i]);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(fileData);
                    mat.mainTexture = tex;
                    mat.mainTextureScale = new Vector2(5, 1);
                }
            }

            config.highwayConfig = newHighwayConfig;
            loader.saveInitialConfig(Path.Combine(Application.persistentDataPath, "ConfigFiles/initialConfig.xml"), config);


            highwayMenu.SetActive(false);
        }

        public void ClickReset()
        {
            InitialConfigLoader loader = new InitialConfigLoader();
            InitialConfigurations config = loader.loadInitialConfig();
            InitialConfigurations tmpConfig = loader.fillConfig();
            config.highwayConfig= tmpConfig.highwayConfig;
            configLoadHelper(config);
        }

        public void ClickCancel()
        {
            loadConfig();
            highwayMenu.SetActive(false);
        }

        private void loadConfig()
        {
            InitialConfigLoader loader = new InitialConfigLoader();
            InitialConfigurations config = loader.loadInitialConfig();
            configLoadHelper(config);
        }

        private void configLoadHelper(InitialConfigurations config)
        {
            List<HighwayConfigurations> conf = config.highwayConfig;

            texturePaths = new string[conf.Count];
            materialPaths = new string[conf.Count];
            isTextureChanged = new bool[conf.Count];

            Transform parentContent = highwayMenu.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel");

            var children = new List<GameObject>();
            foreach (Transform child in parentContent) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));

            for (int i = 0; i < conf.Count; i++)
            {
                isTextureChanged[i] = false;
                texturePaths[i] = "";
                materialPaths[i] = conf[i].materialPath;

                GameObject skinItem = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Menu/HighwaySkin"));
                skinItem.name = "HighwaySkin " + i;
                skinItem.transform.SetParent(parentContent);

                Texture2D colorText;
                Material mat = (Material)Resources.Load(conf[i].materialPath);
                colorText = (Texture2D)mat.mainTexture;
                skinItem.transform.Find("Panel").Find("RawImage").GetComponent<RawImage>().texture = colorText;
                skinItem.transform.Find("Panel").Find("Text_Type").GetComponent<Text>().text = conf[i].type;
                skinItem.transform.Find("Panel").Find("Text_ChangeTexture").GetComponent<Button>().onClick.AddListener(delegate { EditTextureClick(skinItem); });
                skinItem.transform.Find("Panel").Find("InputField_Width").GetComponent<InputField>().text = conf[i].size.ToString();
                skinItem.transform.Find("Panel").Find("InputField LeftSize").GetComponent<InputField>().text = conf[i].leftSidewalkSize.ToString();
                skinItem.transform.Find("Panel").Find("InputField RightSize").GetComponent<InputField>().text = conf[i].rightSidewalkSize.ToString();
                skinItem.transform.Find("Panel").Find("ToggleLeftSideWalk").GetComponent<Toggle>().isOn = conf[i].leftSidewalk;
                skinItem.transform.Find("Panel").Find("ToggleRightSideWalk").GetComponent<Toggle>().isOn = conf[i].rightSidewalk;


            }

        }






    }
}
