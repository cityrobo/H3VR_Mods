#if !(DEBUG || MEATKIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.MagazineTape", "OpenScripts: MagazineTape Script", "1.0.0")]
    class MagazineTape_BepInEx : BaseUnityPlugin
    {
        public MagazineTape_BepInEx()
        {
            //Logger.LogInfo("OpenScripts: MagazineTape Script loaded!");
        }
    }
}
#endif