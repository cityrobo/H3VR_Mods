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
    public class DP12_BoltAction : BoltActionRifle
    {
        [Header("DP-12 Config")]
        public FVRFireArmChamber SecondChamber;
        public Transform SecondMuzzlePos;

        [Header("Replacement Round Ejection Paramenters")]
        public Vector3 RoundEjectionSpeed;
        public Vector3 RoundEjectionSpin;

        [Header("Second Round Config")]
        public Transform Second_Extraction_MagazinePos;
        public Transform Second_Extraction_ChamberPos;
        public Transform Second_Extraction_Ejecting;
        public Transform Second_EjectionPos;

        public Vector3 Second_RoundEjectionSpeed;
        public Vector3 Second_RoundEjectionSpin;

        [Header("Second Hammer Config")]
        public bool HasSecondVisualHammer = false;
        public Transform Second_Hammer;
        public float Second_HammerUncocked;
        public float Second_HammerCocked;

        private FVRFirearmMovingProxyRound m_secondProxyRound;
        private bool _hasHammerDropped = false;

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
            if (PlaysExtraTailOnShot)
            {
                AudioEvent tailSet = SM.GetTailSet(ExtraTail, GM.CurrentPlayerBody.GetCurrentSoundEnvironment());
                m_pool_tail.PlayClipVolumePitchOverride(tailSet, transform.position, tailSet.VolumeRange * 1f, AudioClipSet.TailPitchMod_Main * tailSet.PitchRange.x, null);
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
                string key = "SecondMagazineChamber";
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

            string key = "SecondMagazineChamber";
            string value = string.Empty;

            if (Magazine != null && Magazine is DP12_Mag mag && mag.SecondMagazine.HasARound())
            {
                value += mag.SecondMagazine.LoadedRounds[0].LR_Class.ToString();

                for (int i = 1; i < mag.SecondMagazine.m_numRounds; i++)
                {
                    value += ";" + mag.SecondMagazine.LoadedRounds[i].LR_Class.ToString();
                }

                flagDic.Add(key, value);
            }
            return flagDic;
        }

        private void Unhook()
        {
            On.FistVR.BoltActionRifle.HasExtractedRound -= BoltActionRifle_HasExtractedRound;
            On.FistVR.BoltActionRifle.CockHammer -= BoltActionRifle_CockHammer;
            On.FistVR.BoltActionRifle.DropHammer -= BoltActionRifle_DropHammer;
            On.FistVR.BoltActionRifle.UpdateBolt -= BoltActionRifle_UpdateBolt;
            On.FistVR.BoltActionRifle.UpdateInteraction -= BoltActionRifle_UpdateInteraction; 
            On.FistVR.FVRFireArmRound.DuplicateFromSpawnLock -= FVRFireArmRound_DuplicateFromSpawnLock;
            On.FistVR.FVRFireArmRound.GetNumRoundsPulled -= FVRFireArmRound_GetNumRoundsPulled;
        }
        private void Hook()
        {
            On.FistVR.BoltActionRifle.HasExtractedRound += BoltActionRifle_HasExtractedRound;
            On.FistVR.BoltActionRifle.CockHammer += BoltActionRifle_CockHammer;
            On.FistVR.BoltActionRifle.DropHammer += BoltActionRifle_DropHammer;
            On.FistVR.BoltActionRifle.UpdateBolt += BoltActionRifle_UpdateBolt;
            On.FistVR.BoltActionRifle.UpdateInteraction += BoltActionRifle_UpdateInteraction;
            On.FistVR.FVRFireArmRound.DuplicateFromSpawnLock += FVRFireArmRound_DuplicateFromSpawnLock;
            On.FistVR.FVRFireArmRound.GetNumRoundsPulled += FVRFireArmRound_GetNumRoundsPulled;
        }

        private int FVRFireArmRound_GetNumRoundsPulled(On.FistVR.FVRFireArmRound.orig_GetNumRoundsPulled orig, FVRFireArmRound self, FVRViveHand hand)
        {
            if (hand.OtherHand.CurrentInteractable is DP12_BoltAction dp12_boltaction)
            {
                int num = 0;
                if (dp12_boltaction.RoundType == self.RoundType)
                {
                    DP12_Mag magazine = dp12_boltaction.Magazine as DP12_Mag;
                    if (magazine != null)
                    {
                        num = magazine.m_capacity - magazine.m_numRounds;
                        num += magazine.SecondMagazine.m_capacity - magazine.SecondMagazine.m_numRounds;
                    }
                    for (int i = 0; i < dp12_boltaction.GetChambers().Count; i++)
                    {
                        FVRFireArmChamber fvrfireArmChamber = dp12_boltaction.GetChambers()[i];
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

        // Patching FVRFireArmRound.DuplicateFromSpawnLock to make smart palming work correctly with the dp12 magazine system
        private GameObject FVRFireArmRound_DuplicateFromSpawnLock(On.FistVR.FVRFireArmRound.orig_DuplicateFromSpawnLock orig, FVRFireArmRound self, FVRViveHand hand)
        {
            GameObject returnGO = orig(self, hand);

            FVRFireArmRound component = returnGO.GetComponent<FVRFireArmRound>();

            if (GM.Options.ControlOptions.SmartAmmoPalming == ControlOptions.SmartAmmoPalmingMode.Enabled && component != null && hand.OtherHand.CurrentInteractable != null)
            {
                int num = 0;
                if (hand.OtherHand.CurrentInteractable is DP12_BoltAction dp12_boltaction)
                {
                    if (dp12_boltaction.RoundType == self.RoundType)
                    {
                        DP12_Mag magazine = dp12_boltaction.Magazine as DP12_Mag;
                        if (magazine != null)
                        {
                            num = magazine.m_capacity - magazine.m_numRounds;
                            num += magazine.SecondMagazine.m_capacity - magazine.SecondMagazine.m_numRounds;
                        }
                        for (int i = 0; i < dp12_boltaction.GetChambers().Count; i++)
                        {
                            FVRFireArmChamber fvrfireArmChamber = dp12_boltaction.GetChambers()[i];
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

        private void BoltActionRifle_UpdateInteraction(On.FistVR.BoltActionRifle.orig_UpdateInteraction orig, BoltActionRifle self, FVRViveHand hand)
        {
            orig(self, hand);

            if (_hasHammerDropped && m_triggerFloat <= TriggerResetThreshold) _hasHammerDropped = false;
        }

        private void BoltActionRifle_UpdateBolt(On.FistVR.BoltActionRifle.orig_UpdateBolt orig, BoltActionRifle self, BoltActionRifle_Handle.BoltActionHandleState State, float lerp)
        {
            if (self == this)
            {
                CurBoltHandleState = State;
                BoltLerp = lerp;
                if (CurBoltHandleState != BoltActionRifle_Handle.BoltActionHandleState.Forward && !m_proxy.IsFull && !Chamber.IsFull)
                {
                    Chamber.IsAccessible = true;
                }
                else
                {
                    Chamber.IsAccessible = false;
                }
                if (CurBoltHandleState != BoltActionRifle_Handle.BoltActionHandleState.Forward && !m_secondProxyRound.IsFull && !SecondChamber.IsFull)
                {
                    SecondChamber.IsAccessible = true;
                }
                else
                {
                    SecondChamber.IsAccessible = false;
                }
                if (UsesClips && ClipTrigger != null)
                {
                    if (CurBoltHandleState == BoltActionRifle_Handle.BoltActionHandleState.Rear)
                    {
                        if (!ClipTrigger.activeSelf)
                        {
                            ClipTrigger.SetActive(true);
                        }
                    }
                    else if (ClipTrigger.activeSelf)
                    {
                        ClipTrigger.SetActive(false);
                    }
                }
                if (CurBoltHandleState == BoltActionRifle_Handle.BoltActionHandleState.Rear && LastBoltHandleState != BoltActionRifle_Handle.BoltActionHandleState.Rear)
                {
                    if (CockType == BoltActionRifle.HammerCockType.OnBack)
                    {
                        CockHammer();
                    }
                    if (Chamber.IsFull || SecondChamber.IsFull) PlayAudioEvent(FirearmAudioEventType.HandleBack, 1f);
                    else PlayAudioEvent(FirearmAudioEventType.HandleBackEmpty, 1f);
                    if (Chamber.IsFull)
                    {
                        Chamber.EjectRound(EjectionPos.position, transform.right * RoundEjectionSpeed.x + transform.up * RoundEjectionSpeed.y + transform.forward * RoundEjectionSpeed.z, transform.right * RoundEjectionSpin.x + transform.up * RoundEjectionSpin.y + transform.forward * RoundEjectionSpin.z, false);
                    }
                    if (SecondChamber.IsFull)
                    {
                        SecondChamber.EjectRound(Second_EjectionPos.position, transform.right * Second_RoundEjectionSpeed.x + transform.up * Second_RoundEjectionSpeed.y + transform.forward * Second_RoundEjectionSpeed.z, transform.right * Second_RoundEjectionSpin.x + transform.up * Second_RoundEjectionSpin.y + transform.forward * Second_RoundEjectionSpin.z, false);
                    }

                    BoltMovingForward = true;
                }
                else if (CurBoltHandleState == BoltActionRifle_Handle.BoltActionHandleState.Forward && LastBoltHandleState != BoltActionRifle_Handle.BoltActionHandleState.Forward)
                {
                    if (CockType == BoltActionRifle.HammerCockType.OnForward)
                    {
                        CockHammer();
                    }

                    if ((m_proxy.IsFull && !Chamber.IsFull) || (m_secondProxyRound.IsFull && !SecondChamber.IsFull)) PlayAudioEvent(FirearmAudioEventType.HandleForward, 1f);
                    else PlayAudioEvent(FirearmAudioEventType.HandleForwardEmpty, 1f);
                    if (m_proxy.IsFull && !Chamber.IsFull)
                    {
                        Chamber.SetRound(m_proxy.Round, false);
                        m_proxy.ClearProxy();
                        
                    }
                    if (m_secondProxyRound.IsFull && !SecondChamber.IsFull)
                    {
                        SecondChamber.SetRound(m_secondProxyRound.Round, false);
                        m_secondProxyRound.ClearProxy();

                    }
                    BoltMovingForward = false;
                }
                else if (CurBoltHandleState == BoltActionRifle_Handle.BoltActionHandleState.Mid && LastBoltHandleState == BoltActionRifle_Handle.BoltActionHandleState.Rear && Magazine != null)
                {
                    if (!m_proxy.IsFull && Magazine.HasARound() && !Chamber.IsFull)
                    {
                        GameObject fromPrefabReference = Magazine.RemoveRound(false);
                        m_proxy.SetFromPrefabReference(fromPrefabReference);
                    }

                    DP12_Mag mag = Magazine as DP12_Mag;
                    FVRFireArmMagazine secondMag = mag.SecondMagazine;
                    if (!m_secondProxyRound.IsFull && secondMag.HasARound() && !SecondChamber.IsFull)
                    {
                        GameObject fromPrefabReference = secondMag.RemoveRound(false);
                        m_secondProxyRound.SetFromPrefabReference(fromPrefabReference);
                    }
                    if (EjectsMagazineOnEmpty && !Magazine.HasARound() && !secondMag.HasARound())
                    {
                        EjectMag(false);
                    }
                }
                if (m_proxy.IsFull)
                {
                    m_proxy.ProxyRound.position = Vector3.Lerp(Extraction_ChamberPos.position, Extraction_MagazinePos.position, BoltLerp);
                    m_proxy.ProxyRound.rotation = Quaternion.Slerp(Extraction_ChamberPos.rotation, Extraction_MagazinePos.rotation, BoltLerp);
                }
                if (m_secondProxyRound.IsFull)
                {
                    m_secondProxyRound.ProxyRound.position = Vector3.Lerp(Second_Extraction_ChamberPos.position, Second_Extraction_MagazinePos.position, BoltLerp);
                    m_secondProxyRound.ProxyRound.rotation = Quaternion.Slerp(Second_Extraction_ChamberPos.rotation, Second_Extraction_MagazinePos.rotation, BoltLerp);
                }
                if (Chamber.IsFull)
                {
                    Chamber.ProxyRound.position = Vector3.Lerp(Extraction_ChamberPos.position, Extraction_Ejecting.position, BoltLerp);
                    Chamber.ProxyRound.rotation = Quaternion.Slerp(Extraction_ChamberPos.rotation, Extraction_Ejecting.rotation, BoltLerp);
                }
                if (SecondChamber.IsFull)
                {
                    SecondChamber.ProxyRound.position = Vector3.Lerp(Second_Extraction_ChamberPos.position, Second_Extraction_Ejecting.position, BoltLerp);
                    SecondChamber.ProxyRound.rotation = Quaternion.Slerp(Second_Extraction_ChamberPos.rotation, Second_Extraction_Ejecting.rotation, BoltLerp);
                }
                LastBoltHandleState = CurBoltHandleState;
            }
            else orig(self, State, lerp);
        }

        private void BoltActionRifle_DropHammer(On.FistVR.BoltActionRifle.orig_DropHammer orig, BoltActionRifle self)
        {
            if (self == this)
            {
                if (m_isHammerCocked && !_hasHammerDropped)
                {
                    PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
                    switch (_dP12State)
                    {
                        case EDP12State.FirstShot:
                            Fire();
                            m_isHammerCocked = true;
                            _dP12State = EDP12State.SecondShot;
                            if (HasVisualHammer && HasSecondVisualHammer)
                            {
                                SetAnimatedComponent(Hammer, HammerUncocked, FVRPhysicalObject.InterpStyle.Translate, FVRPhysicalObject.Axis.Z);
                            }
                            break;
                        case EDP12State.SecondShot:
                            FireSecondBarrel();
                            m_isHammerCocked = false;
                            if (HasSecondVisualHammer)
                            {
                                SetAnimatedComponent(Second_Hammer, Second_HammerUncocked, FVRPhysicalObject.InterpStyle.Translate, FVRPhysicalObject.Axis.Z);
                            }
                            else if (HasVisualHammer)
                            {
                                SetAnimatedComponent(Hammer, HammerUncocked, FVRPhysicalObject.InterpStyle.Translate, FVRPhysicalObject.Axis.Z);
                            }
                            break;
                        default:
                            break;
                    }

                    _hasHammerDropped = true;
                }
            }
            else orig(self);
        }

        private void BoltActionRifle_CockHammer(On.FistVR.BoltActionRifle.orig_CockHammer orig, BoltActionRifle self)
        {
            orig(self);
            if (self == this)
            {
                _dP12State = EDP12State.FirstShot;

                if (HasSecondVisualHammer)
                {
                    SetAnimatedComponent(Second_Hammer, Second_HammerCocked, FVRPhysicalObject.InterpStyle.Translate, FVRPhysicalObject.Axis.Z);
                }
            }
        }

        private bool BoltActionRifle_HasExtractedRound(On.FistVR.BoltActionRifle.orig_HasExtractedRound orig, BoltActionRifle self)
        {
            bool returnValue = orig(self);

            if (self == this)
            {
                if (m_secondProxyRound.IsFull) return true;
            }

            return returnValue;
        }
#endif

        [Tooltip("Use this if you're working with a BoltActionRifle prefab and don't feel like repopulating all the field of this script manually. Use the context menu after placing the BoltActionRifle here.")]
        public BoltActionRifle CopyBoltActionRifle;
        [ContextMenu("Copy existing BoltActionRifle Parameters")]
        public void CopyBoltActionRifleParameters()
        {
            System.Type type = CopyBoltActionRifle.GetType();
            Component copy = this;
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(CopyBoltActionRifle));
            }

            RoundEjectionSpeed.x = RightwardEjectionForce;
            RoundEjectionSpeed.y = UpwardEjectionForce;
            RoundEjectionSpin.y = YSpinEjectionTorque;
        }
    }
}
