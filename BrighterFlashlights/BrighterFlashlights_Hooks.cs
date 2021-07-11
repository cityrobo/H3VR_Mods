using FistVR;
//using Deli.Setup;
using MonoMod.Cil;
using UnityEngine;

namespace Cityrobo
{
    class BrighterFlashlights_Hooks
    {
		

		public float tacticalFlashlightBrightness = 1f;
		public float tacticalFlashlightBrightness_dark = 4f;
		public float tacticalFlashlightRange = 300;

		public float flashlightBrightness = 0.5f;
		public float flashlightBrightness_dark = 0.9f;
		public float flashlightRange = 300;

        public BrighterFlashlights_Hooks()
        {
		}
		public void Hook()
        {
			Debug.Log("Hooking Brighter Flashlight!");
			IL.FistVR.TacticalFlashlight.ToggleOn += TacticalFlashlight_ToggleOn;
			On.FistVR.TacticalFlashlight.ToggleOn += TacticalFlashlight_ToggleOn_range;

            IL.FistVR.Flashlight.ToggleOn += Flashlight_ToggleOn;
            On.FistVR.Flashlight.ToggleOn += Flashlight_ToggleOn_range;
        }


        public void Unhook()
        {
			IL.FistVR.TacticalFlashlight.ToggleOn -= TacticalFlashlight_ToggleOn;
			On.FistVR.TacticalFlashlight.ToggleOn -= TacticalFlashlight_ToggleOn_range;

			IL.FistVR.Flashlight.ToggleOn -= Flashlight_ToggleOn;
			On.FistVR.Flashlight.ToggleOn -= Flashlight_ToggleOn_range;
		}


		private void TacticalFlashlight_ToggleOn(ILContext il)
        {
			ILCursor c = new(il);

			c.GotoNext(
				MoveType.Before,
				i => i.MatchLdcR4(2f)
			);

			c.Next.Operand = tacticalFlashlightBrightness_dark;

			c.GotoNext(MoveType.Before,
				i => i.MatchLdcR4(0.5f)
			);
			c.Next.Operand = tacticalFlashlightBrightness;
		}


        private void TacticalFlashlight_ToggleOn_range(On.FistVR.TacticalFlashlight.orig_ToggleOn orig, TacticalFlashlight self)
        {
			orig(self);
			//Debug.Log("Brighter Tactical Flashlight hooked. Using custom ToggleOn method!");
			self.FlashlightLight.gameObject.GetComponent<Light>().range = tacticalFlashlightRange;
		}

		private void Flashlight_ToggleOn(ILContext il)
		{
			ILCursor c = new(il);

			c.GotoNext(
				MoveType.Before,
				i => i.MatchLdcR4(0.9f)
			);

			c.Next.Operand = flashlightBrightness_dark;

			c.GotoNext(
				MoveType.Before,
				i => i.MatchLdcR4(0.5f)
			);

			c.Next.Operand =flashlightBrightness;
		}

		private void Flashlight_ToggleOn_range(On.FistVR.Flashlight.orig_ToggleOn orig, Flashlight self)
		{
			orig(self);
			//Debug.Log("Brighter Utility Flashlight hooked. Using custom ToggleOn method!");
			self.FlashlightLight.gameObject.GetComponent<Light>().range = flashlightRange;
		}

	}
}
