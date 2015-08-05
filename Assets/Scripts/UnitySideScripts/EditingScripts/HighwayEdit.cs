using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.SceneObjects;

namespace Assets.Scripts.UnitySideScripts.EditingScripts
{
    class HighwayEdit : MonoBehaviour
    {

        GameObject highwayEditMenu;

        //GETS THE SCENE OBJECT FROM THEM
        GameObject loadSaveMenu;
        LoadSaveMenu lsm;

        Text TThighwayID;
        Text TThighwayType;
        Text TThighwayName;
        InputField IFhighwaySize;
        Toggle TGleftSidewalk;
        Toggle TGrightSidewalk;
        InputField IFleftSidewalkSize;
        InputField IFrightSidewalkSize;

        private bool fillLock;

        public void clickClose()
        {
            highwayEditMenu.SetActive(false);
        }

        public void clickSave()
        {

        }

        public void fillMenu(string highwayID)
        {
            fillLock = true;

            if (highwayEditMenu == null)
                highwayEditMenu = GameObject.Find("Canvas").transform.Find("HighwayEdit").gameObject;
            if(TThighwayID == null)
            {
                TThighwayID = highwayEditMenu.transform.Find("Panel").Find("Text_ID").GetComponent<Text>();
                TThighwayType = highwayEditMenu.transform.Find("Panel").Find("Text_Type").GetComponent<Text>();
                TThighwayName = highwayEditMenu.transform.Find("Panel").Find("Text_StreetName").GetComponent<Text>();

                IFhighwaySize = highwayEditMenu.transform.Find("Panel").Find("InputField_Size").GetComponent<InputField>();
                TGleftSidewalk = highwayEditMenu.transform.Find("Panel").Find("ToggleLeft").GetComponent<Toggle>();
                TGrightSidewalk = highwayEditMenu.transform.Find("Panel").Find("ToggleRight").GetComponent<Toggle>();
                IFleftSidewalkSize = highwayEditMenu.transform.Find("Panel").Find("InputField_LeftSize").GetComponent<InputField>();
                IFrightSidewalkSize = highwayEditMenu.transform.Find("Panel").Find("InputField_RightSize").GetComponent<InputField>();
            }

            if (loadSaveMenu == null)
            {
                loadSaveMenu = GameObject.Find("Canvas").transform.Find("LoadSave Menu").gameObject;
                lsm = loadSaveMenu.GetComponent<LoadSaveMenu>();
            }

            Highway highway = lsm.scene.highwayList.Find(item => item.id == highwayID);

            TThighwayID.text = highway.id;
            TThighwayType.text = highway.type.ToString("G");
            TThighwayName.text = highway.name;
            IFhighwaySize.text = highway.waySize.ToString();
            TGleftSidewalk.isOn = highway.hasLeftSidewalk;
            TGrightSidewalk.isOn = highway.hasRightSideWalk;
            IFleftSidewalkSize.text = highway.leftSidewalkSize.ToString();
            IFrightSidewalkSize.text = highway.rightSidewalkSize.ToString();

            fillLock = false;
        }

        public void onHighwaySizChanged()
        {
            float newSize = float.Parse(IFhighwaySize.text);
            lsm.scene.highwayModeller.resizeHighway(TThighwayID.text, newSize);
        }

        public void onLeftSidewalkChanged()
        {
            if (fillLock)
                return;

            if (TGleftSidewalk.isOn == false)
                lsm.scene.highwayModeller.deletePavement(TThighwayID.text, Pavement.pavementSide.left);
            else
            {
                lsm.scene.highwayModeller.addNewPavement(TThighwayID.text, Pavement.pavementSide.left, float.Parse(IFleftSidewalkSize.text));
                lsm.scene.highwayModeller.correctPavement(TThighwayID.text, Pavement.pavementSide.left);
            }
        }

        public void onRightSidewalkChanged()
        {
            if (fillLock)
                return;

            if (TGrightSidewalk.isOn == false)
                lsm.scene.highwayModeller.deletePavement(TThighwayID.text, Pavement.pavementSide.right);
            else
            {
                lsm.scene.highwayModeller.addNewPavement(TThighwayID.text, Pavement.pavementSide.right, float.Parse(IFrightSidewalkSize.text));
                lsm.scene.highwayModeller.correctPavement(TThighwayID.text, Pavement.pavementSide.right);
            }
        }
    }
}
