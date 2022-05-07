#if!DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{

    [BepInPlugin("h3vr.cityrobo.openscripts.adjustable_reflexSight", "OpenScripts: Adjustable_ReflexSight Script", "1.1.0")]
    class Adjustable_ReflexSight_BepInEx : BaseUnityPlugin
    {
        public Adjustable_ReflexSight_BepInEx()
        {
            //Logger.LogInfo("OpenScripts: Adjustable_ReflexSight Script loaded!");
        }
    }
}
#endif