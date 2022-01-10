using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.shot_clock", "ShotClock Script", "1.0.0")]
    class ShotClock_BepInEx : BaseUnityPlugin
    {
        public ShotClock_BepInEx()
        {
            Logger.LogInfo("ShotClock Script loaded!");
        }
    }
}
