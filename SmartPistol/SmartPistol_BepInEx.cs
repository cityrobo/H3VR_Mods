#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.smartpistol", "SmartPistol Scripts", "1.0.0")]
    class SmartPistol_BepInEx : BaseUnityPlugin
    {
        public SmartPistol_BepInEx()
        {
            Logger.LogInfo("SmartPistol Scripts loaded!");
        }
    }
}
#endif
