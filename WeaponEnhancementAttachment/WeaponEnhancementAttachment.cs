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

        private FVRFireArm _fireArm = null;
        private FireArmRoundClass _origRoundClass;

#if!(UNITY_EDITOR || UNITY_5)
        public override void FVRUpdate()
        {
            base.FVRUpdate();

            if (base.curMount != null)
            {
                _fireArm = base.curMount.GetRootMount().MyObject as FVRFireArm;
                if (_fireArm != null)
                {
                    FVRFireArmChamber chamber = GetCurentChamber();
                    if (chamber != null && chamber.m_round != null && chamber.m_round.RoundClass != roundClass)
                    {
                        _origRoundClass = chamber.m_round.RoundClass;

                        chamber.m_round = AM.GetRoundSelfPrefab(chamber.m_round.RoundType, roundClass).GetGameObject().GetComponent<FVRFireArmRound>();
                        chamber.UpdateProxyDisplay();
                    }
                }
            }
            else if (_fireArm != null)
            {
                FVRFireArmChamber chamber = GetCurentChamber();
                if (chamber != null && chamber.m_round != null)
                {
                    chamber.m_round = AM.GetRoundSelfPrefab(chamber.m_round.RoundType, _origRoundClass).GetGameObject().GetComponent<FVRFireArmRound>();
                    chamber.UpdateProxyDisplay();
                }
                _fireArm = null;
            }
        }

        FVRFireArmChamber GetCurentChamber()
        {
            switch (_fireArm)
            {
                case Handgun w:
                    return w.Chamber;
                case ClosedBoltWeapon w:
                    return w.Chamber;
                case OpenBoltReceiver w:
                    return w.Chamber;
                case TubeFedShotgun w:
                    return w.Chamber;
                case BoltActionRifle w:
                    return w.Chamber;
                case BreakActionWeapon w:
                    return w.Barrels[w.m_curBarrel].Chamber;
                case Revolver w:
                    return w.Chambers[w.CurChamber];
                case SingleActionRevolver w:
                    return w.Cylinder.Chambers[w.CurChamber];
                case RevolvingShotgun w:
                    return w.Chambers[w.CurChamber];
                case Flaregun w:
                    return w.Chamber;
                case RollingBlock w:
                    return w.Chamber;
                case Derringer w:
                    return w.Barrels[w.m_curBarrel].Chamber;
                case LAPD2019 w:
                    return w.Chambers[w.CurChamber];
                case BAP w:
                    return w.Chamber;
                case HCB w:
                    return w.Chamber;
                case M72 w:
                    return w.Chamber;
                case MF2_RL w:
                    return w.Chamber;
                case RGM40 w:
                    return w.Chamber;
                case RPG7 w:
                    return w.Chamber;
                case SimpleLauncher w:
                    return w.Chamber;
                case SimpleLauncher2 w:
                    return w.Chamber;
                case RemoteMissileLauncher w:
                    return w.Chamber;
                case PotatoGun w:
                    return w.Chamber;
                case GrappleGun w:
                    return w.Chambers[w.m_curChamber];
                default:
                    if (_fireArm.FChambers.Count > 0) return _fireArm.FChambers[0];
                    else return null;
            }
        }
#endif
    }
}