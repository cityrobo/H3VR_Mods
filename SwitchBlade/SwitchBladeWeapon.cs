using System.Collections;
using UnityEngine;

namespace FistVR
{
    [AddComponentMenu("Script/SwitchBlade")]
	public class SwitchBladeWeapon : FVRMeleeWeapon
	{
		public Transform Blade;
		public Vector2 BladeRotRange = new Vector2(-90f, 90f);
		public float BladeOpeningTime;
		public float BladeClosingTime;
		public AudioSource audio_source;
		public AudioClip open_clip;
		public AudioClip close_clip;
		private SwitchBladeWeapon.SwitchBladeState sbState = SwitchBladeState.Closed;
		private float timeElapsed;

		public override void UpdateInteraction(FVRViveHand hand)
		{
			base.UpdateInteraction(hand);
			if (base.IsHeld && this.m_hand.Input.TriggerDown && this.m_hasTriggeredUpSinceBegin)
			{
				this.ToggleSwitchBladeState();
			}
		}

		private void ToggleSwitchBladeState()
		{
			if (this.MP.IsJointedToObject)
			{
				return;
			}
			if (this.sbState == SwitchBladeWeapon.SwitchBladeState.Closed)
			{
				PlaySound(audio_source, open_clip);
				this.StartCoroutine("OpenBlade");
				this.MP.CanNewStab = true;
			}
			else if (this.sbState == SwitchBladeWeapon.SwitchBladeState.Open)
			{
				PlaySound(audio_source, close_clip);
				this.StartCoroutine("CloseBlade");
				this.MP.CanNewStab = false;
			}
		}

		private void SetBladeRot(float f)
		{
			this.Blade.localEulerAngles = new Vector3(Mathf.Lerp(this.BladeRotRange.x, this.BladeRotRange.y, f), 0f, 0f);
		}

		private IEnumerator OpenBlade()
        {
			this.sbState = SwitchBladeWeapon.SwitchBladeState.Opening;
			timeElapsed = 0f;
			while (timeElapsed < BladeOpeningTime)
			{
				timeElapsed += Time.deltaTime;
				SetBladeRot(timeElapsed / BladeOpeningTime);
				yield return null;
			}
			SetBladeRot(1f);
			this.sbState = SwitchBladeWeapon.SwitchBladeState.Open;
		}

		private IEnumerator CloseBlade()
		{
			this.sbState = SwitchBladeWeapon.SwitchBladeState.Closing;
			timeElapsed = 0f;
			while (timeElapsed < BladeClosingTime)
            {
				timeElapsed += Time.deltaTime;
				SetBladeRot(1f - (timeElapsed / BladeClosingTime));
				yield return null;
			}
			SetBladeRot(0f);
			this.sbState = SwitchBladeWeapon.SwitchBladeState.Closed;
		}

		private void PlaySound(AudioSource A_source, AudioClip A_clip)
        {
			A_source.clip = A_clip;
			A_source.Play();
        }

		public enum SwitchBladeState
		{
			Closing,
			Closed,
			Opening,
			Open,
		}

		public static FistVR.SwitchBladeWeapon CopyFromMeleeWeapon(FVRMeleeWeapon original, GameObject target)
        {
			target.gameObject.SetActive(false);
			var real = target.AddComponent<SwitchBladeWeapon>();
			real.ControlType = original.ControlType;
			real.IsSimpleInteract = original.IsSimpleInteract;
			real.HandlingGrabSound = original.HandlingGrabSound;
			real.HandlingReleaseSound = original.HandlingReleaseSound;
			real.PoseOverride = original.PoseOverride;
			real.QBPoseOverride = original.QBPoseOverride;
			real.PoseOverride_Touch = original.PoseOverride_Touch;
			real.UseGrabPointChild = original.UseGrabPointChild;
			real.UseGripRotInterp = original.UseGripRotInterp;
			real.PositionInterpSpeed = original.PositionInterpSpeed;
			real.RotationInterpSpeed = original.RotationInterpSpeed;
			real.EndInteractionIfDistant = original.EndInteractionIfDistant;
			real.EndInteractionDistance = original.EndInteractionDistance;
			real.UXGeo_Held = original.UXGeo_Held;
			real.UXGeo_Hover = original.UXGeo_Hover;
			real.UseFilteredHandTransform = original.UseFilteredHandTransform;
			real.UseFilteredHandRotation = original.UseFilteredHandRotation;
			real.UseFilteredHandPosition = original.UseFilteredHandPosition;
			real.UseSecondStepRotationFiltering = original.UseSecondStepRotationFiltering;
			real.ObjectWrapper = original.ObjectWrapper;
			real.SpawnLockable = original.SpawnLockable;
			real.Harnessable = original.Harnessable;
			real.HandlingReleaseIntoSlotSound = original.HandlingReleaseIntoSlotSound;
			real.Size = original.Size;
			real.QBSlotType = original.QBSlotType;
			real.ThrowVelMultiplier = original.ThrowVelMultiplier;
			real.ThrowAngMultiplier = original.ThrowAngMultiplier;
			real.UsesGravity = original.UsesGravity;
			real.DependantRBs = original.DependantRBs;
			real.DistantGrabbable = original.DistantGrabbable;
			real.IsDebug = original.IsDebug;
			real.IsAltHeld = original.IsAltHeld;
			real.IsKinematicLocked = original.IsKinematicLocked;
			real.DoesQuickbeltSlotFollowHead = original.DoesQuickbeltSlotFollowHead;
			real.IsInWater = original.IsInWater;
			real.AttachmentMounts = original.AttachmentMounts;
			real.IsAltToAltTransfer = original.IsAltToAltTransfer;
			real.CollisionSound = original.CollisionSound;
			real.IsPickUpLocked = original.IsPickUpLocked;
			real.OverridesObjectToHand = original.OverridesObjectToHand;
			real.MP = original.MP;
			Destroy(original);
			return real;
		}
    }
}
