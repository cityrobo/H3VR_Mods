#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.preattached_attachment", "PreattachedAttachment Script", "1.0.0")]
    class PreattachedAttachment_BepInEx : BaseUnityPlugin
    {
        public PreattachedAttachment_BepInEx()
        {
            Logger.LogInfo("PreattachedAttachment Script loaded!");
        }
    }
}
#endif
