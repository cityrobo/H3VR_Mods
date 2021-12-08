using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.backpack_harness_enabler", "BackpackHarnessEnabler Script", "1.0.0")]
    public class BackpackHarnessEnabler : BaseUnityPlugin
    {
        private ConfigEntry<bool> allowBlindGrab;


        BackpackHarnessEnabler()
        {
            allowBlindGrab = Config.Bind<bool>("BackpackHarnessEnabler Settings", "allowBlindGrab", true, "allows the player to interact with the backpack blindly (base game only allows interaction when angle between head and backpack is less than 80 degrees)");

            Hook();
            Logger.LogInfo("BackpackHarnessEnabler Script loaded!");
        }
        public void OnDestroy()
        {
            Unhook();
        }

        private void Hook()
        {
            On.FistVR.PlayerBackPack.FVRUpdate += PlayerBackPack_FVRUpdate;
        }
        private void Unhook()
        {
            On.FistVR.PlayerBackPack.FVRUpdate -= PlayerBackPack_FVRUpdate;
        }

        private void PlayerBackPack_FVRUpdate(On.FistVR.PlayerBackPack.orig_FVRUpdate orig, PlayerBackPack self)
        {
            orig(self);
            if (self.QuickbeltSlot != null)
            {
                if (self.m_isHardnessed && self.IsHeld)
                {
                    self.SetUsable(true);
                }
            }
            else if (allowBlindGrab.Value)
            {
                Vector3 from = base.transform.position - GM.CurrentPlayerBody.Head.position;
                float num = Vector3.Angle(from, GM.CurrentPlayerBody.Head.forward);
                if (num > 80f)
                {
                    self.SetUsable(true);
                }
            }
        }
    }
}
