using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class Ejector : MonoBehaviour
    {
        public OpenBoltReceiver LaserRifle;
        public FVRInteractiveObject EjectorLever;
        public float LeverClosed;
        public float LeverOpen;
        public float WiggleRoom;
        public float EjectionForce;
        public Transform EjectionDirection;
        public Vector3 EjectionSpin = new Vector3(20f, 20f, 5f);

        private bool _hasEjected = false;
#if !DEBUG
        public void Update()
        {
            if (!_hasEjected && Mathf.Abs(EjectorLever.transform.localPosition.y - LeverOpen) <= WiggleRoom && LaserRifle.Magazine != null)
            {
                _hasEjected = true;
                FVRFireArmMagazine mag = LaserRifle.Magazine;
                LaserRifle.ReleaseMag();
                mag.RootRigidbody.velocity = EjectionDirection.forward * EjectionForce;
                mag.RootRigidbody.angularVelocity = LaserRifle.transform.right * EjectionSpin.x + LaserRifle.transform.up * EjectionSpin.y + LaserRifle.transform.forward * EjectionSpin.z;
            }

            if (!EjectorLever.IsHeld && LaserRifle.Magazine != null)
            {
                _hasEjected = false;
                EjectorLever.transform.localPosition = new Vector3(EjectorLever.transform.localPosition.x, LeverClosed, EjectorLever.transform.localPosition.z);
            }
        }
#endif
    }
}
