#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.fo4_laserrifle", "Fallout 4 LaserRifle Scripts", "1.0.0")]
    class FO4_LaserRifle_BepInEx : BaseUnityPlugin
    {
        public FO4_LaserRifle_BepInEx()
        {
            //Logger.LogInfo("Fallout 4 LaserRifle Scripts loaded!");
        }
    }
}
#endif