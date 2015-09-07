using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using Assets.Scripts.SceneObjects;
using Assets.Scripts.UnitySideScripts.EditingScripts;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.MouseScripts
{
    public class MouseActions : MonoBehaviour
    {

        public enum objectType
        {
            building,
            highway,
            barrier,
            object3d,
            cameraVan,
            terrain
        }

        //****************Edit Menus And their Scripts****************
        BuildingEdit buildingEdit;
        HighwayEdit highwayEdit;
        BarrierEdit barrierEdit;
        ObjectEdit objectEdit;
        CameraVanEdit cameraVanEdit;
        GameObject buildingEditMenu;
        GameObject highwayEditMenu;
        GameObject barrierEditMenu;
        GameObject ObjectEditMenu;
        GameObject CameraVanEditMenu;

        InputField translateX, translateY, translateZ, rotateX, rotateY, rotateZ, scaleX, scaleY, scaleZ; 


        //************************************************************
        string currentSelectionID, previousSelectionID;
        GameObject previousSelectionObj, currentSelectionObj;
        //************************************************************


        //*******************Object Selection*************************
        private GameObject SelectedGameObject;
        private TransformGizmos SelectedTransformGizmos;
        //************************************************************


        void Start()
        {
            GameObject canvas = GameObject.Find("Canvas");
            buildingEditMenu = canvas.transform.Find("BuildingEdit").gameObject;
            highwayEditMenu = canvas.transform.Find("HighwayEdit").gameObject;
            barrierEditMenu = canvas.transform.Find("BarrierEdit").gameObject;
            ObjectEditMenu = canvas.transform.Find("3DObjectEdit").gameObject;           
            CameraVanEditMenu = canvas.transform.Find("CameraVanEdit").gameObject;
            buildingEdit = buildingEditMenu.GetComponent<BuildingEdit>();
            highwayEdit = highwayEditMenu.GetComponent<HighwayEdit>();
            barrierEdit = barrierEditMenu.GetComponent<BarrierEdit>();
            objectEdit = ObjectEditMenu.GetComponent<ObjectEdit>();
            cameraVanEdit = CameraVanEditMenu.GetComponent<CameraVanEdit>();

            translateX = ObjectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_TransX").GetComponent<InputField>();
            translateY = ObjectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_TransY").GetComponent<InputField>();
            translateZ = ObjectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_TransZ").GetComponent<InputField>();

            rotateX = ObjectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_RotateX").GetComponent<InputField>();
            rotateY = ObjectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_RotateY").GetComponent<InputField>();
            rotateZ = ObjectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_RotateZ").GetComponent<InputField>();

            scaleX = ObjectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_ScaleX").GetComponent<InputField>();
            scaleY = ObjectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_ScaleY").GetComponent<InputField>();
            scaleZ = ObjectEditMenu.transform.Find("Panel").Find("TransformPart").Find("InputField_ScaleZ").GetComponent<InputField>();

            currentSelectionID = "";
            previousSelectionID = "";

        }

        void Update()
        {

           
          
            //GIZMO TYPE CHANGE  1: TRANSFORM 2: ROTATION 3: SCALE
            if (SelectedTransformGizmos != null && SelectedGameObject != null)
            {
                updateTransformBar();
                if (Input.GetKeyDown(KeyCode.Alpha1))
                    SelectedTransformGizmos.TurnOnTransformationOptionGizmo();
                if (Input.GetKeyDown(KeyCode.Alpha2))
                    SelectedTransformGizmos.TurnOnRotationOptionGizmo();
                if (Input.GetKeyDown(KeyCode.Alpha3))
                    SelectedTransformGizmos.TurnOnScaleOptionGizmo();
            }

        }


        public void clickAction(objectType type, GameObject gameObj, string ID)
        {
            if (drawGizmo(type, gameObj))
                return;

            currentSelectionID = ID;
            currentSelectionObj = gameObj;

            if (gameObj.tag != "3DObject" && gameObj.tag != "CameraVan" && gameObj.tag != "Terrain")
               currentSelectionObj.GetComponent<MeshRenderer>().material.color = new Color(0.3f, 0.3f, 1.0f);

            if (previousSelectionID != "" && previousSelectionObj.tag != "3DObject" && previousSelectionObj.tag != "CameraVan" && previousSelectionObj.tag != "Terrain")
                previousSelectionObj.GetComponent<MeshRenderer>().material.color = new Color(1, 1, 1);

            if (currentSelectionObj == previousSelectionObj)
            {
                highwayEditMenu.SetActive(false);
                barrierEditMenu.SetActive(false);
                ObjectEditMenu.SetActive(false);
                buildingEditMenu.SetActive(false);
                CameraVanEditMenu.SetActive(false);
                previousSelectionObj = null;
                previousSelectionID = "";
                return;
            }


            //fill edit menu
            switch(type)
            {
                case objectType.building :
                    buildingEditMenu.SetActive(true);
                    ObjectEditMenu.SetActive(false);
                    highwayEditMenu.SetActive(false);
                    barrierEditMenu.SetActive(false);
                    CameraVanEditMenu.SetActive(false);
                    int facadeId = int.Parse(gameObj.name.Substring("facade".Length));
                    buildingEdit.fillMenu(ID,facadeId);
                    break;
                case objectType.highway :
                    buildingEditMenu.SetActive(false);
                    ObjectEditMenu.SetActive(false);
                    barrierEditMenu.SetActive(false);
                    highwayEditMenu.SetActive(true);
                    CameraVanEditMenu.SetActive(false);
                    highwayEdit.fillMenu(ID);
                    break;
                case objectType.barrier :
                    buildingEditMenu.SetActive(false);
                    ObjectEditMenu.SetActive(false);
                    highwayEditMenu.SetActive(false);
                    CameraVanEditMenu.SetActive(false);
                    barrierEditMenu.SetActive(true);
                    barrierEdit.fillMenu(ID);
                    break;

                case objectType.terrain :
                    buildingEditMenu.SetActive(false);
                    ObjectEditMenu.SetActive(false);
                    highwayEditMenu.SetActive(false);
                    CameraVanEditMenu.SetActive(false);
                    barrierEditMenu.SetActive(false);
                    break;

                case objectType.object3d :
                    buildingEditMenu.SetActive(false);
                    ObjectEditMenu.SetActive(true);
                    highwayEditMenu.SetActive(false);
                    CameraVanEditMenu.SetActive(false);
                    barrierEditMenu.SetActive(false);
                    objectEdit.fillMenu(gameObj);
                    break;
                case objectType.cameraVan :
                    buildingEditMenu.SetActive(false);
                    ObjectEditMenu.SetActive(false);
                    highwayEditMenu.SetActive(false);
                    CameraVanEditMenu.SetActive(true);
                    barrierEditMenu.SetActive(false);
                    break;
                default :
                    buildingEditMenu.SetActive(false);
                    ObjectEditMenu.SetActive(false);
                    highwayEditMenu.SetActive(false);
                    CameraVanEditMenu.SetActive(false);
                    barrierEditMenu.SetActive(false);
                    break;
            }

            //Update previous selection
            previousSelectionObj = currentSelectionObj;
            previousSelectionID = currentSelectionID;

        }


        private bool drawGizmo(objectType type, GameObject gameObj)
        {

            if (SelectedGameObject != null && SelectedTransformGizmos != null)
            {
                if (SelectedTransformGizmos.SelectedType == TransformGizmos.MOVETYPE.NONE)
                {
                    SelectedTransformGizmos.TurnOffGizmos();
                    Destroy(SelectedGameObject.GetComponent<TransformGizmos>());
                    SelectedGameObject = null;
                    return false;
                }
                else
                    return true;

            }
            else
            {
                if (type == objectType.object3d || type == objectType.cameraVan)
                {
                    SelectedGameObject = gameObj;
                    SelectedGameObject.AddComponent<TransformGizmos>();
                    SelectedTransformGizmos = SelectedGameObject.GetComponent<TransformGizmos>();
                    SelectedTransformGizmos.TurnOnTransformationOptionGizmo();

                }

                return false;
            }

        }

        private void updateTransformBar()
        {
            Vector3 position = SelectedGameObject.transform.position;
            translateX.text = position.x.ToString();
            translateY.text = position.y.ToString();
            translateZ.text = position.z.ToString();

            Vector3 rotation = SelectedGameObject.transform.rotation.eulerAngles;
            rotateX.text = rotation.x.ToString();
            rotateY.text = rotation.y.ToString();
            rotateZ.text = rotation.z.ToString();

            Vector3 scale = SelectedGameObject.transform.localScale;
            scaleX.text = scale.x.ToString();
            scaleY.text = scale.y.ToString();
            scaleZ.text = scale.z.ToString();
        }

    }
}
