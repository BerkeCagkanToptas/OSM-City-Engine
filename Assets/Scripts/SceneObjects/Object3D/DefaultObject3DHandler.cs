using Assets.Scripts.OpenStreetMap;
using Assets.Scripts.UnitySideScripts.MouseScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SceneObjects
{
    class DefaultObject3DHandler
    {

        public static Object3D drawTrafficSign(Vector3 pos)
        {
            Object3D obj = new Object3D();
            obj.name = "Traffic Light";
            obj.type = ObjectType.TrafficSign;
            obj.resourcePath = "Prefabs/CityRelated/Prefabs/TrafficLight1Prefab";
            obj.object3D = (GameObject)MonoBehaviour.Instantiate(Resources.Load("Prefabs/CityRelated/Prefabs/TrafficLight1Prefab"));
            obj.object3D.AddComponent<Object3dMouseHandler>();
            obj.object3D.transform.position = pos + new Vector3(0.2f, 2.0f, -0.2f);
            obj.object3D.tag = "3DObject";
            obj.object3D.name = "Traffic Light";
            return obj;
        }

        public static Object3D drawStreetLamp(Vector3 pos, string id)
        {
            Object3D obj = new Object3D();
            obj.id = id;
            obj.name = "Street Lamp";
            obj.type = ObjectType.Tree;
            obj.resourcePath = "Prefabs/CityRelated/Prefabs/StreetLampPrefab";
            obj.object3D = (GameObject)MonoBehaviour.Instantiate(Resources.Load(obj.resourcePath));
            obj.object3D.AddComponent<Object3dMouseHandler>();
            obj.object3D.transform.position = pos;
            obj.object3D.tag = "3DObject";
            obj.object3D.name = "Street Lamp";

            return obj;
        }

        private static Object3D drawTree(Node nd)
        {
            Object3D obj = new Object3D();
            obj.id = nd.id;
            obj.name = "Broad Leaf Tree";
            obj.type = ObjectType.Tree;
            obj.resourcePath = "Prefabs/Environment/Prefabs/BroadLeafDesktop";
            obj.object3D = (GameObject)MonoBehaviour.Instantiate(Resources.Load(obj.resourcePath));
            obj.object3D.AddComponent<Object3dMouseHandler>();
            obj.object3D.transform.position = nd.meterPosition;
            obj.object3D.tag = "3DObject";
            obj.object3D.name = "Broad Leaf Tree";

            return obj;

        }

        private static Object3D drawPostBox(Node nd)
        {
            Object3D obj = new Object3D();
            obj.id = nd.id;
            obj.name = "Post Box";
            obj.type = ObjectType.Default;
            obj.resourcePath = "Prefabs/CityRelated/Prefabs/MailboxPrefab";
            obj.object3D = (GameObject)MonoBehaviour.Instantiate(Resources.Load(obj.resourcePath));
            obj.object3D.AddComponent<Object3dMouseHandler>();
            obj.object3D.transform.position = nd.meterPosition;
            obj.object3D.tag = "3DObject";
            obj.object3D.name = "Post Box";

            return obj;

        }

        private static Object3D drawPhoneBox(Node nd)
        {
            Object3D obj = new Object3D();
            obj.id = nd.id;
            obj.name = "Phone Box";
            obj.type = ObjectType.Default;
            obj.resourcePath = "Prefabs/CityRelated/Prefabs/PhoneBoxPrefab";
            obj.object3D = (GameObject)MonoBehaviour.Instantiate(Resources.Load(obj.resourcePath));
            obj.object3D.AddComponent<Object3dMouseHandler>();
            obj.object3D.transform.position = nd.meterPosition;
            obj.object3D.tag = "3DObject";
            obj.object3D.name = "Phone Box";

            return obj;

        }

        private static Object3D drawDrinkingFountain(Node nd)
        {
            Object3D obj = new Object3D();
            obj.id = nd.id;
            obj.name = "Drinking Fountain";
            obj.type = ObjectType.Default;
            obj.resourcePath = "Prefabs/Environment/Prefabs/Fountain2";
            obj.object3D = (GameObject)MonoBehaviour.Instantiate(Resources.Load(obj.resourcePath));
            obj.object3D.AddComponent<Object3dMouseHandler>();
            obj.object3D.transform.position = nd.meterPosition;
            obj.object3D.tag = "3DObject";
            obj.object3D.name = "Drinking Fountain";

            return obj;

        }

        public static  List<Object3D> drawDefaultObjects(List<Node> nodeList)
        {
            List<Object3D> defaultObjList = new List<Object3D>();

           foreach (Node nd in nodeList)
           {
               switch(nd.type)
               {
                   case ItemEnumerator.nodeType.Tree :
                       defaultObjList.Add(drawTree(nd));
                       break;
                   case ItemEnumerator.nodeType.PostBox :
                       defaultObjList.Add(drawPostBox(nd));
                       break;
                   case ItemEnumerator.nodeType.PhoneBox :
                       defaultObjList.Add(drawPhoneBox(nd));
                       break;
                   case ItemEnumerator.nodeType.DrinkingFountain:
                       defaultObjList.Add(drawDrinkingFountain(nd));
                       break;
                   case ItemEnumerator.nodeType.None:
                       break;
                }
           }

           return defaultObjList;

       }


    }
}
