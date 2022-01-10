using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.switch_blade", "SwitchBlade Script", "1.0.0")]
    class SwitchBlade_BepInEx : BaseUnityPlugin
    {
        public SwitchBlade_BepInEx()
        {
            Logger.LogInfo("SwitchBlade Script loaded!");
        }
    }
}
