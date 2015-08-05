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
        List<objectListItem> treeObjectList;
        List<objectListItem> barrierObjectList;
        List<objectListItem> trafficSignObjectList;

        Transform contentPanel;

        

        public void fillCarList()
        {
            transform.gameObject.SetActive(true);

            transform.Find("Panel").Find("LabelTitle").GetComponent<Text>().text = "SELECT CAR";

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
            transform.Find("Panel").Find("LabelTitle").GetComponent<Text>().text = "SELECT TREE";

            if (contentPanel == null)
                contentPanel = transform.Find("Panel").Find("ScrollRect").Find("Content Panel");

            var children = new List<GameObject>();
            foreach (Transform child in contentPanel) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));


            treeObjectList = new List<objectListItem>();
            objectListItem obj1 = new objectListItem("Broadleaf 1", "Prefabs/Environment/SpeedTree/Broadleaf/broadLeafDesktopIcon", "Prefabs/Environment/SpeedTree/Broadleaf/BroadleafDesktopPrefab");
            treeObjectList.Add(obj1);
            objectListItem obj2 = new objectListItem("Broadleaf 2", "Prefabs/Environment/SpeedTree/Broadleaf/broadLeafMobileIcon", "Prefabs/Environment/SpeedTree/Broadleaf/BroadleafMobilePrefab");
            treeObjectList.Add(obj2);
            objectListItem obj3 = new objectListItem("Conifer", "Prefabs/Environment/SpeedTree/Conifer/coniferIcon", "Prefabs/Environment/SpeedTree/Conifer/ConiferPrefab");
            treeObjectList.Add(obj3);
            objectListItem obj4 = new objectListItem("Palm", "Prefabs/Environment/SpeedTree/Palm/palmIcon", "Prefabs/Environment/SpeedTree/Palm/PalmPrefab");
            treeObjectList.Add(obj4);
            objectListItem obj5 = new objectListItem("Tree 5", "Prefabs/Environment/Tree5/Tree5Icon", "Prefabs/Environment/Tree5/Tree5Prefab");
            treeObjectList.Add(obj5);

            fillScrollRect(ObjectType.Tree);


        }

        public void fillBarrierList()
        {
            transform.gameObject.SetActive(true);
            transform.Find("Panel").Find("LabelTitle").GetComponent<Text>().text = "SELECT BARRIER";

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
            transform.Find("Panel").Find("LabelTitle").GetComponent<Text>().text = "SELECT TRAFFIC SIGN";

            if (contentPanel == null)
                contentPanel = transform.Find("Panel").Find("ScrollRect").Find("Content Panel");

            var children = new List<GameObject>();
            foreach (Transform child in contentPanel) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));

            trafficSignObjectList = new List<objectListItem>();
            objectListItem obj1 = new objectListItem("Traffic Light 1", "Prefabs/TrafficSign/TrafficLight1/TrafficLight1Icon", "Prefabs/TrafficSign/TrafficLight1/TrafficLight1Prefab");
            trafficSignObjectList.Add(obj1);
            objectListItem obj2 = new objectListItem("Traffic Light 2", "Prefabs/TrafficSign/TrafficLight2/TrafficLight2Icon", "Prefabs/TrafficSign/TrafficLight2/TrafficLight2Prefab");
            trafficSignObjectList.Add(obj2);
            objectListItem obj3 = new objectListItem("Traffic Light 3", "Prefabs/TrafficSign/TrafficLight3/TrafficLight3Icon", "Prefabs/TrafficSign/TrafficLight3/TrafficLight3Prefab");
            trafficSignObjectList.Add(obj3);

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
                    objList = trafficSignObjectList;
                    break;
                case ObjectType.Tree :
                    objList = treeObjectList;
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
