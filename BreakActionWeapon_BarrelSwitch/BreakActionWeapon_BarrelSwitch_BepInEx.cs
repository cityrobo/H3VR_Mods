#if!DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.break_action_weapon_barrel_switch", "OpenScripts: BreakActionWeapon_BarrelSwitch Script", "1.0.0")]
    class BreakActionWeapon_BarrelSwitch_BepInEx : BaseUnityPlugin
    {
        public BreakActionWeapon_BarrelSwitch_BepInEx()
        {
            //Logger.LogInfo("OpenScripts: BreakActionWeapon_BarrelSwitch Script loaded!");
        }
    }
}
#endif