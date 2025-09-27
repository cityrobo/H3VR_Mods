using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using UnityEngine;

namespace Cityrobo
{
    public class MultiBarrelMount : FVRFireArmAttachmentMount 
    {
        private static readonly Dictionary<FVRFireArmAttachment, MultiBarrelAttachment> _existingMultiBarrelAttachments = [];

        [Serializable]
        public class MultiBarrelAttachment
        {
            public readonly FVRFireArmAttachment Attachment;
            public GameObject Viz = null;
            public List<GameObject> VizCopies = [];
            private Vector3 _origMuzzlePos;
            private readonly Dictionary<MuzzleEffect, Vector3> _origMuzzleEffectPos = [];
            private bool _wasSuppressorWellMounted = false;

            public MultiBarrelAttachment(FVRFireArmAttachment attachment)
            {
                Attachment = attachment;
                if (Attachment is MuzzleDevice muzzleDevice)
                {
                    _origMuzzlePos = muzzleDevice.Muzzle.localPosition;
                    foreach (var muzzleEffect in muzzleDevice.MuzzleEffects)
                    {
                        if (muzzleEffect.OverridePoint != null) _origMuzzleEffectPos.Add(muzzleEffect, muzzleEffect.OverridePoint.localPosition);
                    }
                }
                if (Attachment is Suppressor suppressor && suppressor.CatchRot >= 359f) _wasSuppressorWellMounted = true;
            }

            public void Destroy()
            {
                Viz?.SetActive(true);

                for (int i = 0; i < VizCopies.Count; i++)
                {
                    UnityEngine.Object.Destroy(VizCopies[i]);
                }

                if (Attachment is MuzzleDevice muzzleDevice)
                {
                    muzzleDevice.Muzzle.localPosition = _origMuzzlePos;

                    foreach (var muzzleEffectPos in _origMuzzleEffectPos)
                    {
                        muzzleEffectPos.Key.OverridePoint.localPosition = muzzleEffectPos.Value;
                    }
                }
                _existingMultiBarrelAttachments.Remove(Attachment);
            }

            public void Update()
            {
                if (Attachment is Suppressor suppressor)
                {
                    if (suppressor.CatchRot < 359f && _wasSuppressorWellMounted)
                    {
                        foreach (var VizCopy in VizCopies)
                        {
                            VizCopy.SetActive(false);
                        }

                        _wasSuppressorWellMounted = false;
                        Viz?.SetActive(true);
                    }
                    else if (suppressor.CatchRot >= 359f && !_wasSuppressorWellMounted)
                    {
                        foreach (var VizCopy in VizCopies)
                        {
                            VizCopy.SetActive(true);
                        }

                        Viz?.SetActive(false);
                        _wasSuppressorWellMounted = true;
                        suppressor.ForceBreakInteraction();
                        suppressor.AttachmentInterface.ForceBreakInteraction();
                    }
                }
            }
        }

        private static void CreateNewMultiBarrelAttachment(FVRFireArmAttachment newAttachment)
        {
            if (newAttachment is MuzzleDevice && newAttachment.curMount.GetRootMount().ScaleModifier != 1 && newAttachment.curMount.MyObject is FVRFireArmAttachment parentAttachment && !parentAttachment.CanScaleToMount && newAttachment.CanScaleToMount) newAttachment.ScaleToMount(parentAttachment.curMount.GetRootMount());

            BreakActionWeapon breakAction = newAttachment.curMount.Parent as BreakActionWeapon;
            Derringer derringer = newAttachment.curMount.Parent as Derringer;

            if ((breakAction != null || derringer != null) && newAttachment != null && newAttachment is MuzzleDevice && !_existingMultiBarrelAttachments.ContainsKey(newAttachment))
            {
                Renderer[] renderers = newAttachment.GetComponentsInChildren<Renderer>().Where
                    (
                        obj => obj is not ParticleSystemRenderer
                        && obj.sharedMaterials.Length > 0
                        && obj.sharedMaterials[0] != null
                        && !obj.sharedMaterials[0].name.Contains("Default-Material")
                    ).ToArray();

                if (renderers.Length > 0)
                {
                    MultiBarrelAttachment multiBarrelAttachment = new MultiBarrelAttachment(newAttachment);
                    _existingMultiBarrelAttachments.Add(newAttachment, multiBarrelAttachment);

                    // If attachment is a muzzle device, set up the Viz object and create copies for each barrel
                    if (newAttachment is MuzzleDevice)
                    {
                        if (renderers.Length == 1) multiBarrelAttachment.Viz = renderers[0].gameObject;
                        else
                        {
                            Transform viz = newAttachment.transform.Find("Viz");
                            if (viz != null) multiBarrelAttachment.Viz = viz.gameObject;
                            if (multiBarrelAttachment.Viz == null)
                            {
                                multiBarrelAttachment.Viz = new GameObject("Viz");
                                multiBarrelAttachment.Viz.transform.SetParent(newAttachment.transform);
                                multiBarrelAttachment.Viz.transform.localPosition = Vector3.zero;
                                multiBarrelAttachment.Viz.transform.localRotation = Quaternion.identity;

                                foreach (var meshRenderer in renderers)
                                {
                                    meshRenderer.transform.SetParent(multiBarrelAttachment.Viz.transform);
                                }
                            }
                        }

                        Vector3 attachmentOffset;
                        Suppressor suppressor = newAttachment as Suppressor;
                        if (breakAction != null && multiBarrelAttachment.Viz != null)
                        {
                            foreach (var barrel in breakAction.Barrels)
                            {
                                attachmentOffset = barrel.Muzzle.position - newAttachment.curMount.GetRootMount().transform.position;
                                GameObject vizCopy = Instantiate(multiBarrelAttachment.Viz, multiBarrelAttachment.Viz.transform.position + attachmentOffset, multiBarrelAttachment.Viz.transform.rotation, newAttachment.transform);
                                multiBarrelAttachment.VizCopies.Add(vizCopy);
                                if (suppressor != null && suppressor.CatchRot < 359f) vizCopy.SetActive(false);
                            }
                            if (!(suppressor != null && suppressor.CatchRot < 359f)) multiBarrelAttachment.Viz.SetActive(false);
                        }
                        else if (derringer != null && multiBarrelAttachment.Viz != null)
                        {
                            foreach (var barrel in derringer.Barrels)
                            {
                                attachmentOffset = barrel.MuzzlePoint.position - newAttachment.curMount.GetRootMount().transform.position;
                                GameObject vizCopy = Instantiate(multiBarrelAttachment.Viz, multiBarrelAttachment.Viz.transform.position + attachmentOffset, multiBarrelAttachment.Viz.transform.rotation, newAttachment.transform);
                                multiBarrelAttachment.VizCopies.Add(vizCopy);
                                if (suppressor != null && suppressor.CatchRot < 359f) vizCopy.SetActive(false);
                            }
                            if (!(suppressor != null && suppressor.CatchRot < 359f)) multiBarrelAttachment.Viz.SetActive(false);
                        }
                        if (suppressor != null && suppressor.CatchRot >= 359f)
                        {
                            suppressor.ForceBreakInteraction();
                            suppressor.AttachmentInterface.ForceBreakInteraction();
                        }
                    }
                }
            }
        }

        public void Update()
        {
            FVRFireArmAttachment attachment = null;
            if (AttachmentsList.Count > 0) attachment = AttachmentsList[0];

            BreakActionWeapon breakAction = Parent as BreakActionWeapon;
            Derringer derringer = Parent as Derringer;
            if ((breakAction != null || derringer != null) && attachment != null && !_existingMultiBarrelAttachments.ContainsKey(attachment))  
            {
                Suppressor suppressor = attachment as Suppressor;

                // If attachment is a suppressor, only proceed if it's fully screwed in
                if (suppressor != null && suppressor.CatchRot < 359f) return;

                CreateNewMultiBarrelAttachment(attachment);

                if (suppressor != null)
                {
                    suppressor.ForceBreakInteraction();
                    suppressor.AttachmentInterface.ForceBreakInteraction();
                }
            }

            foreach (var multiBarrelAttachment in _existingMultiBarrelAttachments.Values)
            {
                multiBarrelAttachment?.Update();
            }
        }

#if !DEBUG
        static MultiBarrelMount()
        {
            On.FistVR.FVRPhysicalObject.RegisterAttachment += FVRPhysicalObject_RegisterAttachment;
            On.FistVR.FVRPhysicalObject.DeRegisterAttachment += FVRPhysicalObject_DeRegisterAttachment;

            On.FistVR.FVRFireArm.GetMuzzle += FVRFireArm_GetMuzzle;
            On.FistVR.BreakActionWeapon.GetMuzzle += BreakActionWeapon_GetMuzzle;
            On.FistVR.BreakActionWeapon.Fire += BreakActionWeapon_Fire;
            On.FistVR.Derringer.GetMuzzle += Derringer_GetMuzzle;
            On.FistVR.Derringer.FireBarrel += Derringer_FireBarrel;
        }

        private static void FVRPhysicalObject_DeRegisterAttachment(On.FistVR.FVRPhysicalObject.orig_DeRegisterAttachment orig, FVRPhysicalObject self, FVRFireArmAttachment attachment)
        {
            if (attachment.curMount.GetRootMount() is MultiBarrelMount)
            {
                if (_existingMultiBarrelAttachments.TryGetValue(attachment, out var multiBarrelAttachment))
                {
                    multiBarrelAttachment.Destroy();
                }
            }

            orig(self, attachment);
        }

        private static void FVRPhysicalObject_RegisterAttachment(On.FistVR.FVRPhysicalObject.orig_RegisterAttachment orig, FVRPhysicalObject self, FVRFireArmAttachment attachment)
        {
            orig(self, attachment);

            if (attachment.curMount.GetRootMount() is MultiBarrelMount)
            {
                CreateNewMultiBarrelAttachment(attachment);
            }
        }

        private static void Derringer_FireBarrel(On.FistVR.Derringer.orig_FireBarrel orig, Derringer self, int i)
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

        private static bool BreakActionWeapon_Fire(On.FistVR.BreakActionWeapon.orig_Fire orig, BreakActionWeapon self, int b, bool FireAllBarrels, int index)
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

        private static Transform FVRFireArm_GetMuzzle(On.FistVR.FVRFireArm.orig_GetMuzzle orig, FVRFireArm self)
        {
            if (self is FlintlockWeapon flintlock) return flintlock.GetComponentInChildren<FlintlockBarrel>().Muzzle;
            else return orig(self);
        }

        private static Transform Derringer_GetMuzzle(On.FistVR.Derringer.orig_GetMuzzle orig, Derringer self)
        {
            if (self.MuzzleDevices.Count != 0) return self.MuzzleDevices[self.MuzzleDevices.Count - 1].Muzzle;
            else return orig(self);
        }

        private static Transform BreakActionWeapon_GetMuzzle(On.FistVR.BreakActionWeapon.orig_GetMuzzle orig, BreakActionWeapon self)
        {
            if (self.MuzzleDevices.Count != 0) return self.MuzzleDevices[self.MuzzleDevices.Count - 1].Muzzle;
            else return orig(self);
        }
#endif
    }
}
