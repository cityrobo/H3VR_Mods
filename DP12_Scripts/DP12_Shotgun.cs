using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class DP12_Shotgun : TubeFedShotgun
    {
        [Header("DP-12 Config")]
        //public FVRFireArmMagazine SecondMagazineTube;
        public FVRFireArmChamber SecondChamber;
        public Transform SecondMuzzlePos;

        [Header("Second Round Config")]
        public Transform Second_RoundPos_LowerPath_Forward;
        public Transform Second_RoundPos_LowerPath_Rearward;
        public Transform Second_RoundPos_UpperPath_Forward;
        public Transform Second_RoundPos_UpperPath_Rearward;
        public Transform Second_RoundPos_Ejecting;
        public Transform Second_RoundPos_Ejection;
        public Vector3 Second_RoundEjectionSpeed;
        public Vector3 Second_RoundEjectionSpin;

        private FVRFirearmMovingProxyRound m_secondProxy;
        private bool m_isSecondExtractedRoundOnLowerPath = true;
        private bool m_isSecondChamberRoundOnExtractor = false;

        private enum EDP12State
        {
            FirstShot,
            SecondShot
        }
        private EDP12State _dP12State;

#if !(DEBUG || MEATKIT)
        public override void Awake()
        {
            base.Awake();
            FChambers.Add(SecondChamber);

            GameObject gameObject = new GameObject("m_secondProxyRound");
            m_secondProxy = gameObject.AddComponent<FVRFirearmMovingProxyRound>();
            m_secondProxy.Init(transform);

            Hook();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Unhook();
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();

            if (Magazine != null && Magazine is DP12_Mag mag)
            {
                if (HasExtractedRound() && m_isSecondExtractedRoundOnLowerPath) mag.SecondMagazine.IsDropInLoadable = false;
                else mag.SecondMagazine.IsDropInLoadable = true;
            }
        }

        public bool FireSecondBarrel()
        {
            if (!SecondChamber.Fire())
            {
                return false;
            }
            Fire(SecondChamber, GetMuzzle(), true, 1f, -1f);
            FireMuzzleSmoke();
            bool twoHandStabilized = IsTwoHandStabilized();
            bool foregripStabilized = IsForegripStabilized();
            bool shoulderStabilized = IsShoulderStabilized();
            Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
            PlayAudioGunShot(SecondChamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 1f);
            if (Mode == TubeFedShotgun.ShotgunMode.Automatic && SecondChamber.GetRound().IsHighPressure)
            {
                Bolt.ImpartFiringImpulse();
            }
            return true;
        }

        public override Transform GetMuzzle()
        {
            switch (_dP12State)
            {
                case EDP12State.FirstShot:
                    return MuzzlePos;
                case EDP12State.SecondShot:
                    return SecondMuzzlePos;
                default:
                    return MuzzlePos;
            }
        }

        public override List<FireArmRoundClass> GetChamberRoundList()
        {
            List<FireArmRoundClass> returnList = base.GetChamberRoundList();

            if (SecondChamber.IsFull && !SecondChamber.IsSpent)
            {
                returnList.Add(SecondChamber.GetRound().RoundClass);
            }
            return returnList;
        }

        public override void SetLoadedChambers(List<FireArmRoundClass> rounds)
        {
            base.SetLoadedChambers(rounds);

            if (rounds.Count > 1)
            {
                SecondChamber.Autochamber(rounds[0]);
            }
        }

        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            base.ConfigureFromFlagDic(f);

            if (Magazine != null && Magazine is DP12_Mag mag)
            {
                string key = "SecondMagazine";
                string value = string.Empty;

                string[] roundClassStrings;
                if (f.ContainsKey(key))
                {
                    value = f[key];

                    roundClassStrings = value.Split(';');

                    foreach (string roundClassString in roundClassStrings)
                    {
                        mag.SecondMagazine.AddRound((FireArmRoundClass)Enum.Parse(typeof(FireArmRoundClass), roundClassString), false, false);
                    }

                    mag.SecondMagazine.UpdateBulletDisplay();
                }
            }
        }

        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();

            if (Magazine != null && Magazine is DP12_Mag mag)
            {
                string key = "SecondMagazine";
                string value = string.Empty;

                if (mag.SecondMagazine.HasARound())
                {
                    value += mag.SecondMagazine.LoadedRounds[0].LR_Class.ToString();

                    for (int i = 1; i < mag.SecondMagazine.m_numRounds; i++)
                    {
                        value += ";" + mag.SecondMagazine.LoadedRounds[i].LR_Class.ToString();
                    }

                    flagDic.Add(key, value);
                }
            }
            return flagDic;
        }

        private void Unhook()
        {
            On.FistVR.TubeFedShotgun.EjectExtractedRound -= TubeFedShotgun_EjectExtractedRound;
            On.FistVR.TubeFedShotgun.ExtractRound -= TubeFedShotgun_ExtractRound;
            On.FistVR.TubeFedShotgun.ChamberRound -= TubeFedShotgun_ChamberRound;
            On.FistVR.TubeFedShotgun.ReturnCarrierRoundToMagazineIfRelevant -= TubeFedShotgun_ReturnCarrierRoundToMagazineIfRelevant;
            On.FistVR.TubeFedShotgun.UpdateDisplayRoundPositions -= TubeFedShotgun_UpdateDisplayRoundPositions;
            On.FistVR.TubeFedShotgun.ReleaseHammer -= TubeFedShotgun_ReleaseHammer;
            On.FistVR.TubeFedShotgun.CockHammer -= TubeFedShotgun_CockHammer;
            On.FistVR.TubeFedShotgun.TransferShellToUpperTrack -= TubeFedShotgun_TransferShellToUpperTrack;
            On.FistVR.TubeFedShotgun.HasExtractedRound -= TubeFedShotgun_HasExtractedRound;
            On.FistVR.TubeFedShotgun.UpdateCarrier -= TubeFedShotgun_UpdateCarrier;
            On.FistVR.FVRFireArmRound.DuplicateFromSpawnLock -= FVRFireArmRound_DuplicateFromSpawnLock;
            On.FistVR.FVRFireArmRound.GetNumRoundsPulled -= FVRFireArmRound_GetNumRoundsPulled;
        }
        private void Hook()
        {
            On.FistVR.TubeFedShotgun.EjectExtractedRound += TubeFedShotgun_EjectExtractedRound;
            On.FistVR.TubeFedShotgun.ExtractRound += TubeFedShotgun_ExtractRound;
            On.FistVR.TubeFedShotgun.ChamberRound += TubeFedShotgun_ChamberRound;
            On.FistVR.TubeFedShotgun.ReturnCarrierRoundToMagazineIfRelevant += TubeFedShotgun_ReturnCarrierRoundToMagazineIfRelevant;
            On.FistVR.TubeFedShotgun.UpdateDisplayRoundPositions += TubeFedShotgun_UpdateDisplayRoundPositions;
            On.FistVR.TubeFedShotgun.ReleaseHammer += TubeFedShotgun_ReleaseHammer;
            On.FistVR.TubeFedShotgun.CockHammer += TubeFedShotgun_CockHammer;
            On.FistVR.TubeFedShotgun.TransferShellToUpperTrack += TubeFedShotgun_TransferShellToUpperTrack;
            On.FistVR.TubeFedShotgun.HasExtractedRound += TubeFedShotgun_HasExtractedRound;
            On.FistVR.TubeFedShotgun.UpdateCarrier += TubeFedShotgun_UpdateCarrier;
            On.FistVR.FVRFireArmRound.DuplicateFromSpawnLock += FVRFireArmRound_DuplicateFromSpawnLock;
            On.FistVR.FVRFireArmRound.GetNumRoundsPulled += FVRFireArmRound_GetNumRoundsPulled;
        }

        // Patching FVRFireArmRound.GetNumRoundsPulled to make smart palming work correctly with the DP-12 magazine system
        private int FVRFireArmRound_GetNumRoundsPulled(On.FistVR.FVRFireArmRound.orig_GetNumRoundsPulled orig, FVRFireArmRound self, FVRViveHand hand)
        {
            if (hand.OtherHand.CurrentInteractable is DP12_Shotgun dp12)
            {
                int num = 0;
                if (dp12.RoundType == self.RoundType)
                {
                    DP12_Mag magazine = dp12.Magazine as DP12_Mag;
                    if (magazine != null)
                    {
                        num = magazine.m_capacity - magazine.m_numRounds;
                        num += magazine.SecondMagazine.m_capacity - magazine.SecondMagazine.m_numRounds;
                    }
                    for (int i = 0; i < dp12.GetChambers().Count; i++)
                    {
                        FVRFireArmChamber fvrfireArmChamber = dp12.GetChambers()[i];
                        if (fvrfireArmChamber.IsManuallyChamberable && (!fvrfireArmChamber.IsFull || fvrfireArmChamber.IsSpent))
                        {
                            num++;
                        }
                    }
                }
                if (num == 0)
                {
                    num = 1 + self.ProxyRounds.Count;
                }
                return num;
            }
            else return orig(self, hand);
        }

        // Patching FVRFireArmRound.DuplicateFromSpawnLock to make smart palming work correctly with the DP-12 magazine system
        private GameObject FVRFireArmRound_DuplicateFromSpawnLock(On.FistVR.FVRFireArmRound.orig_DuplicateFromSpawnLock orig, FVRFireArmRound self, FVRViveHand hand)
        {
            GameObject returnGO = orig(self, hand);

            FVRFireArmRound component = returnGO.GetComponent<FVRFireArmRound>();

            if (GM.Options.ControlOptions.SmartAmmoPalming == ControlOptions.SmartAmmoPalmingMode.Enabled && component != null && hand.OtherHand.CurrentInteractable != null)
            {
                int num = 0;
                if (hand.OtherHand.CurrentInteractable is DP12_Shotgun dp12)
                {
                    if (dp12.RoundType == self.RoundType)
                    {
                        DP12_Mag magazine = dp12.Magazine as DP12_Mag;
                        if (magazine != null)
                        {
                            num = magazine.m_capacity - magazine.m_numRounds;
                            num += magazine.SecondMagazine.m_capacity - magazine.SecondMagazine.m_numRounds;
                        }
                        for (int i = 0; i < dp12.GetChambers().Count; i++)
                        {
                            FVRFireArmChamber fvrfireArmChamber = dp12.GetChambers()[i];
                            if (fvrfireArmChamber.IsManuallyChamberable && (!fvrfireArmChamber.IsFull || fvrfireArmChamber.IsSpent))
                            {
                                num++;
                            }
                        }
                    }
                    if (num < 1)
                    {
                        num = self.ProxyRounds.Count;
                    }

                    component.DestroyAllProxies();
                    int num2 = Mathf.Min(self.ProxyRounds.Count, num - 1);
                    for (int k = 0; k < num2; k++)
                    {
                        component.AddProxy(self.ProxyRounds[k].Class, self.ProxyRounds[k].ObjectWrapper);
                    }
                    component.UpdateProxyDisplay();
                }
            }

            return returnGO;
        }

        private void TubeFedShotgun_UpdateCarrier(On.FistVR.TubeFedShotgun.orig_UpdateCarrier orig, TubeFedShotgun self)
        {
            if (self == this)
            {
                if (UsesAnimatedCarrier)
                {
                    if (IsHeld)
                    {
                        if (m_hand.OtherHand.CurrentInteractable != null)
                        {
                            if (m_hand.OtherHand.CurrentInteractable is FVRFireArmRound)
                            {
                                float num = Vector3.Distance(m_hand.OtherHand.CurrentInteractable.transform.position, GetClosestValidPoint(CarrierComparePoint1.position, CarrierComparePoint2.position, m_hand.OtherHand.CurrentInteractable.transform.position));
                                if (num < CarrierDetectDistance)
                                {
                                    m_tarCarrierRot = CarrierRots.y;
                                }
                                else
                                {
                                    m_tarCarrierRot = CarrierRots.x;
                                }
                            }
                            else
                            {
                                m_tarCarrierRot = CarrierRots.x;
                            }
                        }
                        else
                        {
                            m_tarCarrierRot = CarrierRots.x;
                        }
                    }
                    else
                    {
                        m_tarCarrierRot = CarrierRots.x;
                    }
                    if (HasExtractedRound() && !(m_isExtractedRoundOnLowerPath || m_isSecondExtractedRoundOnLowerPath))
                    {
                        m_tarCarrierRot = CarrierRots.y;
                    }
                    if (Mathf.Abs(m_curCarrierRot - m_tarCarrierRot) > 0.001f)
                    {
                        m_curCarrierRot = Mathf.MoveTowards(m_curCarrierRot, m_tarCarrierRot, 270f * Time.deltaTime);
                        Carrier.localEulerAngles = new Vector3(m_curCarrierRot, 0f, 0f);
                    }
                }
            }
            else orig(self);
        }

        private bool TubeFedShotgun_HasExtractedRound(On.FistVR.TubeFedShotgun.orig_HasExtractedRound orig, TubeFedShotgun self)
        {
            if (self == this) return m_proxy.IsFull || m_secondProxy.IsFull;
            else return orig(self);
        }

        private void TubeFedShotgun_TransferShellToUpperTrack(On.FistVR.TubeFedShotgun.orig_TransferShellToUpperTrack orig, TubeFedShotgun self)
        {
            orig(self);
            if (self == this)
            {
                if (m_secondProxy.IsFull && m_isSecondExtractedRoundOnLowerPath && !SecondChamber.IsFull)
                {
                    m_isSecondExtractedRoundOnLowerPath = false;
                }
            }
        }

        private void TubeFedShotgun_EjectExtractedRound(On.FistVR.TubeFedShotgun.orig_EjectExtractedRound orig, TubeFedShotgun self)
        {
            orig(self);
            if (self == this)
            {
                if (!m_isSecondChamberRoundOnExtractor)
                {
                    return;
                }
                m_isSecondChamberRoundOnExtractor = false;
                if (SecondChamber.IsFull)
                {
                    SecondChamber.EjectRound(Second_RoundPos_Ejection.position, transform.right * Second_RoundEjectionSpeed.x + transform.up * Second_RoundEjectionSpeed.y + transform.forward * Second_RoundEjectionSpeed.z, transform.right * Second_RoundEjectionSpin.x + transform.up * Second_RoundEjectionSpin.y + transform.forward * Second_RoundEjectionSpin.z, false);
                }
            }
        }

        private void TubeFedShotgun_ExtractRound(On.FistVR.TubeFedShotgun.orig_ExtractRound orig, TubeFedShotgun self)
        {
            orig(self);
            if (self == this)
            {
                DP12_Mag mag = null;
                if (Magazine != null && Magazine is DP12_Mag) mag = Magazine as DP12_Mag;
                if (mag == null)
                {
                    return;
                }
                if (m_secondProxy.IsFull)
                {
                    return;
                }
                if (!m_secondProxy.IsFull && mag.SecondMagazine.HasARound())
                {
                    GameObject fromPrefabReference = mag.SecondMagazine.RemoveRound(false);
                    m_secondProxy.SetFromPrefabReference(fromPrefabReference);
                    m_isSecondExtractedRoundOnLowerPath = true;
                }
            }
        }

        private bool TubeFedShotgun_ChamberRound(On.FistVR.TubeFedShotgun.orig_ChamberRound orig, TubeFedShotgun self)
        {
            bool returnBool = orig(self);
            if (self == this)
            {
                if (SecondChamber.IsFull)
                {
                    m_isSecondChamberRoundOnExtractor = true;
                }
                if (m_secondProxy.IsFull && !SecondChamber.IsFull && !m_isSecondExtractedRoundOnLowerPath)
                {
                    m_isSecondChamberRoundOnExtractor = true;
                    SecondChamber.SetRound(m_secondProxy.Round, false);
                    m_secondProxy.ClearProxy();
                    return true;
                }
            }
            return returnBool;
        }

        private bool TubeFedShotgun_ReturnCarrierRoundToMagazineIfRelevant(On.FistVR.TubeFedShotgun.orig_ReturnCarrierRoundToMagazineIfRelevant orig, TubeFedShotgun self)
        {
            bool origReturn = orig(self);
            if (self == this)
            {
                if (m_secondProxy.IsFull && m_isSecondExtractedRoundOnLowerPath && Magazine != null && Magazine is DP12_Mag mag)
                {
                    mag.SecondMagazine.AddRound(m_secondProxy.Round.RoundClass, false, true);
                    m_secondProxy.ClearProxy();
                    return true;
                }
            }
            return origReturn;
        }

        private void TubeFedShotgun_UpdateDisplayRoundPositions(On.FistVR.TubeFedShotgun.orig_UpdateDisplayRoundPositions orig, TubeFedShotgun self)
        {
            orig(self);
            if (self == this)
            {
                float boltLerpBetweenLockAndFore = Bolt.GetBoltLerpBetweenLockAndFore();
                if (SecondChamber.IsFull)
                {
                    if (m_isSecondChamberRoundOnExtractor)
                    {
                        SecondChamber.ProxyRound.position = Vector3.Lerp(Second_RoundPos_Ejecting.position, SecondChamber.transform.position, boltLerpBetweenLockAndFore);
                        SecondChamber.ProxyRound.rotation = Quaternion.Slerp(Second_RoundPos_Ejecting.rotation, SecondChamber.transform.rotation, boltLerpBetweenLockAndFore);
                    }
                    else
                    {
                        SecondChamber.ProxyRound.position = SecondChamber.transform.position;
                        SecondChamber.ProxyRound.rotation = SecondChamber.transform.rotation;
                    }
                }
                if (m_secondProxy.IsFull)
                {
                    if (m_isSecondExtractedRoundOnLowerPath || SecondChamber.IsFull)
                    {
                        m_secondProxy.ProxyRound.position = Vector3.Lerp(Second_RoundPos_LowerPath_Rearward.position, Second_RoundPos_LowerPath_Forward.position, boltLerpBetweenLockAndFore);
                        m_secondProxy.ProxyRound.rotation = Quaternion.Slerp(Second_RoundPos_LowerPath_Rearward.rotation, Second_RoundPos_LowerPath_Forward.rotation, boltLerpBetweenLockAndFore);
                    }
                    else
                    {
                        m_secondProxy.ProxyRound.position = Vector3.Lerp(Second_RoundPos_UpperPath_Rearward.position, Second_RoundPos_UpperPath_Forward.position, boltLerpBetweenLockAndFore);
                        m_secondProxy.ProxyRound.rotation = Quaternion.Slerp(Second_RoundPos_UpperPath_Rearward.rotation, Second_RoundPos_UpperPath_Forward.rotation, boltLerpBetweenLockAndFore);
                    }
                }
            }
        }

        private void TubeFedShotgun_CockHammer(On.FistVR.TubeFedShotgun.orig_CockHammer orig, TubeFedShotgun self)
        {
            orig(self);
            if (self == this)
            {
                _dP12State = EDP12State.FirstShot;
            }
        }

        private void TubeFedShotgun_ReleaseHammer(On.FistVR.TubeFedShotgun.orig_ReleaseHammer orig, TubeFedShotgun self)
        {
            if (self == this)
            {
                if (m_isHammerCocked && Bolt.CurPos == TubeFedShotgunBolt.BoltPos.Forward)
                {
                    PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
                    switch (_dP12State)
                    {
                        case EDP12State.FirstShot:
                            Fire();
                            m_isHammerCocked = true;
                            _dP12State = EDP12State.SecondShot;
                            break;
                        case EDP12State.SecondShot:
                            FireSecondBarrel();
                            m_isHammerCocked = false;
                            break;
                        default:
                            break;
                    }

                    if (HasHandle && !m_isHammerCocked && Mode == TubeFedShotgun.ShotgunMode.PumpMode && _dP12State == EDP12State.SecondShot)
                    {

                        Handle.UnlockHandle();
                    }
                }
            }
            else orig(self);
        }
#endif
        [Tooltip("Use this if you're working with a TubeFedMagazine prefab and don't feel like repopulating all the field of this script manually. Use the context menu after placing the TubeFedShotgun here.")]
        public TubeFedShotgun CopyTubeFedShotgun;
        [ContextMenu("Copy existing TubeFedShotgun Parameters")]
        public void CopyTubeFedShotgunParameters()
        {
            System.Type type = CopyTubeFedShotgun.GetType();
            Component copy = this;
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(CopyTubeFedShotgun));
            }
        }
    }
}
