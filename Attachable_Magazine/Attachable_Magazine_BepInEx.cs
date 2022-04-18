#if!DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.attachable_magazine", "OpenScripts: Attachable_Magazine Script", "1.0.0")]
    class Attachable_Magazine_BepInEx : BaseUnityPlugin
    {
        public Attachable_Magazine_BepInEx()
        {
            Logger.LogInfo("OpenScripts: Attachable_Magazine Script loaded!");
        }
    }
}
#endif