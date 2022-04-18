using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{

    public class Jetpack : FVRPhysicalObject
    {
        [Header("Jetpack Config")]
        public JetpackJoystick joystick;
        public Transform leftThruster;
        public Transform rightThruster;
        public ParticleSystem[] thrusterParticles;

        public float force = 0.5f;
        public float spinSpeed = 30f;
#if !(UNITY_EDITOR || UNITY_5)
        private GameObject direction;
        private bool m_isHoveringMode;

        public bool IsHoveringMode
        {
            get
            {
                return m_isHoveringMode;
            }
            set
            {
                m_isHoveringMode = value;
            }
        }
        public override void Start()
        {
            base.Start();
            direction = new GameObject("JetpackDirection");
            direction.transform.SetParent(this.transform);
            direction.transform.localPosition = new Vector3(0,0,0);
            direction.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();

            Vector3 joystickRotation = joystick.Joystick.localEulerAngles;

            leftThruster.localRotation = Quaternion.Euler(joystickRotation.x * 3f + joystickRotation.y, 0, joystickRotation.z * 3f);
            rightThruster.localRotation = Quaternion.Euler(joystickRotation.x * 3f - joystickRotation.y, 0, joystickRotation.z * 3f);

            if (m_quickbeltSlot != null)
            {
                foreach (var item in thrusterParticles)
                {
                    item.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (var item in thrusterParticles)
                {
                    item.gameObject.SetActive(false);
                }
            }

            if (m_isHoveringMode && this.RootRigidbody.velocity.y < 0)
            {
                GM.CurrentMovementManager.Blast(this.transform.up, force);
            }
        }

        public void Boost(float strength)
        {
            if (m_quickbeltSlot != null)
            {
                Vector3 joystickRotation = joystick.Joystick.localEulerAngles;
                if (joystickRotation.x > 180f)
                {
                    joystickRotation.x -= 360f;
                }
                if (joystickRotation.y > 180f)
                {
                    joystickRotation.y -= 360f;
                }
                if (joystickRotation.z > 180f)
                {
                    joystickRotation.z -= 360f;
                }

                direction.transform.localRotation = Quaternion.Euler(joystickRotation.x * 3f, 0, joystickRotation.z * 3f);
                if (strength > 0f) GM.CurrentMovementManager.Blast(direction.transform.up, strength * force);


                float anglePerSec = (joystickRotation.y / 30f) * spinSpeed * strength * Time.deltaTime;
                GM.CurrentMovementManager.transform.Rotate(0, anglePerSec, 0);

                foreach (var item in thrusterParticles)
                {
                    var main = item.main;
                    main.startLifetimeMultiplier = strength;
                }
            }
        }
#endif
    }
}
