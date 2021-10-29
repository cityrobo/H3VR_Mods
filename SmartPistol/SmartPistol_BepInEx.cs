using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
#if !DEBUG
    [BepInPlugin("h3vr.cityrobo.smartpistol", "SmartPistol Scripts", "1.0.0")]
    class SmartPistol_BepInEx : BaseUnityPlugin
    {
        public SmartPistol_BepInEx()
        {
            Logger.LogInfo("SmartPistol Scripts loaded!");
        }
    }
#endif
}
