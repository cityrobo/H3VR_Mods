#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.multiple_hide_on_attach", "OpenScripts: MultipleHideOnAttach Script", "1.0.0")]
    class MultipleHideOnAttach_BepInEx : BaseUnityPlugin
    {
        public MultipleHideOnAttach_BepInEx()
        {
            Logger.LogInfo("OpenScripts: MultipleHideOnAttach Script loaded!");
        }
    }
}
#endif