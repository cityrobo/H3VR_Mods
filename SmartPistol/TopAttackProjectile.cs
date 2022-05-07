using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class TopAttackProjectile : MonoBehaviour
    {
        public BallisticProjectile Projectile;
        [HideInInspector]
        //public Vector3 TargetPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

        public float MaxRangeOverride = 10000f;
        [HideInInspector]
        public Vector3? TargetPoint = null;
        [HideInInspector]
        public Rigidbody TargetRB;

        public float TurningSpeed = 45f;
        public float LaunchDelay = 0.5f;
        public float MaximumFlightSpeed = 150f;
        public float AccelerationWithRocketEngaged = 150f;

        public enum EAttackMode
        {
            Top,
            Direct
        }

        public EAttackMode AttackMode;

        //public float MaxHeightInFrontalAttack = 60f;

        public float TopAttackLaunchAngle = 60f;
        public float TopAttackAttackAngle = 45f;
        public float DirectAttackLaunchAngle = 45f;
        public float DirectAttackAttackAngle = 1.75f;

        public float MaxHeightInTopAttack = 160f;
        private enum AttackState
        {
            ClimbOut,
            AltitudeHold,
            Terminal
        }

        private AttackState _attackState;

        private float _startingDistanceToTarget;
        private float _startingHeight;
        private Vector3 _startingPosition;

        private Vector3 _targetDirection;
        private float _targetHeight;
        private bool _doesAltitudeHolt = false;

        private float _altitudeHoldMaxDistance;
        private float _altitudeHoldCurrentDistance;

        private bool _debug = true;

        private float _launchDelayTick = 0f;
        private bool _rocketEngaged = false;
#if !DEBUG

        public void Start()
        {
            Projectile.ForceSetMaxDist(MaxRangeOverride);
        }

        public void Update()
        {
            Vector3 TargetPos = Vector3.zero;
            if (TargetRB != null) TargetPos = TargetRB.position;
            else if (TargetPoint != null) TargetPos = TargetPoint.Value;
            else Destroy(this);

            if (!_rocketEngaged && _launchDelayTick >= LaunchDelay)
            {
                CalculateLaunchProfile();

                _rocketEngaged = true;
                if (Projectile.ExtraDisplay != null) Projectile.ExtraDisplay.SetActive(true);
                Projectile.GravityMultiplier = 0f;
            }
            else if (!_rocketEngaged)
            {
                Projectile.GravityMultiplier = 1f;
                if (Projectile.ExtraDisplay != null) Projectile.ExtraDisplay.SetActive(false);
            }

            if (AttackMode == EAttackMode.Top && _rocketEngaged)
            {
                if (_doesAltitudeHolt && _attackState == AttackState.ClimbOut && Projectile.transform.position.y >= _targetHeight + _startingHeight)
                {
                    _attackState = AttackState.AltitudeHold;

                    //_targetDirection = Quaternion.AngleAxis(TopAttackLaunchAngle, Projectile.transform.right) * Projectile.transform.forward;
                    _targetDirection = Vector3.ProjectOnPlane(TargetPos - Projectile.transform.position, Vector3.up);
                }
                else if (!_doesAltitudeHolt && _attackState == AttackState.ClimbOut && Projectile.transform.position.y >= _targetHeight + _startingHeight)
                {
                    _attackState = AttackState.Terminal;
                }

                if (_attackState == AttackState.AltitudeHold)
                {
                    //if (_altitudeHoldCurrentDistance >= _altitudeHoldMaxDistance) _attackState = AttackState.Terminal;
                    if (Vector3.ProjectOnPlane(Projectile.transform.position - _startingPosition,Vector3.up).magnitude >= _altitudeHoldMaxDistance) _attackState = AttackState.Terminal;
                }
            }
            else if (AttackMode == EAttackMode.Direct && _rocketEngaged)
            {
                if (_attackState == AttackState.ClimbOut && Projectile.transform.position.y >= _targetHeight + _startingHeight)
                {
                    _attackState = AttackState.Terminal;
                }
            }
        }

		public void FixedUpdate()
        {
            _launchDelayTick += Time.fixedDeltaTime;

            Vector3 TargetPos = Vector3.zero;
            if (TargetRB != null) TargetPos = TargetRB.position;
            else if (TargetPoint != null) TargetPos = TargetPoint.Value;
            else Destroy(this);

            if (_rocketEngaged && !Projectile.hasTurnedOffRends && Projectile.m_velocity.magnitude < MaximumFlightSpeed)
            {
                Projectile.m_velocity = Projectile.m_velocity + Projectile.m_velocity.normalized * AccelerationWithRocketEngaged * Time.fixedDeltaTime;
            }


            Quaternion flightRotation;
            if (Projectile.m_velocity.magnitude == 0) flightRotation = Quaternion.LookRotation(Projectile.transform.forward);
            else flightRotation = Quaternion.LookRotation(Projectile.m_velocity);
            if (AttackMode == EAttackMode.Top)
            {
                if ((_attackState == AttackState.ClimbOut || _attackState == AttackState.AltitudeHold) && _rocketEngaged)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                    Projectile.m_velocity = deltaRotation * Projectile.m_velocity;
                }
                else if (_attackState == AttackState.Terminal && _rocketEngaged)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(TargetPos - Projectile.transform.position);
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation); 
                    Projectile.m_velocity = deltaRotation * Projectile.m_velocity;
                }
            }
            else
            {
                if (_attackState == AttackState.ClimbOut && _rocketEngaged)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                    Projectile.m_velocity = deltaRotation * Projectile.m_velocity;
                }
                else if (_attackState == AttackState.Terminal && _rocketEngaged)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(TargetPos - Projectile.transform.position);
                    Quaternion deltaRotation = Quaternion.RotateTowards(flightRotation, targetRotation, TurningSpeed * Time.fixedDeltaTime) * Quaternion.Inverse(flightRotation);
                    Projectile.m_velocity = deltaRotation * Projectile.m_velocity;
                }
            }
            
            /*
            if (_attackState == AttackState.AltitudeHold && _rocketEngaged)
            {
                _altitudeHoldCurrentDistance += Projectile.m_velocity.magnitude * Time.fixedDeltaTime;
            }
            */
        }

        void CalculateLaunchProfile()
        {
            float turnRadius = (180 * MaximumFlightSpeed) / (Mathf.PI * TurningSpeed);
            _startingPosition = Projectile.transform.position;
            _startingHeight = Projectile.transform.position.y;


            Vector3 TargetPos = Vector3.zero;

            if (TargetRB != null) TargetPos = TargetRB.position;
            else if (TargetPoint != null) TargetPos = TargetPoint.Value;
            else Destroy(this);

            float _deltaHeight = TargetPos.y - _startingHeight;
            //_startingDistanceToTarget = Vector3.Distance(TargetPos, Projectile.transform.position);

            float _deltaXZ = Vector3.ProjectOnPlane(TargetPos - Projectile.transform.position, Vector3.up).magnitude;


            if (AttackMode == EAttackMode.Top)
            {
                float _heightCorrection = _deltaHeight / Mathf.Tan(TopAttackAttackAngle * Mathf.Deg2Rad);
                _startingDistanceToTarget = _deltaXZ + _heightCorrection;
                float deltaAngle = TopAttackLaunchAngle - Vector3.Angle(Projectile.transform.forward, Vector3.ProjectOnPlane(Projectile.transform.forward, Vector3.up)) * (Projectile.transform.forward.y / Mathf.Abs(Projectile.transform.forward.y));

                _targetDirection = Quaternion.AngleAxis(-deltaAngle, Projectile.transform.right) * Projectile.transform.forward;

                _targetHeight = _startingDistanceToTarget * (Mathf.Tan(TopAttackLaunchAngle * Mathf.Deg2Rad) * Mathf.Tan(TopAttackAttackAngle * Mathf.Deg2Rad)) / (Mathf.Tan(TopAttackLaunchAngle * Mathf.Deg2Rad) + Mathf.Tan(TopAttackAttackAngle * Mathf.Deg2Rad));

                if (_debug) Debug.Log($"First _targetHeight: {_targetHeight}");
                if (_deltaHeight > 0 && _targetHeight > MaxHeightInTopAttack + _deltaHeight)
                {
                    _doesAltitudeHolt = true;
                    _targetHeight = MaxHeightInTopAttack + _deltaHeight;
                }
                else
                if (_targetHeight > MaxHeightInTopAttack)
                {
                    _doesAltitudeHolt = true;
                    _targetHeight = MaxHeightInTopAttack;
                }
                if (_debug) Debug.Log($"_doesAltitudeHolt: {_doesAltitudeHolt}");
                float thirdAngle = 180f - (TopAttackLaunchAngle + TopAttackAttackAngle);
                
                float climbDistance = _targetHeight / Mathf.Sin(TopAttackLaunchAngle * Mathf.Deg2Rad);

                Vector2 highestPointInTriangle = new Vector2(Mathf.Cos(TopAttackLaunchAngle * Mathf.Deg2Rad) * climbDistance, Mathf.Sin(TopAttackLaunchAngle * Mathf.Deg2Rad) * climbDistance);
                float distanceToTurnCenter = turnRadius / Mathf.Sin((thirdAngle / 2) * Mathf.Deg2Rad);

                /*
                Vector2 centerPoint = -highestPointInTriangle.normalized * distanceToTurnCenter;
                centerPoint.x = centerPoint.x * Mathf.Cos((turnAngle / 2) * Mathf.Deg2Rad) - centerPoint.y * Mathf.Sin((turnAngle / 2) * Mathf.Deg2Rad) + highestPointInTriangle.x;
                centerPoint.y = centerPoint.x * Mathf.Sin((turnAngle / 2) * Mathf.Deg2Rad) + centerPoint.y * Mathf.Cos((turnAngle / 2) * Mathf.Deg2Rad) + highestPointInTriangle.y;
                */

                Vector2 tangentPointEnter = highestPointInTriangle - highestPointInTriangle.normalized * distanceToTurnCenter;
                Vector2 tangentPointExit = highestPointInTriangle + Vector2.right * distanceToTurnCenter;

                _targetHeight = tangentPointEnter.y;

                if (_debug)
                {
                    Debug.Log($"turnRadius: {turnRadius}");
                    Debug.Log($"TargetDir: {TargetPos - Projectile.transform.position}");
                    Debug.Log($"deltaAngle: {deltaAngle}");
                    Debug.Log($"_deltaHeight: {_deltaHeight}");
                    Debug.Log($"_deltaXZ: {_deltaXZ}");
                    Debug.Log($"_heightCorrection: {_heightCorrection}");
                    Debug.Log($"_startingDistanceToTarget: {_startingDistanceToTarget}");
                    
                    Debug.Log($"climbDistance: {climbDistance}");
                    Debug.Log($"highestPointInTriangle: {highestPointInTriangle}");
                    Debug.Log($"distanceToTurnCenter: {distanceToTurnCenter}");
                    Debug.Log($"Final _targetHeight: {_targetHeight}");
                    Debug.Log($"tangentPointEnter: {tangentPointEnter}");
                }

                if (_doesAltitudeHolt)
                {
                    float turnDistance = Mathf.PI * turnRadius * ((180 - thirdAngle) / 180);
                    _altitudeHoldCurrentDistance = -turnDistance;
                    float descentDistance;
                    if (_deltaHeight > 0) descentDistance = (MaxHeightInTopAttack + _deltaHeight) / Mathf.Sin(TopAttackAttackAngle * Mathf.Deg2Rad);
                    else descentDistance = MaxHeightInTopAttack / Mathf.Sin(TopAttackAttackAngle * Mathf.Deg2Rad);
                    thirdAngle = 180 - TopAttackAttackAngle;

                    highestPointInTriangle = new Vector2(_deltaXZ, 0) + new Vector2(-Mathf.Cos(TopAttackAttackAngle * Mathf.Deg2Rad) * descentDistance, Mathf.Sin(TopAttackAttackAngle * Mathf.Deg2Rad) * descentDistance);
                    distanceToTurnCenter = turnRadius / Mathf.Sin((thirdAngle / 2) * Mathf.Deg2Rad);

                    Vector2 tangentPointEnter2 = highestPointInTriangle + Vector2.left * distanceToTurnCenter;

                    //_altitudeHoldMaxDistance = Vector2.Distance(tangentPointEnter2, tangentPointExit);
                    _altitudeHoldMaxDistance = tangentPointEnter2.x;

                    if (_debug)
                    {
                        Debug.Log($"turnDistance: {turnDistance}");
                        Debug.Log($"descentDistance: {descentDistance}");
                        Debug.Log($"highestPointInTriangle AltitudeHold: {highestPointInTriangle}");
                        Debug.Log($"tangentPointExit: {tangentPointExit}");
                        Debug.Log($"tangentPointEnter2: {tangentPointEnter2}");
                        Debug.Log($"_startingDistanceToTarget: {_startingDistanceToTarget}");
                        Debug.Log($"_altitudeHoldMaxDistance: {_altitudeHoldMaxDistance}");
                    }
                }


            }
            else
            {
                float _heightCorrection = _deltaHeight / Mathf.Tan(DirectAttackAttackAngle * Mathf.Deg2Rad);
                _startingDistanceToTarget = _deltaXZ + _heightCorrection;

                float deltaAngle = DirectAttackLaunchAngle - Vector3.Angle(Projectile.transform.forward, Vector3.ProjectOnPlane(Projectile.transform.forward, Vector3.up)) * (Projectile.transform.forward.y / Mathf.Abs(Projectile.transform.forward.y)); ;

                _targetDirection = Quaternion.AngleAxis(-deltaAngle, Projectile.transform.right) * Projectile.transform.forward;
                _targetHeight = _startingDistanceToTarget * (Mathf.Tan(DirectAttackLaunchAngle * Mathf.Deg2Rad) * Mathf.Tan(DirectAttackAttackAngle * Mathf.Deg2Rad)) / (Mathf.Tan(DirectAttackLaunchAngle * Mathf.Deg2Rad) + Mathf.Tan(DirectAttackAttackAngle * Mathf.Deg2Rad));

                if (_debug) Debug.Log($"First _targetHeight: {_targetHeight}");
                float thirdAngle = 180f - (DirectAttackLaunchAngle + DirectAttackAttackAngle);
                float climbDistance = _targetHeight / Mathf.Sin(DirectAttackLaunchAngle * Mathf.Deg2Rad);

                Vector2 highestPointInTriangle = new Vector2(Mathf.Cos(DirectAttackLaunchAngle * Mathf.Deg2Rad) * climbDistance, Mathf.Sin(DirectAttackLaunchAngle * Mathf.Deg2Rad) * climbDistance);
                float distanceToTurnCenter = turnRadius / Mathf.Sin(thirdAngle * Mathf.Deg2Rad / 2);
                /*
                Vector2 centerPoint = -highestPointInTriangle.normalized * distanceToTurnCenter;
                centerPoint.x = centerPoint.x * Mathf.Cos(turnAngle * Mathf.Deg2Rad / 2) - centerPoint.y * Mathf.Sin(turnAngle * Mathf.Deg2Rad / 2) + highestPointInTriangle.x;
                centerPoint.y = centerPoint.x * Mathf.Sin(turnAngle * Mathf.Deg2Rad / 2) + centerPoint.y * Mathf.Cos(turnAngle * Mathf.Deg2Rad / 2) + highestPointInTriangle.y;
                */
                Vector2 tangentPointEnter = highestPointInTriangle - highestPointInTriangle.normalized * distanceToTurnCenter;

                _targetHeight = tangentPointEnter.y;

                if (_debug)
                {
                    Debug.Log($"turnRadius: {turnRadius}");
                    Debug.Log($"TargetDir: {TargetPos - Projectile.transform.position}");
                    Debug.Log($"deltaAngle: {deltaAngle}");
                    Debug.Log($"_deltaHeight: {_deltaHeight}");
                    Debug.Log($"_deltaXZ: {_deltaXZ}");
                    Debug.Log($"_heightCorrection: {_heightCorrection}");
                    Debug.Log($"_startingDistanceToTarget: {_startingDistanceToTarget}");

                    Debug.Log($"climbDistance: {climbDistance}");
                    Debug.Log($"highestPointInTriangle: {highestPointInTriangle}");
                    Debug.Log($"distanceToTurnCenter: {distanceToTurnCenter}");
                    Debug.Log($"Final _targetHeight: {_targetHeight}");
                    Debug.Log($"tangentPointEnter: {tangentPointEnter}");
                }
            }
        }
#endif
	}
}
