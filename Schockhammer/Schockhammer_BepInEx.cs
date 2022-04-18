#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.schockhammer", "Wolfenstein 2 Schockhammer Scripts", "1.0.0")]
    class Schockhammer_BepInEx : BaseUnityPlugin
    {
        public Schockhammer_BepInEx()
        {
            Logger.LogInfo("Wolfenstein 2 Schockhammer Scripts loaded!");
        }
    }
}
#endif