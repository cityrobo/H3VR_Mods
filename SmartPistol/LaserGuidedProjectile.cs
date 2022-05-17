using FistVR;
using System;
using System.Collections;
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
        public float TurnAngleLimit = 45f;

        [Tooltip("Doesn't work with AngleLimit cause the constant correction causes flown angles to add up quickly in target approach.")]
        public bool UsesInheritInaccuracy = true;
        public float AccuracyCircleRadius = 0.3f;
        public bool ScalesWithDistance = true;
        public float InaccuracyScaleOverDistanceFactor = 1.25f;
        public float BulletSwaySpeed = 0.5f;

        private float _turnedAngle = 0f;
        private Vector3 _curTarget;
        private Vector2 _currentInaccuracy = Vector2.zero;

#if !DEBUG
        public void Awake()
        {
             if(UsesInheritInaccuracy) _currentInaccuracy = UnityEngine.Random.insideUnitCircle * AccuracyCircleRadius;
        }


        public void Update()
        {
            if (UsesInheritInaccuracy)
            {
                Vector2 inaccuracy;
                inaccuracy = UnityEngine.Random.insideUnitCircle * AccuracyCircleRadius * BulletSwaySpeed * Time.deltaTime;

                float distanceFactor = 0f;
                if (ScalesWithDistance)
                {
                    float distance = Vector3.Distance(_curTarget, Projectile.transform.position);

                    if (distance > 1f)
                    {
                        distanceFactor = Mathf.Pow((distance - 1f) * (InaccuracyScaleOverDistanceFactor - 1f), 2f);
                        inaccuracy *= distanceFactor;
                    }
                }

                if (_currentInaccuracy.magnitude > AccuracyCircleRadius * (1f + distanceFactor)) _currentInaccuracy = _currentInaccuracy.normalized * AccuracyCircleRadius * (1f + distanceFactor);
                if ((inaccuracy + _currentInaccuracy).magnitude < AccuracyCircleRadius * (1f + distanceFactor))
                {
                    _currentInaccuracy += inaccuracy;
                }
            }
        }

		public void FixedUpdate()
        {
            Vector3 m_velocity = Projectile.m_velocity;
            Quaternion flightRotation;
            if (m_velocity.magnitude == 0) flightRotation = Quaternion.LookRotation(Projectile.transform.forward);
            else if (m_velocity.normalized != Vector3.up) flightRotation = Quaternion.LookRotation(m_velocity, Vector3.up);
            else flightRotation = Quaternion.LookRotation(m_velocity, Vector3.right);

            if ((!HasLimitedTurnAngle || _turnedAngle < TurnAngleLimit) && LaserGuidanceSystem.LaserTargets.Count != 0)
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
                    _curTarget = validLaserTargets[UnityEngine.Random.Range(0, validLaserTargets.Count)];

                    Vector3 targetCorrected = _curTarget;
                    targetCorrected += flightRotation * _currentInaccuracy;
                    Quaternion targetRotation = Quaternion.LookRotation(targetCorrected - Projectile.transform.position);
                    
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                    Projectile.m_velocity = deltaRotation * m_velocity;

                    if (!UsesInheritInaccuracy)_turnedAngle += Vector3.Angle(Projectile.m_velocity, m_velocity);
                }
            }
        }
#endif
	}
}
