#if !(DEBUG || MEATKIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.rifle_grenade_adapter", "OpenScripts: RifleGrenadeAdapter Scripts", "1.0.0")]
    class RifleGrenadeAdapter_BepInEx : BaseUnityPlugin
    {
        public RifleGrenadeAdapter_BepInEx()
        {
            Logger.LogInfo("OpenScripts: RifleGrenadeAdapter Script loaded!");
        }
    }
}
#endif