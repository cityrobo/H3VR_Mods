#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.weapon_enhancement_attachment", "WeaponEnhancementAttachment Script", "1.0.0")]
    class WeaponEnhancementAttachment_BepInEx : BaseUnityPlugin
    {
        public WeaponEnhancementAttachment_BepInEx()
        {
            Logger.LogInfo("JiggleBones Script loaded!");
        }
    }
}
#endif