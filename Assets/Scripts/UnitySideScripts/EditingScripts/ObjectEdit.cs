using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.SceneObjects;

namespace Assets.Scripts.UnitySideScripts.EditingScripts
{
    class ObjectEdit : MonoBehaviour
    {
        GameObject objectEditMenu;

        InputField[] IFtranslate;
        InputField[] IFrotate;
        InputField[] IFscale;
        Text TTObjectName;

        GameObject selectedObj;

        public void clickClose()
        {
            objectEditMenu.SetActive(false);
        }

        public void clickDestroy()
        {
            LoadSaveMenu lsm = GameObject.Find("Canvas").transform.Find("LoadSave Menu").GetComponent<LoadSaveMenu>();
            int index = lsm.scene.object3DList.FindIndex(o => o.object3D == selectedObj);
            Destroy(selectedObj);
            lsm.scene.object3DList.RemoveAt(index);
            objectEditMenu.SetActive(false);
        }

        public void clickSave()
        {

        }

        public void fillMenu(GameObject obj)
        {
            selectedObj = obj;

            if (objectEditMenu == null)
                objectEditMenu = GameObject.Find("Canvas").transform.Find("3DObjectEdit").gameObject;

            if (TTObjectName == null)
            {
                TTObjectName = objectEditMenu.transform.Find("Panel").Find("Text_Name").GetComponent<Text>();
                IFtranslate = new InputField[3];
                IFtranslate[0] = objectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_TransX").GetComponent<InputField>();
                IFtranslate[1] = objectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_TransY").GetComponent<InputField>();
                IFtranslate[2] = objectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_TransZ").GetComponent<InputField>();
                IFrotate = new InputField[3];
                IFrotate[0] = objectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_RotateX").GetComponent<InputField>();
                IFrotate[1] = objectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_RotateY").GetComponent<InputField>();
                IFrotate[2] = objectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_RotateZ").GetComponent<InputField>();
                IFscale = new InputField[3];
                IFscale[0] = objectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_ScaleX").GetComponent<InputField>();
                IFscale[1] = objectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_ScaleY").GetComponent<InputField>();
                IFscale[2] = objectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_ScaleZ").GetComponent<InputField>();
            }

            TTObjectName.text = obj.name;

            IFtranslate[0].text = obj.transform.position.x.ToString();
            IFtranslate[1].text = obj.transform.position.y.ToString();
            IFtranslate[2].text = obj.transform.position.z.ToString();
            IFrotate[0].text = obj.transform.rotation.eulerAngles.x.ToString();
            IFrotate[1].text = obj.transform.rotation.eulerAngles.y.ToString();
            IFrotate[2].text = obj.transform.rotation.eulerAngles.z.ToString();
            IFscale[0].text = obj.transform.localScale.x.ToString();
            IFscale[1].text = obj.transform.localScale.y.ToString();
            IFscale[2].text = obj.transform.localScale.z.ToString();

        }


    }
}
