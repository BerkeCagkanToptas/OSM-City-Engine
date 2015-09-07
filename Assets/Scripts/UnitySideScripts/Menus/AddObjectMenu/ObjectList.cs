using Assets.Scripts.SceneObjects;
using Assets.Scripts.UnitySideScripts.MouseScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.Menus.AddObjectMenu
{
    class ObjectList : MonoBehaviour
    {

        struct objectListItem
        {
            public string iconPath;
            public string prefabPath;
            public string name;

            public objectListItem(string _name, string _iconPath, string _prefabPath)
            {
                iconPath = _iconPath;
                prefabPath = _prefabPath;
                name = _name;
            }
        }

        List<objectListItem> carObjectList;
        List<objectListItem> environmentObjectList;
        List<objectListItem> barrierObjectList;
        List<objectListItem> cityRelatedObjectList;

        Transform contentPanel;

        

        public void fillCarList()
        {
            transform.gameObject.SetActive(true);

            transform.Find("Panel").Find("LabelTitle").GetComponent<Text>().text = "SELECT VEHICLE OBJECT";

            if(contentPanel == null)
                contentPanel = transform.Find("Panel").Find("ScrollRect").Find("Content Panel");

            var children = new List<GameObject>();
            foreach (Transform child in contentPanel) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));


            carObjectList = new List<objectListItem>();
            objectListItem obj1 = new objectListItem("Bus", "Prefabs/Car/Bus/busIcon","Prefabs/Car/Bus/busObj");
            carObjectList.Add(obj1);
            objectListItem obj2 = new objectListItem("Audi Station Wagon", "Prefabs/Car/Lincoln/LincolnIcon", "Prefabs/Car/Lincoln/LincolnObj");
            carObjectList.Add(obj2);
            objectListItem obj3 = new objectListItem("Police Car", "Prefabs/Car/PoliceCar/policeCarIcon","Prefabs/Car/PoliceCar/policeCarObj");
            carObjectList.Add(obj3);
            objectListItem obj4 = new objectListItem("Yellow Taxi", "Prefabs/Car/Taxi/taxiIcon", "Prefabs/Car/Taxi/taxiObj");
            carObjectList.Add(obj4);
            objectListItem obj5 = new objectListItem("Van", "Prefabs/Car/Van/vanIcon", "Prefabs/Car/Van/vanObj");
            carObjectList.Add(obj5);

            fillScrollRect(ObjectType.Car);

        }

        public void fillTreeList()
        {
            transform.gameObject.SetActive(true);
            transform.Find("Panel").Find("LabelTitle").GetComponent<Text>().text = "SELECT ENVIRONMENT OBJECT";

            if (contentPanel == null)
                contentPanel = transform.Find("Panel").Find("ScrollRect").Find("Content Panel");

            var children = new List<GameObject>();
            foreach (Transform child in contentPanel) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));


            environmentObjectList = new List<objectListItem>();
            objectListItem obj1 = new objectListItem("Broadleaf 1", "Prefabs/Environment/Icons/broadLeafDesktopIcon", "Prefabs/Environment/Prefabs/BroadleafDesktop");
            environmentObjectList.Add(obj1);
            objectListItem obj2 = new objectListItem("Broadleaf 2", "Prefabs/Environment/Icons/broadLeafMobileIcon", "Prefabs/Environment/Prefabs/BroadleafMobile");
            environmentObjectList.Add(obj2);
            objectListItem obj3 = new objectListItem("Conifer", "Prefabs/Environment/Icons/coniferIcon", "Prefabs/Environment/Prefabs/Conifer");
            environmentObjectList.Add(obj3);
            objectListItem obj4 = new objectListItem("Palm", "Prefabs/Environment/Icons/palmIcon", "Prefabs/Environment/Prefabs/Palm");
            environmentObjectList.Add(obj4);
            objectListItem obj5 = new objectListItem("Tree 5", "Prefabs/Environment/Icons/Tree5Icon", "Prefabs/Environment/Prefabs/Tree5");
            environmentObjectList.Add(obj5);
            objectListItem obj6 = new objectListItem("Fountain 1", "Prefabs/Environment/Icons/Fountain1", "Prefabs/Environment/Prefabs/Fountain1");
            environmentObjectList.Add(obj6);
            objectListItem obj7 = new objectListItem("Fountain 2", "Prefabs/Environment/Icons/Fountain2", "Prefabs/Environment/Prefabs/Fountain2");
            environmentObjectList.Add(obj7);
            objectListItem obj8 = new objectListItem("Sculpture 1", "Prefabs/Environment/Icons/Sculpture1", "Prefabs/Environment/Prefabs/Sculpture1");
            environmentObjectList.Add(obj8);
            objectListItem obj9 = new objectListItem("Sculpture 2", "Prefabs/Environment/Icons/Sculpture2", "Prefabs/Environment/Prefabs/Sculpture2");
            environmentObjectList.Add(obj9);
            objectListItem obj10 = new objectListItem("Sculpture 3", "Prefabs/Environment/Icons/Sculpture3", "Prefabs/Environment/Prefabs/Sculpture3");
            environmentObjectList.Add(obj10);

            fillScrollRect(ObjectType.Tree);


        }

        public void fillBarrierList()
        {
            transform.gameObject.SetActive(true);
            transform.Find("Panel").Find("LabelTitle").GetComponent<Text>().text = "SELECT BARRIER OBJECT";

            if (contentPanel == null)
                contentPanel = transform.Find("Panel").Find("ScrollRect").Find("Content Panel");

            var children = new List<GameObject>();
            foreach (Transform child in contentPanel) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));

            barrierObjectList = new List<objectListItem>();
            objectListItem obj1 = new objectListItem("Metal Fence", "Prefabs/Barrier/MetalFence/metalFenceIcon", "Prefabs/Barrier/MetalFence/metalFencePrefab");
            barrierObjectList.Add(obj1);
            objectListItem obj2 = new objectListItem("Concreate Wall","Prefabs/Barrier/WallConcrete/ConcreteWallIcon", "Prefabs/Barrier/WallConcrete/wallConcretePrefab");
            barrierObjectList.Add(obj2);

            fillScrollRect(ObjectType.Wall);
        }

        public void fillTrafficSignList()
        {
            transform.gameObject.SetActive(true);
            transform.Find("Panel").Find("LabelTitle").GetComponent<Text>().text = "SELECT CITY RELATED OBJECT";

            if (contentPanel == null)
                contentPanel = transform.Find("Panel").Find("ScrollRect").Find("Content Panel");

            var children = new List<GameObject>();
            foreach (Transform child in contentPanel) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));

            cityRelatedObjectList = new List<objectListItem>();
            objectListItem obj1 = new objectListItem("Traffic Light 1", "Prefabs/CityRelated/Icons/TrafficLight1Icon", "Prefabs/CityRelated/Prefabs/TrafficLight1Prefab");
            cityRelatedObjectList.Add(obj1);
            objectListItem obj2 = new objectListItem("Traffic Light 2", "Prefabs/CityRelated/Icons/TrafficLight2Icon", "Prefabs/CityRelated/Prefabs/TrafficLight2Prefab");
            cityRelatedObjectList.Add(obj2);
            objectListItem obj3 = new objectListItem("Traffic Light 3", "Prefabs/CityRelated/Icons/TrafficLight3Icon", "Prefabs/CityRelated/Prefabs/TrafficLight3Prefab");
            cityRelatedObjectList.Add(obj3);
            objectListItem obj4 = new objectListItem("Street Lamp", "Prefabs/CityRelated/Icons/StreetLamp", "Prefabs/CityRelated/Prefabs/StreetLampPrefab");
            cityRelatedObjectList.Add(obj4);
            objectListItem obj5 = new objectListItem("Phone Box", "Prefabs/CityRelated/Icons/PhoneBox", "Prefabs/CityRelated/Prefabs/PhoneBoxPrefab");
            cityRelatedObjectList.Add(obj5);
            objectListItem obj6 = new objectListItem("Garden Chair", "Prefabs/CityRelated/Icons/GardenChair", "Prefabs/CityRelated/Prefabs/GardenChairPrefab");
            cityRelatedObjectList.Add(obj6);
            objectListItem obj7 = new objectListItem("Hydrant", "Prefabs/CityRelated/Icons/Hydrant", "Prefabs/CityRelated/Prefabs/HydrantPrefab");           
            cityRelatedObjectList.Add(obj7);
            fillScrollRect(ObjectType.TrafficSign);

        }

        private void selectItem(Text prefabPath,Text name, ObjectType type)
        {

            Transform camera = GameObject.Find("Main Camera").transform;
            Vector3 position = camera.position + camera.forward * 10.0f;

            Object3D obj = new Object3D();
            obj.name = name.text;
            obj.type = type;
            obj.resourcePath = prefabPath.text;
            obj.object3D = (GameObject)Instantiate(Resources.Load(prefabPath.text));
            obj.object3D.tag = "3DObject";
            obj.object3D.name = name.text;
            obj.object3D.AddComponent<Object3dMouseHandler>();
            obj.object3D.transform.position = position;

            LoadSaveMenu lsm = GameObject.Find("Canvas").transform.Find("LoadSave Menu").GetComponent<LoadSaveMenu>();
            lsm.scene.object3DList.Add(obj);
            this.gameObject.SetActive(false);
        }

        public void clickCancel()
        {
            transform.gameObject.SetActive(false);
        }

        private void fillScrollRect(ObjectType type)
        {
            List<objectListItem> objList = new List<objectListItem>(); ;

            switch(type)
            {
                case ObjectType.Car :
                    objList = carObjectList;
                    break;
                case ObjectType.TrafficSign:
                    objList = cityRelatedObjectList;
                    break;
                case ObjectType.Tree :
                    objList = environmentObjectList;
                    break;
                case ObjectType.Wall :
                    objList = barrierObjectList;
                    break;
                case ObjectType.External :
                    //Not Implemented
                    break;

                default :
                    objList = new List<objectListItem>();
                    break;
            }


            for (int i = 0; i < objList.Count; i++)
            {
                GameObject objectItemSkin = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/Menu/AddObjectSkin"));
                objectItemSkin.transform.SetParent(contentPanel);

                RawImage RIicon = objectItemSkin.transform.Find("Panel").Find("Icon").GetComponent<RawImage>();
                Text LBname = objectItemSkin.transform.Find("Panel").Find("Name").GetComponent<Text>();
                Text LBobjPath = objectItemSkin.transform.Find("Panel").Find("objPath").GetComponent<Text>();
                objectItemSkin.transform.Find("Panel").Find("Button").GetComponent<Button>().onClick.AddListener(delegate { selectItem(LBobjPath,LBname,type); });

                RIicon.texture = (Texture)Resources.Load(objList[i].iconPath);
                LBname.text = objList[i].name;
                LBobjPath.text = objList[i].prefabPath;
            }
           


        }


    }
}
