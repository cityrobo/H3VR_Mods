using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
	public class Javelin : MonoBehaviour
	{
		public RGM40 FireArm;

		public LayerMask LM_OverlapCapsuleTargetMask;
		public LayerMask LM_RaycastTargetMask;
		public LayerMask LM_BlockMask;
		public float MaxRange = 2000f;
		public float ObjectTargetingFOV = 1f;

		private Vector3? _targetPoint = null;
		private Rigidbody _targetRB;

		public TopAttackProjectile.EAttackMode AttackMode;

		public float MinRangeTopAttackMode = 150f;
		public float MinRangeDirectAttackMode = 65f;

		public Text RangeTextField;
		public string OutOfRangeText = "INF";

		public Camera VisibleCamera;
		public Camera IRCamera;
		public Camera SeekCamera;

		public GameObject MissileColliders;

		public MeshRenderer DayIcon;
		public MeshRenderer WFOVIcon;
		public MeshRenderer NFOVIcon;
		public MeshRenderer SeekIcon;
		public MeshRenderer TopAttackIcon;
		public MeshRenderer DirectAttackIcon;
		public MeshRenderer MissileLoadedIcon;

		public AudioSource AudSource_TargetSound;
		public AudioClip AudClip_Targetting;
		public AudioClip AudClip_TargetLock;

		public Color[] IconColors = new Color[] { new Color(0.3f, 0.3f, 0.3f, 1f), new Color(0, 1f, 0f, 1f), new Color(1f, 0.5f, 0f, 1) };

		private enum EVisionMode
		{
			Day,
			WFOV,
			NFOV,
			Seek
		}

		private EVisionMode VisionMode;
		private float _overlapCapsuleRadius;

		private RaycastHit _raycastHit;
		private Collider[] _targetArray = new Collider[32];
		

#if !(DEBUG || MEATKIT)

		public void Awake()
		{
			Hook();

			InitializeIcons();
			JavelinScript_BepInEx.MaxRange.SettingChanged += SettingsChanged;
            JavelinScript_BepInEx.MinRangeTopAttackMode.SettingChanged += SettingsChanged;
			JavelinScript_BepInEx.MinRangeDirectAttackMode.SettingChanged += SettingsChanged;

			MaxRange = JavelinScript_BepInEx.MaxRange.Value;
			MinRangeTopAttackMode = JavelinScript_BepInEx.MinRangeTopAttackMode.Value;
			MinRangeDirectAttackMode = JavelinScript_BepInEx.MinRangeDirectAttackMode.Value;

			VisibleCamera.fieldOfView = 53.6f * Mathf.Pow(4f, -0.9364f) - 0.3666f;

			_overlapCapsuleRadius = Mathf.Tan(ObjectTargetingFOV * Mathf.Deg2Rad) * MaxRange;
			AudSource_TargetSound.Stop();
		}

        public void OnDestroy()
		{
			Unhook();
			JavelinScript_BepInEx.MaxRange.SettingChanged -= SettingsChanged;
			JavelinScript_BepInEx.MinRangeTopAttackMode.SettingChanged -= SettingsChanged;
			JavelinScript_BepInEx.MinRangeDirectAttackMode.SettingChanged -= SettingsChanged;
		}

		private void SettingsChanged(object sender, EventArgs e)
		{
			MaxRange = JavelinScript_BepInEx.MaxRange.Value;
			MinRangeTopAttackMode = JavelinScript_BepInEx.MinRangeTopAttackMode.Value;
			MinRangeDirectAttackMode = JavelinScript_BepInEx.MinRangeDirectAttackMode.Value;
		}

		private void InitializeIcons()
        {
			TopAttackIcon.material.color = IconColors[1];
			DirectAttackIcon.material.color = IconColors[0];

			SeekIcon.material.color = IconColors[0];
			MissileLoadedIcon.material.color = IconColors[2];
		}

		public void Update()
        {
			int numTargets = Physics.OverlapCapsuleNonAlloc(transform.position, transform.position + MaxRange * transform.forward, _overlapCapsuleRadius, _targetArray ,LM_OverlapCapsuleTargetMask, QueryTriggerInteraction.Collide);

			float distance = MaxRange + 100f;

			Collider finalTarget = null;
			Vector3 direction;
			if (VisionMode == EVisionMode.Seek)
			{
				for (int i = 0; i < numTargets; i++)
				{
					direction = _targetArray[i].transform.position - transform.position;

					if (Vector3.Angle(direction, transform.forward) > ObjectTargetingFOV) continue;
					if (direction.magnitude < distance && !Physics.Linecast(transform.position, _targetArray[i].transform.position, LM_BlockMask))
					{
						distance = direction.magnitude;

						finalTarget = _targetArray[i];
					}
				}
			}

			if (finalTarget != null)
			{
				_targetRB = finalTarget.attachedRigidbody;
			}
            else
            {
				_targetRB = null;

			}

			if (Physics.Raycast(transform.position, transform.forward, out _raycastHit, MaxRange, LM_RaycastTargetMask, QueryTriggerInteraction.Ignore))
			{
				RangeTextField.text = string.Format("{0:F0}", _raycastHit.distance);

				if (AttackMode == TopAttackProjectile.EAttackMode.Top && _raycastHit.distance > MinRangeTopAttackMode)
				{
					_targetPoint = _raycastHit.point;
				}
				else if (AttackMode == TopAttackProjectile.EAttackMode.Direct && _raycastHit.distance > MinRangeDirectAttackMode)
				{
					_targetPoint = _raycastHit.point;
				}
				else
				{
					_targetPoint = null;
				}
			}
			else
			{
				RangeTextField.text = OutOfRangeText;

				_targetPoint = null;
				_targetRB = null;
			}
			/*
			if (_targetRB != null) SeekIcon.material.color = IconColors[1];
			else SeekIcon.material.color = IconColors[0];
			*/

			if (FireArm.Chamber.IsFull)
			{
				MissileColliders.SetActive(true);
			}
            else
            {
				MissileColliders.SetActive(false);
			}

			if (FireArm.Chamber.IsFull && !FireArm.Chamber.IsSpent)
			{
				MissileLoadedIcon.material.color = IconColors[0];
			}
			else
			{
				MissileLoadedIcon.material.color = IconColors[2];

				if (VisionMode == EVisionMode.Seek)
				{
					VisionMode = EVisionMode.Day;

					DayIcon.material.color = IconColors[1];
					WFOVIcon.material.color = IconColors[0];
					NFOVIcon.material.color = IconColors[0];
					SeekIcon.material.color = IconColors[0];

					VisibleCamera.gameObject.SetActive(true);
					SeekCamera.gameObject.SetActive(false);
				}
			}

			if (VisionMode == EVisionMode.Seek)
			{
				if (_targetRB != null) AudSource_TargetSound.clip = AudClip_TargetLock;
				else AudSource_TargetSound.clip = AudClip_Targetting;
				if (!AudSource_TargetSound.isPlaying) AudSource_TargetSound.Play();
			}
			else if (AudSource_TargetSound.isPlaying) AudSource_TargetSound.Stop();
		}
		public void Unhook()
		{
			On.FistVR.FVRFireArm.Fire -= FVRFireArm_Fire; 
			On.FistVR.FVRPhysicalObject.UpdateInteraction -= FVRPhysicalObject_UpdateInteraction;
		}
		public void Hook()
		{
			On.FistVR.FVRFireArm.Fire += FVRFireArm_Fire;
            On.FistVR.FVRPhysicalObject.UpdateInteraction += FVRPhysicalObject_UpdateInteraction;
		}

        private void FVRPhysicalObject_UpdateInteraction(On.FistVR.FVRPhysicalObject.orig_UpdateInteraction orig, FVRPhysicalObject self, FVRViveHand hand)
        {
            orig(self, hand);
			if (self == FireArm)
            {
                if (!hand.IsInStreamlinedMode)
                {
					if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f)
					{
						ChangeAttackMode();
					}
					else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f)
					{
						ChangeVisionMode();
					}
				}
                else
                {
					if (hand.Input.AXButtonDown)
					{
						ChangeAttackMode();
					}
					else if (hand.Input.BYButtonDown)
					{
						ChangeVisionMode();
					}
				}
			}
        }

		private void ChangeAttackMode()
        {
			switch (AttackMode)
			{
				case TopAttackProjectile.EAttackMode.Top:
					AttackMode = TopAttackProjectile.EAttackMode.Direct;
					TopAttackIcon.material.color = IconColors[0];
					DirectAttackIcon.material.color = IconColors[1];
					break;
				case TopAttackProjectile.EAttackMode.Direct:
					AttackMode = TopAttackProjectile.EAttackMode.Top;
					TopAttackIcon.material.color = IconColors[1];
					DirectAttackIcon.material.color = IconColors[0];
					break;
				default:
					break;
			}
		}

		private void ChangeVisionMode()
        {
            switch (VisionMode)
            {
                case EVisionMode.Day:
					VisionMode = EVisionMode.WFOV;
					DayIcon.material.color = IconColors[0];
					WFOVIcon.material.color = IconColors[1];
					NFOVIcon.material.color = IconColors[0];
					SeekIcon.material.color = IconColors[0];

					VisibleCamera.gameObject.SetActive(false);
					IRCamera.gameObject.SetActive(true);

					IRCamera.fieldOfView = 53.6f * Mathf.Pow(4f, -0.9364f) - 0.3666f;
					break;
                case EVisionMode.WFOV:
					VisionMode = EVisionMode.NFOV;

					DayIcon.material.color = IconColors[0];
					WFOVIcon.material.color = IconColors[0];
					NFOVIcon.material.color = IconColors[1];
					SeekIcon.material.color = IconColors[0];

					IRCamera.fieldOfView = 53.6f * Mathf.Pow(9f, -0.9364f) - 0.3666f;
					break;
                case EVisionMode.NFOV:
					if (FireArm.Chamber.IsFull && !FireArm.Chamber.IsSpent)
					{
						VisionMode = EVisionMode.Seek;

						DayIcon.material.color = IconColors[0];
						WFOVIcon.material.color = IconColors[0];
						NFOVIcon.material.color = IconColors[0];
						SeekIcon.material.color = IconColors[1];

						SeekCamera.gameObject.SetActive(true);
						IRCamera.gameObject.SetActive(false);

						SeekCamera.fieldOfView = 53.6f * Mathf.Pow(12f, -0.9364f) - 0.3666f;
					}
                    else
                    {
						VisionMode = EVisionMode.Day;

						DayIcon.material.color = IconColors[1];
						WFOVIcon.material.color = IconColors[0];
						NFOVIcon.material.color = IconColors[0];
						SeekIcon.material.color = IconColors[0];

						VisibleCamera.gameObject.SetActive(true);
						IRCamera.gameObject.SetActive(false);
					}

					break;
				case EVisionMode.Seek:
					VisionMode = EVisionMode.Day;

					DayIcon.material.color = IconColors[1];
					WFOVIcon.material.color = IconColors[0];
					NFOVIcon.material.color = IconColors[0];
					SeekIcon.material.color = IconColors[0];

					VisibleCamera.gameObject.SetActive(true);
					SeekCamera.gameObject.SetActive(false);
					break;
				default:
                    break;
            }
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
						GameObject gameObject = Instantiate<GameObject>(chamber.GetRound().BallisticProjectilePrefab, muzzle.position - b, muzzle.rotation);
						Vector2 vector2 = (UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle) * 0.33333334f * d;
						gameObject.transform.Rotate(new Vector3(vector2.x + vector.y + num, vector2.y + vector.x, 0f));
						BallisticProjectile component = gameObject.GetComponent<BallisticProjectile>();
						component.Fire(component.MuzzleVelocityBase * chamber.ChamberVelocityMultiplier * velMult * chamberVelMult, gameObject.transform.forward, self, true);

						TopAttackProjectile smartProjectile = gameObject.GetComponent<TopAttackProjectile>();
						if (smartProjectile != null && _targetPoint != null)
						{
							if (_targetRB == null) smartProjectile.TargetPoint = _targetPoint;
							else  smartProjectile.TargetRB = _targetRB;

							smartProjectile.AttackMode = AttackMode;
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
#endif
	}
}
