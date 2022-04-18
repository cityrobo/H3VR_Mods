using UnityEngine;
using FistVR;

namespace Cityrobo
{
	public class FireArmMagGrabTrigger : FVRInteractiveObject
	{
		public FVRFireArm FireArm;

#if !DEBUG
		public override bool IsInteractable()
		{
			return !(this.FireArm.Magazine == null);
		}

		public override void BeginInteraction(FVRViveHand hand)
		{
			base.BeginInteraction(hand);
			if (this.FireArm.Magazine != null)
			{
				this.EndInteraction(hand);
				FVRFireArmMagazine magazine = this.FireArm.Magazine;
				this.FireArm.EjectMag(false);
				hand.ForceSetInteractable(magazine);
				magazine.BeginInteraction(hand);
			}
		}

		public override void UpdateInteraction(FVRViveHand hand)
		{
			base.UpdateInteraction(hand);
			if (hand.Input.TouchpadDown && this.FireArm.Magazine != null)
			{
				this.EndInteraction(hand);
				FVRFireArmMagazine magazine = this.FireArm.Magazine;
				this.FireArm.EjectMag(false);
				hand.ForceSetInteractable(magazine);
				magazine.BeginInteraction(hand);
			}
		}
	}
#endif
}
