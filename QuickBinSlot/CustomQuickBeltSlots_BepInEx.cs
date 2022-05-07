#if !(DEBUG || MEATKIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.CustomQuickBeltSlots", "OpenScripts: CustomQuickBeltSlots Scripts", "1.0.0")]
    class CustomQuickBeltSlots_BepInEx : BaseUnityPlugin
    {
        public CustomQuickBeltSlots_BepInEx()
        {
            //Logger.LogInfo("OpenScripts: CustomQuickBeltSlots Scripts loaded!");
        }
    }
}
#endif