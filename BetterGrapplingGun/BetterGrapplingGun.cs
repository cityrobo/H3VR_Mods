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
    [BepInPlugin("h3vr.cityrobo.bettergrapplinggun", "Better GrapplingGun", "1.0.0")]
    public class BetterGrapplingGun : BaseUnityPlugin
    {

        private ConfigEntry<float> grappleRange;
        private ConfigEntry<float> grappleRetractRange;
        private ConfigEntry<float> grapplePullForce;
        private ConfigEntry<float> grappleHookSpeed;
        private ConfigEntry<float> grappleRetractSpeed;
        private ConfigEntry<float> grappleHandForce;

        private ConfigEntry<bool> useRange;


        //Default Game values to search for and replace
        private float defaultRange = 15f;
        private float defaultRetractRange = 25f;
        private float defaultPullForce = 10f;
        private float defaultHandForce = 0.25f;
        private float defaultBoltSpeed = 35f;
        private float defaultRetractSpeed = 80f;

        public BetterGrapplingGun()
        {
            Logger.LogInfo("BetterGrapplingGun loading!");
            grappleRange = Config.Bind<float>("Grappling Gun", "GrapplingGunRange", 300f, "Range of the grappling gun.");
            useRange = Config.Bind<bool>("Grappling Gun", "UseRange", true, "If this is set to \"false\" the Grappling gun will fire despite no surface being in range. good for arching projectiles. (will still retract if hook reaches max range.)");
            grappleRetractRange = Config.Bind<float>("Grappling Gun", "GrapplingMaximumRange", 350f, "Distance at which the hook will automatically retract.");
            grapplePullForce = Config.Bind<float>("Grappling Gun", "GrapplingGunPullForce", 15f, "Force/Speed that you get pulled towards the grapple point on retract.");
            grappleHandForce = Config.Bind<float>("Grappling Gun", "GrapplingHandForce", 0.25f, "Force/Speed that you can add by swinging your arm while retracting.");
            grappleHookSpeed = Config.Bind<float>("Grappling Gun", "GrapplingHookSpeed", 35f, "Muzzle velocity of the fired hook projectiles.");
            grappleRetractSpeed = Config.Bind<float>("Grappling Gun", "GrapplingRetractSpeed", 80f, "Speed that the hooks retract at.");



            Hook();

            Logger.LogInfo("BetterGrapplingGun loaded!");
        }

        private void OnDestroy()
        {
            Unhook();
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

        private void GrappleGun_Retracting(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(defaultRetractSpeed)
            );
            c.Next.Operand = grappleRetractSpeed.Value;
        }

        private void GrappleGun_LateUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(defaultRetractRange)
            );

            c.Next.Operand = grappleRetractRange.Value;
        }


        private void GrappleGun_Fire(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(1f)
            );

            c.Next.Operand = grappleHookSpeed.Value / defaultBoltSpeed;
        }

        //Bolt Speed Adjustment
        private void GrappleGunBolt_Fire(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(defaultBoltSpeed)
            );

            c.Next.Operand = grappleHookSpeed.Value;

            c.GotoNext(
               MoveType.Before,
               i => i.MatchLdcR4(defaultBoltSpeed)
           );

            c.Next.Operand = grappleHookSpeed.Value;
        }

        //Pull and Hand Force Adjustment
        private void GrappleGun_AttemptRetract(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(defaultHandForce)
                );

            c.Next.Operand = grappleHandForce.Value;

            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdcR4(defaultPullForce)
            );

            c.Next.Operand = grapplePullForce.Value;
        }

        //Range Adjustment
        private void GrappleGun_CanFireCheck(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            if (useRange.Value)
            {
                c.GotoNext(
                    MoveType.Before,
                    i => i.MatchLdcR4(defaultRange)
                );
                c.Next.Operand = grappleRange.Value;
            }
            else
            {
                c.GotoNext(
                    MoveType.Before,
                    i => i.MatchLdarg(0),
                    i => i.MatchCallvirt<FistVR.FVRFireArm>(nameof(FistVR.FVRFireArm.GetMuzzle))
                    );;
                c.RemoveRange(14);
                /*c.Emit(OpCodes.Ldc_I4_1);
                c.Emit(OpCodes.Ret);*/
            }
        }

        private void Unhook()
        {
            IL.FistVR.GrappleGun.CanFireCheck -= GrappleGun_CanFireCheck;
            IL.FistVR.GrappleGun.AttemptRetract -= GrappleGun_AttemptRetract;
            IL.FistVR.GrappleGun.Fire -= GrappleGun_Fire;
            IL.FistVR.GrappleGunBolt.Fire -= GrappleGunBolt_Fire;
            IL.FistVR.GrappleGun.LateUpdate -= GrappleGun_LateUpdate;
        }
    }
}
