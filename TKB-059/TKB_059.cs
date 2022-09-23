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
    public class TKB_059 : ClosedBoltWeapon
    {
        [Header("TKB-059 Config")]
        public Transform leftMuzzle;
        public Transform rightMuzzle;

        public FVRFireArmChamber leftChamber;
        public FVRFireArmChamber rightChamber;

        public Transform leftEjection;
        public Transform rightEjection;

        public Transform leftMagExit = null;
        public Transform rightMagExit = null;

        public Transform leftEjecting = null;
        public Transform rightEjecting = null;

        private FVRFirearmMovingProxyRound m_leftProxy;
        private FVRFirearmMovingProxyRound m_rightProxy;

#if !(UNITY_EDITOR || UNITY_5)
        
        public override void Awake()
        {
            base.Awake();

            FChambers.Add(leftChamber);
            FChambers.Add(rightChamber);

            GameObject leftProxy = new GameObject("m_proxyRoundLeft");
            m_leftProxy = leftProxy.AddComponent<FVRFirearmMovingProxyRound>();
            m_leftProxy.Init(transform);

            GameObject rightProxy = new GameObject("m_proxyRoundRight");
            m_rightProxy = rightProxy.AddComponent<FVRFirearmMovingProxyRound>();
            m_rightProxy.Init(transform);

            if (leftMagExit == null && rightMagExit == null && leftEjecting == null && rightEjecting == null)
            {
                leftMagExit = RoundPos_MagazinePos;
                rightMagExit = RoundPos_MagazinePos;

                leftEjecting = RoundPos_Ejecting;
                rightEjecting = RoundPos_Ejecting;
            }

            Hook();
        }


        public override List<FireArmRoundClass> GetChamberRoundList()
        {
            List<FireArmRoundClass> roundList = new List<FireArmRoundClass>();

            if (Chamber.IsFull && !Chamber.IsSpent)
            {
                roundList.Add(Chamber.GetRound().RoundClass);
            }
            if (leftChamber.IsFull && !leftChamber.IsSpent)
            {
                roundList.Add(leftChamber.GetRound().RoundClass);
            }
            if (rightChamber.IsFull && !rightChamber.IsSpent)
            {
                roundList.Add(rightChamber.GetRound().RoundClass);
            }
            return roundList;
        }
        public override void SetLoadedChambers(List<FireArmRoundClass> rounds)
        {
            base.SetLoadedChambers(rounds);

            if (rounds.Count > 1) leftChamber.Autochamber(rounds[1]);
            if (rounds.Count > 2) rightChamber.Autochamber(rounds[2]);
        }

        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            base.ConfigureFromFlagDic(f);
            if (Magazine != null && Magazine is TKB_059_Magazine mag)
            {
                string key = "LeftMagazineChamber";
                string value = string.Empty;

                string[] roundClassStrings;
                if (f.ContainsKey(key))
                {
                    value = f[key];

                    roundClassStrings = value.Split(';');

                    foreach (string roundClassString in roundClassStrings)
                    {
                        mag.leftMag.AddRound((FireArmRoundClass)Enum.Parse(typeof(FireArmRoundClass), roundClassString), false, false);
                    }

                    mag.leftMag.UpdateBulletDisplay();
                }

                key = "RightMagazineChamber";
                value = string.Empty;

                if (f.ContainsKey(key))
                {
                    value = f[key];

                    roundClassStrings = value.Split(';');

                    foreach (string roundClassString in roundClassStrings)
                    {
                        mag.rightMag.AddRound((FireArmRoundClass)Enum.Parse(typeof(FireArmRoundClass), roundClassString), false, false);
                    }

                    mag.rightMag.UpdateBulletDisplay();
                }
            }
        }

        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();

            string key = "LeftMagazineChamber";
            string value = string.Empty;

            if (Magazine != null && Magazine is TKB_059_Magazine mag && mag.leftMag.HasARound())
            {
                value += mag.leftMag.LoadedRounds[0].LR_Class.ToString();

                for (int i = 1; i < mag.leftMag.m_numRounds; i++)
                {
                    value += ";" + mag.leftMag.LoadedRounds[i].LR_Class.ToString();
                }

                flagDic.Add(key, value);

                key = "RightMagazineChamber";
                value = string.Empty;

                if (mag.rightMag.HasARound())
                {
                    value += mag.rightMag.LoadedRounds[0].LR_Class.ToString();

                    for (int i = 1; i < mag.rightMag.m_numRounds; i++)
                    {
                        value += ";" + mag.rightMag.LoadedRounds[i].LR_Class.ToString();
                    }

                    flagDic.Add(key, value);
                }
            }
            return flagDic;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Unhook();
        }
        public void Unhook()
        {
            On.FistVR.ClosedBoltWeapon.Fire -= ClosedBoltWeapon_Fire;
            On.FistVR.ClosedBoltWeapon.BeginChamberingRound -= ClosedBoltWeapon_BeginChamberingRound;
            On.FistVR.ClosedBoltWeapon.ChamberRound -= ClosedBoltWeapon_ChamberRound;
            On.FistVR.ClosedBoltWeapon.EjectExtractedRound -= ClosedBoltWeapon_EjectExtractedRound;
            On.FistVR.ClosedBoltWeapon.HasExtractedRound -= ClosedBoltWeapon_HasExtractedRound;
            On.FistVR.ClosedBoltWeapon.UpdateDisplayRoundPositions -= ClosedBoltWeapon_UpdateDisplayRoundPositions;
        }

        public void Hook()
        {
            On.FistVR.ClosedBoltWeapon.Fire += ClosedBoltWeapon_Fire;
            On.FistVR.ClosedBoltWeapon.BeginChamberingRound += ClosedBoltWeapon_BeginChamberingRound;
            On.FistVR.ClosedBoltWeapon.ChamberRound += ClosedBoltWeapon_ChamberRound;
            On.FistVR.ClosedBoltWeapon.EjectExtractedRound += ClosedBoltWeapon_EjectExtractedRound;
            On.FistVR.ClosedBoltWeapon.HasExtractedRound += ClosedBoltWeapon_HasExtractedRound;
            On.FistVR.ClosedBoltWeapon.UpdateDisplayRoundPositions += ClosedBoltWeapon_UpdateDisplayRoundPositions;
        }

        private void ClosedBoltWeapon_UpdateDisplayRoundPositions(On.FistVR.ClosedBoltWeapon.orig_UpdateDisplayRoundPositions orig, ClosedBoltWeapon self)
        {
            if (self == this)
            {
                float boltLerpBetweenLockAndFore = Bolt.GetBoltLerpBetweenLockAndFore();
                if (Chamber.IsFull)
                {
                    Chamber.ProxyRound.position = Vector3.Lerp(RoundPos_Ejecting.position, Chamber.transform.position, boltLerpBetweenLockAndFore);
                    Chamber.ProxyRound.rotation = Quaternion.Slerp(RoundPos_Ejecting.rotation, Chamber.transform.rotation, boltLerpBetweenLockAndFore);
                }
                if (m_proxy.IsFull)
                {
                    m_proxy.ProxyRound.position = Vector3.Lerp(RoundPos_MagazinePos.position, Chamber.transform.position, boltLerpBetweenLockAndFore);
                    m_proxy.ProxyRound.rotation = Quaternion.Slerp(RoundPos_MagazinePos.rotation, Chamber.transform.rotation, boltLerpBetweenLockAndFore);
                }

                if (leftChamber.IsFull)
                {
                    leftChamber.ProxyRound.position = Vector3.Lerp(leftEjecting.position, leftChamber.transform.position, boltLerpBetweenLockAndFore);
                    leftChamber.ProxyRound.rotation = Quaternion.Slerp(leftEjecting.rotation, leftChamber.transform.rotation, boltLerpBetweenLockAndFore);
                }
                if (m_leftProxy.IsFull)
                {
                    m_leftProxy.ProxyRound.position = Vector3.Lerp(leftMagExit.position, leftChamber.transform.position, boltLerpBetweenLockAndFore);
                    m_leftProxy.ProxyRound.rotation = Quaternion.Slerp(leftMagExit.rotation, leftChamber.transform.rotation, boltLerpBetweenLockAndFore);
                }

                if (rightChamber.IsFull)
                {
                    rightChamber.ProxyRound.position = Vector3.Lerp(rightEjecting.position, rightChamber.transform.position, boltLerpBetweenLockAndFore);
                    rightChamber.ProxyRound.rotation = Quaternion.Slerp(rightEjecting.rotation, rightChamber.transform.rotation, boltLerpBetweenLockAndFore);
                }
                if (m_rightProxy.IsFull)
                {
                    m_rightProxy.ProxyRound.position = Vector3.Lerp(rightMagExit.position, rightChamber.transform.position, boltLerpBetweenLockAndFore);
                    m_rightProxy.ProxyRound.rotation = Quaternion.Slerp(rightMagExit.rotation, rightChamber.transform.rotation, boltLerpBetweenLockAndFore);
                }
            }
            else orig(self);
        }

        private bool ClosedBoltWeapon_HasExtractedRound(On.FistVR.ClosedBoltWeapon.orig_HasExtractedRound orig, ClosedBoltWeapon self)
        {
            if (self == this)
            {
                return (m_proxy.IsFull || m_leftProxy.IsFull || m_rightProxy.IsFull);
            }
            else return orig(self);
        }

        private void ClosedBoltWeapon_EjectExtractedRound(On.FistVR.ClosedBoltWeapon.orig_EjectExtractedRound orig, ClosedBoltWeapon self)
        {
            if (self == this)
            {
                if (Chamber.IsFull)
                {
                    Chamber.EjectRound(RoundPos_Ejection.position, transform.right * EjectionSpeed.x + transform.up * EjectionSpeed.y + transform.forward * EjectionSpeed.z, transform.right * EjectionSpin.x + transform.up * EjectionSpin.y + transform.forward * EjectionSpin.z, false);
                }
                if (leftChamber.IsFull)
                {
                    leftChamber.EjectRound(leftEjection.position, transform.right * EjectionSpeed.x + transform.up * EjectionSpeed.y + transform.forward * EjectionSpeed.z, transform.right * EjectionSpin.x + transform.up * EjectionSpin.y + transform.forward * EjectionSpin.z, false);
                }
                if (rightChamber.IsFull)
                {
                    rightChamber.EjectRound(rightEjection.position, transform.right * EjectionSpeed.x + transform.up * EjectionSpeed.y + transform.forward * EjectionSpeed.z, transform.right * EjectionSpin.x + transform.up * EjectionSpin.y + transform.forward * EjectionSpin.z, false);
                }
            }
            else orig(self);
        }

        private bool ClosedBoltWeapon_ChamberRound(On.FistVR.ClosedBoltWeapon.orig_ChamberRound orig, ClosedBoltWeapon self)
        {
            if (this == self)
            {
                if (!(m_proxy.IsFull && !Chamber.IsFull) && !(m_leftProxy.IsFull && !leftChamber.IsFull) && !(m_rightProxy.IsFull && !rightChamber.IsFull))
                {
                    //Debug.Log("Not Chambering Round");

                    return false;
                }

                if (m_proxy.IsFull && !Chamber.IsFull)
                {
                    Chamber.SetRound(m_proxy.Round, false);
                    m_proxy.ClearProxy();
                }
                if (m_leftProxy.IsFull && !leftChamber.IsFull)
                {
                    leftChamber.SetRound(m_leftProxy.Round, false);
                    m_leftProxy.ClearProxy();
                }
                if (m_rightProxy.IsFull && !rightChamber.IsFull)
                {
                    rightChamber.SetRound(m_rightProxy.Round, false);
                    m_rightProxy.ClearProxy();
                }
                return true;
            }
            else return orig(self);
        }

        private void ClosedBoltWeapon_BeginChamberingRound(On.FistVR.ClosedBoltWeapon.orig_BeginChamberingRound orig, ClosedBoltWeapon self)
        {
            if (this == self)
            {
                bool flag = false;
                bool flagLeft = false;
                bool flagRight = false;
                GameObject fromPrefabReference = null;
                GameObject fromPrefabReferenceLeft = null;
                GameObject fromPrefabReferenceRight = null;

                if (!m_proxy.IsFull && Magazine != null && !Magazine.IsBeltBox && Magazine.HasARound())
                {
                    flag = true;
                    fromPrefabReference = Magazine.RemoveRound(false);
                }

                if (!m_leftProxy.IsFull && Magazine != null && !Magazine.IsBeltBox && (Magazine as TKB_059_Magazine).leftMag.HasARound())
                {
                    flagLeft = true;
                    fromPrefabReferenceLeft = (Magazine as TKB_059_Magazine).leftMag.RemoveRound(false);
                }

                if (!m_rightProxy.IsFull && Magazine != null && !Magazine.IsBeltBox && (Magazine as TKB_059_Magazine).rightMag.HasARound())
                {
                    flagRight = true;
                    fromPrefabReferenceRight = (Magazine as TKB_059_Magazine).rightMag.RemoveRound(false);
                }

                //Debug.Log("Chamber flag: " + flag);
                //Debug.Log("Chamber flagLeft: " + flagLeft);
                //Debug.Log("Chamber flagRight: " + flagRight);


                if (!flag && !flagLeft && !flagRight)
                {
                    //Debug.Log("Not beginning Chambering Round");

                    return;
                }
                if (flag)
                {
                    m_proxy.SetFromPrefabReference(fromPrefabReference);
                }
                if (flagLeft)
                {
                    m_leftProxy.SetFromPrefabReference(fromPrefabReferenceLeft);
                }
                if (flagRight)
                {
                    m_rightProxy.SetFromPrefabReference(fromPrefabReferenceRight);
                }
            }
            else orig(self);
        }

        private bool ClosedBoltWeapon_Fire(On.FistVR.ClosedBoltWeapon.orig_Fire orig, ClosedBoltWeapon self)
        {
            if (self == this)
            {
                bool chamberFire = Chamber.Fire();
                bool leftChamberFire = leftChamber.Fire();
                bool rightChamberFire = rightChamber.Fire();

                //Debug.Log("chamberFire: " + chamberFire);
                //Debug.Log("leftChamberFire: " + leftChamberFire);
                //Debug.Log("rightChamberFire: " + rightChamberFire);

                if (!chamberFire && !leftChamberFire && !rightChamberFire)
                {
                    //Debug.Log("Not Firing!");

                    return false;
                }
                m_timeSinceFiredShot = 0f;
                float velMult = 1f;
                if (UsesStickyDetonation)
                {
                    velMult = 1f + Mathf.Lerp(0f, StickyMaxMultBonus, m_stickyChargeUp);
                }
                bool twoHandStabilized = IsTwoHandStabilized();
                bool foregripStabilized = AltGrip != null;
                bool shoulderStabilized = IsShoulderStabilized();

                int numberShots = 0;
                if (chamberFire)
                {
                    Fire(Chamber, GetMuzzle(), true, velMult);
                    numberShots++;
                }
                if (leftChamberFire)
                {
                    Fire(leftChamber, leftMuzzle, true, velMult);
                    numberShots++;
                }
                if (rightChamberFire)
                {
                    Fire(rightChamber, rightMuzzle, true, velMult);
                    numberShots++;
                }

                bool flag = false;
                ClosedBoltWeapon.FireSelectorMode fireSelectorMode = FireSelector_Modes[m_fireSelectorMode];
                if (fireSelectorMode.ModeType == ClosedBoltWeapon.FireSelectorModeType.SuperFastBurst)
                {
                    for (int i = 0; i < fireSelectorMode.BurstAmount - 1; i++)
                    {
                        if (Magazine.HasARound())
                        {
                            Magazine.RemoveRound();
                            Fire(Chamber, GetMuzzle(), false, 1f);
                            flag = true;
                            Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
                        }
                        if ((Magazine as TKB_059_Magazine).HasARound())
                        {
                            Magazine.RemoveRound();
                            Fire(Chamber, GetMuzzle(), false, 1f);
                            flag = true;
                            Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
                        }
                    }
                }
                FireMuzzleSmoke();
                if (UsesDelinker && HasBelt)
                {
                    DelinkerSystem.Emit(1);
                }
                if (HasBelt)
                {
                    BeltDD.AddJitter();
                }
                if (flag)
                {
                    PlayAudioGunShot(false, Chamber.GetRound().TailClass, Chamber.GetRound().TailClassSuppressed, GM.CurrentPlayerBody.GetCurrentSoundEnvironment());
                }
                else
                {
                    if (numberShots > 0)
                    {
                        Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f * numberShots);
                        PlayAudioGunShot(Chamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 0.7f + 0.1f * numberShots);
                    }
                }
                if (ReciprocatesOnShot)
                {
                    Bolt.ImpartFiringImpulse();
                }
                return true;
            }
            else return orig(self);
        }
#endif
    }
}
