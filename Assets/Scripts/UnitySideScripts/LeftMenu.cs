using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Utils;

namespace Assets.Scripts.UnitySideScripts
{
    class LeftMenu : MonoBehaviour
    {

        public Canvas leftmenu;
        public Button selectOsmButton;
        public Button exitButton;

        public Text OsmfileName;

        private String OsmfilePath;
        private FileBrowser fileBrowser;
        private bool isFileSelection;

        void Start()
        {
            leftmenu = leftmenu.GetComponent<Canvas>();
            exitButton = exitButton.GetComponent<Button>();
            selectOsmButton = selectOsmButton.GetComponent<Button>();
            OsmfileName = OsmfileName.GetComponent<Text>();
            isFileSelection = false;
            leftmenu.enabled = false;

           // Scene scene = new Scene();
           // scene.initializeScene("OSMFiles/Pannilani", HeightMap.HeightmapContinent.Eurasia, MapProvider.BingMapAerial);

        }

        void Update()
        {
            if (Input.mousePosition.x < 250)
                leftmenu.enabled = true;
            else
                leftmenu.enabled = false;
        }




        protected void OnGUI()
        {
            if (isFileSelection)
            {
                if (fileBrowser != null)
                {
                    fileBrowser.OnGUI();
                }
                else
                {
                    fileBrowser = new FileBrowser(new Rect(250, 70, 500, 500), "Choose OSM File", FileSelectedCallback);
                    fileBrowser.SelectionPattern = "*.txt";
                    fileBrowser.DirectoryImage = (Texture2D)Resources.Load("Textures/LeftMenu/foldericon");
                    fileBrowser.FileImage = (Texture2D)Resources.Load("Textures/LeftMenu/fileicon");
                }
            }
        }

        public void SelectFilePress()
        {
            isFileSelection = true;
        }


        private void FileSelectedCallback(string path)
        {
            fileBrowser = null;
            OsmfilePath = path;

            string[] subPaths = path.Split(new char[] { '/', '\\' });
            OsmfileName.text = subPaths[subPaths.Length - 1];
            isFileSelection = false;
        }


        public void RenderScene()
        {




        }

        public void ExitGame()
        {
            Application.Quit();
        }


    }
}
