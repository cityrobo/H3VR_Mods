using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
#if !DEBUG
    [BepInPlugin("h3vr.cityrobo.fg42_selectorswitch", "FG-42 Scripts", "1.0.0")]
    class FG42_SelectorSwitch_BepInEx : BaseUnityPlugin
    {
        public FG42_SelectorSwitch_BepInEx()
        {
            Logger.LogInfo("FG-42 Scripts loaded!");
        }
    }
#endif
}
