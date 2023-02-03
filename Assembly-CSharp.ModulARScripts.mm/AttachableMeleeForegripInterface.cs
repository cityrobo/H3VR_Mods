using FistVR;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ModulAR
{
    public class AttachableMeleeForegripInterface : AttachableForegrip
    {
#if !(UNITY_EDITOR || UNITY_5)
        /*
        public override void Awake()
        {
            base.Awake();

            IL.FistVR.FVRPhysicalObject.MeleeParams.FixedUpdate += MeleeParams_FixedUpdate;

            if (Attachment.MP.IsLongThrowable && !Attachment.IsHeld && !Attachment.RootRigidbody.isKinematic && Attachment.RootRigidbody.velocity.magnitude > 5f) 
            {
                Debug.Log("Boop");
            }
            Debug.Log("Beep");
        }

        private void MeleeParams_FixedUpdate(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<FVRPhysicalObject.MeleeParams>(nameof(FVRPhysicalObject.MeleeParams.m_obj)),
                i => i.MatchCallvirt<bool>("get_IsHeld")
            );

            throw new NotImplementedException();
        }
        */
        public override void OnAttach()
        {
            base.OnAttach();
            if (Attachment.curMount.GetRootMount().Parent is FVRFireArm)
            {
                FVRFireArm fvrfireArm = Attachment.curMount.GetRootMount().Parent as FVRFireArm;
                fvrfireArm.RegisterAttachedMeleeWeapon(Attachment as AttachableMeleeWeapon);
            }
        }

        public override void OnDetach()
        {
            if (Attachment.curMount.GetRootMount().Parent is FVRFireArm)
            {
                FVRFireArm fvrfireArm = Attachment.curMount.GetRootMount().Parent as FVRFireArm;
                fvrfireArm.RegisterAttachedMeleeWeapon(null);
            }
            base.OnDetach();
        }
#endif
    }
}