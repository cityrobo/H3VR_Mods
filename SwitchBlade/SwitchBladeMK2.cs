using System.Collections;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
	public class SwitchBladeMK2 : MonoBehaviour
	{
		public FVRMeleeWeapon MeleeWeapon;
		public Transform Blade;
		public Vector2 BladeRotRange = new Vector2(-90f, 90f);
		public float BladeOpeningTime;
		public float BladeClosingTime;
		public AudioEvent OpenSound;
		public AudioEvent CloseSound;

		public enum SwitchBladeState
		{
			Closing,
			Closed,
			Opening,
			Open,
		}

		private SwitchBladeState _switchBladeState = SwitchBladeState.Closed;
		private float _timeElapsed;


		public void Update()
        {
			FVRViveHand hand = MeleeWeapon.m_hand;
			if (hand != null && hand.Input.TriggerDown)
            {
				ToggleSwitchBladeState();
			}
        }

		private void ToggleSwitchBladeState()
		{
			if (MeleeWeapon.MP.IsJointedToObject)
			{
				return;
			}
			if (_switchBladeState == SwitchBladeState.Closed)
			{
				SM.PlayGenericSound(OpenSound, MeleeWeapon.transform.position);
				StartCoroutine(OpenBlade());
				MeleeWeapon.MP.CanNewStab = true;
			}
			else if (_switchBladeState == SwitchBladeState.Open)
			{
				SM.PlayGenericSound(CloseSound, MeleeWeapon.transform.position);
				StartCoroutine(CloseBlade());
				MeleeWeapon.MP.CanNewStab = false;
			}
		}

		private void SetBladeRot(float f)
		{
			float lerp = Mathf.Lerp(BladeRotRange.x, BladeRotRange.y, f);
			Quaternion target = Quaternion.Euler(lerp, 0f, 0f);
			Blade.localRotation = Quaternion.RotateTowards(Blade.localRotation, target, float.MaxValue);
		}

		private IEnumerator OpenBlade()
        {
			_switchBladeState = SwitchBladeState.Opening;
			_timeElapsed = 0f;
			while (_timeElapsed < BladeOpeningTime)
			{
				_timeElapsed += Time.deltaTime;
				SetBladeRot(_timeElapsed / BladeOpeningTime);
				yield return null;
			}
			SetBladeRot(1f);
			_switchBladeState = SwitchBladeState.Open;
		}

		private IEnumerator CloseBlade()
		{
			_switchBladeState = SwitchBladeState.Closing;
			_timeElapsed = 0f;
			while (_timeElapsed < BladeClosingTime)
            {
				_timeElapsed += Time.deltaTime;
				SetBladeRot(1f - (_timeElapsed / BladeClosingTime));
				yield return null;
			}
			SetBladeRot(0f);
			_switchBladeState = SwitchBladeState.Closed;
		}

    }
}
