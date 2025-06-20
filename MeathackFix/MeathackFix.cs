using BepInEx;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.MeathackFix", "Meathack Fix", "1.0.0")]
    public class MeathackFix : BaseUnityPlugin
    {
        public void Awake()
        {
            On.FistVR.AutoMeater.Start += AutoMeater_Start;
        }

        private void AutoMeater_Start(On.FistVR.AutoMeater.orig_Start orig, AutoMeater self)
        {
            orig(self);

            if (GM.TNH_Manager != null)
            {
                self.Priority.SetAllFriendly();
                self.Priority.MakeEnemy(GM.CurrentPlayerBody.GetPlayerIFF());
            }
        }
#if !DEBUG

#endif
    }
}
