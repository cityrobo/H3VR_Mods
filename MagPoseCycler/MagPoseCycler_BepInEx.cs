#if!(DEBUG || MEATKIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.mag_pose_cycler", "OpenScripts: MagPoseCycler Script", "1.0.0")]
    class MagPoseCycler_BepInEx : BaseUnityPlugin
    {
        public MagPoseCycler_BepInEx()
        {
            Logger.LogInfo("OpenScripts: MagPoseCycler Script loaded!");
        }
    }
}
#endif