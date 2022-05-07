#if !DEBUG
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.OpenScripts", "OpenScripts BepInEx Loader Plugin", "1.0.0")]
    class OpenScripts_BepInEx : BaseUnityPlugin
    {
        public static string PluginPath;

        public static List<string> LoadedPluginPaths = new List<string>();
        public OpenScripts_BepInEx()
        {
            PluginPath = this.Info.Location;
            //Logger.LogInfo($"PluginPath: {PluginPath}");

            string pluginName = Path.GetFileName(PluginPath);
            //Logger.LogInfo($"pluginName: {pluginName}");
            string pluginFolder = Path.GetDirectoryName(PluginPath);
            //Logger.LogInfo($"pluginFolder: {pluginFolder}");

            DirectoryInfo directoryInfo = new DirectoryInfo(pluginFolder);
            FileInfo[] filesInDir = directoryInfo.GetFiles("*.dll");

            foreach (FileInfo file in filesInDir)
            {
                //Logger.LogInfo($"file.FullName: {file.FullName}");
                if (file.FullName == PluginPath) continue;
                System.Reflection.Assembly.LoadFrom(file.FullName);
                LoadedPluginPaths.Add(file.FullName);
            }
            Logger.LogInfo("OpenScripts loaded!");
        }
    }
}
#endif
