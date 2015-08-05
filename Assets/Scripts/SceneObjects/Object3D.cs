using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.SceneObjects
{
    public enum ObjectType
    {
        Car,
        Tree,
        TrafficSign,
        Wall,
        External
    }

    public class Object3D
    {
        public string name,resourcePath;
        public ObjectType type;
        public GameObject object3D;
    }
}
