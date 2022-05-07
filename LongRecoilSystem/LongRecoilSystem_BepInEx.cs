#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.LongRecoilSystem", "OpenScripts: LongRecoilSystem Script", "1.0.0")]
    class LongRecoilSystem_BepInEx : BaseUnityPlugin
    {
        public LongRecoilSystem_BepInEx()
        {
            //Logger.LogInfo("OpenScripts: LongRecoilSystem Script loaded!");
        }
    }
}
#endif