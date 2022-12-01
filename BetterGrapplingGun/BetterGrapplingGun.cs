using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.BetterGrapplingGun", "Better GrapplingGun", "1.0.0")]
    public class BetterGrapplingGun : BaseUnityPlugin
    {
        // Config Entries
        private static ConfigEntry<float> grappleRange;
        private static ConfigEntry<float> grappleRetractRange;
        private static ConfigEntry<float> grapplePullForce;
        private static ConfigEntry<float> grappleHookSpeed;
        private static ConfigEntry<float> grappleRetractSpeed;
        private static ConfigEntry<float> grappleHandForce;
        private static ConfigEntry<float> grappleSosigPullForce;

        private static ConfigEntry<bool> useRange;

        //Default Game values to search for and replace
        private const float c_defaultRange = 15f;
        private const float c_defaultRetractRange = 25f;
        private const float c_defaultPullForce = 10f;
        private const float c_defaultHandForce = 0.25f;
        private const float c_defaultBoltSpeed = 70f;
        private const float c_defaultRetractSpeed = 80f;

        public BetterGrapplingGun()
        {
            grappleRange = Config.Bind("Grappling Gun", "GrapplingGunRange", 300f, "Range of the grappling gun.");
            useRange = Config.Bind("Grappling Gun", "UseRange", true, "If this is set to \"false\" the Grappling gun will fire despite no surface being in range. good for arching projectiles. (will still retract if hook reaches max range.)");
            grappleRetractRange = Config.Bind("Grappling Gun", "GrapplingMaximumRange", 350f, "Distance at which the hook will automatically retract.");
            grapplePullForce = Config.Bind("Grappling Gun", "GrapplingGunPullForce", 15f, "Force/Speed that you get pulled towards the grapple point on retract.");
            grappleHandForce = Config.Bind("Grappling Gun", "GrapplingHandForce", 0.25f, "Force/Speed that you can add by swinging your arm while retracting.");
            grappleHookSpeed = Config.Bind("Grappling Gun", "GrapplingHookSpeed", 70f, "Muzzle velocity of the fired hook projectiles.");
            grappleRetractSpeed = Config.Bind("Grappling Gun", "GrapplingRetractSpeed", 80f, "Speed that the hooks retract at.");
            grappleSosigPullForce = Config.Bind("Grappling Gun", "GrapplingGunSosigPullForce", 0.25f, "Force/Speed that you can pull Sosigs towards you, based on Hand movement.");

            grappleRange.SettingChanged += ConfigChanged;
            useRange.SettingChanged += ConfigChanged;
            grappleRetractRange.SettingChanged += ConfigChanged;
            grapplePullForce.SettingChanged += ConfigChanged;
            grappleHandForce.SettingChanged += ConfigChanged;
            grappleHookSpeed.SettingChanged += ConfigChanged;
            grappleRetractSpeed.SettingChanged += ConfigChanged;
            grappleSosigPullForce.SettingChanged += ConfigChanged;
        }

        private void Awake()
        {
            Hook();
        }

        private void OnDestroy()
        {
            Unhook();
        }
        private void Unhook()
        {
            IL.FistVR.GrappleGun.CanFireCheck -= GrappleGun_CanFireCheck;
            IL.FistVR.GrappleGun.AttemptRetract -= GrappleGun_AttemptRetract;
            IL.FistVR.GrappleGun.Fire -= GrappleGun_Fire;
            IL.FistVR.GrappleGunBolt.Fire -= GrappleGunBolt_Fire;
            IL.FistVR.GrappleGun.Retracting -= GrappleGun_Retracting;
            IL.FistVR.GrappleGun.LateUpdate -= GrappleGun_LateUpdate;
        }

        private void Hook()
        {
            IL.FistVR.GrappleGun.CanFireCheck += GrappleGun_CanFireCheck;
            IL.FistVR.GrappleGun.AttemptRetract += GrappleGun_AttemptRetract;
            IL.FistVR.GrappleGun.Fire += GrappleGun_Fire;
            IL.FistVR.GrappleGunBolt.Fire += GrappleGunBolt_Fire;
            IL.FistVR.GrappleGun.Retracting += GrappleGun_Retracting;
            IL.FistVR.GrappleGun.LateUpdate += GrappleGun_LateUpdate;
        }

        // Range Check Adjustment
        private void GrappleGun_CanFireCheck(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (useRange.Value)
            {
                c.GotoNext(
                    MoveType.Before,
                    i => i.MatchLdarg(0),
                    i => i.MatchLdfld<GrappleGun>(nameof(GrappleGun.CastRange))
                );
                c.RemoveRange(2);
                c.Emit(OpCodes.Ldc_R4, grappleRange.Value);
            }
            else
            {
                c.GotoNext(
                    MoveType.Before,
                    i => i.MatchLdarg(0),
                    i => i.MatchCallvirt<FVRFireArm>(nameof(FVRFireArm.GetMuzzle))
                    ); ;
                c.RemoveRange(15);
            }
        }
        // Retraction Speed Adjustment
        private void GrappleGun_LateUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdarg(0),
               i => i.MatchLdfld<GrappleGun>(nameof(GrappleGun.CastRange))
            );
            c.RemoveRange(2);
            c.Emit(OpCodes.Ldc_R4, grappleRange.Value);
        }
        // Pull and Hand Force Adjustment
        private void GrappleGun_AttemptRetract(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(c_defaultHandForce)
                );

            c.Next.Operand = grappleHandForce.Value;

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(c_defaultPullForce)
            );

            c.Next.Operand = grapplePullForce.Value;

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(c_defaultHandForce)
            );
            c.Next.Operand = grappleSosigPullForce.Value;

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(c_defaultHandForce)
            );
            c.Next.Operand = grappleSosigPullForce.Value;
        }
        // Retraction Speed Adjustment
        private void GrappleGun_Retracting(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(c_defaultRetractSpeed)
            );
            c.Next.Operand = grappleRetractSpeed.Value;
        }
        // Hook Speed Adjustment
        private void GrappleGun_Fire(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(1f)
            );

            c.Next.Operand = grappleHookSpeed.Value / c_defaultBoltSpeed;
        }
        // Hook Speed Adjustment
        private void GrappleGunBolt_Fire(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(c_defaultBoltSpeed)
            );

            c.Next.Operand = grappleHookSpeed.Value;

            c.GotoNext(
               MoveType.Before,
               i => i.MatchLdcR4(c_defaultBoltSpeed)
           );

            c.Next.Operand = grappleHookSpeed.Value;
        }

        private void ConfigChanged(object sender, EventArgs e)
        {
            Unhook();
            Hook();
        }
    }
}
