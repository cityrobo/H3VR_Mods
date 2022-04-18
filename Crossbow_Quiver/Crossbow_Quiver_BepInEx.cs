#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.crossbow_quiver", "Crossbow Quiver Scripts", "1.0.0")]
    class Crossbow_Quiver_BepInEx : BaseUnityPlugin
    {
        public Crossbow_Quiver_BepInEx()
        {
            Logger.LogInfo("Crossbow Quiver Scripts loaded!");
        }
    }
}
#endif