#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.additional_barrel", "OpenScripts: AdditionalBarrel Script", "1.0.0")]
    class AdditionalBarrel_BepInEx : BaseUnityPlugin
    {
        public AdditionalBarrel_BepInEx()
        {
            Logger.LogInfo("OpenScripts: AdditionalBarrel Script loaded!");
        }
    }
}
#endif