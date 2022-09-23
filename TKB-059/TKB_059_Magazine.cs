using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace Cityrobo
{
    class TKB_059_Magazine : FVRFireArmMagazine
    {
        [Header("TKB_059_Magazine Config")]
        public FVRFireArmMagazine leftMag;
        public FVRFireArmMagazine rightMag;

#if !(UNITY_EDITOR || UNITY_5)
		public override void Start()
        {
            base.Start();

            leftMag.StoreAndDestroyRigidbody();
            rightMag.StoreAndDestroyRigidbody();

            leftMag.gameObject.layer = LayerMask.NameToLayer("Default");
            rightMag.gameObject.layer = LayerMask.NameToLayer("Default");

			Hook();
        }

        public override void OnDestroy()
        {
			base.OnDestroy();

			Unhook();
        }

		public void Unhook()
        {
			//On.FistVR.AmmoSpawnerV2.LoadIntoHeldObjects -= AmmoSpawnerV2_LoadIntoHeldObjects;
			On.FistVR.FVRFireArmMagazine.ReloadMagWithType -= FVRFireArmMagazine_ReloadMagWithType;

		}
		public void Hook()
		{
			//On.FistVR.AmmoSpawnerV2.LoadIntoHeldObjects += AmmoSpawnerV2_LoadIntoHeldObjects;
			On.FistVR.FVRFireArmMagazine.ReloadMagWithType += FVRFireArmMagazine_ReloadMagWithType;
		}

		private void FVRFireArmMagazine_ReloadMagWithType(On.FistVR.FVRFireArmMagazine.orig_ReloadMagWithType orig, FVRFireArmMagazine self, FireArmRoundClass rClass)
        {
			if (self == this)
			{
				m_numRounds = 0;
				for (int i = 0; i < m_capacity; i++)
				{
					AddRound(rClass, false, false);
				}
				leftMag.ReloadMagWithType(rClass);
				rightMag.ReloadMagWithType(rClass);
				UpdateBulletDisplay();
			}
			else orig(self, rClass);
        }

        private void AmmoSpawnerV2_LoadIntoHeldObjects(On.FistVR.AmmoSpawnerV2.orig_LoadIntoHeldObjects orig, AmmoSpawnerV2 self)
        {
			FireArmRoundType curAmmoType = self.m_curAmmoType;
			FireArmRoundClass curAmmoClass = self.m_curAmmoClass;
			for (int i = 0; i < GM.CurrentMovementManager.Hands.Length; i++)
			{
				if (GM.CurrentMovementManager.Hands[i].CurrentInteractable != null && GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRPhysicalObject)
				{
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is TKB_059_Magazine)
					{
						TKB_059_Magazine fvrfireArmMagazine = GM.CurrentMovementManager.Hands[i].CurrentInteractable as TKB_059_Magazine;
						if (fvrfireArmMagazine.RoundType == curAmmoType)
						{
							fvrfireArmMagazine.ReloadMagWithType(curAmmoClass);
						}
					}

					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRFireArmMagazine)
					{
						FVRFireArmMagazine fvrfireArmMagazine = GM.CurrentMovementManager.Hands[i].CurrentInteractable as FVRFireArmMagazine;
						if (fvrfireArmMagazine.RoundType == curAmmoType)
						{
							fvrfireArmMagazine.ReloadMagWithType(curAmmoClass);
						}
					}
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRFireArm)
					{
						FVRFireArm fvrfireArm = GM.CurrentMovementManager.Hands[i].CurrentInteractable as FVRFireArm;
						if (fvrfireArm.RoundType == curAmmoType && fvrfireArm.Magazine != null)
						{
							fvrfireArm.Magazine.ReloadMagWithType(curAmmoClass);
						}
					}
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is FVRFireArmClip)
					{
						FVRFireArmClip fvrfireArmClip = GM.CurrentMovementManager.Hands[i].CurrentInteractable as FVRFireArmClip;
						if (fvrfireArmClip.RoundType == curAmmoType)
						{
							fvrfireArmClip.ReloadClipWithType(curAmmoClass);
						}
					}
					if (GM.CurrentMovementManager.Hands[i].CurrentInteractable is Speedloader)
					{
						Speedloader speedloader = GM.CurrentMovementManager.Hands[i].CurrentInteractable as Speedloader;
						if (speedloader.Chambers[0].Type == curAmmoType)
						{
							speedloader.ReloadClipWithType(curAmmoClass);
						}
					}
				}
			}
		}

        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);

			if (leftMag.CanManuallyEjectRounds && leftMag.RoundEjectionPos != null && leftMag.HasARound())
			{
				bool flag3 = false;
				if (hand.IsInStreamlinedMode && hand.Input.BYButtonDown)
				{
					flag3 = true;
				}
				else if (!hand.IsInStreamlinedMode && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
				{
					flag3 = true;
				}
				if (flag3)
				{
					if (FireArm != null)
					{
						FireArm.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound, 1f);
					}
					else
					{
						SM.PlayGenericSound(Profile.MagazineEjectRound, base.transform.position);
					}
					if (hand.OtherHand.CurrentInteractable == null && hand.OtherHand.Input.IsGrabbing && Vector3.Distance(leftMag.RoundEjectionPos.position, hand.OtherHand.Input.Pos) < 0.15f)
					{
						GameObject original = leftMag.RemoveRound(false);
						GameObject gameObject2 = Instantiate(original, leftMag.RoundEjectionPos.position, leftMag.RoundEjectionPos.rotation);
						FVRFireArmRound component2 = gameObject2.GetComponent<FVRFireArmRound>();
						component2.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						hand.OtherHand.ForceSetInteractable(component2);
						component2.BeginInteraction(hand.OtherHand);
					}
					else if (hand.OtherHand.CurrentInteractable is FVRFireArmRound && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).RoundType == leftMag.RoundType && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).ProxyRounds.Count < ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).MaxPalmedAmount && Vector3.Distance(hand.Input.Pos, hand.OtherHand.Input.Pos) < 0.15f)
					{
						FireArmRoundClass lr_Class = leftMag.LoadedRounds[leftMag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper = leftMag.LoadedRounds[leftMag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).AddProxy(lr_Class, lr_ObjectWrapper);
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).UpdateProxyDisplay();
						leftMag.RemoveRound();
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject == null)
					{
						GameObject original2 = leftMag.RemoveRound(false);
						GameObject gameObject3 = Instantiate(original2, leftMag.RoundEjectionPos.position, leftMag.RoundEjectionPos.rotation);
						FVRFireArmRound component3 = gameObject3.GetComponent<FVRFireArmRound>();
						component3.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						component3.SetQuickBeltSlot(hand.CurrentHoveredQuickbeltSlotDirty);
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject is FVRFireArmRound && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).RoundType == leftMag.RoundType && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).ProxyRounds.Count < ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).MaxPalmedAmount)
					{
						FireArmRoundClass lr_Class2 = leftMag.LoadedRounds[leftMag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper2 = leftMag.LoadedRounds[leftMag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).AddProxy(lr_Class2, lr_ObjectWrapper2);
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).UpdateProxyDisplay();
						leftMag.RemoveRound();
					}
					else
					{
						GameObject original3 = leftMag.RemoveRound(false);
						GameObject gameObject4 = Instantiate(original3, leftMag.RoundEjectionPos.position, leftMag.RoundEjectionPos.rotation);
						gameObject4.GetComponent<FVRFireArmRound>().SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						gameObject4.GetComponent<Rigidbody>().AddForce(gameObject4.transform.forward * 0.5f);
						if (leftMag.DisplayBullets.Length > 0 && leftMag.DisplayBullets[0] != null)
						{
							gameObject4.GetComponent<FVRFireArmRound>().BeginAnimationFrom(leftMag.DisplayBullets[0].transform.position, leftMag.DisplayBullets[0].transform.rotation);
						}
					}
				}
			}
			//Debug.Log("Mag oof 4");

			if (rightMag.CanManuallyEjectRounds && rightMag.RoundEjectionPos != null && rightMag.HasARound())
			{
				bool flag3 = false;
				if (hand.IsInStreamlinedMode && hand.Input.BYButtonDown)
				{
					flag3 = true;
				}
				else if (!hand.IsInStreamlinedMode && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
				{
					flag3 = true;
				}
				if (flag3)
				{
					if (FireArm != null)
					{
						FireArm.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound, 1f);
					}
					else
					{
						SM.PlayGenericSound(Profile.MagazineEjectRound, transform.position);
					}
					if (hand.OtherHand.CurrentInteractable == null && hand.OtherHand.Input.IsGrabbing && Vector3.Distance(rightMag.RoundEjectionPos.position, hand.OtherHand.Input.Pos) < 0.15f)
					{
						GameObject original = rightMag.RemoveRound(false);
						GameObject gameObject2 = Instantiate(original, rightMag.RoundEjectionPos.position, rightMag.RoundEjectionPos.rotation);
						FVRFireArmRound component2 = gameObject2.GetComponent<FVRFireArmRound>();
						component2.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						hand.OtherHand.ForceSetInteractable(component2);
						component2.BeginInteraction(hand.OtherHand);
					}
					else if (hand.OtherHand.CurrentInteractable is FVRFireArmRound && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).RoundType == rightMag.RoundType && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).ProxyRounds.Count < ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).MaxPalmedAmount && Vector3.Distance(hand.Input.Pos, hand.OtherHand.Input.Pos) < 0.15f)
					{
						FireArmRoundClass lr_Class = rightMag.LoadedRounds[rightMag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper = rightMag.LoadedRounds[rightMag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).AddProxy(lr_Class, lr_ObjectWrapper);
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).UpdateProxyDisplay();
						rightMag.RemoveRound();
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject == null)
					{
						GameObject original2 = rightMag.RemoveRound(false);
						GameObject gameObject3 = Instantiate(original2, rightMag.RoundEjectionPos.position, rightMag.RoundEjectionPos.rotation);
						FVRFireArmRound component3 = gameObject3.GetComponent<FVRFireArmRound>();
						component3.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						component3.SetQuickBeltSlot(hand.CurrentHoveredQuickbeltSlotDirty);
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject is FVRFireArmRound && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).RoundType == rightMag.RoundType && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).ProxyRounds.Count < ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).MaxPalmedAmount)
					{
						FireArmRoundClass lr_Class2 = rightMag.LoadedRounds[rightMag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper2 = rightMag.LoadedRounds[rightMag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).AddProxy(lr_Class2, lr_ObjectWrapper2);
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).UpdateProxyDisplay();
						rightMag.RemoveRound();
					}
					else
					{
						GameObject original3 = rightMag.RemoveRound(false);
						GameObject gameObject4 = Instantiate(original3, rightMag.RoundEjectionPos.position, rightMag.RoundEjectionPos.rotation);
						gameObject4.GetComponent<FVRFireArmRound>().SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						gameObject4.GetComponent<Rigidbody>().AddForce(gameObject4.transform.forward * 0.5f);
						if (rightMag.DisplayBullets.Length > 0 && rightMag.DisplayBullets[0] != null)
						{
							gameObject4.GetComponent<FVRFireArmRound>().BeginAnimationFrom(rightMag.DisplayBullets[0].transform.position, rightMag.DisplayBullets[0].transform.rotation);
						}
					}
				}
			}
		}

		public override GameObject DuplicateFromSpawnLock(FVRViveHand hand)
		{
			GameObject gameObject = base.DuplicateFromSpawnLock(hand);
			TKB_059_Magazine mag = gameObject.GetComponent<TKB_059_Magazine>();

			FVRFireArmMagazine leftComponent = mag.leftMag.GetComponent<FVRFireArmMagazine>();
			for (int i = 0; i < Mathf.Min(leftMag.LoadedRounds.Length, leftComponent.LoadedRounds.Length); i++)
			{
				if (leftMag.LoadedRounds[i] != null && leftMag.LoadedRounds[i].LR_Mesh != null)
				{
					leftComponent.LoadedRounds[i].LR_Class = leftMag.LoadedRounds[i].LR_Class;
					leftComponent.LoadedRounds[i].LR_Mesh = leftMag.LoadedRounds[i].LR_Mesh;
					leftComponent.LoadedRounds[i].LR_Material = leftMag.LoadedRounds[i].LR_Material;
					leftComponent.LoadedRounds[i].LR_ObjectWrapper = leftMag.LoadedRounds[i].LR_ObjectWrapper;
				}
			}
			leftComponent.m_numRounds = leftMag.m_numRounds;
			leftComponent.UpdateBulletDisplay();

			FVRFireArmMagazine rightComponent = mag.rightMag.GetComponent<FVRFireArmMagazine>();
			for (int i = 0; i < Mathf.Min(rightMag.LoadedRounds.Length, rightComponent.LoadedRounds.Length); i++)
			{
				if (rightMag.LoadedRounds[i] != null && rightMag.LoadedRounds[i].LR_Mesh != null)
				{
					rightComponent.LoadedRounds[i].LR_Class = rightMag.LoadedRounds[i].LR_Class;
					rightComponent.LoadedRounds[i].LR_Mesh = rightMag.LoadedRounds[i].LR_Mesh;
					rightComponent.LoadedRounds[i].LR_Material = rightMag.LoadedRounds[i].LR_Material;
					rightComponent.LoadedRounds[i].LR_ObjectWrapper = rightMag.LoadedRounds[i].LR_ObjectWrapper;
				}
			}
			rightComponent.m_numRounds = rightMag.m_numRounds;
			rightComponent.UpdateBulletDisplay();

			return gameObject;
		}
#endif 
	}
}
