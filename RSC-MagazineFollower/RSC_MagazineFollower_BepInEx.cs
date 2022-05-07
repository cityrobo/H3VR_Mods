#if ! (DEBUG || MEATKIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.rsc_magazine_follower", "RSC_MagazineFollower Script", "1.0.0")]
    class RSC_MagazineFollower_BepInEx : BaseUnityPlugin
    {
        public RSC_MagazineFollower_BepInEx()
        {
            Logger.LogInfo("RSC_MagazineFollower Scripts loaded!");
        }
    }
}
#endif