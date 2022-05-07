#if !(DEBUG || MEATKIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.magazine_scripts", "OpenScripts: MagazineScripts Scripts", "1.0.0")]
    class MagazineScripts_BepInEx : BaseUnityPlugin
    {
        public MagazineScripts_BepInEx()
        {
            //Logger.LogInfo("OpenScripts: MagazineScripts Script loaded!");
        }
    }
}
#endif