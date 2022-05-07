using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class SmartAttachment : MonoBehaviour
    {
        public FVRFireArmAttachment Attachment;

		[Header("Weapon Config")]
		public float EngageRange = 15f;
		[Range(1f, 179f)]
		public float EngageAngle = 45f;
		public float PrecisionAngle = 5f;

		public LayerMask LatchingMask;
		public LayerMask BlockingMask;

		public bool DoesRandomRotationOfBarrelForCinematicBulletTrails = true;
		public float RandomAngleMagnitude = 5f;

		[Tooltip("Use this if you want the last target to stay locked on for a certain period. Good for shooting around corners!")]
		public float LastTargetTimeout = 1f;

		[Tooltip("Makes Bullets slower so that they can arc better than with max speed. Compensates with mass so damage should be about the same.")]
		public float BulletVelocityModifier = 0.1f;
		[Header("Projectile Config")]
		[Tooltip("Puts smart projectile component on every projectile leaving the muzzle.")]
		public bool ActivatesBullets = true;
		public float TurningSpeed = 180f;

		public bool HasLimitedTurnAngle = false;
		public float TurnAngleLimit = 45f;

		[Tooltip("Doesn't work with AngleLimit cause the constant correction causes flown angles to add up quickly in target approach.")]
		public bool UsesInheritInaccuracy = true;
		public float AccuracyCircleRadius = 0.15f;
		public bool ScalesWithDistance = true;
		public float InaccuracyScaleOverDistanceFactor = 1.25f;
		public float BulletSwaySpeed = 0.5f;


		private bool _isAttached = false;
		private FVRFireArm _fireArm;
		private SmartWeapon _smartWeapon = null;
#if !(DEBUG || MEATKIT)
		public void Update()
        {
			if (!_isAttached && Attachment.curMount != null && Attachment.curMount.GetRootMount().MyObject is FVRFireArm)
            {
				
				_fireArm = Attachment.curMount.GetRootMount().MyObject as FVRFireArm;
				_smartWeapon = _fireArm.GetComponent<SmartWeapon>();
				if (_smartWeapon == null)
				{
					_fireArm.gameObject.SetActive(false);
					_smartWeapon = _fireArm.gameObject.AddComponent<SmartWeapon>();
					_smartWeapon.FireArm = _fireArm;
					_smartWeapon.EngageRange = EngageRange;
					_smartWeapon.EngageAngle = EngageAngle;
					_smartWeapon.PrecisionAngle = PrecisionAngle;
					_smartWeapon.LatchingMask = LatchingMask;
					_smartWeapon.BlockingMask = BlockingMask;
					_smartWeapon.DoesRandomRotationOfBarrelForCinematicBulletTrails = DoesRandomRotationOfBarrelForCinematicBulletTrails;
					_smartWeapon.RandomAngleMagnitude = RandomAngleMagnitude;
					_smartWeapon.LastTargetTimeout = LastTargetTimeout;

					_smartWeapon.WasManuallyAdded = ActivatesBullets;
					_smartWeapon.BulletVelocityModifier = BulletVelocityModifier;
					SmartProjectile.SmartProjectileData smartProjectileData = new SmartProjectile.SmartProjectileData();

					smartProjectileData.TurningSpeed = TurningSpeed;
					smartProjectileData.HasLimitedTurnAngle = HasLimitedTurnAngle;
					smartProjectileData.TurnAngleLimit = TurnAngleLimit;
					smartProjectileData.UsesInheritInaccuracy = UsesInheritInaccuracy;
					smartProjectileData.AccuracyCircleRadius = AccuracyCircleRadius;
					smartProjectileData.ScalesWithDistance = ScalesWithDistance;
					smartProjectileData.InaccuracyScaleOverDistanceFactor = InaccuracyScaleOverDistanceFactor;
					smartProjectileData.BulletSwaySpeed = BulletSwaySpeed;

					_smartWeapon.ProjectileData = smartProjectileData;

					_isAttached = true;
					_fireArm.gameObject.SetActive(true);
				}
			}
			else if (_isAttached && Attachment.curMount == null && _fireArm != null)
            {
				_isAttached = false;

				Destroy(_smartWeapon);
				_fireArm = null;
            }
        }

#endif
    }
}
