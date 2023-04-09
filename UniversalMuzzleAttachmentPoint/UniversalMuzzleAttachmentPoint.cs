using FistVR;
using System;
using System.IO;
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
        private const string MOUNTNAME = "_PointSuppressorMount";
        private const string LAYERNAME = "Interactable";
        private const string TAG = "FVRFireArmAttachmentMount";
        

        private string _dictionaryPath;
        private const string DICTIONARYNAME = "RoundScaleDictionary.txt";

        private string _roundScaleDictionaryString;

        private Dictionary<FireArmRoundType, float> _roundScaleDictionary = new Dictionary<FireArmRoundType, float>();
        private float GetScale(FireArmRoundType roundType)
        {
            float scale;
            if (_roundScaleDictionary.TryGetValue(roundType, out scale)) return scale;
            else return 1f;

            //switch (roundType)
            //{
            //    case FireArmRoundType.a50_BMG:
            //        return 2f;
            //    case FireArmRoundType.a762_51_Nato:
            //        return 1.5f;
            //    case FireArmRoundType.a556_45_Nato:
            //        return 1.25f;
            //    case FireArmRoundType.a22_LR:
            //        return 0.5f;
            //    case FireArmRoundType.a12g_Shotgun:
            //        return 1.5f;
            //    case FireArmRoundType.a40_46_Grenade:
            //        return 3f;
            //    case FireArmRoundType.a762_39_Soviet:
            //        return 1.25f;
            //    default: return 1f;
            //}
        }

        public void Awake()
        {
            Hook();

            _dictionaryPath = Info.Location;
            _dictionaryPath = Path.GetDirectoryName(_dictionaryPath);

            _dictionaryPath = Path.Combine(_dictionaryPath,DICTIONARYNAME);
            StreamReader reader = new StreamReader(_dictionaryPath);
            _roundScaleDictionaryString = reader.ReadToEnd();
            reader.Close();

            string[] _roundScaleDictionaryLines = _roundScaleDictionaryString.Split('\n');

            foreach (string _roundScaleDictionaryLine in _roundScaleDictionaryLines)
            {
                string[] _roundScaleDictionaryData = _roundScaleDictionaryLine.Split(';');

                FireArmRoundType type = (FireArmRoundType)Enum.Parse(typeof(FireArmRoundType), _roundScaleDictionaryData[0]);
                float scale = (float)float.Parse(_roundScaleDictionaryData[1]);

                _roundScaleDictionary.Add(type, scale);
            }
        }
        public void OnDestroy()
        {
            Unhook();
        }

        private void Hook()
        {
            On.FistVR.FVRFireArm.Awake += FVRFireArm_Awake;
            On.FistVR.MuzzleDevice.Awake += MuzzleDevice_Awake;
            On.FistVR.FVRFireArm.GetMuzzle += FVRFireArm_GetMuzzle;
            On.FistVR.BreakActionWeapon.GetMuzzle += BreakActionWeapon_GetMuzzle;
            On.FistVR.BreakActionWeapon.Fire += BreakActionWeapon_Fire;
            On.FistVR.Derringer.GetMuzzle += Derringer_GetMuzzle;
            On.FistVR.Derringer.FireBarrel += Derringer_FireBarrel;
        }



        private void Unhook()
        {
            On.FistVR.FVRFireArm.Awake -= FVRFireArm_Awake;
            On.FistVR.MuzzleDevice.Awake -= MuzzleDevice_Awake;
            On.FistVR.FVRFireArm.GetMuzzle -= FVRFireArm_GetMuzzle;
            On.FistVR.BreakActionWeapon.GetMuzzle -= BreakActionWeapon_GetMuzzle;
            On.FistVR.BreakActionWeapon.Fire -= BreakActionWeapon_Fire;
            On.FistVR.Derringer.GetMuzzle -= Derringer_GetMuzzle;
            On.FistVR.Derringer.FireBarrel -= Derringer_FireBarrel;
        }

        private void FVRFireArm_Awake(On.FistVR.FVRFireArm.orig_Awake orig, FVRFireArm self)
        {
            orig(self);

            bool hasMuzzleMount = false;
            if (self.AttachmentMounts == null) self.AttachmentMounts = new List<FVRFireArmAttachmentMount>();
            bool hasNullMount = false;
            // Check if Muzzle mount exists
            foreach (var Mount in self.AttachmentMounts)
            {
                // Check for potential null mount
                if (Mount == null)
                {
                    hasNullMount = true;
                    continue;
                }
                if (Mount.Type == FVRFireArmAttachementMountType.Suppressor) hasMuzzleMount = true;
            }

            if (!hasMuzzleMount)
            {
                GameObject MuzzleMountGameObject = new GameObject(MOUNTNAME); ;

                // Setup Mount GameObject
                MuzzleMountGameObject.layer = LayerMask.NameToLayer(LAYERNAME);
                MuzzleMountGameObject.tag = TAG;
                MuzzleMountGameObject.SetActive(false);

                bool parentToThis = false;
                FVRFireArmAttachmentMount MountComponent;
                // Setup Mount differently for Multi Barrel Weapons
                if (self is BreakActionWeapon breakAction)
                {
                    MuzzleMountGameObject.transform.parent = breakAction.Hinge.transform;
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
                    MuzzleMountGameObject.transform.position = MountPos;
                    MuzzleMountGameObject.transform.rotation = Quaternion.Euler(MountRot);
                    MountComponent = MuzzleMountGameObject.AddComponent<MultiBarrelMount>();
                    (MountComponent as MultiBarrelMount).breakAction = breakAction;
                }
                else if (self is Derringer derringer)
                {
                    MuzzleMountGameObject.transform.parent = derringer.Hinge;
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
                    MuzzleMountGameObject.transform.position = MountPos;
                    MuzzleMountGameObject.transform.rotation = Quaternion.Euler(MountRot);
                    MountComponent = MuzzleMountGameObject.AddComponent<MultiBarrelMount>();
                    (MountComponent as MultiBarrelMount).derringer = derringer;
                }
                else if (self is Flaregun flaregun)
                {
                    MuzzleMountGameObject.transform.parent = flaregun.Hinge;
                    parentToThis = true;

                    MuzzleMountGameObject.transform.position = self.GetMuzzle().position;
                    MuzzleMountGameObject.transform.rotation = self.GetMuzzle().rotation;
                    MountComponent = MuzzleMountGameObject.AddComponent<FVRFireArmAttachmentMount>();
                }
                else if (self is FlintlockWeapon flintlock)
                {
                    MuzzleMountGameObject.transform.parent = self.transform;
                    MuzzleMountGameObject.transform.position = flintlock.GetComponentInChildren<FlintlockBarrel>().Muzzle.position;
                    MuzzleMountGameObject.transform.rotation = flintlock.GetComponentInChildren<FlintlockBarrel>().Muzzle.rotation;
                    MountComponent = MuzzleMountGameObject.AddComponent<FVRFireArmAttachmentMount>();
                }
                else
                {
                    MuzzleMountGameObject.transform.parent = self.transform;
                    MuzzleMountGameObject.transform.position = self.GetMuzzle().position;
                    MuzzleMountGameObject.transform.rotation = self.GetMuzzle().rotation;
                    MountComponent = MuzzleMountGameObject.AddComponent<FVRFireArmAttachmentMount>();
                }

                // Setup Mount Component
                MountComponent.MyObject = self;
                MountComponent.Parent = self;
                MountComponent.ScaleModifier = GetScale(self.RoundType);
                MountComponent.Type = FVRFireArmAttachementMountType.Suppressor;
                MountComponent.Point_Front = MountComponent.transform;
                MountComponent.Point_Rear = MountComponent.transform;
                MountComponent.AttachmentsList = new List<FVRFireArmAttachment>();
                MountComponent.SubMounts = new List<FVRFireArmAttachmentMount>();

                if (parentToThis) MountComponent.ParentToThis = true;

                // Create Trigger
                CapsuleCollider collider = MuzzleMountGameObject.AddComponent<CapsuleCollider>();
                collider.radius = 0.01f;
                collider.height = 0.01f;
                collider.isTrigger = true;

                // Replace potential null mount with new mount and remove other null mounts
                if (hasNullMount)
                {
                    for (int i = 0; i < self.AttachmentMounts.Count; i++)
                    {
                        if (self.AttachmentMounts[i] == null)
                        {
                            self.AttachmentMounts[i] = MountComponent;
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
                else self.AttachmentMounts.Add(MountComponent);

                MuzzleMountGameObject.SetActive(true);
            }
        }

        private void MuzzleDevice_Awake(On.FistVR.MuzzleDevice.orig_Awake orig, MuzzleDevice self)
        {
            orig(self);

            bool hasMuzzleMount = false;
            if (self.AttachmentMounts == null) self.AttachmentMounts = new List<FVRFireArmAttachmentMount>();
            bool hasNullMount = false;
            // Check if Muzzle mount exists
            foreach (var Mount in self.AttachmentMounts)
            {
                // Check for potential null mount
                if (Mount == null)
                {
                    hasNullMount = true;
                    continue;
                }
                if (Mount.Type == FVRFireArmAttachementMountType.Suppressor) hasMuzzleMount = true;
            }

            if (!hasMuzzleMount)
            {
                GameObject MuzzleMountGameObject = new GameObject(MOUNTNAME);
                FVRFireArmAttachmentMount MountComponent;

                // Setup Mount GameObject
                MuzzleMountGameObject.transform.parent = self.transform;
                MuzzleMountGameObject.transform.position = self.Muzzle.position;
                MuzzleMountGameObject.transform.rotation = self.Muzzle.rotation;
                MuzzleMountGameObject.layer = LayerMask.NameToLayer(LAYERNAME);
                MuzzleMountGameObject.tag = TAG;
                MuzzleMountGameObject.SetActive(false);

                // Setup Mount Component
                MountComponent = MuzzleMountGameObject.AddComponent<FVRFireArmAttachmentMount>();
                MountComponent.MyObject = self;
                MountComponent.Parent = self;
                MountComponent.ScaleModifier = 1f;
                MountComponent.Type = FVRFireArmAttachementMountType.Suppressor;
                MountComponent.Point_Front = MountComponent.transform;
                MountComponent.Point_Rear = MountComponent.transform;
                MountComponent.AttachmentsList = new List<FVRFireArmAttachment>();
                MountComponent.SubMounts = new List<FVRFireArmAttachmentMount>();

                // Create Trigger
                CapsuleCollider collider = MuzzleMountGameObject.AddComponent<CapsuleCollider>();
                collider.radius = 0.01f;
                collider.height = 0.01f;
                collider.isTrigger = true;

                // Replace potential null mount with new mount and remove other null mounts
                if (hasNullMount)
                {
                    for (int i = 0; i < self.AttachmentMounts.Count; i++)
                    {
                        if (self.AttachmentMounts[i] == null)
                        {
                            self.AttachmentMounts[i] = MountComponent;
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
                else self.AttachmentMounts.Add(MountComponent);

                MuzzleMountGameObject.SetActive(true);
            }
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
                            muzzleEffect.OverridePoint.GetChild(j).position = muzzleEffect.OverridePoint.position + (self.Barrels[i].MuzzlePoint.position - muzzleDevice.curMount.GetRootMount().transform.position);
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
                            muzzleEffect.OverridePoint.GetChild(i).position = muzzleEffect.OverridePoint.position + (self.Barrels[b].Muzzle.position - muzzleDevice.curMount.GetRootMount().transform.position);
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
            if (self.MuzzleDevices.Count != 0)
            {
                return self.MuzzleDevices[self.MuzzleDevices.Count - 1].Muzzle;
            }
            else return orig(self);
        }
#if !(DEBUG || MEATKIT)

#endif
    }
}
