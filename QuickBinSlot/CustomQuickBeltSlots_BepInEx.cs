#if !(DEBUG || MEATKIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.custom_quickbelt_slots", "OpenScripts: CustomQuickBeltSlots Scripts", "1.0.0")]
    class MagazineTape_BepInEx : BaseUnityPlugin
    {
        public MagazineTape_BepInEx()
        {
            Logger.LogInfo("OpenScripts: CustomQuickBeltSlots Scripts loaded!");
        }
    }
}
#endif