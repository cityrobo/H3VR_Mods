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
    [BepInPlugin("h3vr.cityrobo.HandsomeCheats", "Handsome Cheats", "1.0.0")]
    [BepInDependency(Sodalite.SodaliteConstants.Guid)]
    public class HandsomeCheats : BaseUnityPlugin
    {
        public void Awake()
        {
            Sodalite.Api.LeaderboardAPI.LeaderboardDisabled.TakeLock();
        }

        public void Update()
        {
            if (GM.CurrentSceneSettings != null)
            {
                // Enanbles infinite health and ammo
                GM.CurrentSceneSettings.IsAmmoInfinite = true;
                GM.CurrentSceneSettings.DoesDamageGetRegistered = false;
            }
        }
#if !DEBUG

#endif
    }
}
