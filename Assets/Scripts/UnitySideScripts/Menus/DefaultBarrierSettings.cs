using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Assets.Scripts.ConfigHandler;

namespace Assets.Scripts.UnitySideScripts.Menus
{
    class DefaultBarrierSettings : MonoBehaviour
    {

        private GameObject barrierMenu;

        private int TextureSelection;
        private bool[] isTextureChanged;
        private string[] texturePaths;
        private string[] materialPaths;

        //File browser
        FileBrowser fb;
        
        void Start()
        {
            barrierMenu = GameObject.Find("Barrier Menu");
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
                    string GOname = "BarrierSkin " + TextureSelection;
                    texturePaths[TextureSelection] = fb.outputFile.FullName;
                    GameObject skinItem =  barrierMenu.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel").Find(GOname).gameObject;

                    RawImage skinTexture = skinItem.transform.Find("Panel").Find("RawImage").GetComponent<RawImage>();

                    byte[] fileData = File.ReadAllBytes(fb.outputFile.FullName);                      
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(fileData);
                    skinTexture.texture = tex;

                    TextureSelection = -1;
                }

                barrierMenu.transform.Find("Panel").gameObject.SetActive(true);
                
            }

        }

        void Update()
        {
            if(TextureSelection > 0)
                barrierMenu.transform.Find("Panel").gameObject.SetActive(false);
        }

        public void EditTextureClick(GameObject skinItem)
        {
            string objName = skinItem.name;
            string[] substrings = objName.Split(new char[] { ' ' });
            TextureSelection = int.Parse(substrings[substrings.Length-1]);
        }

        public void ClickSave()
        {
            InitialConfigLoader loader = new InitialConfigLoader();
            InitialConfigurations config = loader.loadInitialConfig();

            List<BarrierConfigurations> newBarrierConfig = new List<BarrierConfigurations>();

            for(int i = 0 ; i < config.barrierConfig.Count ; i++)
            {
                string GOname = "BarrierSkin " + i;
                var skinitem = barrierMenu.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel").Find(GOname).Find("Panel");
 
                BarrierConfigurations barrieritem = new BarrierConfigurations();

                barrieritem.name = skinitem.Find("TextName").GetComponent<Text>().text;
                barrieritem.width = float.Parse(skinitem.Find("InputField_Thickness").GetComponent<InputField>().text);
                barrieritem.height = float.Parse(skinitem.Find("InputField_Height").GetComponent<InputField>().text);
                barrieritem.Path = materialPaths[i]; 
                if(isTextureChanged[i])
                {
                    Material mat = (Material)Resources.Load(barrieritem.Path);
                    byte[] fileData = File.ReadAllBytes(texturePaths[i]);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(fileData);
                    mat.mainTexture = tex;
                    mat.mainTextureScale = new Vector2(5, 1);
                }
            }

            config.barrierConfig = newBarrierConfig;
            loader.saveInitialConfig(Path.Combine(Application.persistentDataPath, "ConfigFiles/initialConfig.xml"), config);


            barrierMenu.SetActive(false);
        }

        public void ClickReset()
        {
            InitialConfigLoader loader = new InitialConfigLoader();
            InitialConfigurations config = loader.loadInitialConfig();
            InitialConfigurations tmpConfig = loader.fillConfig();
            config.barrierConfig = tmpConfig.barrierConfig;
            configLoadHelper(config);
        }

        public void ClickCancel()
        {
            loadConfig();
            barrierMenu.SetActive(false);
        }

        private void loadConfig()
        {
            InitialConfigLoader loader = new InitialConfigLoader();
            InitialConfigurations config = loader.loadInitialConfig();
            configLoadHelper(config);
        }

        private void configLoadHelper(InitialConfigurations config)
        {
            List<BarrierConfigurations> conf = config.barrierConfig;

            texturePaths = new string[conf.Count];
            materialPaths = new string[conf.Count];
            isTextureChanged = new bool[conf.Count];

            Transform parentContent = barrierMenu.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel");

            var children = new List<GameObject>();
            foreach (Transform child in parentContent) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));

            for(int i = 0 ; i < conf.Count ; i++)
            {
                isTextureChanged[i] = false;
                texturePaths[i] = "";
                materialPaths[i] = conf[i].Path;

                GameObject skinItem = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Menu/BarrierSkin"));
                skinItem.name = "BarrierSkin " + i;
                skinItem.transform.SetParent(parentContent);
                
                Texture2D colorText;
                Material mat = (Material)Resources.Load(config.barrierConfig[i].Path);
                colorText = (Texture2D)mat.mainTexture;
                skinItem.transform.Find("Panel").Find("RawImage").GetComponent<RawImage>().texture = colorText;
                skinItem.transform.Find("Panel").Find("TextName").GetComponent<Text>().text = config.barrierConfig[i].name;
                skinItem.transform.Find("Panel").Find("TextTextureChange").GetComponent<Button>().onClick.AddListener(delegate { EditTextureClick(skinItem); });
                skinItem.transform.Find("Panel").Find("InputField_Height").GetComponent<InputField>().text = config.barrierConfig[i].height.ToString();
                skinItem.transform.Find("Panel").Find("InputField_Thickness").GetComponent<InputField>().text = config.barrierConfig[i].width.ToString();

                
            
            }

        }

    }
}
