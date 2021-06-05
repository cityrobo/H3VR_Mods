using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using Deli.Setup;
using UnityEngine;

namespace Cityrobo
{
    class BrighterFlashlights_Hooks
    {
        public void Hook()
        {
			Debug.Log("Hooking Brighter Flashlight!");
			On.FistVR.TacticalFlashlight.ToggleOn += TacticalFlashlight_ToggleOn;
            On.FistVR.Flashlight.ToggleOn += Flashlight_ToggleOn;
        }
        public void Unhook()
        {
			On.FistVR.TacticalFlashlight.ToggleOn -= TacticalFlashlight_ToggleOn;
			On.FistVR.Flashlight.ToggleOn -= Flashlight_ToggleOn;
		}

        private void TacticalFlashlight_ToggleOn(On.FistVR.TacticalFlashlight.orig_ToggleOn orig, TacticalFlashlight self)
        {
			//Debug.Log("Brighter Tactical Flashlight hooked. Using custom ToggleOn method!");
			self.IsOn = !self.IsOn;
			self.LightParts.SetActive(self.IsOn);
			if (self.IsOn)
			{
				if (GM.CurrentSceneSettings.IsSceneLowLight)
				{
					self.FlashlightLight.Intensity = 4f;
				}
				else
				{
					self.FlashlightLight.Intensity = 2f;
				}
			}
			if (self.IsOn)
			{
				SM.PlayCoreSound(FVRPooledAudioType.GenericClose, self.AudEvent_LaserOnClip, self.transform.position);
			}
			else
			{
				SM.PlayCoreSound(FVRPooledAudioType.GenericClose, self.AudEvent_LaserOffClip, self.transform.position);
			}

			self.FlashlightLight.gameObject.GetComponent<Light>().range = 300;
		}

		private void Flashlight_ToggleOn(On.FistVR.Flashlight.orig_ToggleOn orig, Flashlight self)
		{
			//Debug.Log("Brighter Utility Flashlight hooked. Using custom ToggleOn method!");
			self.IsOn = !self.IsOn;
			self.LightParts.SetActive(self.IsOn);
			if (self.IsOn)
			{
				if (GM.CurrentSceneSettings.IsSceneLowLight)
				{
					self.FlashlightLight.Intensity = 0.9f;
				}
				else
				{
					self.FlashlightLight.Intensity = 0.5f;
				}
			}
			if (self.IsOn)
			{
				self.Aud.PlayOneShot(self.LaserOnClip, 1f);
			}
			else
			{
				self.Aud.PlayOneShot(self.LaserOffClip, 1f);
			}

			self.FlashlightLight.gameObject.GetComponent<Light>().range = 300;
		}

	}
}
