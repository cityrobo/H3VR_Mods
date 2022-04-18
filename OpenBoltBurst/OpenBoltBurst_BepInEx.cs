#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.open_bolt_burst", "OpenBoltBurst Script", "1.0.0")]
    class OpenBoltBurst_BepInEx : BaseUnityPlugin
    {
        public OpenBoltBurst_BepInEx()
        {
            Logger.LogInfo("OpenBoltBurst Script loaded!");
        }
    }
}
#endif