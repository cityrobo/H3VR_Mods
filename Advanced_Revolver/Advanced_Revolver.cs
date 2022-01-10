using System;
using System.Collections.Generic;
using UnityEngine;

namespace FistVR
{
	public class Advanced_Revolver : FVRFireArm
	{
        public override void Awake()
        {
            base.Awake();
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();
        }

        public override void BeginInteraction(FVRViveHand hand)
        {
            base.BeginInteraction(hand);
        }

        public override void EndInteraction(FVRViveHand hand)
        {
            base.EndInteraction(hand);
        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
        }
    }
}
