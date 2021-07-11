using BepInEx;
using BepInEx.Configuration;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.brighterflashlights", "Brighter Flashlights", "1.2.0")]
    public class BrighterFlashlights_Plugin : BaseUnityPlugin
    {
        
        private readonly BrighterFlashlights_Hooks _hooks;

        private ConfigEntry<float> tacticalFlashlight_range;
        private ConfigEntry<float> tacticalFlashlightIntensity;
        private ConfigEntry<float> tacticalFlashlightIntensity_dark;
        private ConfigEntry<float> flashlight_range;
        private ConfigEntry<float> flashlightIntensity;
        private ConfigEntry<float> flashlightIntensity_dark;

        public BrighterFlashlights_Plugin()
        {
            Logger.LogInfo("BrighterFlashlights loaded!");

            flashlight_range = Config.Bind<float>("Flashlight Settings", "Flashlight_Range", 300f, "Range of the handheld Flashlight, called Flashlight by the game");
            flashlightIntensity = Config.Bind<float>("Flashlight Settings", "Flashlight_Brightness", 0.5f, "Brightness of the Flashlight");
            flashlightIntensity_dark = Config.Bind<float>("Flashlight Settings", "Flashlight_Brightness_dark", 0.9f, "Brightness of the Flashlight when Scene is set to low light. (Not all dark scenes do this, try what ever fits, or set both values as the same)");

            tacticalFlashlight_range = Config.Bind<float>("Tactical Flashlight Settings","TacticalFlashlight_Range",300f,"Range of the attachable Flashlight, called TacticalFlashlight by the game");
            tacticalFlashlightIntensity = Config.Bind<float>("Tactical Flashlight Settings", "TacticalFlashlight_Brightness", 0.5f, "Brightness of the TacticalFlashlight");
            tacticalFlashlightIntensity_dark = Config.Bind<float>("Tactical Flashlight Settings", "TacticalFlashlight_Brightness_dark", 2f, "Brightness of the TacticalFlashlight when Scene is set to low light. (Not all dark scenes do this, try what ever fits, or set both values as the same)");

            _hooks = new BrighterFlashlights_Hooks();

            _hooks.flashlightRange = flashlight_range.Value;
            _hooks.flashlightBrightness = flashlightIntensity.Value;
            _hooks.flashlightBrightness_dark = flashlightIntensity_dark.Value;

            _hooks.tacticalFlashlightRange = tacticalFlashlight_range.Value;
            _hooks.tacticalFlashlightBrightness = tacticalFlashlightIntensity.Value;
            _hooks.tacticalFlashlightBrightness_dark = tacticalFlashlightIntensity_dark.Value;
            _hooks.Hook();
        }

        private void OnDestroy()
        {
            _hooks?.Unhook();
        }

    }
}
