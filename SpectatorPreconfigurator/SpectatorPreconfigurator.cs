using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using BepInEx.Configuration;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.spectator_preconfigurator", "Spectator Preconfigurator", "1.0.0")]
    public class SpectatorPreconfigurator : BaseUnityPlugin
    {
        public ConfigEntry<SosigEnemyCategory> outfitCategory;
        public ConfigEntry<int> outfitIndex;

        public ConfigEntry<ControlOptions.MeatBody> playerBody;
        public ConfigEntry<ControlOptions.DesktopCameraMode> cameraMode;
        public ConfigEntry<float> camSmoothingLinear;
        public ConfigEntry<float> camSmoothingRotational;
        public ConfigEntry<float> camLeveling;
        public ConfigEntry<float> cameraFOV;
        public ConfigEntry<ControlOptions.DesktopCameraEye> cameraEye;
        public ConfigEntry<ControlOptions.DesktopRenderQuality> renderQuality;
        public ConfigEntry<ControlOptions.PreviewCamMode> previewCamMode;
        public ConfigEntry<int> thirdPersonDistanceIndex;
        public ConfigEntry<int> thirdPersonLateralIndex;
        public ConfigEntry<bool> makeSpectatorPanelPersistent;

        public SpectatorPreconfigurator()
        {
            Logger.LogInfo("SpectatorPreconfigurator loaded!");
            outfitCategory = Config.Bind<SosigEnemyCategory>("SpectatorPreconfigurator", "Outfit Category", SosigEnemyCategory.Bangers, "Outfit Category");
            outfitIndex = Config.Bind<int>("SpectatorPreconfigurator", "Outfit Index", 0, "Index of the Outfit from the chosen category (starts at 0 for the first outfit from the category)");
            playerBody = Config.Bind<ControlOptions.MeatBody>("SpectatorPreconfigurator", "Player Body Enabled", ControlOptions.MeatBody.Disabled, "Enable player body by default");
            cameraMode = Config.Bind<ControlOptions.DesktopCameraMode>("SpectatorPreconfigurator","Camera Mode",ControlOptions.DesktopCameraMode.Default,"Camera mode to use by default");
            camSmoothingLinear = Config.Bind<float>("SpectatorPreconfigurator", "Linear Camera Smoothing", 0, "Linear Camera Smoothing to use by default");
            camSmoothingRotational = Config.Bind<float>("SpectatorPreconfigurator", "Rotational Camera Smoothing", 0, "Rotational Camera Smoothing to use by default");
            camLeveling = Config.Bind<float>("SpectatorPreconfigurator", "Camera Leveling", 0, "Camera Leveling to use by default");
            cameraFOV = Config.Bind<float>("SpectatorPreconfigurator", "Camera FOV", 45f, "Camera FOV to use by default");
            cameraEye = Config.Bind<ControlOptions.DesktopCameraEye>("SpectatorPreconfigurator", "Camera Eye", ControlOptions.DesktopCameraEye.Right, "Which to use for the camera by default");
            renderQuality = Config.Bind<ControlOptions.DesktopRenderQuality>("SpectatorPreconfigurator", "Camera Render Quality", ControlOptions.DesktopRenderQuality.Low, "Default Camera Render Quality");
            previewCamMode = Config.Bind<ControlOptions.PreviewCamMode>("SpectatorPreconfigurator", "Preview Camera Mode", ControlOptions.PreviewCamMode.Disabled, "Preview Camera Mode by default");
            thirdPersonDistanceIndex = Config.Bind<int>("SpectatorPreconfigurator", "Third Person Camera Distance Index", 2, "Third Person Camera Distance by default");
            thirdPersonLateralIndex = Config.Bind<int>("SpectatorPreconfigurator", "Third Person Camera Lateral Distance Index", 2, "Third Person Camera Lateral Distance by default");
            makeSpectatorPanelPersistent = Config.Bind<bool>("SpectatorPreconfigurator", "Make Spectator Panel persistent", true, "Changes made in the spectator panel will be automatically applied to the config file");
        }

        public void Start()
        {
            Logger.LogInfo("Preconfiguring Spectator Settings: ");
            
            GM.Options.ControlOptions.MBMode = playerBody.Value;
            Logger.LogInfo("Player body: "+ GM.Options.ControlOptions.MBMode);
            GM.Options.ControlOptions.CamMode = cameraMode.Value;
            Logger.LogInfo("Camera Mode: " + GM.Options.ControlOptions.CamMode);
            GM.Options.ControlOptions.CamSmoothingLinear = camSmoothingLinear.Value;
            Logger.LogInfo("Linear Camera Smoothing: " + GM.Options.ControlOptions.CamSmoothingLinear);
            GM.Options.ControlOptions.CamSmoothingRotational = camSmoothingRotational.Value;
            Logger.LogInfo("Rotational Camera Smoothing: " + GM.Options.ControlOptions.CamSmoothingRotational);
            GM.Options.ControlOptions.CamLeveling = camLeveling.Value;
            Logger.LogInfo("Camera Leveling: " + GM.Options.ControlOptions.CamLeveling);
            GM.Options.ControlOptions.CamFOV = cameraFOV.Value;
            Logger.LogInfo("Camera FOV: " + GM.Options.ControlOptions.CamFOV);
            GM.Options.ControlOptions.CamEye = cameraEye.Value;
            Logger.LogInfo("Camera Eye: " + GM.Options.ControlOptions.CamEye);
            GM.Options.ControlOptions.CamQual = renderQuality.Value;
            Logger.LogInfo("Camera Render Quality: " + GM.Options.ControlOptions.CamQual);
            GM.Options.ControlOptions.PCamMode = previewCamMode.Value;
            Logger.LogInfo("Preview Camera Mode: " + GM.Options.ControlOptions.PCamMode);
            GM.Options.ControlOptions.TPCDistanceIndex = thirdPersonDistanceIndex.Value;
            Logger.LogInfo("Third Person Camera Distance Index: " + GM.Options.ControlOptions.TPCDistanceIndex);
            GM.Options.ControlOptions.TPCLateralIndex = thirdPersonLateralIndex.Value;
            Logger.LogInfo("Third Person Camera Lateral Distance Index: " + GM.Options.ControlOptions.TPCLateralIndex);
            List<SosigEnemyTemplate> sosigEnemyTemplates = ManagerSingleton<IM>.Instance.odicSosigObjsByCategory[outfitCategory.Value];
            Logger.LogInfo("Outfit Category: " + outfitCategory.Value);
            GM.CurrentPlayerBody.SetOutfit(sosigEnemyTemplates[outfitIndex.Value]);
            Logger.LogInfo("Outfit: " + sosigEnemyTemplates[outfitIndex.Value].name);

            if (makeSpectatorPanelPersistent.Value) Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }

        void Unhook()
        {
            On.FistVR.SpectatorPanel.BTN_Template_SetGroup -= SpectatorPanel_BTN_Template_SetGroup;
            On.FistVR.SpectatorPanel.BTN_Template_Set -= SpectatorPanel_BTN_Template_Set;
            On.FistVR.SpectatorPanel.BTN_TPCLateralIndex -= SpectatorPanel_BTN_TPCLateralIndex;
            On.FistVR.SpectatorPanel.BTN_TPCDistanceIndex -= SpectatorPanel_BTN_TPCDistanceIndex;
            On.FistVR.SpectatorPanel.BTN_SetPCam -= SpectatorPanel_BTN_SetPCam;
            On.FistVR.SpectatorPanel.BTN_SetCamQual -= SpectatorPanel_BTN_SetCamQual;
            On.FistVR.SpectatorPanel.BTN_SetCamEye -= SpectatorPanel_BTN_SetCamEye;
            On.FistVR.SpectatorPanel.BTN_SetFOV -= SpectatorPanel_BTN_SetFOV;
            On.FistVR.SpectatorPanel.BTN_SetCamLevelingSmooth -= SpectatorPanel_BTN_SetCamLevelingSmooth;
            On.FistVR.SpectatorPanel.BTN_SetCamAngularSmooth -= SpectatorPanel_BTN_SetCamAngularSmooth;
            On.FistVR.SpectatorPanel.BTN_SetCamLinearSmooth -= SpectatorPanel_BTN_SetCamLinearSmooth;
            On.FistVR.SpectatorPanel.BTN_SetCamMode -= SpectatorPanel_BTN_SetCamMode;
            On.FistVR.SpectatorPanel.BTN_SetBodyMode -= SpectatorPanel_BTN_SetBodyMode;
        }

        void Hook()
        {
            On.FistVR.SpectatorPanel.BTN_Template_SetGroup += SpectatorPanel_BTN_Template_SetGroup;
            On.FistVR.SpectatorPanel.BTN_Template_Set += SpectatorPanel_BTN_Template_Set;
            On.FistVR.SpectatorPanel.BTN_TPCLateralIndex += SpectatorPanel_BTN_TPCLateralIndex;
            On.FistVR.SpectatorPanel.BTN_TPCDistanceIndex += SpectatorPanel_BTN_TPCDistanceIndex;
            On.FistVR.SpectatorPanel.BTN_SetPCam += SpectatorPanel_BTN_SetPCam;
            On.FistVR.SpectatorPanel.BTN_SetCamQual += SpectatorPanel_BTN_SetCamQual;
            On.FistVR.SpectatorPanel.BTN_SetCamEye += SpectatorPanel_BTN_SetCamEye;
            On.FistVR.SpectatorPanel.BTN_SetFOV += SpectatorPanel_BTN_SetFOV;
            On.FistVR.SpectatorPanel.BTN_SetCamLevelingSmooth += SpectatorPanel_BTN_SetCamLevelingSmooth;
            On.FistVR.SpectatorPanel.BTN_SetCamAngularSmooth += SpectatorPanel_BTN_SetCamAngularSmooth;
            On.FistVR.SpectatorPanel.BTN_SetCamLinearSmooth += SpectatorPanel_BTN_SetCamLinearSmooth;
            On.FistVR.SpectatorPanel.BTN_SetCamMode += SpectatorPanel_BTN_SetCamMode;
            On.FistVR.SpectatorPanel.BTN_SetBodyMode += SpectatorPanel_BTN_SetBodyMode;
        }

        private void SpectatorPanel_BTN_SetBodyMode(On.FistVR.SpectatorPanel.orig_BTN_SetBodyMode orig, SpectatorPanel self, int i)
        {
            orig(self, i);

            playerBody.Value = (ControlOptions.MeatBody)i;
        }

        private void SpectatorPanel_BTN_SetCamMode(On.FistVR.SpectatorPanel.orig_BTN_SetCamMode orig, SpectatorPanel self, int i)
        {
            orig(self, i);

            cameraMode.Value = (ControlOptions.DesktopCameraMode)i;
        }

        private void SpectatorPanel_BTN_SetCamLinearSmooth(On.FistVR.SpectatorPanel.orig_BTN_SetCamLinearSmooth orig, SpectatorPanel self, float i)
        {
            orig(self, i);

            camSmoothingLinear.Value = i;
        }

        private void SpectatorPanel_BTN_SetCamAngularSmooth(On.FistVR.SpectatorPanel.orig_BTN_SetCamAngularSmooth orig, SpectatorPanel self, float i)
        {
            orig(self, i);

            camSmoothingRotational.Value = i;
        }

        private void SpectatorPanel_BTN_SetCamLevelingSmooth(On.FistVR.SpectatorPanel.orig_BTN_SetCamLevelingSmooth orig, SpectatorPanel self, float i)
        {
            orig(self, i);

            camLeveling.Value = i;
        }

        private void SpectatorPanel_BTN_SetFOV(On.FistVR.SpectatorPanel.orig_BTN_SetFOV orig, SpectatorPanel self, float i)
        {
            orig(self, i);

            cameraFOV.Value = GM.Options.ControlOptions.CamFOV + i;
        }

        private void SpectatorPanel_BTN_SetCamEye(On.FistVR.SpectatorPanel.orig_BTN_SetCamEye orig, SpectatorPanel self, int i)
        {
            orig(self, i);

            cameraEye.Value = (ControlOptions.DesktopCameraEye)i;
        }

        private void SpectatorPanel_BTN_SetCamQual(On.FistVR.SpectatorPanel.orig_BTN_SetCamQual orig, SpectatorPanel self, int i)
        {
            orig(self, i);

            renderQuality.Value = (ControlOptions.DesktopRenderQuality)i;
        }

        private void SpectatorPanel_BTN_SetPCam(On.FistVR.SpectatorPanel.orig_BTN_SetPCam orig, SpectatorPanel self, int i)
        {
            orig(self, i);

            previewCamMode.Value = (ControlOptions.PreviewCamMode)i;
        }

        private void SpectatorPanel_BTN_TPCDistanceIndex(On.FistVR.SpectatorPanel.orig_BTN_TPCDistanceIndex orig, SpectatorPanel self, int i)
        {
            orig(self, i);

            thirdPersonDistanceIndex.Value = i;
        }

        private void SpectatorPanel_BTN_TPCLateralIndex(On.FistVR.SpectatorPanel.orig_BTN_TPCLateralIndex orig, SpectatorPanel self, int i)
        {
            orig(self, i);

            thirdPersonLateralIndex.Value = i;
        }

        private void SpectatorPanel_BTN_Template_SetGroup(On.FistVR.SpectatorPanel.orig_BTN_Template_SetGroup orig, SpectatorPanel self, int i)
        {
            orig(self, i);

            outfitCategory.Value = self.Cats[i];
        }

        private void SpectatorPanel_BTN_Template_Set(On.FistVR.SpectatorPanel.orig_BTN_Template_Set orig, SpectatorPanel self, int i)
        {
            orig(self, i);

            outfitIndex.Value = i;
        }
    }
}
