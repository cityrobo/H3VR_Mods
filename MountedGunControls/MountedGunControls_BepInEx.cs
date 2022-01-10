using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
#if!DEBUG
    [BepInPlugin("h3vr.cityrobo.openscripts.mounted_gun_controls", "OpenScripts: MountedGunControls Script", "1.0.0")]
    class MountedGunControls_BepInEx : BaseUnityPlugin
    {
        public MountedGunControls_BepInEx()
        {
            Logger.LogInfo("OpenScripts: MountedGunControls Script loaded!");
        }
    }
#endif
}