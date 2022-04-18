#if !(DEBUG || MEATKIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.movable_weapon_part", "MovableWeaponPart Script", "1.0.0")]
    class MovableWeaponPart_BepInEx : BaseUnityPlugin
    {
        public MovableWeaponPart_BepInEx()
        {
            Logger.LogInfo("MovableWeaponPart Script loaded!");
        }
    }
}
#endif