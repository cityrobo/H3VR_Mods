#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.typhoon", "Typhoon Scripts", "1.0.0")]
    class Typhoon_BepInEx : BaseUnityPlugin
    {
        public Typhoon_BepInEx()
        {
            Logger.LogInfo("Typhoon Scripts loaded!");
        }
    }
}
#endif