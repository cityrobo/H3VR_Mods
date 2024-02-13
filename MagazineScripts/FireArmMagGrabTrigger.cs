using UnityEngine;
using FistVR;

namespace Cityrobo
{
	public class FireArmMagGrabTrigger : FVRInteractiveObject
	{
		public FVRFireArm FireArm;

#if !DEBUG

		public override void Awake()
		{
			gameObject.SetActive(false);
			OpenScripts2.UniversalMagazineGrabTrigger universalMagazineGrabTrigger = gameObject.AddComponent<OpenScripts2.UniversalMagazineGrabTrigger>();
            universalMagazineGrabTrigger.FireArm = FireArm;
			gameObject.SetActive(true);

			Destroy(this);
        }

        //public override bool IsInteractable()
        //{
        //	return !(FireArm.Magazine == null);
        //}

        //public override void BeginInteraction(FVRViveHand hand)
        //{
        //	base.BeginInteraction(hand);
        //	if (FireArm.Magazine != null)
        //	{
        //		EndInteraction(hand);
        //		FVRFireArmMagazine magazine = FireArm.Magazine;
        //		FireArm.EjectMag(false);
        //		hand.ForceSetInteractable(magazine);
        //		magazine.BeginInteraction(hand);
        //	}
        //}

        //public override void UpdateInteraction(FVRViveHand hand)
        //{
        //	base.UpdateInteraction(hand);
        //	if (hand.Input.TouchpadDown && FireArm.Magazine != null)
        //	{
        //		EndInteraction(hand);
        //		FVRFireArmMagazine magazine = FireArm.Magazine;
        //		FireArm.EjectMag(false);
        //		hand.ForceSetInteractable(magazine);
        //		magazine.BeginInteraction(hand);
        //	}
        //}
#endif
    }
}
