using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Assets.Scripts.UnitySideScripts
{
    class CarController : MonoBehaviour
    {
        public WheelCollider[] wheelColliders = new WheelCollider[4];
        public Transform[] wheelTransforms = new Transform[4];
        public Transform centerOfMass = null;
        public float maxTorque = 100.0f;

        private Rigidbody rigidBody;



        void Start()
        {
            rigidBody = transform.GetComponent<Rigidbody>();
            rigidBody.centerOfMass = centerOfMass.localPosition;
        }

        void updateMeshPositions()
        {
            for(int i = 0 ; i < 4 ; i++)
            {
                Quaternion quat;
                Vector3 pos;
                wheelColliders[i].GetWorldPose(out pos, out quat);
                wheelTransforms[i].position = pos;
                wheelTransforms[i].rotation = quat;
            }


        }

        void FixedUpdate()
        {
            float steer = Input.GetAxis("Horizontal");
            float accelerate = Input.GetAxis("Vertical");

            for (int i = 0; i < 4; i++)
                wheelColliders[i].motorTorque = maxTorque * accelerate;

            wheelColliders[0].steerAngle = 45 * steer;
            wheelColliders[1].steerAngle = 45 * steer;
        }

        void Update()
        {
            updateMeshPositions();
        }


    }
}
