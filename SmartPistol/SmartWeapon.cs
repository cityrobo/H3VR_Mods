using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class SmartWeapon : MonoBehaviour
    {
        public FVRFireArm FireArm;
		public MeshRenderer ReticleMesh;
		public bool DisableReticleWithoutTarget = true;
		public float EngageRange = 15f;
		[Range(1f,179f)]
		public float EngageAngle = 45f;
		public float PrecisionAngle = 5f;

		public LayerMask LatchingMask;
		public LayerMask BlockingMask;

		public bool DoesRandomRotationOfBarrelForCinematicBulletTrails = true;
		public float RandomAngleMagnitude = 5f;

		[Tooltip("Use this if you want the last target to stay locked on for a certain period. good for shooting around corners!")]
		public float LastTargetTimeout = 1f;
		//constants
		private const string _nameOfDistanceVariable = "_RedDotDist";

		private Rigidbody _lastTarget;

		private GameObject _origMuzzlePos;

		private bool _timeoutStarted = false;

#if !(DEBUG || MEATKIT)
		public void Awake()
        {
			Hook();

			_origMuzzlePos = Instantiate(FireArm.MuzzlePos.gameObject, this.transform);
			_origMuzzlePos.transform.localPosition = FireArm.MuzzlePos.localPosition;
			_origMuzzlePos.transform.localRotation = FireArm.MuzzlePos.localRotation;
        }
		public void OnDestroy()
        {
			Unhook();
        }

		public void Unhook()
        {
			On.FistVR.FVRFireArm.Fire -= FVRFireArm_Fire;
		}
		public void Hook()
        {
            On.FistVR.FVRFireArm.Fire += FVRFireArm_Fire;
        }

        private void FVRFireArm_Fire(On.FistVR.FVRFireArm.orig_Fire orig, FVRFireArm self, FVRFireArmChamber chamber, Transform muzzle, bool doBuzz, float velMult, float rangeOverride)
        {
			if (self == FireArm)
			{
				if (doBuzz && self.m_hand != null)
				{
					self.m_hand.Buzz(self.m_hand.Buzzer.Buzz_GunShot);
					if (self.AltGrip != null && self.AltGrip.m_hand != null)
					{
						self.AltGrip.m_hand.Buzz(self.m_hand.Buzzer.Buzz_GunShot);
					}
				}
				GM.CurrentSceneSettings.OnShotFired(self);
				if (self.IsSuppressed())
				{
					GM.CurrentPlayerBody.VisibleEvent(0.1f);
				}
				else
				{
					GM.CurrentPlayerBody.VisibleEvent(2f);
				}
				float chamberVelMult = AM.GetChamberVelMult(chamber.RoundType, Vector3.Distance(chamber.transform.position, muzzle.position));
				float num = self.GetCombinedFixedDrop(self.AccuracyClass) * 0.0166667f;
				Vector2 vector = self.GetCombinedFixedDrift(self.AccuracyClass) * 0.0166667f;
				for (int i = 0; i < chamber.GetRound().NumProjectiles; i++)
				{
					float d = chamber.GetRound().ProjectileSpread + self.m_internalMechanicalMOA + self.GetCombinedMuzzleDeviceAccuracy();
					if (chamber.GetRound().BallisticProjectilePrefab != null)
					{
						Vector3 b = muzzle.forward * 0.005f;
						GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(chamber.GetRound().BallisticProjectilePrefab, muzzle.position - b, muzzle.rotation);
						Vector2 vector2 = (UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle) * 0.33333334f * d;
						gameObject.transform.Rotate(new Vector3(vector2.x + vector.y + num, vector2.y + vector.x, 0f));
						BallisticProjectile component = gameObject.GetComponent<BallisticProjectile>();
						component.Fire(component.MuzzleVelocityBase * chamber.ChamberVelocityMultiplier * velMult * chamberVelMult, gameObject.transform.forward, self, true);

						SmartProjectile smartProjectile = gameObject.GetComponent<SmartProjectile>();
                        if (smartProjectile != null)
                        {
							smartProjectile.TargetRB = _lastTarget;
                        }
						if (rangeOverride > 0f)
						{
							component.ForceSetMaxDist(rangeOverride);
						}
					}
				}
			}
			else orig(self, chamber, muzzle, doBuzz, velMult, rangeOverride);
        }

        public void Update()
        {
            if (FireArm.IsHeld)
            {
				Rigidbody _target = FindTarget();

				if (_target != null)
                {
					//Debug.Log(target);
					if (_timeoutStarted && LastTargetTimeout != 0f) StopCoroutine("LastTargetTimeoutCoroutine");
					_lastTarget = _target;
				}
				else
                {
					if (!_timeoutStarted && LastTargetTimeout != 0f)
					{
						StopCoroutine("LastTargetTimeoutCoroutine");
						StartCoroutine("LastTargetTimeoutCoroutine");
					}

					if (DisableReticleWithoutTarget && ReticleMesh != null) ReticleMesh.gameObject.SetActive(false);
				}

				if (_lastTarget != null && ReticleMesh != null)
                {
					ReticleMesh.transform.LookAt(_lastTarget.position);
					ReticleMesh.material.SetFloat(_nameOfDistanceVariable, Vector3.Distance(_lastTarget.position, ReticleMesh.transform.position));
					if (DisableReticleWithoutTarget) ReticleMesh.gameObject.SetActive(true);
				}
				else if (_lastTarget == null && ReticleMesh != null)
                {
					ReticleMesh.transform.localRotation = Quaternion.identity;

					if (DisableReticleWithoutTarget) ReticleMesh.gameObject.SetActive(false);
				}

				if (DoesRandomRotationOfBarrelForCinematicBulletTrails)
                {
					Vector3 randRot = new Vector3();
					randRot.x = UnityEngine.Random.Range(-RandomAngleMagnitude, RandomAngleMagnitude);
					randRot.y = UnityEngine.Random.Range(-RandomAngleMagnitude, RandomAngleMagnitude);

					FireArm.CurrentMuzzle.localEulerAngles = randRot;
				}
			}
        }

		private IEnumerator LastTargetTimeoutCoroutine()
        {
			_timeoutStarted = true;
			yield return new WaitForSeconds(LastTargetTimeout);
			_lastTarget = null;
        }

		private Rigidbody FindTarget()
        {
			float radius = EngageRange * Mathf.Tan(0.5f * EngageAngle * Mathf.Deg2Rad);
			Collider[] colliderArray = Physics.OverlapCapsule(FireArm.CurrentMuzzle.position, FireArm.CurrentMuzzle.position + _origMuzzlePos.transform.forward * EngageRange, radius, LatchingMask);
			List<Rigidbody> rigidbodyList = new List<Rigidbody>();
			for (int i = 0; i < colliderArray.Length; i++)
			{
				if (colliderArray[i].attachedRigidbody != null && !rigidbodyList.Contains(colliderArray[i].attachedRigidbody))
				{
					rigidbodyList.Add(colliderArray[i].attachedRigidbody);
				}
			}
			SosigLink targetSosigLink = null;
			SosigLink tempSosigLink = null;
			float minAngle = EngageAngle;
			for (int j = 0; j < rigidbodyList.Count; j++)
			{
				SosigLink component = rigidbodyList[j].GetComponent<SosigLink>();

				if (component != null && component.S.BodyState != Sosig.SosigBodyState.Dead)
				{
					if (true || component.S.E.IFFCode == 1)
					{
						Vector3 from = rigidbodyList[j].transform.position - FireArm.CurrentMuzzle.position;
						float angle = Vector3.Angle(from, _origMuzzlePos.transform.forward);

						Sosig s = component.S;
						if (angle <= PrecisionAngle) tempSosigLink = s.Links[0];
						else tempSosigLink = s.Links[1];

						if (angle < minAngle && !Physics.Linecast(FireArm.CurrentMuzzle.position, tempSosigLink.transform.position, BlockingMask, QueryTriggerInteraction.Ignore))
						{
							targetSosigLink = tempSosigLink;
							minAngle = angle;
						}
					}
				}

			}
			return targetSosigLink.R;
		}
#endif
	}
}
