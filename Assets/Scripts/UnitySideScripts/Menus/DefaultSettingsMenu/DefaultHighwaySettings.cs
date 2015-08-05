using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.ConfigHandler;
using Assets.Scripts.UnitySideScripts.MouseScripts;


namespace Assets.Scripts.UnitySideScripts.Menus
{
    class DefaultHighwaySettings : MonoBehaviour
    {
        private GameObject highwayMenu;

        private int TextureSelection;
        private bool[] isTextureChanged;
        private string[] texturePaths;
        private string[] materialPaths;

        private GameObject fileBrowser;
        private myFileBrowserDialog fbd;

        void Start()
        {
            highwayMenu = GameObject.Find("Highway Menu");
            TextureSelection = -1;
            loadConfig();

            fileBrowser = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Menu/FileBrowser"));
            fileBrowser.transform.SetParent(GameObject.Find("Canvas").transform);
            fileBrowser.SetActive(false);
            RectTransform rt = fileBrowser.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);
            fbd = fileBrowser.GetComponent<myFileBrowserDialog>();
        }

        void Update()
        {
            if (fbd.state == myFileBrowserDialog.BrowserState.Selected && TextureSelection > 0)
            {
                string GOname = "HighwaySkin " + TextureSelection;
                texturePaths[TextureSelection] = fbd.selectedPath;
                GameObject skinItem = highwayMenu.transform.Find("Panel").Find("Scroll Rect").Find("Content Panel").Find(GOname).gameObject;

                RawImage skinTexture = skinItem.transform.Find("Panel").Find("RawImage").GetComponent<RawImage>();

                byte[] fileData = File.ReadAllBytes(fbd.selectedPath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                skinTexture.texture = tex;
                 highwayMenu.transform.Find("Panel").gameObject.SetActive(true);
                TextureSelection = -1;
            }

            if(fbd.state == myFileBrowserDialog.BrowserState.Cancelled)
                highwayMenu.transform.Find("Panel").gameObject.SetActive(true);
                
        }

        public void EditTextureClick(GameObject skinItem)
        {
            string objName = skinItem.name;
            string[] substrings = objName.Split(new char[] { ' ' });
            TextureSelection = int.Parse(substrings[substrings.Length - 1]);
            highwayMenu.transform.Find("Panel").gameObject.SetActive(false);
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            fbd.draw(myFileBrowserDialog.BrowserMode.FileSelect, di, new string[] { ".png", ".jpg", ".bmp", ".tif" });
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
                highwayitem.leftSidewalk = skinitem.Find("ToggleLeftSideWalk").GetComponent<Toggle>().isOn;
                highwayitem.rightSidewalk = skinitem.Find("ToggleRightSideWalk").GetComponent<Toggle>().isOn;
                highwayitem.leftSidewalkSize = float.Parse(skinitem.Find("InputField LeftSize").GetComponent<InputField>().text);
                highwayitem.rightSidewalkSize = float.Parse(skinitem.Find("InputField RightSize").GetComponent<InputField>().text);
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

                newHighwayConfig.Add(highwayitem);
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
