using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UnitySideScripts.MouseScripts
{
    class pinPointMouseHandler : MonoBehaviour, IDragHandler, IDropHandler
    {
        //sectorID defines which sector the pinpoint is:   1 : Top Right  2:Bottom Right  3: Bottom Left  4: Top Left 
        public int sectorID = -1;
        public RawImage uvRawImage = null;
        public InputField U=null, V=null;
        public Sprite selected=null, unselected=null;

        Vector3 rawPosition;
        Vector2 windowSize;

        void Start()
        {
            rawPosition = uvRawImage.transform.position;
            windowSize = uvRawImage.rectTransform.sizeDelta;

        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3 newPosition;
            newPosition = Input.mousePosition;
            transform.GetComponent<Image>().overrideSprite = selected;
           
            switch (sectorID)
            {
                case 1: //TOP RIGHT
                    newPosition.x = Math.Max(rawPosition.x, Math.Min(newPosition.x, rawPosition.x + windowSize.x/2));
                    newPosition.y = Math.Max(rawPosition.y, Math.Min(newPosition.y, rawPosition.y + windowSize.x/2));
                    U.text = (0.5f + (newPosition.x - rawPosition.x)/windowSize.x).ToString();
                    V.text = (0.5f + (newPosition.y - rawPosition.y)/windowSize.y).ToString();
                    break;
                case 2: //BOTTOM RIGHT
                    newPosition.x = Math.Max(rawPosition.x, Math.Min(newPosition.x, rawPosition.x + windowSize.x / 2));
                    newPosition.y = Math.Max(rawPosition.y - windowSize.y / 2, Math.Min(newPosition.y, rawPosition.y));
                    U.text = (0.5f + (newPosition.x - rawPosition.x) / windowSize.x).ToString();
                    V.text = (0.5f - (rawPosition.y - newPosition.y) / windowSize.x).ToString();
                    break;
                case 3: //BOTTOM LEFT
                    newPosition.x = Math.Max(rawPosition.x - windowSize.x / 2, Math.Min(newPosition.x,rawPosition.x));
                    newPosition.y = Math.Max(rawPosition.y - windowSize.y / 2, Math.Min(newPosition.y, rawPosition.y));
                    U.text = (0.5f - (rawPosition.x - newPosition.x) / windowSize.x).ToString();
                    V.text = (0.5f - (rawPosition.y - newPosition.y) / windowSize.x).ToString();
                    break;
                case 4: //TOP LEFT
                    newPosition.x = Math.Max(rawPosition.x - windowSize.x / 2, Math.Min(newPosition.x, rawPosition.x));
                    newPosition.y = Math.Max(rawPosition.y, Math.Min(newPosition.y, rawPosition.y + windowSize.x / 2));
                    U.text = (0.5f - (rawPosition.x - newPosition.x) / windowSize.x).ToString();
                    V.text = (0.5f + (newPosition.y - rawPosition.y) / windowSize.y).ToString();
                    break;

            }

            this.transform.position = newPosition;
        }

        public void OnDrop(PointerEventData eventData)
        {
            transform.GetComponent<Image>().overrideSprite = unselected;
        }
    }
}
