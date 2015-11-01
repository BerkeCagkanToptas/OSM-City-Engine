using Assets.Scripts.SceneObjects;
using Assets.Scripts.UnitySideScripts.Menus;
using Assets.Scripts.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.EditingScripts
{
    class BarrierEdit : MonoBehaviour
    {

        private GameObject fileBrowser;
        private myFileBrowserDialog fbd;

        //GETS THE SCENE OBJECT FROM THEM
        GameObject loadSaveMenu;
        LoadSaveMenu lsm;

        Text textID;
        Text textType;
        InputField IFthickness, IFheight;
        RawImage texture;
        Button editTexture;
        int barrierIndex;

        void Start()
        {
            fileBrowser = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/Menu/FileBrowser"));
            fileBrowser.transform.SetParent(GameObject.Find("Canvas").transform);
            fileBrowser.SetActive(false);
            RectTransform rt = fileBrowser.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, 0);
            fbd = fileBrowser.GetComponent<myFileBrowserDialog>();
        }

        void Update()
        {
            if (fbd.state == myFileBrowserDialog.BrowserState.Selected)
            {
                lsm.scene.barrierList[barrierIndex].updateTexture(fbd.selectedPath);
                texture.texture = InGameTextureHandler.getTexture(fbd.selectedPath);
            }

        }
        public void clickClose()
        {
            this.gameObject.SetActive(false);
        }

        public void clickSave()
        {

        }

        public void fillMenu(string barrierID)
        {
            textID = transform.Find("Panel").Find("TextID").GetComponent<Text>();
            textType = transform.Find("Panel").Find("TextType").GetComponent<Text>();
            IFthickness = transform.Find("Panel").Find("IFthickness").GetComponent<InputField>();
            IFheight = transform.Find("Panel").Find("IFheight").GetComponent<InputField>();
            texture = transform.Find("Panel").Find("RawImage").GetComponent<RawImage>();
            editTexture = transform.Find("Panel").Find("ButtonText").GetComponent<Button>();

            if (loadSaveMenu == null)
            {
                loadSaveMenu = GameObject.Find("Canvas").transform.Find("LoadSave Menu").gameObject;
                lsm = loadSaveMenu.GetComponent<LoadSaveMenu>();
            }

            barrierIndex = lsm.scene.barrierList.FindIndex(o => o.id == barrierID);
            Barrier barrier = lsm.scene.barrierList[barrierIndex];
            textID.text = barrierID;
            textType.text = barrier.type.ToString();
            IFthickness.text = barrier.thickness.ToString();
            IFheight.text = barrier.height.ToString();
            texture.texture = barrier.barrierMaterial.mainTexture; 
        }


        public void onThicknessChanged()
        {
            float newSize = float.Parse(IFthickness.text);
            lsm.scene.barrierList[barrierIndex].updateThickness(newSize);
            
        }

        public void onHeightChanged()
        {
            float newheight = float.Parse(IFheight.text);
            lsm.scene.barrierList[barrierIndex].updateHeight(newheight);
        }

        public void clickTextureChange()
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            fbd.draw(myFileBrowserDialog.BrowserMode.FileSelect, di, new string[] { ".png", ".bmp", ".jpg", ".tif" });
        }

    }
}
