using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace Cityrobo
{
    public class WeaponEnhancementAttachment : FVRFireArmAttachment
    {
        public FireArmRoundClass roundClass = FireArmRoundClass.AP;

        private FVRFireArm firearm = null;
        private FireArmRoundClass origRoundClass;

#if!(UNITY_EDITOR || UNITY_5)
        public override void FVRUpdate()
        {
            base.FVRUpdate();

            if (base.curMount != null)
            {
                firearm = base.curMount.GetRootMount().MyObject as FVRFireArm;
                if (firearm != null)
                {
                    FVRFireArmChamber chamber = GetChamber();
                    if (chamber != null && chamber.m_round != null && chamber.m_round.RoundClass != roundClass)
                    {
                        origRoundClass = chamber.m_round.RoundClass;

                        chamber.m_round = AM.GetRoundSelfPrefab(chamber.m_round.RoundType, roundClass).GetGameObject().GetComponent<FVRFireArmRound>();
                        chamber.UpdateProxyDisplay();
                    }
                }
            }
            else if (firearm != null)
            {
                FVRFireArmChamber chamber = GetChamber();
                if (chamber != null && chamber.m_round != null)
                {
                    chamber.m_round = AM.GetRoundSelfPrefab(chamber.m_round.RoundType, origRoundClass).GetGameObject().GetComponent<FVRFireArmRound>();
                    chamber.UpdateProxyDisplay();
                }
                firearm = null;
            }
        }

        FVRFireArmChamber GetChamber()
        {
            switch (firearm)
            {
                case ClosedBoltWeapon w:
                    return w.Chamber;
                case OpenBoltReceiver w:
                    return w.Chamber;
                case Handgun w:
                    return w.Chamber;
                case Revolver w:
                    return w.Chambers[w.CurChamber];
                case BoltActionRifle w:
                    return w.Chamber;
                case SingleActionRevolver w:
                    return w.Cylinder.Chambers[w.CurChamber];
                default:
                    return null;
            }
        }
#endif
    }
}