#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.range_finder", "RangeFinder Script", "1.0.0")]
    class RangeFinder_BepInEx : BaseUnityPlugin
    {
        public RangeFinder_BepInEx()
        {
            Logger.LogInfo("RangeFinder Script loaded!");
        }
    }
}
#endif