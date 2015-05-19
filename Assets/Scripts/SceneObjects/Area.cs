using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.ConfigHandler;

namespace Assets.Scripts.OpenStreetMap
{

    public enum areaType
    {
        standard,
        amenity,
        amenityParking,
        amenitySocialFacility,
        amenityMotorCycleParking,
        amenityFireStation,
        landuseForest,
        landuseGrass,
        landuseMeadow,
        leisure,
        none
    }


    class Area
    {
        private AreaConfigurations areaConfig; 

        public Area(Way w, AreaConfigurations config)
        {


        }



    }
}
