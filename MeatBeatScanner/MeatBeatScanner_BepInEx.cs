#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.meat_beat_scanner", "MeatBeatScanner Script", "1.0.0")]
    class MeatBeatScanner_BepInEx : BaseUnityPlugin
    {
        public MeatBeatScanner_BepInEx()
        {
            Logger.LogInfo("MeatBeatScanner Script loaded!");
        }
    }
}
#endif