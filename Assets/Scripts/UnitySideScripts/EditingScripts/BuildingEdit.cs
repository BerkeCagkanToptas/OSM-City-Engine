using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.SceneObjects;
using System.IO;
using Assets.Scripts.UnitySideScripts.MouseScripts;
using Assets.Scripts.UnitySideScripts.Menus;

namespace Assets.Scripts.UnitySideScripts.EditingScripts
{
    public class BuildingEdit : MonoBehaviour
    {
        GameObject buildingEditMenu;
        GameObject buildingEditSkinMenu;

        //GETS THE SCENE OBJECT FROM THEM
        GameObject loadsavemenu;
        LoadSaveMenu lsm;

        Building building;
        int facadeID;

        private GameObject fileBrowser;
        private myFileBrowserDialog fbd;
        

        private bool colorTextureSelect = false;
        private bool normalTextureSelect = false;
        private bool specularTextureSelect = false;
        private string colorTexturePath, normalTexturePath, specularTexturePath;
        private Texture2D colorTexture, normalTexture, specularTexture;

        InputField IFheight;
        RawImage RIcolorTexture;
        InputField[] IFtranslate;
        InputField[] IFrotate;
        InputField[] IFscale;
        Text TTbuildingID;


        //SKIN EDIT VARIABLES
        RawImage RIcolor, RInormal,RIspecular,RIuv;
        Text colorText, normalText, specularText;
        InputField U_bottomLeft,V_bottomLeft,U_bottomRight,V_bottomRight;
        InputField U_topLeft,V_topLeft,U_topRight,V_topRight;

        void Start()
        {
            colorTexturePath = "";
            normalTexturePath = "";
            specularTexturePath = "";

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
                if(colorTextureSelect)
                {
                    colorTexturePath = fbd.selectedPath;
                    colorText.GetComponent<Text>().text = " ";

                    byte[] fileData = File.ReadAllBytes(colorTexturePath);

                    colorTexture = new Texture2D(2, 2);
                    colorTexture.LoadImage(fileData);
                    RIcolor.texture = colorTexture;
                    RIuv.texture = colorTexture;
                    colorTextureSelect = false;
                    buildingEditSkinMenu.SetActive(true);
                }
                else if(normalTextureSelect)
                {
                    normalTexturePath = fbd.selectedPath;
                    normalText.text = " ";

                    byte[] fileData = File.ReadAllBytes(normalTexturePath);

                    normalTexture = new Texture2D(2, 2);
                    normalTexture.LoadImage(fileData);
                    RInormal.texture = normalTexture;
                    normalTextureSelect = false;
                    buildingEditSkinMenu.SetActive(true);

                }
                else if(specularTextureSelect)
                {
                    specularTexturePath = fbd.selectedPath;
                    specularText.text = " ";

                    byte[] fileData = File.ReadAllBytes(specularTexturePath);

                    specularTexture = new Texture2D(2, 2);
                    specularTexture.LoadImage(fileData);
                    RIspecular.texture = specularTexture;
                    specularTextureSelect = false;
                    buildingEditSkinMenu.SetActive(true);
                }

                
            
            }

            if (fbd.state == myFileBrowserDialog.BrowserState.Cancelled)
                buildingEditSkinMenu.SetActive(true);

        }

        public void clickClose()
        {
            buildingEditMenu.SetActive(false);
        }

        public void clickSave()
        {

        }

        public void clickLoad3DModel()
        {

        }

        public void clickEditFacadeTexture(GameObject EditTextureMenu)
        {
            EditTextureMenu.SetActive(true);
            fillBuildingSkinEdit();
        }

        public void clickResetTextureCoordinates(RawImage UVselector)
        {
            //Update coordinates
            U_bottomLeft.text = "0";
            V_bottomLeft.text = "0";
            U_bottomRight.text = "1";
            V_bottomRight.text = "0";
            U_topRight.text = "1";
            V_topRight.text = "1";
            U_topLeft.text = "0";
            V_topLeft.text = "1";
            //Transform pin points
            Vector2 windowSize = UVselector.rectTransform.sizeDelta;
            Vector3 rawPosition = UVselector.transform.position;
            GameObject pptopleft, pptopright, ppbottomleft, ppbottomright;
            GameObject UVcomponent = buildingEditSkinMenu.transform.Find("Panel").Find("TextureCoordinates").Find("Panel").gameObject;
            pptopleft = UVcomponent.transform.Find("PinPointTopLeft").gameObject;
            pptopright = UVcomponent.transform.Find("PinPointTopRight").gameObject;
            ppbottomleft = UVcomponent.transform.Find("PinPointBottomLeft").gameObject;
            ppbottomright = UVcomponent.transform.Find("PinPointBottomRight").gameObject;

            pptopleft.transform.position = new Vector3(rawPosition.x - windowSize.x / 2, rawPosition.y + windowSize.y / 2, 0);
            pptopright.transform.position = new Vector3(rawPosition.x + windowSize.x / 2, rawPosition.y + windowSize.y / 2, 0);
            ppbottomleft.transform.position = new Vector3(rawPosition.x - windowSize.x / 2, rawPosition.y - windowSize.y / 2, 0);
            ppbottomright.transform.position = new Vector3(rawPosition.x + windowSize.x / 2, rawPosition.y - windowSize.y / 2, 0);

        }

        public void clickCloseEditSkin()
        {
            building.setMaterial(facadeID,colorTexture,normalTexture,specularTexture,colorTexturePath,normalTexturePath,specularTexturePath);

            Vector2 bottomleft = new Vector2(float.Parse(U_bottomLeft.text),float.Parse(V_bottomLeft.text));
            Vector2 bottomright = new Vector2(float.Parse(U_bottomRight.text),float.Parse(V_bottomRight.text));
            Vector2 topleft = new Vector2(float.Parse(U_topLeft.text),float.Parse(V_topLeft.text));
            Vector2 topright = new Vector2(float.Parse(U_topRight.text),float.Parse(V_topRight.text));

            building.setTextureCoordinate(facadeID, bottomleft, bottomright, topleft, topright);
            if (buildingEditSkinMenu == null)
                buildingEditSkinMenu = GameObject.Find("Canvas").transform.Find("EditBuildingSkin").gameObject;
            buildingEditSkinMenu.SetActive(false);
        }

        private void fillBuildingSkinEdit()
        {
            if (buildingEditSkinMenu == null)
                buildingEditSkinMenu = GameObject.Find("Canvas").transform.Find("EditBuildingSkin").gameObject;

            RIcolor = buildingEditSkinMenu.transform.Find("Panel").Find("ColorTexture").GetComponent<RawImage>();
            RInormal = buildingEditSkinMenu.transform.Find("Panel").Find("NormalTexture").GetComponent<RawImage>();
            RIspecular = buildingEditSkinMenu.transform.Find("Panel").Find("SpecularTexture").GetComponent<RawImage>();

            colorText = buildingEditSkinMenu.transform.Find("Panel").Find("TextButtonColor").GetComponent<Text>();
            normalText = buildingEditSkinMenu.transform.Find("Panel").Find("TextButtonNormal").GetComponent<Text>();
            specularText = buildingEditSkinMenu.transform.Find("Panel").Find("TextButtonSpecular").GetComponent<Text>();

            GameObject UVcomponent = buildingEditSkinMenu.transform.Find("Panel").Find("TextureCoordinates").Find("Panel").gameObject;

            RIuv = UVcomponent.transform.Find("UVSelector").GetComponent<RawImage>();
            U_bottomLeft = UVcomponent.transform.Find("IFbottomleftU").GetComponent<InputField>();
            V_bottomLeft = UVcomponent.transform.Find("IFbottomleftV").GetComponent<InputField>();
            U_bottomRight = UVcomponent.transform.Find("IFbottomrightU").GetComponent<InputField>();
            V_bottomRight = UVcomponent.transform.Find("IFbottomrightV").GetComponent<InputField>();
            U_topLeft = UVcomponent.transform.Find("IFtopleftU").GetComponent<InputField>();
            V_topLeft = UVcomponent.transform.Find("IFtopleftV").GetComponent<InputField>();
            U_topRight = UVcomponent.transform.Find("IFtoprightU").GetComponent<InputField>();
            V_topRight = UVcomponent.transform.Find("IFtoprightV").GetComponent<InputField>();

            Material currentMaterial = building.facades[facadeID].GetComponent<MeshRenderer>().material;
            RIcolor.texture = currentMaterial.mainTexture;
            RInormal.texture = currentMaterial.GetTexture("_BumpMap");
           // RIspecular.texture = currentMaterial.GetTexture("_SpecGlossMap");
            RIuv.texture = currentMaterial.mainTexture;

            Mesh facadeMesh = building.facades[facadeID].GetComponent<MeshFilter>().mesh;
            Vector2[] uv = facadeMesh.uv;

            Transform ppTopLeft, ppTopRight, ppBottomLeft, ppBottomRight;

            ppTopLeft = UVcomponent.transform.Find("PinPointTopLeft");
            ppTopRight = UVcomponent.transform.Find("PinPointTopRight");
            ppBottomLeft = UVcomponent.transform.Find("PinPointBottomLeft");
            ppBottomRight = UVcomponent.transform.Find("PinPointBottomRight");
              
            if(building.isClockwise)
            {
                U_bottomLeft.text = uv[0].x.ToString();
                V_bottomLeft.text = uv[0].y.ToString();
                U_bottomRight.text = uv[1].x.ToString();
                V_bottomRight.text = uv[1].y.ToString();
                U_topLeft.text = uv[3].x.ToString();
                V_topLeft.text = uv[3].y.ToString();
                U_topRight.text = uv[2].x.ToString();
                V_topRight.text = uv[2].y.ToString();
            }
            else
            {
                U_bottomLeft.text = uv[1].x.ToString();
                V_bottomLeft.text = uv[1].y.ToString();
                U_bottomRight.text = uv[0].x.ToString();
                V_bottomRight.text = uv[0].y.ToString();
                U_topLeft.text = uv[2].x.ToString();
                V_topLeft.text = uv[2].y.ToString();
                U_topRight.text = uv[3].x.ToString();
                V_topRight.text = uv[3].y.ToString();
            }

            Vector3 rawPosition = RIuv.transform.position;
            Vector2 windowSize = RIuv.rectTransform.sizeDelta;

            float posX, posY;

            posX = rawPosition.x - windowSize.x / 2 + float.Parse(U_topLeft.text) * windowSize.x;
            posY = rawPosition.y - windowSize.y / 2 + float.Parse(V_topLeft.text) * windowSize.y;
            ppTopLeft.position = new Vector3(posX, posY, 0.0f);
            posX = rawPosition.x - windowSize.x / 2 + Mathf.Clamp(float.Parse(U_topRight.text),0.0f,1.0f) * windowSize.x;
            posY = rawPosition.y - windowSize.y / 2 + float.Parse(V_topRight.text) * windowSize.y;
            ppTopRight.position = new Vector3(posX, posY, 0.0f);
            posX = rawPosition.x - windowSize.x / 2 + float.Parse(U_bottomLeft.text) * windowSize.x;
            posY = rawPosition.y - windowSize.y / 2 + float.Parse(V_bottomLeft.text) * windowSize.y;
            ppBottomLeft.position = new Vector3(posX, posY, 0.0f);
            posX = rawPosition.x - windowSize.x / 2 + Mathf.Clamp(float.Parse(U_bottomRight.text),0.0f,1.0f) * windowSize.x;
            posY = rawPosition.y - windowSize.y / 2 + float.Parse(V_bottomRight.text) * windowSize.y;
            ppBottomRight.position = new Vector3(posX, posY, 0.0f);

        }

        public void editTextureClick(int no)
        {
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
            fbd.draw(myFileBrowserDialog.BrowserMode.FileSelect, di, new string[] { ".png", ".bmp", ".jpg", ".tif" });
            buildingEditSkinMenu.SetActive(false);           
            switch(no)
            {
                case 1 :
                    colorTextureSelect = true;
                    break;
                case 2:
                    normalTextureSelect = true;
                    break;
                case 3:
                    specularTextureSelect = true;
                    break;
            }

 
        }

        public void fillMenu(string buildingID, int _facadeID)
        {
            facadeID = _facadeID; 

            if(buildingEditMenu == null)
                buildingEditMenu = GameObject.Find("Canvas").transform.Find("BuildingEdit").gameObject;
            if (TTbuildingID == null)
            {
                TTbuildingID = buildingEditMenu.transform.Find("Panel").Find("TextID").GetComponent<Text>();
                IFheight = buildingEditMenu.transform.Find("Panel").Find("InputField_Height").GetComponent<InputField>();
                RIcolorTexture = buildingEditMenu.transform.Find("Panel").Find("FacadeTexture").GetComponent<RawImage>();
                IFtranslate = new InputField[3];
                IFtranslate[0] = buildingEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_TransX").GetComponent<InputField>();
                IFtranslate[1] = buildingEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_TransY").GetComponent<InputField>();
                IFtranslate[2] = buildingEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_TransZ").GetComponent<InputField>();
                IFrotate = new InputField[3];
                IFrotate[0] = buildingEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_RotateX").GetComponent<InputField>();
                IFrotate[1] = buildingEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_RotateY").GetComponent<InputField>();
                IFrotate[2] = buildingEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_RotateZ").GetComponent<InputField>();
                IFscale = new InputField[3];
                IFscale[0] = buildingEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_ScaleX").GetComponent<InputField>();
                IFscale[1] = buildingEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_ScaleY").GetComponent<InputField>();
                IFscale[2] = buildingEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_ScaleZ").GetComponent<InputField>();
            }

            if (loadsavemenu == null)
            {
                loadsavemenu = GameObject.Find("Canvas").transform.Find("LoadSave Menu").gameObject;
                lsm = loadsavemenu.GetComponent<LoadSaveMenu>();
            }

            building = lsm.scene.buildingList.Find(item => item.id == buildingID);

            IFheight.text = building.buildingHeight.ToString();
            TTbuildingID.text = building.id;
            IFtranslate[0].text = building.GOtransform.x.ToString();
            IFtranslate[1].text = building.GOtransform.y.ToString();
            IFtranslate[2].text = building.GOtransform.z.ToString();
            IFrotate[0].text = building.GOrotate.x.ToString();
            IFrotate[1].text = building.GOrotate.y.ToString();
            IFrotate[2].text = building.GOrotate.z.ToString();
            IFscale[0].text = building.GOscale.x.ToString();
            IFscale[1].text = building.GOscale.y.ToString();
            IFscale[2].text = building.GOscale.z.ToString();
            RIcolorTexture.texture = building.defaultMaterial.mainTexture;
            
        }


        public void endHeightEditing()
        {         
            building.updateHeightMesh(float.Parse(IFheight.text));
        }
    }
}
