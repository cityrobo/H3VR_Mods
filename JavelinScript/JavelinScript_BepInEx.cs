#if !DEBUG
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.JavelinScript", "Javelin Script", "1.0.0")]
    class JavelinScript_BepInEx : BaseUnityPlugin
    {
        public static ConfigEntry<float> MaxRange;
        public static ConfigEntry<float> MinRangeTopAttackMode;
        public static ConfigEntry<float> MinRangeDirectAttackMode;

        JavelinScript_BepInEx()
        {
            MaxRange = Config.Bind<float>("Javelin Settings", "Maximum range", 2000f, "Maximum target aquisition range for positions or AI.");
            MinRangeTopAttackMode = Config.Bind<float>("Javelin Settings", "Minimum range top attack", 150f, "Minimum range in top attack mode.");
            MinRangeDirectAttackMode = Config.Bind<float>("Javelin Settings", "Minimum range direct attack", 65f, "Minimum range in direct attack mode.");
        }
    }
}
#endif
