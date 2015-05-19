using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.OpenStreetMap;
using Assets.Scripts.HeightMap;

namespace Assets.Scripts
{
    class CreateScene : MonoBehaviour
    {

        void Start()
        {
            Scene scene = new Scene();
            scene.initializeScene("OSMFiles/comoGeneral", HeightMap.HeightmapContinent.Eurasia,MapProvider.BingMapAerial);
            
        }

    }
}
