using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.Menus
{
    class DefaultSkyboxSettings : MonoBehaviour
    {


        public void changeDayTime(GameObject _scrollBarObject)
        {
            Scrollbar scrollbar = _scrollBarObject.GetComponent<Scrollbar>();
           
            Light light = GameObject.Find("Directional Light").GetComponent<Light>();
            Vector3 oldAngle = light.transform.rotation.eulerAngles;
            
            //ANGLE
            if (scrollbar.value <= 0.95f)
                light.transform.eulerAngles = new Vector3(scrollbar.value * 180.0f, 90, 0);
            else 
                light.transform.eulerAngles = new Vector3(scrollbar.value * 190.0f, 90, 0);

            //INTENSITY
            if (scrollbar.value <= 0.50f)
                light.intensity = 0.75f + scrollbar.value * 0.5f;
            else if (scrollbar.value <= 0.95f)
                light.intensity = 1.0f - (scrollbar.value - 0.5f) * 0.5f;
            else
                light.intensity = 0.0f;

            //FLARE
            if (scrollbar.value <= 0.05f)
                light.flare = null;
            else if (scrollbar.value <= 0.95f)
                light.flare = (Flare)Resources.Load("LightFlares/Flares/50mmZoom");
            else
                light.flare = null;

        }

        public void clickOkay(GameObject skyboxmenu)
        {
            skyboxmenu.SetActive(false);
        }

    }
}
