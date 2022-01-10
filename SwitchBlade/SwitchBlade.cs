using System.Collections;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
	public class SwitchBlade : FVRMeleeWeapon
	{
		public Transform Blade;
		public Vector2 BladeRotRange = new Vector2(-90f, 90f);
		public float BladeOpeningTime;
		public float BladeClosingTime;
		public AudioSource audio_source;
		public AudioClip open_clip;
		public AudioClip close_clip;
		private SwitchBladeState sbState = SwitchBladeState.Closed;
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
			if (this.sbState == SwitchBladeState.Closed)
			{
				PlaySound(audio_source, open_clip);
				this.StartCoroutine("OpenBlade");
				this.MP.CanNewStab = true;
			}
			else if (this.sbState == SwitchBladeState.Open)
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
			this.sbState = SwitchBladeState.Opening;
			timeElapsed = 0f;
			while (timeElapsed < BladeOpeningTime)
			{
				timeElapsed += Time.deltaTime;
				SetBladeRot(timeElapsed / BladeOpeningTime);
				yield return null;
			}
			SetBladeRot(1f);
			this.sbState = SwitchBladeState.Open;
		}

		private IEnumerator CloseBlade()
		{
			this.sbState = SwitchBladeState.Closing;
			timeElapsed = 0f;
			while (timeElapsed < BladeClosingTime)
            {
				timeElapsed += Time.deltaTime;
				SetBladeRot(1f - (timeElapsed / BladeClosingTime));
				yield return null;
			}
			SetBladeRot(0f);
			this.sbState = SwitchBladeState.Closed;
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
    }
}
