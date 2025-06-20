using BepInEx;
using BepInEx.Configuration;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.SpectatorCameraScreenshot", "Spectator Camera Screenshot", "1.0.0")]
    public class SpectatorCameraScreenshot : BaseUnityPlugin
    {

        public static ConfigEntry<bool> UseCustomPath;
        public static ConfigEntry<string> CustomPath;

        private static readonly string s_defaultScreenshotPath = Path.Combine(Paths.BepInExRootPath, "Screenshots");
#if !DEBUG
        public void Awake()
        {
            On.FistVR.Camcorder.UpdateInteraction += Camcorder_UpdateInteraction;

            UseCustomPath = Config.Bind("Spectator Camera Screenshot", "Use custom screenshot path?", false, "If set to false, screenshots will be placed in ProfileFolder/BepInEx/Screenshots by default.");
            CustomPath = Config.Bind("Spectator Camera Screenshot", "Custom screenshot path", string.Empty, "Only used when above setting is set to true. Folder will be created if it doesn't already exist. it should work with \"/\" and \"\\\\\", because text strings stuff.");

            if (!Directory.Exists(s_defaultScreenshotPath)) Directory.CreateDirectory(s_defaultScreenshotPath);

            if (UseCustomPath.Value && !Directory.Exists(CustomPath.Value))
            {
                Directory.CreateDirectory(CustomPath.Value);
            }
        }

        public void OnDestroy()
        {
            On.FistVR.Camcorder.UpdateInteraction -= Camcorder_UpdateInteraction;
        }
        private static void Camcorder_UpdateInteraction(On.FistVR.Camcorder.orig_UpdateInteraction orig, Camcorder self, FVRViveHand hand)
        {
            orig(self, hand);

            bool inputPressed = !hand.IsInStreamlinedMode ? hand.Input.TouchpadDown : hand.Input.AXButtonDown || hand.Input.BYButtonDown;

            if (inputPressed)
            {
                string screenshotPath = UseCustomPath.Value ? CustomPath.Value : Path.Combine(s_defaultScreenshotPath, "Screenshot_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".png");

                Application.CaptureScreenshot(screenshotPath);
            }
        }
#endif
    }
}
