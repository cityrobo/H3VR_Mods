using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class LaserGuidedProjectile : MonoBehaviour
    {
        public BallisticProjectile Projectile;

        public float TurningSpeed = 45f;
        public float TargetingConeAngle = 180f;

        public bool HasLimitedTurnAngle = true;
        public float AngleLimit = 45f;

        private float _turnedAngle = 0f;

#if !DEBUG
		public void FixedUpdate()
        {
            Vector3 m_velocity = Projectile.m_velocity;
            Quaternion flightRotation = Quaternion.LookRotation(Projectile.transform.forward);
            if ((!HasLimitedTurnAngle || _turnedAngle < AngleLimit) && LaserGuidanceSystem.LaserTargets.Count != 0)
            {
                List<Vector3> validLaserTargets = new List<Vector3>();
                foreach (var laserTarget in LaserGuidanceSystem.LaserTargets)
                {
                    Vector3 direction = laserTarget - Projectile.transform.position;

                    float angle = Vector3.Angle(direction, Projectile.transform.forward);

                    if (angle <= TargetingConeAngle) validLaserTargets.Add(laserTarget);
                }
                if (validLaserTargets.Count != 0)
                {
                    Vector3 TargetPoint = validLaserTargets[UnityEngine.Random.Range(0, validLaserTargets.Count)];

                    Quaternion targetRotation = Quaternion.LookRotation(TargetPoint - Projectile.transform.position);
                    
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                    Projectile.m_velocity = deltaRotation * m_velocity;

                    _turnedAngle += Quaternion.Angle(flightRotation, Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime));
                }
            }
        }
#endif
	}
}
