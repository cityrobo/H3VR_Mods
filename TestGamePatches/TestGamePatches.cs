using BepInEx;
using BepInEx.Configuration;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using MonoMod.Cil;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.TestGamePatches", "TestGamePatches", "1.0.0")]
    public class TestGamePatches : BaseUnityPlugin
    {
        private static ConfigEntry<float> ReticleScale;

        public void Awake()
        {
            On.HolographicSight.OnEnable += HolographicSight_OnEnable;

            ReticleScale = Config.Bind("TestGamePatches", "ReticleScaleMultiplier", 0.1f);
            IL.ScopeCam.OnWillRenderObject += ScopeCam_OnWillRenderObject;
        }

        private void HolographicSight_OnEnable(On.HolographicSight.orig_OnEnable orig, HolographicSight self)
        {
            self.Scale *= ReticleScale.Value;
            orig(self);
        }

        private void ScopeCam_OnWillRenderObject(ILContext il)
        {
            ILCursor c = new(il);
            c.GotoNext
                (
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<ScopeCam>(nameof(ScopeCam.ScopeCamera)),
                i => i.MatchCallvirt<Component>("get_transform"),
                i => i.MatchCallvirt<Transform>("get_position"),
                i => i.MatchLdarg(0),
                i => i.MatchCall<Component>("get_transform"),
                i => i.MatchCallvirt<Transform>("get_forward")
                );
            c.Next.Operand = 0.011f;
        }
#if !DEBUG

#endif
    }
}
