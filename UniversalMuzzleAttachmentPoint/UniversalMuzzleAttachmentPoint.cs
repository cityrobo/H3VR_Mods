using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.UniversalMuzzleAttachmentPoint", "UniversalMuzzleAttachmentPoint Script", "1.0.0")]
    public class UniversalMuzzleAttachmentPoint : BaseUnityPlugin
    {
        public void Awake()
        {
            Hook();
        }
        public void OnDestroy()
        {
            Unhook();
        }
        private void Unhook()
        {
            On.FistVR.FVRFireArm.Awake -= FVRFireArm_Awake;
            On.FistVR.FVRFireArm.GetMuzzle -= FVRFireArm_GetMuzzle;
            On.FistVR.BreakActionWeapon.GetMuzzle -= BreakActionWeapon_GetMuzzle;
            On.FistVR.BreakActionWeapon.Fire -= BreakActionWeapon_Fire;
            On.FistVR.Derringer.GetMuzzle -= Derringer_GetMuzzle;
        }
        private void Hook()
        {
            On.FistVR.FVRFireArm.Awake += FVRFireArm_Awake;
            On.FistVR.FVRFireArm.GetMuzzle += FVRFireArm_GetMuzzle;
            On.FistVR.BreakActionWeapon.GetMuzzle += BreakActionWeapon_GetMuzzle;
            On.FistVR.BreakActionWeapon.Fire += BreakActionWeapon_Fire;
            On.FistVR.Derringer.GetMuzzle += Derringer_GetMuzzle;
            On.FistVR.Derringer.FireBarrel += Derringer_FireBarrel;
        }

        private void Derringer_FireBarrel(On.FistVR.Derringer.orig_FireBarrel orig, Derringer self, int i)
        {
            if (self.MuzzleDevices.Count == 0) orig(self, i);
            else
            {
                if (self.GetHingeState() != Derringer.HingeState.Closed) return;
                FVRFireArmChamber chamber = self.Barrels[self.m_curBarrel].Chamber;
                if (!chamber.Fire()) return;
                Transform muzzle = self.GetMuzzle();
                MuzzleDevice muzzleDevice = self.MuzzleDevices[self.MuzzleDevices.Count - 1];

                Vector3 origMuzzlePos = muzzle.localPosition;
                muzzle.position = self.Barrels[i].MuzzlePoint.position + (muzzle.position - muzzleDevice.curMount.GetRootMount().transform.position);

                self.Fire(chamber, muzzle, true, 1f, -1f);

                foreach (var muzzleEffect in self.MuzzleDevices[self.MuzzleDevices.Count - 1].MuzzleEffects)
                {
                    if (muzzleEffect.OverridePoint != null)
                    {
                        for (int j = 0; j < muzzleEffect.OverridePoint.childCount; j++)
                        {
                            muzzleEffect.OverridePoint.GetChild(j).localPosition = muzzleEffect.OverridePoint.GetChild(j).InverseTransformDirection(self.Barrels[i].MuzzlePoint.position - muzzleDevice.curMount.GetRootMount().transform.position);
                        }
                    }
                }

                self.FireMuzzleSmoke();
                bool twoHandStabilized = self.IsTwoHandStabilized();
                bool foregripStabilized = self.AltGrip != null;
                bool shoulderStabilized = self.IsShoulderStabilized();
                self.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
                self.PlayAudioGunShot(chamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 1f);
                if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
                {
                    chamber.IsSpent = false;
                    chamber.UpdateProxyDisplay();
                }
                else if (chamber.GetRound().IsCaseless)
                {
                    chamber.SetRound(null, false);
                }
                if (self.DeletesCartridgeOnFire)
                {
                    chamber.SetRound(null, false);
                }

                muzzle.localPosition = origMuzzlePos;
            }
        }

        private Transform FVRFireArm_GetMuzzle(On.FistVR.FVRFireArm.orig_GetMuzzle orig, FVRFireArm self)
        {
            if (self is FlintlockWeapon flintlock) return flintlock.GetComponentInChildren<FlintlockBarrel>().Muzzle;
            else return orig(self);
        }
        private Transform Derringer_GetMuzzle(On.FistVR.Derringer.orig_GetMuzzle orig, Derringer self)
        {
            if (self.MuzzleDevices.Count != 0) return self.MuzzleDevices[self.MuzzleDevices.Count - 1].Muzzle;
            else return orig(self);
        }
        private bool BreakActionWeapon_Fire(On.FistVR.BreakActionWeapon.orig_Fire orig, BreakActionWeapon self, int b, bool FireAllBarrels, int index)
        {
            if (self.MuzzleDevices.Count == 0) return orig(self, b, FireAllBarrels, index);
            else
            {
                self.m_curBarrel = b;
                if (!self.Barrels[b].Chamber.Fire()) return false;
                Transform muzzle = self.GetMuzzle();
                MuzzleDevice muzzleDevice = self.MuzzleDevices[self.MuzzleDevices.Count - 1];

                Vector3 origMuzzlePos = muzzle.localPosition;               
                muzzle.position = self.Barrels[b].Muzzle.position + (muzzle.position - muzzleDevice.curMount.GetRootMount().transform.position);

                self.Fire(self.Barrels[b].Chamber, muzzle, true, 1f, -1f);

                self.FireMuzzleSmoke();
                foreach (var muzzleEffect in self.MuzzleDevices[self.MuzzleDevices.Count - 1].MuzzleEffects)
                {
                    if (muzzleEffect.OverridePoint != null)
                    {
                        for (int i = 0; i < muzzleEffect.OverridePoint.childCount; i++)
                        {
                            muzzleEffect.OverridePoint.GetChild(i).localPosition = muzzleEffect.OverridePoint.GetChild(i).InverseTransformDirection(self.Barrels[b].Muzzle.position - muzzleDevice.curMount.GetRootMount().transform.position);
                        }
                    }
                }

                self.AddGas(self.Barrels[b].GasOutIndexBarrel);
                self.AddGas(self.Barrels[b].GasOutIndexBreach);
                bool twoHandStabilized = self.IsTwoHandStabilized();
                bool foregripStabilized = self.IsForegripStabilized();
                bool shoulderStabilized = self.IsShoulderStabilized();
                self.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
                if (!self.OnlyOneShotSound || !self.firedOneShot)
                {
                    self.firedOneShot = true;
                    self.PlayAudioGunShot(self.Barrels[b].Chamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 1f);
                }
                if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
                {
                    self.Barrels[b].Chamber.IsSpent = false;
                    self.Barrels[b].Chamber.UpdateProxyDisplay();
                }

                muzzle.localPosition = origMuzzlePos;

                return true;
            }
        }
        private Transform BreakActionWeapon_GetMuzzle(On.FistVR.BreakActionWeapon.orig_GetMuzzle orig, BreakActionWeapon self)
        {
            if (self.MuzzleDevices.Count != 0) return self.MuzzleDevices[self.MuzzleDevices.Count - 1].Muzzle;
            else return orig(self);
        }
        private void FVRFireArm_Awake(On.FistVR.FVRFireArm.orig_Awake orig, FVRFireArm self)
        {
            orig(self);

            bool hasMuzzleMount = false;
            if (self.AttachmentMounts == null) self.AttachmentMounts = new List<FVRFireArmAttachmentMount>();
            bool hasNullMount = false;
            foreach (var Mount in self.AttachmentMounts)
            {
                if (Mount == null)
                {
                    hasNullMount = true;
                    continue;
                }
                if (Mount.Type == FVRFireArmAttachementMountType.Suppressor) hasMuzzleMount = true;
            }

            if (!hasMuzzleMount)
            {
                GameObject MuzzleMount = new GameObject("_PointSuppressorMount");
                bool parentToThis = false;
                FVRFireArmAttachmentMount Mount;
                if (self is BreakActionWeapon breakAction)
                {
                    MuzzleMount.transform.parent = breakAction.Hinge.transform;
                    parentToThis = true;

                    Vector3 MountPos = new Vector3();
                    Vector3 MountRot = new Vector3();
                    foreach (var Barrel in breakAction.Barrels)
                    {
                        MountPos += Barrel.Muzzle.position;
                        MountRot += Barrel.Muzzle.eulerAngles;
                    }
                    MountPos /= breakAction.Barrels.Length;
                    MountRot /= breakAction.Barrels.Length;
                    MuzzleMount.transform.position = MountPos;
                    MuzzleMount.transform.rotation = Quaternion.Euler(MountRot);
                    Mount = MuzzleMount.AddComponent<MultiBarrelMount>();
                    (Mount as MultiBarrelMount).breakAction = breakAction;
                }
                else if (self is Derringer derringer)
                {
                    MuzzleMount.transform.parent = derringer.Hinge;
                    parentToThis = true;

                    Vector3 MountPos = new Vector3();
                    Vector3 MountRot = new Vector3();
                    foreach (var Barrel in derringer.Barrels)
                    {
                        MountPos += Barrel.MuzzlePoint.position;
                        MountRot += Barrel.MuzzlePoint.eulerAngles;
                    }
                    MountPos /= derringer.Barrels.Count;
                    MountRot /= derringer.Barrels.Count;
                    MuzzleMount.transform.position = MountPos;
                    MuzzleMount.transform.rotation = Quaternion.Euler(MountRot);
                    Mount = MuzzleMount.AddComponent<MultiBarrelMount>();
                    (Mount as MultiBarrelMount).derringer = derringer;
                }
                else if (self is Flaregun flaregun)
                {
                    MuzzleMount.transform.parent = flaregun.Hinge;
                    parentToThis = true;

                    MuzzleMount.transform.position = self.GetMuzzle().position;
                    MuzzleMount.transform.rotation = self.GetMuzzle().rotation;
                    Mount = MuzzleMount.AddComponent<FVRFireArmAttachmentMount>();
                }
                else if (self is FlintlockWeapon flintlock)
                {
                    MuzzleMount.transform.parent = self.transform;
                    MuzzleMount.transform.position = flintlock.GetComponentInChildren<FlintlockBarrel>().Muzzle.position;
                    MuzzleMount.transform.rotation = flintlock.GetComponentInChildren<FlintlockBarrel>().Muzzle.rotation;
                    Mount = MuzzleMount.AddComponent<FVRFireArmAttachmentMount>();
                }
                else
                {
                    MuzzleMount.transform.parent = self.transform;
                    MuzzleMount.transform.position = self.GetMuzzle().position;
                    MuzzleMount.transform.rotation = self.GetMuzzle().rotation;
                    Mount = MuzzleMount.AddComponent<FVRFireArmAttachmentMount>();
                }

                MuzzleMount.layer = LayerMask.NameToLayer("Interactable");
                MuzzleMount.tag = "FVRFireArmAttachmentMount";
                MuzzleMount.SetActive(false);
                
                Mount.MyObject = self;
                Mount.Parent = self;
                Mount.ScaleModifier = GetScale(self.RoundType);
                Mount.Type = FVRFireArmAttachementMountType.Suppressor;
                Mount.Point_Front = Mount.transform;
                Mount.Point_Rear = Mount.transform;
                Mount.AttachmentsList = new List<FVRFireArmAttachment>();
                Mount.SubMounts = new List<FVRFireArmAttachmentMount>();

                if (parentToThis) Mount.ParentToThis = true;

                CapsuleCollider collider = MuzzleMount.AddComponent<CapsuleCollider>();
                collider.radius = 0.01f;
                collider.height = 0.01f;
                collider.isTrigger = true;

                if (self.AttachmentMounts == null) self.AttachmentMounts = new List<FVRFireArmAttachmentMount>();
                if (hasNullMount)
                {
                    for (int i = 0; i < self.AttachmentMounts.Count; i++)
                    {
                        if (self.AttachmentMounts[i] == null)
                        {
                            self.AttachmentMounts[i] = Mount;
                            break;
                        }
                    }
                    for (int i = 0; i < self.AttachmentMounts.Count; i++)
                    {
                        if (self.AttachmentMounts[i] == null)
                        {
                            self.AttachmentMounts.RemoveAt(i);
                        }
                    }
                }
                else self.AttachmentMounts.Add(Mount);

                MuzzleMount.SetActive(true);
            }
        }

        private float GetScale(FireArmRoundType roundType)
        {
            switch (roundType)
            {
                case FireArmRoundType.a50_BMG:
                    return 2f;
                case FireArmRoundType.a762_51_Nato:
                    return 1.5f;
                case FireArmRoundType.a556_45_Nato:
                    return 1.25f;
                case FireArmRoundType.a22_LR:
                    return 0.5f;
                case FireArmRoundType.a12g_Shotgun:
                    return 1.5f;
                case FireArmRoundType.a40_46_Grenade:
                    return 3f;
                case FireArmRoundType.a762_39_Soviet:
                    return 1.25f;
                default: return 1f;
            }
        }
#if !(DEBUG || MEATKIT)

#endif
    }
}
