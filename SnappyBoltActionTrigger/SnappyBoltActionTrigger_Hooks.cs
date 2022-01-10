using FistVR;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;

namespace Cityrobo
{
	public class SnappyBoltActionTrigger_Hooks
	{

		public SnappyBoltActionTrigger_Hooks(){}

        private void BoltActionRifle_UpdateInteraction(ILContext il)
        {
            ILCursor c = new(il);

            // Advance to right before these 5 instructions
            ILLabel? label = null;
            c.GotoNext(
                MoveType.Before,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld("FistVR.BoltActionRifle", "m_triggerFloat"),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld("FistVR.BoltActionRifle", "TriggerFiringThreshold"),
                i => i.MatchBltUn(out label)
            );

            // Remove the 5 instructions
            c.RemoveRange(5);

            // Emit out new instructions
            c.Emit(OpCodes.Ldarg_1);
            c.Emit(OpCodes.Ldflda, typeof(FVRViveHand).GetField("Input"));
            c.Emit(OpCodes.Ldfld, typeof(HandInput).GetField("TriggerDown"));
            c.Emit(OpCodes.Brfalse_S, label);
        }

        public void Hook()
		{
            IL.FistVR.BoltActionRifle.UpdateInteraction += BoltActionRifle_UpdateInteraction;
        }

		public void Unhook()
		{
            IL.FistVR.BoltActionRifle.UpdateInteraction -= BoltActionRifle_UpdateInteraction;
        }

	}
}
