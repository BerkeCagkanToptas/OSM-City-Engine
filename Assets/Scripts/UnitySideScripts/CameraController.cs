using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.SceneObjects;
using Assets.Scripts.UnitySideScripts.EditingScripts;
using UnityEngine.EventSystems;
using Assets.Scripts.UnitySideScripts.MouseScripts;

namespace Assets.Scripts.UnitySideScripts
{
    public class CameraController : MonoBehaviour
    {
        //*********************Free Fly Camera Settings***************
        Vector2 _mouseAbsolute;
        Vector2 _smoothMouse;
        Vector2 clampInDegrees = new Vector2(360, 180);
        Vector2 sensitivity = new Vector2(2, 2);
        Vector2 smoothing = new Vector2(3, 3);
        Vector2 targetDirection;
        public float CameraSpeed = 20.0f;
        //************************************************************

        //********************Car Camera *********************
        public Transform target;
        //************************************************************

        //Mouse Click Textures
        Texture2D normalCursorTexture;
        Texture2D clickedCursorTexture;

        public enum CameraMode
        {
            freeFlyMode,
            recordMode,
        }

        public CameraMode mode;
        private bool isDragging = false;

        void Start()
        {
            normalCursorTexture = (Texture2D)Resources.Load("Textures/Cursor/CursorNormal");
            clickedCursorTexture = (Texture2D)Resources.Load("Textures/Cursor/CursorClicked");
          
            Cursor.SetCursor(normalCursorTexture,Vector2.zero,CursorMode.ForceSoftware);

            //Set initial camera Position [SHOULD BE CHANGED]
            transform.position = new Vector3(0.0f, 300.0f, 0.0f);

            // Set target direction to the camera's initial orientation.
            targetDirection = transform.localRotation.eulerAngles;
        }

        void Update()
        {
            if (mode == CameraMode.freeFlyMode)
                freeFlyUpdate();
        }

        private void freeFlyUpdate()
        {
            Vector3 dir = new Vector3(); //(0,0,0)

            if (Input.GetKey(KeyCode.W))//|| Input.GetKey(KeyCode.UpArrow))
                dir.z += 1.0f;
            else if (Input.GetKey(KeyCode.A)) //|| Input.GetKey(KeyCode.LeftArrow))
                dir.x -= 1.0f;
            else if (Input.GetKey(KeyCode.S))// || Input.GetKey(KeyCode.DownArrow))
                dir.z -= 1.0f;
            else if (Input.GetKey(KeyCode.D)) //|| Input.GetKey(KeyCode.RightArrow))
                dir.x += 1.0f;

            if (Input.GetKeyDown(KeyCode.LeftShift))
                CameraSpeed = 200.0f;
            if (Input.GetKeyUp(KeyCode.LeftShift))
                CameraSpeed = 20.0f;

            dir.Normalize();
            transform.Translate(dir * CameraSpeed * Time.deltaTime);

            if (Input.GetMouseButtonDown(1))
                isDragging = true;
            if (Input.GetMouseButtonUp(1))
                isDragging = false;

            if (Input.GetMouseButtonDown(0))
                Cursor.SetCursor(clickedCursorTexture, Vector2.zero, CursorMode.ForceSoftware);

            if (Input.GetMouseButtonUp(0))
                Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.ForceSoftware);

            if (isDragging)
                dragScreen();
        }

        public void cameraUpdate()
        {
            switch(mode)
            {
                case CameraMode.recordMode:
                    target.gameObject.SetActive(true);
                    this.transform.position = target.position - (target.forward.normalized * 7.0f) + new Vector3(0.0f, 4.0f, 0.0f);
                    this.transform.rotation = target.transform.rotation;
                    this.transform.SetParent(target);
                    break;
                case CameraMode.freeFlyMode:
                    this.transform.SetParent(null);
                    break;
            }
            
        }

        private void dragScreen()
        {

            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler(targetDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            // Find the absolute mouse movement value from point zero.
            _mouseAbsolute += _smoothMouse;

            // Clamp and apply the local x value first, so as not to be affected by world transforms.
            if (clampInDegrees.x < 360)
                _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

            var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
            transform.localRotation = xRotation;

            // Then clamp and apply the global y value.
            if (clampInDegrees.y < 360)
                _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

            transform.localRotation *= targetOrientation;

            var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
            transform.localRotation *= yRotation;


        }

    }
    


}






