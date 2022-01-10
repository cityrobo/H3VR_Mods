using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
#if!DEBUG
    [BepInPlugin("h3vr.cityrobo.openscripts.modify_weapon_cartride_and_magazine_attachment", "OpenScripts: ModifyWeaponCartrideAndMagazineAttachment Script", "1.0.0")]
    class ModifyWeaponCartrideAndMagazineAttachment_BepInEx : BaseUnityPlugin
    {
        public ModifyWeaponCartrideAndMagazineAttachment_BepInEx()
        {
            Logger.LogInfo("OpenScripts: ModifyWeaponCartrideAndMagazineAttachment Script loaded!");
        }
    }
#endif
}