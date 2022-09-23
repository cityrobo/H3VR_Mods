#if !DEBUG
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.OpenScripts", "OpenScripts BepInEx Loader Plugin", "1.0.0")]
    public class OpenScripts_BepInEx : BaseUnityPlugin
    {
        public static string PluginPath;
        public static OpenScripts_BepInEx Instance;

        public static List<string> LoadedPluginPaths = new List<string>();

        // FirearmHeatingEffect Config Entries:
        public static ConfigEntry<bool> FirearmHeatingEffect_CanExplode;
        public static ConfigEntry<bool> FirearmHeatingEffect_CanRecover;
        public static ConfigEntry<float> FirearmHeatingEffect_RecoverThreshold;
        public static ConfigEntry<bool> FirearmHeatingEffect_CanChangeFirerate;
        public static ConfigEntry<bool> FirearmHeatingEffect_CanChanceAccuracy;

        public OpenScripts_BepInEx()
        {
            Instance = this;
            // FirearmHeatingEffect Config Bindings
            FirearmHeatingEffect_CanExplode = Config.Bind("Firearm Heating Effect", "Part can explode", true, "If true, and the part is setup to do so, the parts with heating effects can explode.");
            FirearmHeatingEffect_CanRecover = Config.Bind("Firearm Heating Effect", "Part can recover", false, "If true, parts can recover from being exploded.");
            FirearmHeatingEffect_RecoverThreshold = Config.Bind("Firearm Heating Effect", "Recover heat threshold", 0f, new ConfigDescription("Defines the heat value, at which the part will recover from being exploded", new AcceptableValueRange<float>(0, 1)));
            FirearmHeatingEffect_CanChangeFirerate = Config.Bind("Firearm Heating Effect", "Gun can change firerate", true, "If true, enables firearm firerate changes based on heat.");
            FirearmHeatingEffect_CanChanceAccuracy = Config.Bind("Firearm Heating Effect", "Gun can change accuracy", true, "If true, enables firearm accuracy changes based on heat.");

            PluginPath = this.Info.Location;
            string pluginName = Path.GetFileName(PluginPath);
            string pluginFolder = Path.GetDirectoryName(PluginPath);

            DirectoryInfo directoryInfo = new DirectoryInfo(pluginFolder);
            FileInfo[] filesInDir = directoryInfo.GetFiles("*.dll");

            foreach (FileInfo file in filesInDir)
            {
                if (file.FullName == PluginPath) continue;
                System.Reflection.Assembly.LoadFrom(file.FullName);
                LoadedPluginPaths.Add(file.FullName);
            }
            Logger.LogInfo("Fully loaded all OpenScripts DLLs!");
        }
    }
}
#endif
