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
        public Vector3? TargetPoint = null;
        [HideInInspector]
        public Rigidbody TargetRB = null;
        [HideInInspector]
        public SosigLink TargetLink = null;
        public float TurningSpeed = 180f;

        public bool HasLimitedTurnAngle = false;
        public float TurnAngleLimit = 45f;

        [Tooltip("Doesn't work with AngleLimit cause the constant correction causes flown angles to add up quickly in target approach.")]
        public bool UsesInheritInaccuracy = true;
        public float AccuracyCircleRadius = 0.15f;
        public bool ScalesWithDistance = true;
        public float InaccuracyScaleOverDistanceFactor = 1.25f;
        public float BulletSwaySpeed = 0.5f;

        public class SmartProjectileData
        {
            public float TurningSpeed = 180f;

            public bool HasLimitedTurnAngle = false;
            public float TurnAngleLimit = 45f;

            [Tooltip("Doesn't work with AngleLimit cause the constant correction causes flown angles to add up quickly in target approach.")]
            public bool UsesInheritInaccuracy = true;
            public float AccuracyCircleRadius = 0.15f;
            public bool ScalesWithDistance = true;
            public float InaccuracyScaleOverDistanceFactor = 1.25f;
            public float BulletSwaySpeed = 0.5f;
        }


        private float _turnedAngle = 0f;
        private Vector3 _curTarget;
        private Vector2 _currentInaccuracy = Vector2.zero;

#if !DEBUG
        public void Awake()
        {
            if (TargetLink != null) _curTarget = TargetLink.transform.position;
            else if (TargetRB != null) _curTarget = TargetRB.position;
            else if (TargetPoint != null) _curTarget = TargetPoint.Value;
            else Destroy(this);
            if (UsesInheritInaccuracy) _currentInaccuracy = UnityEngine.Random.insideUnitCircle * AccuracyCircleRadius;
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
            if (_turnedAngle < TurnAngleLimit)
            {
                Vector3 m_velocity = Projectile.m_velocity;
                Quaternion flightRotation;
                if (m_velocity.magnitude == 0) flightRotation = Quaternion.LookRotation(Projectile.transform.forward);
                else if (m_velocity.normalized != Vector3.up) flightRotation = Quaternion.LookRotation(m_velocity, Vector3.up);
                else flightRotation = Quaternion.LookRotation(m_velocity, Vector3.right);

                _curTarget = Vector3.zero;
                if (TargetLink != null) _curTarget = TargetLink.transform.position;
                else if (TargetRB != null) _curTarget = TargetRB.position;
                else if (TargetPoint != null) _curTarget = TargetPoint.Value;
                else Destroy(this);

                Vector3 targetCorrected = _curTarget;
                targetCorrected += flightRotation * _currentInaccuracy;
                Quaternion targetRotation = Quaternion.LookRotation(targetCorrected - Projectile.transform.position);

                Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                Projectile.m_velocity = deltaRotation * m_velocity;

                if (!UsesInheritInaccuracy) _turnedAngle += Vector3.Angle(Projectile.m_velocity, m_velocity);
            }
        }

        public void ConfigureFromData(SmartProjectileData smartProjectileData)
        {
            this.TurningSpeed = smartProjectileData.TurningSpeed;
            this.HasLimitedTurnAngle = smartProjectileData.HasLimitedTurnAngle;
            this.TurnAngleLimit = smartProjectileData.TurnAngleLimit;
            this.UsesInheritInaccuracy = smartProjectileData.UsesInheritInaccuracy;
            this.AccuracyCircleRadius = smartProjectileData.AccuracyCircleRadius;
            this.ScalesWithDistance = smartProjectileData.ScalesWithDistance;
            this.InaccuracyScaleOverDistanceFactor = smartProjectileData.InaccuracyScaleOverDistanceFactor;
            this.BulletSwaySpeed = smartProjectileData.BulletSwaySpeed;
        }
#endif
    }
}
