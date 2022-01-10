using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
#if!DEBUG
    [BepInPlugin("h3vr.cityrobo.openscripts.manipulate_animator", "OpenScripts: Manipulate_Animator Script", "1.0.0")]
    class Manipulate_Animator_BepInEx : BaseUnityPlugin
    {
        public Manipulate_Animator_BepInEx()
        {
            Logger.LogInfo("OpenScripts: Manipulate_Animator Script loaded!");
        }
    }
#endif
}