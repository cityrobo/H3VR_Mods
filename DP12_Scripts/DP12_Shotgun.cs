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

        private FVRFirearmMovingProxyRound m_secondProxyRound;
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
            m_secondProxyRound = gameObject.AddComponent<FVRFirearmMovingProxyRound>();
            m_secondProxyRound.Init(transform);

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
                if (HasExtractedRound() && m_isExtractedRoundOnLowerPath) mag.SecondMagazine.IsDropInLoadable = false;
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
        }

        private void TubeFedShotgun_EjectExtractedRound(On.FistVR.TubeFedShotgun.orig_EjectExtractedRound orig, TubeFedShotgun self)
        {
            orig(self);
            if (self == this)
            {
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
                if (m_secondProxyRound.IsFull)
                {
                    return;
                }
                if (!m_secondProxyRound.IsFull && mag.SecondMagazine.HasARound())
                {
                    GameObject fromPrefabReference = mag.SecondMagazine.RemoveRound(false);
                    m_secondProxyRound.SetFromPrefabReference(fromPrefabReference);
                    m_isExtractedRoundOnLowerPath = true;
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
                    m_isChamberRoundOnExtractor = true;
                }
                if (m_secondProxyRound.IsFull && !SecondChamber.IsFull && !m_isExtractedRoundOnLowerPath)
                {
                    m_isChamberRoundOnExtractor = true;
                    SecondChamber.SetRound(m_secondProxyRound.Round, false);
                    m_secondProxyRound.ClearProxy();
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
                if (m_secondProxyRound.IsFull && m_isExtractedRoundOnLowerPath && Magazine != null && Magazine is DP12_Mag mag)
                {
                    mag.SecondMagazine.AddRound(m_secondProxyRound.Round.RoundClass, false, true);
                    m_secondProxyRound.ClearProxy();
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
                    if (m_isChamberRoundOnExtractor)
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
                if (m_proxy.IsFull)
                {
                    if (m_isExtractedRoundOnLowerPath || Chamber.IsFull)
                    {
                        m_secondProxyRound.ProxyRound.position = Vector3.Lerp(Second_RoundPos_LowerPath_Rearward.position, Second_RoundPos_LowerPath_Forward.position, boltLerpBetweenLockAndFore);
                        m_secondProxyRound.ProxyRound.rotation = Quaternion.Slerp(Second_RoundPos_LowerPath_Rearward.rotation, Second_RoundPos_LowerPath_Forward.rotation, boltLerpBetweenLockAndFore);
                    }
                    else
                    {
                        m_secondProxyRound.ProxyRound.position = Vector3.Lerp(Second_RoundPos_UpperPath_Rearward.position, Second_RoundPos_UpperPath_Forward.position, boltLerpBetweenLockAndFore);
                        m_secondProxyRound.ProxyRound.rotation = Quaternion.Slerp(Second_RoundPos_UpperPath_Rearward.rotation, Second_RoundPos_UpperPath_Forward.rotation, boltLerpBetweenLockAndFore);
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

        public TubeFedShotgun CopyTubFedShotgun;
        [ContextMenu("Copy Shotgun")]
        public void CopyShotgun()
        {
            System.Type type = CopyTubFedShotgun.GetType();
            Component copy = this;
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(CopyTubFedShotgun));
            }
        }
    }
}
