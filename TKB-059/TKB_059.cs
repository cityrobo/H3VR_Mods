using FistVR;
using System;
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

            this.FChambers.Add(leftChamber);
            this.FChambers.Add(rightChamber);

            GameObject leftProxy = new GameObject("m_proxyRoundLeft");
            this.m_leftProxy = leftProxy.AddComponent<FVRFirearmMovingProxyRound>();
            this.m_leftProxy.Init(base.transform);

            GameObject rightProxy = new GameObject("m_proxyRoundRight");
            this.m_rightProxy = rightProxy.AddComponent<FVRFirearmMovingProxyRound>();
            this.m_rightProxy.Init(base.transform);

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

            if (this.Chamber.IsFull && !this.Chamber.IsSpent)
            {
                roundList.Add(this.Chamber.GetRound().RoundClass);
            }
            if (this.leftChamber.IsFull && !this.leftChamber.IsSpent)
            {
                roundList.Add(this.leftChamber.GetRound().RoundClass);
            }
            if (this.rightChamber.IsFull && !this.rightChamber.IsSpent)
            {
                roundList.Add(this.rightChamber.GetRound().RoundClass);
            }
            return roundList;
        }
        public override void SetLoadedChambers(List<FireArmRoundClass> rounds)
        {
            if (rounds.Count > 0)
            {
                this.Chamber.Autochamber(rounds[0]);
                this.leftChamber.Autochamber(rounds[0]);
                this.rightChamber.Autochamber(rounds[0]);
            }
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
                float boltLerpBetweenLockAndFore = this.Bolt.GetBoltLerpBetweenLockAndFore();
                if (this.Chamber.IsFull)
                {
                    this.Chamber.ProxyRound.position = Vector3.Lerp(this.RoundPos_Ejecting.position, this.Chamber.transform.position, boltLerpBetweenLockAndFore);
                    this.Chamber.ProxyRound.rotation = Quaternion.Slerp(this.RoundPos_Ejecting.rotation, this.Chamber.transform.rotation, boltLerpBetweenLockAndFore);
                }
                if (this.m_proxy.IsFull)
                {
                    this.m_proxy.ProxyRound.position = Vector3.Lerp(this.RoundPos_MagazinePos.position, this.Chamber.transform.position, boltLerpBetweenLockAndFore);
                    this.m_proxy.ProxyRound.rotation = Quaternion.Slerp(this.RoundPos_MagazinePos.rotation, this.Chamber.transform.rotation, boltLerpBetweenLockAndFore);
                }

                if (this.leftChamber.IsFull)
                {
                    this.leftChamber.ProxyRound.position = Vector3.Lerp(this.leftEjecting.position, this.leftChamber.transform.position, boltLerpBetweenLockAndFore);
                    this.leftChamber.ProxyRound.rotation = Quaternion.Slerp(this.leftEjecting.rotation, this.leftChamber.transform.rotation, boltLerpBetweenLockAndFore);
                }
                if (this.m_leftProxy.IsFull)
                {
                    this.m_leftProxy.ProxyRound.position = Vector3.Lerp(this.leftMagExit.position, this.leftChamber.transform.position, boltLerpBetweenLockAndFore);
                    this.m_leftProxy.ProxyRound.rotation = Quaternion.Slerp(this.leftMagExit.rotation, this.leftChamber.transform.rotation, boltLerpBetweenLockAndFore);
                }

                if (this.rightChamber.IsFull)
                {
                    this.rightChamber.ProxyRound.position = Vector3.Lerp(this.rightEjecting.position, this.rightChamber.transform.position, boltLerpBetweenLockAndFore);
                    this.rightChamber.ProxyRound.rotation = Quaternion.Slerp(this.rightEjecting.rotation, this.rightChamber.transform.rotation, boltLerpBetweenLockAndFore);
                }
                if (this.m_rightProxy.IsFull)
                {
                    this.m_rightProxy.ProxyRound.position = Vector3.Lerp(this.rightMagExit.position, this.rightChamber.transform.position, boltLerpBetweenLockAndFore);
                    this.m_rightProxy.ProxyRound.rotation = Quaternion.Slerp(this.rightMagExit.rotation, this.rightChamber.transform.rotation, boltLerpBetweenLockAndFore);
                }
            }
            else orig(self);
        }

        private bool ClosedBoltWeapon_HasExtractedRound(On.FistVR.ClosedBoltWeapon.orig_HasExtractedRound orig, ClosedBoltWeapon self)
        {
            if (self == this)
            {
                return (this.m_proxy.IsFull || this.m_leftProxy.IsFull || this.m_rightProxy.IsFull);
            }
            else return orig(self);
        }

        private void ClosedBoltWeapon_EjectExtractedRound(On.FistVR.ClosedBoltWeapon.orig_EjectExtractedRound orig, ClosedBoltWeapon self)
        {
            if (self == this)
            {
                if (this.Chamber.IsFull)
                {
                    this.Chamber.EjectRound(this.RoundPos_Ejection.position, base.transform.right * this.EjectionSpeed.x + base.transform.up * this.EjectionSpeed.y + base.transform.forward * this.EjectionSpeed.z, base.transform.right * this.EjectionSpin.x + base.transform.up * this.EjectionSpin.y + base.transform.forward * this.EjectionSpin.z, false);
                }
                if (this.leftChamber.IsFull)
                {
                    this.leftChamber.EjectRound(this.leftEjection.position, base.transform.right * this.EjectionSpeed.x + base.transform.up * this.EjectionSpeed.y + base.transform.forward * this.EjectionSpeed.z, base.transform.right * this.EjectionSpin.x + base.transform.up * this.EjectionSpin.y + base.transform.forward * this.EjectionSpin.z, false);
                }
                if (this.rightChamber.IsFull)
                {
                    this.rightChamber.EjectRound(this.rightEjection.position, base.transform.right * this.EjectionSpeed.x + base.transform.up * this.EjectionSpeed.y + base.transform.forward * this.EjectionSpeed.z, base.transform.right * this.EjectionSpin.x + base.transform.up * this.EjectionSpin.y + base.transform.forward * this.EjectionSpin.z, false);
                }
            }
            else orig(self);
        }

        private bool ClosedBoltWeapon_ChamberRound(On.FistVR.ClosedBoltWeapon.orig_ChamberRound orig, ClosedBoltWeapon self)
        {
            if (this == self)
            {
                if (!(this.m_proxy.IsFull && !this.Chamber.IsFull) && !(this.m_leftProxy.IsFull && !this.leftChamber.IsFull) && !(this.m_rightProxy.IsFull && !this.rightChamber.IsFull))
                {
                    //Debug.Log("Not Chambering Round");

                    return false;
                }

                if (this.m_proxy.IsFull && !this.Chamber.IsFull)
                {
                    this.Chamber.SetRound(this.m_proxy.Round, false);
                    this.m_proxy.ClearProxy();
                }
                if (this.m_leftProxy.IsFull && !this.leftChamber.IsFull)
                {
                    this.leftChamber.SetRound(this.m_leftProxy.Round, false);
                    this.m_leftProxy.ClearProxy();
                }
                if (this.m_rightProxy.IsFull && !this.rightChamber.IsFull)
                {
                    this.rightChamber.SetRound(this.m_rightProxy.Round, false);
                    this.m_rightProxy.ClearProxy();
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

                if (!this.m_proxy.IsFull && this.Magazine != null && !this.Magazine.IsBeltBox && this.Magazine.HasARound())
                {
                    flag = true;
                    fromPrefabReference = this.Magazine.RemoveRound(false);
                }

                if (!this.m_leftProxy.IsFull && this.Magazine != null && !this.Magazine.IsBeltBox && (this.Magazine as TKB_059_Magazine).leftMag.HasARound())
                {
                    flagLeft = true;
                    fromPrefabReferenceLeft = (this.Magazine as TKB_059_Magazine).leftMag.RemoveRound(false);
                }

                if (!this.m_rightProxy.IsFull && this.Magazine != null && !this.Magazine.IsBeltBox && (this.Magazine as TKB_059_Magazine).rightMag.HasARound())
                {
                    flagRight = true;
                    fromPrefabReferenceRight = (this.Magazine as TKB_059_Magazine).rightMag.RemoveRound(false);
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
                    this.m_proxy.SetFromPrefabReference(fromPrefabReference);
                }
                if (flagLeft)
                {
                    this.m_leftProxy.SetFromPrefabReference(fromPrefabReferenceLeft);
                }
                if (flagRight)
                {
                    this.m_rightProxy.SetFromPrefabReference(fromPrefabReferenceRight);
                }
            }
            else orig(self);
        }

        private bool ClosedBoltWeapon_Fire(On.FistVR.ClosedBoltWeapon.orig_Fire orig, ClosedBoltWeapon self)
        {
            if (self == this)
            {
                bool chamberFire = this.Chamber.Fire();
                bool leftChamberFire = this.leftChamber.Fire();
                bool rightChamberFire = this.rightChamber.Fire();

                //Debug.Log("chamberFire: " + chamberFire);
                //Debug.Log("leftChamberFire: " + leftChamberFire);
                //Debug.Log("rightChamberFire: " + rightChamberFire);

                if (!chamberFire && !leftChamberFire && !rightChamberFire)
                {
                    //Debug.Log("Not Firing!");

                    return false;
                }
                this.m_timeSinceFiredShot = 0f;
                float velMult = 1f;
                if (this.UsesStickyDetonation)
                {
                    velMult = 1f + Mathf.Lerp(0f, this.StickyMaxMultBonus, this.m_stickyChargeUp);
                }
                bool twoHandStabilized = this.IsTwoHandStabilized();
                bool foregripStabilized = base.AltGrip != null;
                bool shoulderStabilized = this.IsShoulderStabilized();

                if (chamberFire)
                {
                    base.Fire(this.Chamber, this.GetMuzzle(), true, velMult);
                    this.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
                }
                if (leftChamberFire)
                {
                    base.Fire(leftChamber, leftMuzzle, true, velMult);
                    this.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
                }
                if (rightChamberFire)
                {
                    base.Fire(rightChamber, rightMuzzle, true, velMult);
                    this.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
                }
                bool flag = false;
                ClosedBoltWeapon.FireSelectorMode fireSelectorMode = this.FireSelector_Modes[this.m_fireSelectorMode];
                if (fireSelectorMode.ModeType == ClosedBoltWeapon.FireSelectorModeType.SuperFastBurst)
                {
                    for (int i = 0; i < fireSelectorMode.BurstAmount - 1; i++)
                    {
                        if (this.Magazine.HasARound())
                        {
                            this.Magazine.RemoveRound();
                            base.Fire(this.Chamber, this.GetMuzzle(), false, 1f);
                            flag = true;
                            this.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
                        }
                        if ((this.Magazine as TKB_059_Magazine).HasARound())
                        {
                            this.Magazine.RemoveRound();
                            base.Fire(this.Chamber, this.GetMuzzle(), false, 1f);
                            flag = true;
                            this.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
                        }
                    }
                }
                this.FireMuzzleSmoke();
                if (this.UsesDelinker && this.HasBelt)
                {
                    this.DelinkerSystem.Emit(1);
                }
                if (this.HasBelt)
                {
                    this.BeltDD.AddJitter();
                }
                if (flag)
                {
                    base.PlayAudioGunShot(false, this.Chamber.GetRound().TailClass, this.Chamber.GetRound().TailClassSuppressed, GM.CurrentPlayerBody.GetCurrentSoundEnvironment());
                }
                else
                {
                    if (chamberFire) base.PlayAudioGunShot(this.Chamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 1f);
                    if (leftChamberFire) base.PlayAudioGunShot(this.leftChamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 1f);
                    if (rightChamberFire) base.PlayAudioGunShot(this.rightChamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 1f);
                }
                if (this.ReciprocatesOnShot)
                {
                    this.Bolt.ImpartFiringImpulse();
                }
                return true;
            }
            else return orig(self);
        }
#endif
    }
}
