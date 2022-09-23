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
			if (IsHeld && m_hand.Input.TriggerDown && m_hasTriggeredUpSinceBegin)
			{
				ToggleSwitchBladeState();
			}
		}

		private void ToggleSwitchBladeState()
		{
			if (MP.IsJointedToObject)
			{
				return;
			}
			if (sbState == SwitchBladeState.Closed)
			{
				PlaySound(audio_source, open_clip);
				StartCoroutine("OpenBlade");
				MP.CanNewStab = true;
			}
			else if (sbState == SwitchBladeState.Open)
			{
				PlaySound(audio_source, close_clip);
				StartCoroutine("CloseBlade");
				MP.CanNewStab = false;
			}
		}

		private void SetBladeRot(float f)
		{
			Blade.localEulerAngles = new Vector3(Mathf.Lerp(BladeRotRange.x, BladeRotRange.y, f), 0f, 0f);
		}

		private IEnumerator OpenBlade()
        {
			sbState = SwitchBladeState.Opening;
			timeElapsed = 0f;
			while (timeElapsed < BladeOpeningTime)
			{
				timeElapsed += Time.deltaTime;
				SetBladeRot(timeElapsed / BladeOpeningTime);
				yield return null;
			}
			SetBladeRot(1f);
			sbState = SwitchBladeState.Open;
		}

		private IEnumerator CloseBlade()
		{
			sbState = SwitchBladeState.Closing;
			timeElapsed = 0f;
			while (timeElapsed < BladeClosingTime)
            {
				timeElapsed += Time.deltaTime;
				SetBladeRot(1f - (timeElapsed / BladeClosingTime));
				yield return null;
			}
			SetBladeRot(0f);
			sbState = SwitchBladeState.Closed;
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
