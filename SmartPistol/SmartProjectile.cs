using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class SmartProjectile : MonoBehaviour
    {
        public BallisticProjectile Projectile;
        [HideInInspector]
        public Vector3 TargetPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        [HideInInspector]
        public Rigidbody TargetRB;

        public float TurningSpeed = 45f;

#if !DEBUG
		public void FixedUpdate()
        {
            Vector3 m_velocity = Projectile.m_velocity;
            Quaternion flightRotation = Quaternion.LookRotation(Projectile.transform.forward);
            if (TargetPoint != new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
            {
                Quaternion targetRotation = Quaternion.LookRotation(TargetPoint - Projectile.transform.position);
                Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                Projectile.m_velocity = deltaRotation * m_velocity;
            }
            else if (TargetRB != null)
            {
                Quaternion targetRotation = Quaternion.LookRotation(TargetRB.position - Projectile.transform.position);
                Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                Projectile.m_velocity = deltaRotation * m_velocity;
            }
        }
#endif
	}
}
