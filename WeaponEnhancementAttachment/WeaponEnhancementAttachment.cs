using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FistVR
{
    public class WeaponEnhancementAttachment : FVRFireArmAttachment
    {

        private FVRFireArm firearm;
        private FVRLoadedRound[] loadedRounds;
        private FVRLoadedRound loadedRound;

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void FVRUpdate()
        {
            base.FVRUpdate();

            if (base.curMount != null)
            {
                firearm = (FVRFireArm) base.curMount.MyObject;
                loadedRounds = firearm.Magazine.LoadedRounds;

                loadedRound = loadedRounds.First();
                loadedRound.LR_Class = FireArmRoundClass.AP;
            }
        }


    }
}
