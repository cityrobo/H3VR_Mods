using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
	[BepInPlugin("h3vr.cityrobo.reloadfromquickbelt", "ReloadFromQuickBelt", "1.0.0")]
	public class ReloadFromQuickBelt : BaseUnityPlugin
    {
        static ReloadFromQuickBelt()
        {
            On.FistVR.FVRFireArmReloadTriggerMag.OnTriggerEnter += FVRFireArmReloadTriggerMag_OnTriggerEnter;
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
