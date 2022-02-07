#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.jiggle_bones", "JiggleBones Script", "1.0.0")]
    class JiggleBones_BepInEx : BaseUnityPlugin
    {
        public JiggleBones_BepInEx()
        {
            Logger.LogInfo("JiggleBones Script loaded!");
        }
    }
}
#endif