using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
	public class BreakOpenTrigger : MonoBehaviour
	{
		public FVRPhysicalObject physicalObject;

		[Header("Break Parts")]
		public HingeJoint Hinge;
		public float HingeLimit = 45f;
		public float HingeEjectThreshhold = 30f;
		public Transform centerOfMassOverride;

		[Header("LatchGeo (leave at default if no visible latch)")]
		public float maxLatchRot = 45f;
		[Tooltip("If latch is below this angle the fore will latch. Latch rot dependend on how far up you press on touchpad (like break action shotgun)")]
		public float latchLatchingRot = 5f;
		public bool hasLatchObject;
		public Transform Latch;

		[Header("Objects that turn off or on dependend on break state")]
		public GameObject[] TurnOffObjectsOnOpen;
		public GameObject[] TurnOnObjectsOnOpen;
		public bool doesEjectMag = false;
		public float MagEjectSpeed = 5f;

		[Header("Audio")]
		public AudioEvent BreakOpenAudio;
		public AudioEvent BreakCloseAudio;

		private float m_latchRot;
		private Vector3 m_foreStartPos;

		private bool m_isLatched = true;
		private bool m_latchHeldOpen;
		private bool m_hasEjectedMag = false;

#if !(UNITY_EDITOR || UNITY_5)

		public void Awake()
        {
			m_foreStartPos = Hinge.transform.localPosition;
            if (centerOfMassOverride != null)
            {
				Rigidbody RB = Hinge.GetComponent<Rigidbody>();
				RB.centerOfMass = centerOfMassOverride.localPosition;
			}

			SetBreakObjectsState(false);
		}

		public void Start()
		{
			Hook();
		}

		public void OnDestroy()
        {
			Unhook();
        }

		public void FixedUpdate()
		{
			UpdateBreakFore();
		}

		public void Update()
        {
			FVRViveHand hand = physicalObject.m_hand;
			if (hand != null) UpdateInputAndAnimate(hand);
			else this.m_latchHeldOpen = false;
		}

		void UpdateInputAndAnimate(FVRViveHand hand)
		{
			this.m_latchHeldOpen = false;
			if (hand.IsInStreamlinedMode)
			{
				if (hand.Input.BYButtonPressed)
				{
					this.m_latchHeldOpen = true;
					this.m_latchRot = 1f * this.maxLatchRot;
				}
				else
				{
					this.m_latchRot = Mathf.MoveTowards(this.m_latchRot, 0f, Time.deltaTime * this.maxLatchRot * 3f);
				}
			}
			else
			{
				if (hand.Input.TouchpadPressed && hand.Input.TouchpadAxes.y > 0.1f)
				{
					this.m_latchHeldOpen = true;
					this.m_latchRot = hand.Input.TouchpadAxes.y * this.maxLatchRot;
				}
				else
				{
					this.m_latchRot = Mathf.MoveTowards(this.m_latchRot, 0f, Time.deltaTime * this.maxLatchRot * 3f);
				}
			}

			if (this.hasLatchObject)
			{
				this.Latch.localEulerAngles = new Vector3(0f, this.m_latchRot, 0f);
			}
		}

		void UpdateBreakFore()
		{
			if (this.m_isLatched && Mathf.Abs(this.m_latchRot) > 5f)
			{
				this.m_isLatched = false;
				SM.PlayGenericSound(BreakOpenAudio, physicalObject.transform.position);
				JointLimits limits = this.Hinge.limits;
				limits.max = this.HingeLimit;
				this.Hinge.limits = limits;
				SetBreakObjectsState(true);
			}
			if (!this.m_isLatched)
			{
				if (!this.m_latchHeldOpen && this.Hinge.transform.localEulerAngles.x <= 1f && Mathf.Abs(this.m_latchRot) < latchLatchingRot)
				{
					this.m_isLatched = true;
					SM.PlayGenericSound(BreakCloseAudio, physicalObject.transform.position);
					JointLimits limits = this.Hinge.limits;
					limits.max = 0f;
					this.Hinge.limits = limits;
					SetBreakObjectsState(false);
					this.Hinge.transform.localPosition = this.m_foreStartPos;
					m_hasEjectedMag = false;
				}
				if (doesEjectMag && Mathf.Abs(this.Hinge.transform.localEulerAngles.x) >= this.HingeEjectThreshhold && Mathf.Abs(this.Hinge.transform.localEulerAngles.x) <= HingeLimit)
				{
					TryEjectMag();
				}
			}
		}

		void TryEjectMag()
		{
			if (!m_hasEjectedMag)
			{
				EjectMag();
				m_hasEjectedMag = true;
			}
		}

		public void EjectMag(bool PhysicalRelease = false)
		{
			FVRFireArm fireArm = physicalObject as FVRFireArm;

			if (fireArm.Magazine != null)
			{
				if (fireArm.Magazine.UsesOverrideInOut)
				{
					fireArm.PlayAudioEventHandling(fireArm.Magazine.ProfileOverride.MagazineOut);
				}
				else
				{
					fireArm.PlayAudioEvent(FirearmAudioEventType.MagazineOut, 1f);
				}
				fireArm.m_lastEjectedMag = fireArm.Magazine;
				fireArm.m_ejectDelay = 0.4f;
				if (fireArm.m_hand != null)
				{
					fireArm.m_hand.Buzz(fireArm.m_hand.Buzzer.Buzz_BeginInteraction);
				}
				fireArm.Magazine.Release(PhysicalRelease);

				fireArm.Magazine.RootRigidbody.velocity = -fireArm.MagazineEjectPos.up * MagEjectSpeed;
				if (fireArm.Magazine.m_hand != null)
				{
					fireArm.Magazine.m_hand.Buzz(fireArm.m_hand.Buzzer.Buzz_BeginInteraction);
				}
				fireArm.Magazine = null;
			}
		}

		void SetBreakObjectsState(bool active)
		{
            foreach (var TurnOnObjectOnOpen in TurnOnObjectsOnOpen)
            {
				if (TurnOnObjectOnOpen != null) TurnOnObjectOnOpen.SetActive(active);
			}
			foreach (var TurnOffObjectOnOpen in TurnOffObjectsOnOpen)
			{
				if (TurnOffObjectOnOpen != null) TurnOffObjectOnOpen.SetActive(!active);
			}
		}

		void Unhook()
		{
#if !MEATKIT
			switch (physicalObject)
			{
				case ClosedBoltWeapon w:
					On.FistVR.ClosedBoltWeapon.DropHammer += ClosedBoltWeapon_DropHammer;
					break;
				case OpenBoltReceiver w:
					On.FistVR.OpenBoltReceiver.ReleaseSeer += OpenBoltReceiver_ReleaseSeer;
					break;
				case Handgun w:
					On.FistVR.Handgun.ReleaseSeer += Handgun_ReleaseSeer;
					break;
				case TubeFedShotgun w:
					On.FistVR.TubeFedShotgun.ReleaseHammer -= TubeFedShotgun_ReleaseHammer;
					break;
				default:
					break;
			}
#endif
		}
		void Hook()
		{
#if !MEATKIT
			switch (physicalObject)
            {
				case ClosedBoltWeapon w:
                    On.FistVR.ClosedBoltWeapon.DropHammer += ClosedBoltWeapon_DropHammer;
					break;
				case OpenBoltReceiver w:
                    On.FistVR.OpenBoltReceiver.ReleaseSeer += OpenBoltReceiver_ReleaseSeer;
					break;
				case Handgun w:
                    On.FistVR.Handgun.ReleaseSeer += Handgun_ReleaseSeer;
					break;
				case TubeFedShotgun w:
                    On.FistVR.TubeFedShotgun.ReleaseHammer += TubeFedShotgun_ReleaseHammer;
					break;
                default:
                    break;
            }
#endif
		}
#if !MEATKIT
		private void TubeFedShotgun_ReleaseHammer(On.FistVR.TubeFedShotgun.orig_ReleaseHammer orig, TubeFedShotgun self)
        {
			if (self == physicalObject)
			{
				if (!m_isLatched || m_latchHeldOpen) return;
			}
			orig(self);
		}

        private void Handgun_ReleaseSeer(On.FistVR.Handgun.orig_ReleaseSeer orig, Handgun self)
        {
			if (self == physicalObject)
			{
				if (!m_isLatched || m_latchHeldOpen) return;
			}
			orig(self);
		}

        private void OpenBoltReceiver_ReleaseSeer(On.FistVR.OpenBoltReceiver.orig_ReleaseSeer orig, OpenBoltReceiver self)
        {
			if (self == physicalObject)
			{
				if (!m_isLatched || m_latchHeldOpen) return;
			}
			orig(self);
		}

        private void ClosedBoltWeapon_DropHammer(On.FistVR.ClosedBoltWeapon.orig_DropHammer orig, ClosedBoltWeapon self)
        {
            if (self == physicalObject)
            {
				if (!m_isLatched || m_latchHeldOpen) return;
            }
			orig(self);
        }
#endif
#endif
	}
}