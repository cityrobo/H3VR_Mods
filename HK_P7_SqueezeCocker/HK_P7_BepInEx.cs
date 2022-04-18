#if !(DEBUG || MEATKIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.hk_p7", "Heckler&Koch P7 Script", "1.0.0")]
    class HK_P7_BepInEx : BaseUnityPlugin
    {
        public HK_P7_BepInEx()
        {
            Logger.LogInfo("Heckler&Koch P7 Script loaded!");
        }
    }
}
#endif