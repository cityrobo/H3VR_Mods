using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using System.ComponentModel;

namespace Cityrobo
{
	[BepInPlugin("h3vr.cityrobo.reloadfromquickbelt", "ReloadFromQuickBelt", "1.0.0")]
	public class ReloadFromQuickBelt : BaseUnityPlugin
    {
        private static IntPtr s_FVRPhysicalObject_DuplicateFromSpawnLock_Pointer;


        static ReloadFromQuickBelt()
        {
            On.FistVR.FVRFireArmReloadTriggerMag.OnTriggerEnter += FVRFireArmReloadTriggerMag_OnTriggerEnter;
            On.FistVR.Speedloader.OnTriggerEnter += Speedloader_OnTriggerEnter;
            On.FistVR.FVRFireArmRound.OnTriggerEnter += FVRFireArmRound_OnTriggerEnter;

            s_FVRPhysicalObject_DuplicateFromSpawnLock_Pointer = typeof(FVRPhysicalObject).GetMethod("DuplicateFromSpawnLock").MethodHandle.GetFunctionPointer();
        }

        private static void FVRFireArmRound_OnTriggerEnter(On.FistVR.FVRFireArmRound.orig_OnTriggerEnter orig, FVRFireArmRound self, Collider collider)
        {
            orig(self, collider);

            if (self.QuickbeltSlot != null)
            {
                if (self.IsSpent)
                {
                    return;
                }
                if (self.isManuallyChamberable && !self.IsSpent && self.HoveredOverChamber == null && self.m_hoverOverReloadTrigger == null && !self.IsSpent && collider.gameObject.CompareTag("FVRFireArmChamber"))
                {
                    FVRFireArmChamber component = collider.gameObject.GetComponent<FVRFireArmChamber>();
                    if (component.RoundType == self.RoundType && component.IsManuallyChamberable && component.IsAccessible && !component.IsFull)
                    {
                        if (self.m_isSpawnLock)
                        {
                            Func<FVRViveHand, GameObject> DuplicateFromSpawnLock = (Func<FVRViveHand, GameObject>) Activator.CreateInstance(typeof(Func<FVRViveHand, GameObject>), self, s_FVRPhysicalObject_DuplicateFromSpawnLock_Pointer);

                            GameObject copy = DuplicateFromSpawnLock(null);
                            FVRFireArmRound copyRound = copy.GetComponent<FVRFireArmRound>();
                            copyRound.DestroyAllProxies();

                            copyRound.HoveredOverChamber = component;
                        }
                        else
                        {
                            self.ClearQuickbeltState();
                            self.HoveredOverChamber = component;
                        }
                    }
                }
                if (self.isMagazineLoadable && self.HoveredOverChamber == null && !self.IsSpent && collider.gameObject.CompareTag("FVRFireArmMagazineReloadTrigger"))
                {
                    FVRFireArmMagazineReloadTrigger component2 = collider.gameObject.GetComponent<FVRFireArmMagazineReloadTrigger>();
                    if (component2.IsClipTrigger)
                    {
                        if (component2 != null && component2.Clip != null && component2.Clip.RoundType == self.RoundType && !component2.Clip.IsFull() && (component2.Clip.FireArm == null || component2.Clip.IsDropInLoadable))
                        {
                            if (self.m_isSpawnLock)
                            {
                                Func<FVRViveHand, GameObject> DuplicateFromSpawnLock = (Func<FVRViveHand, GameObject>)Activator.CreateInstance(typeof(Func<FVRViveHand, GameObject>), self, s_FVRPhysicalObject_DuplicateFromSpawnLock_Pointer);

                                GameObject copy = DuplicateFromSpawnLock(null);
                                FVRFireArmRound copyRound = copy.GetComponent<FVRFireArmRound>();
                                copyRound.DestroyAllProxies();

                                copyRound.m_hoverOverReloadTrigger = component2;
                            }
                            else
                            {
                                self.ClearQuickbeltState();
                                self.m_hoverOverReloadTrigger = component2;
                            }
                        }
                    }
                    else if (component2.IsSpeedloaderTrigger && !component2.SpeedloaderChamber.IsLoaded)
                    {
                        if (self.m_isSpawnLock)
                        {
                            Func<FVRViveHand, GameObject> DuplicateFromSpawnLock = (Func<FVRViveHand, GameObject>)Activator.CreateInstance(typeof(Func<FVRViveHand, GameObject>), self, s_FVRPhysicalObject_DuplicateFromSpawnLock_Pointer);

                            GameObject copy = DuplicateFromSpawnLock(null);
                            FVRFireArmRound copyRound = copy.GetComponent<FVRFireArmRound>();
                            copyRound.DestroyAllProxies();

                            copyRound.m_hoverOverReloadTrigger = component2;
                        }
                        else
                        {
                            self.ClearQuickbeltState();
                            self.m_hoverOverReloadTrigger = component2;
                        }
                    }
                    if (component2.IsRemoteGunChamber)
                    {
                        if (self.m_isSpawnLock)
                        {
                            Func<FVRViveHand, GameObject> DuplicateFromSpawnLock = (Func<FVRViveHand, GameObject>)Activator.CreateInstance(typeof(Func<FVRViveHand, GameObject>), self, s_FVRPhysicalObject_DuplicateFromSpawnLock_Pointer);

                            GameObject copy = DuplicateFromSpawnLock(null);
                            FVRFireArmRound copyRound = copy.GetComponent<FVRFireArmRound>();
                            copyRound.DestroyAllProxies();

                            copyRound.m_hoverOverReloadTrigger = component2;
                        }
                        else
                        {
                            self.ClearQuickbeltState();
                            self.m_hoverOverReloadTrigger = component2;
                        }
                    }
                    else if (component2 != null && component2.Magazine != null && component2.Magazine.RoundType == self.RoundType && !component2.Magazine.IsFull() && (component2.Magazine.FireArm == null || component2.Magazine.IsDropInLoadable))
                    {
                        if (self.m_isSpawnLock)
                        {
                            Func<FVRViveHand, GameObject> DuplicateFromSpawnLock = (Func<FVRViveHand, GameObject>)Activator.CreateInstance(typeof(Func<FVRViveHand, GameObject>), self, s_FVRPhysicalObject_DuplicateFromSpawnLock_Pointer);

                            GameObject copy = DuplicateFromSpawnLock(null);
                            FVRFireArmRound copyRound = copy.GetComponent<FVRFireArmRound>();
                            copyRound.DestroyAllProxies();

                            copyRound.m_hoverOverReloadTrigger = component2;
                        }
                        else
                        {
                            self.ClearQuickbeltState();
                            self.m_hoverOverReloadTrigger = component2;
                        }
                    }
                }
                //if (self.isPalmable && self.ProxyRounds.Count < self.MaxPalmedAmount && !self.IsSpent && collider.gameObject.CompareTag("FVRFireArmRound"))
                //{
                //    FVRFireArmRound component3 = collider.gameObject.GetComponent<FVRFireArmRound>();
                //    if (component3.RoundType == self.RoundType && !component3.IsSpent && component3.QuickbeltSlot == null)
                //    {
                //        self.HoveredOverRound = component3;
                //    }
                //}
            }
        }

        private static void Speedloader_OnTriggerEnter(On.FistVR.Speedloader.orig_OnTriggerEnter orig, Speedloader self, Collider c)
        {
            orig(self, c);
            if (self.QuickbeltSlot != null)
            {
                RevolverCylinder component = c.GetComponent<RevolverCylinder>();
                if (component != null && component.Revolver.RoundType == self.Chambers[0].Type && component.CanAccept())
                {
                    if (self.m_isSpawnLock)
                    {
                        GameObject copy = self.DuplicateFromSpawnLock(null);
                        Speedloader copySpeedloader = copy.GetComponent<Speedloader>();

                        copySpeedloader.HoveredCylinder = component;
                    }
                    else
                    {
                        self.ClearQuickbeltState();
                        self.HoveredCylinder = component;
                    }
                }
                RevolvingShotgunTrigger component2 = c.GetComponent<RevolvingShotgunTrigger>();
                bool flag = false;
                if (component2 != null && component2.Shotgun != null && component2.Shotgun.EjectDelay <= 0f && c.gameObject.CompareTag("FVRFireArmReloadTriggerWell") && component2.Shotgun.RoundType == self.Chambers[0].Type && self.SLType == component2.Shotgun.SLType)
                {
                    flag = true;
                }
                else if (component2 != null && component2.GrappleGun != null && component2.GrappleGun.EjectDelay <= 0f && c.gameObject.CompareTag("FVRFireArmReloadTriggerWell") && component2.GrappleGun.RoundType == self.Chambers[0].Type && self.SLType == component2.GrappleGun.SLType && self.Chambers[0].IsLoaded && !self.Chambers[0].IsSpent && self.Chambers[1].IsLoaded && !self.Chambers[1].IsSpent)
                {
                    flag = true;
                }
                if (flag)
                {
                    if (self.m_isSpawnLock)
                    {
                        GameObject copy = self.DuplicateFromSpawnLock(null);
                        Speedloader copySpeedloader = copy.GetComponent<Speedloader>();

                        copySpeedloader.HoveredRSTrigger = component2;
                    }
                    else
                    {
                        self.ClearQuickbeltState();
                        self.HoveredRSTrigger = component2;
                    }
                }
            }
        }

        private static void FVRFireArmReloadTriggerMag_OnTriggerEnter(On.FistVR.FVRFireArmReloadTriggerMag.orig_OnTriggerEnter orig, FVRFireArmReloadTriggerMag self, Collider collider)
        {
            orig(self,collider);
			if (self.Magazine != null && self.Magazine.FireArm == null && self.Magazine.QuickbeltSlot != null && collider.gameObject.tag == "FVRFireArmReloadTriggerWell")
			{
				FVRFireArmReloadTriggerWell component = collider.gameObject.GetComponent<FVRFireArmReloadTriggerWell>();
				bool flag = false;
				if (component != null && !self.Magazine.IsBeltBox && component.FireArm.HasBelt)
				{
					flag = true;
				}
				if (component != null && component.IsBeltBox == self.Magazine.IsBeltBox && component.FireArm != null && component.FireArm.Magazine == null && !flag)
				{
					FireArmMagazineType fireArmMagazineType = component.FireArm.MagazineType;
					if (component.UsesTypeOverride)
					{
						fireArmMagazineType = component.TypeOverride;
					}
					if (fireArmMagazineType == self.Magazine.MagazineType && (component.FireArm.EjectDelay <= 0f || self.Magazine != component.FireArm.LastEjectedMag) && component.FireArm.Magazine == null)
					{
						if (self.Magazine.m_isSpawnLock)
						{
							self.Magazine.DuplicateFromSpawnLock(null).GetComponent<FVRFireArmMagazine>().Load(component.FireArm);
						}
						else
						{
							self.Magazine.ClearQuickbeltState();
							self.Magazine.Load(component.FireArm);
						}
					}
				}
			}
		}
#if !(DEBUG || MEATKIT)

#endif
    }
}
