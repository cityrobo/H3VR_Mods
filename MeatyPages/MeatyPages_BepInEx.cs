#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.meaty_pages", "MeatyPages Script", "1.0.0")]
    class MeatBeatScanner_BepInEx : BaseUnityPlugin
    {
        public MeatBeatScanner_BepInEx()
        {
            Logger.LogInfo("MeatyPages Script loaded!");
        }
    }
}
#endif