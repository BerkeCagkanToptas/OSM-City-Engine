using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UnitySideScripts.MouseScripts
{
    class CameraVanMouseHandler : MonoBehaviour, IPointerClickHandler
    {

        public MouseActions actionhandler;

        public void Start()
        {
            actionhandler = GameObject.Find("MouseAction").GetComponent<MouseActions>();
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)             
                actionhandler.clickAction(MouseActions.objectType.cameraVan, transform.gameObject, "");
                
        }
    }
}
