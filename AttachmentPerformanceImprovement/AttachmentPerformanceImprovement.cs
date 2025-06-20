using BepInEx;
using FistVR;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Mono.Cecil.Cil;
using HarmonyLib;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.AttachmentPerformanceImprovement", "Attachment Performance Improvement", "1.0.0")]
    public class AttachmentPerformanceImprovement : BaseUnityPlugin
    {

#if !DEBUG
        public void Awake()
        {
            //IL.FistVR.FVRFireArmAttachment.AttachToMount += FVRFireArmAttachment_AttachToMount_IL;
            //On.FistVR.FVRFireArmAttachment.AttachToMount += FVRFireArmAttachment_AttachToMount_On;
            Harmony.CreateAndPatchAll(typeof(AttachmentPerformanceImprovement));
        }

        public void OnDestroy()
        {
            //IL.FistVR.FVRFireArmAttachment.AttachToMount -= FVRFireArmAttachment_AttachToMount_IL;
            //On.FistVR.FVRFireArmAttachment.AttachToMount -= FVRFireArmAttachment_AttachToMount_On;
        }

        //private void FVRFireArmAttachment_AttachToMount_On(On.FistVR.FVRFireArmAttachment.orig_AttachToMount orig, FVRFireArmAttachment self, FVRFireArmAttachmentMount m, bool playSound)
        //{
        //    orig(self, m, playSound);
        //    if (self.curMount.GetRootMount().ParentToThis)
        //    {
        //        self.SetParentage(self.curMount.GetRootMount().transform);
        //    }
        //    else
        //    {
        //        self.SetParentage(self.curMount.Parent.transform);
        //    }
        //}

        //private static void FVRFireArmAttachment_AttachToMount_IL(ILContext il)
        //{
        //    ILCursor c = new(il);

        //    c.GotoNext
        //    (
        //        MoveType.Before,
        //        i => i.MatchLdarg(0),
        //        i => i.MatchLdarg(0),
        //        i => i.MatchLdfld<FVRFireArmAttachment>(nameof(FVRFireArmAttachment.curMount)),
        //        i => i.MatchLdfld<FVRFireArmAttachmentMount>(nameof(FVRFireArmAttachmentMount.MyObject)),
        //        i => i.MatchCallvirt<Component>("get_transform")
        //    );
        //    c.GotoNext
        //    (
        //        MoveType.Before,
        //        i => i.MatchLdfld<FVRFireArmAttachmentMount>(nameof(FVRFireArmAttachmentMount.MyObject))
        //    );
        //    c.Remove();
        //    c.Emit<FVRFireArmAttachmentMount>(OpCodes.Ldfld, nameof(FVRFireArmAttachmentMount.Parent));
        //}

        [HarmonyPatch(typeof(FVRFireArmAttachment), nameof(FVRFireArmAttachment.AttachToMount))]
        [HarmonyPostfix]
        public static void FVRFireArmAttachment_AttachToMount_Harmony(FVRFireArmAttachment __instance)
        {
            if (__instance.curMount.GetRootMount().ParentToThis)
            {
                __instance.SetParentage(__instance.curMount.GetRootMount().transform);
            }
            else
            {
                __instance.SetParentage(__instance.curMount.Parent.transform);
            }
        }
#endif
    }
}
