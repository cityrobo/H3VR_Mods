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
		public override void Awake()
        {
            base.Awake();

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
				this.m_numRounds = 0;
				for (int i = 0; i < this.m_capacity; i++)
				{
					this.AddRound(rClass, false, false);
				}
				leftMag.ReloadMagWithType(rClass);
				rightMag.ReloadMagWithType(rClass);
				this.UpdateBulletDisplay();
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

			if (this.leftMag.CanManuallyEjectRounds && this.leftMag.RoundEjectionPos != null && this.leftMag.HasARound())
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
					if (this.FireArm != null)
					{
						this.FireArm.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound, 1f);
					}
					else
					{
						SM.PlayGenericSound(this.Profile.MagazineEjectRound, base.transform.position);
					}
					if (hand.OtherHand.CurrentInteractable == null && hand.OtherHand.Input.IsGrabbing && Vector3.Distance(this.leftMag.RoundEjectionPos.position, hand.OtherHand.Input.Pos) < 0.15f)
					{
						GameObject original = this.leftMag.RemoveRound(false);
						GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original, this.leftMag.RoundEjectionPos.position, this.leftMag.RoundEjectionPos.rotation);
						FVRFireArmRound component2 = gameObject2.GetComponent<FVRFireArmRound>();
						component2.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						hand.OtherHand.ForceSetInteractable(component2);
						component2.BeginInteraction(hand.OtherHand);
					}
					else if (hand.OtherHand.CurrentInteractable is FVRFireArmRound && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).RoundType == this.leftMag.RoundType && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).ProxyRounds.Count < ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).MaxPalmedAmount && Vector3.Distance(hand.Input.Pos, hand.OtherHand.Input.Pos) < 0.15f)
					{
						FireArmRoundClass lr_Class = this.leftMag.LoadedRounds[this.leftMag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper = this.leftMag.LoadedRounds[this.leftMag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).AddProxy(lr_Class, lr_ObjectWrapper);
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).UpdateProxyDisplay();
						this.leftMag.RemoveRound();
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject == null)
					{
						GameObject original2 = this.leftMag.RemoveRound(false);
						GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(original2, this.leftMag.RoundEjectionPos.position, this.leftMag.RoundEjectionPos.rotation);
						FVRFireArmRound component3 = gameObject3.GetComponent<FVRFireArmRound>();
						component3.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						component3.SetQuickBeltSlot(hand.CurrentHoveredQuickbeltSlotDirty);
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject is FVRFireArmRound && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).RoundType == this.leftMag.RoundType && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).ProxyRounds.Count < ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).MaxPalmedAmount)
					{
						FireArmRoundClass lr_Class2 = this.leftMag.LoadedRounds[this.leftMag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper2 = this.leftMag.LoadedRounds[this.leftMag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).AddProxy(lr_Class2, lr_ObjectWrapper2);
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).UpdateProxyDisplay();
						this.leftMag.RemoveRound();
					}
					else
					{
						GameObject original3 = this.leftMag.RemoveRound(false);
						GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(original3, this.leftMag.RoundEjectionPos.position, this.leftMag.RoundEjectionPos.rotation);
						gameObject4.GetComponent<FVRFireArmRound>().SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						gameObject4.GetComponent<Rigidbody>().AddForce(gameObject4.transform.forward * 0.5f);
						if (this.leftMag.DisplayBullets.Length > 0 && this.leftMag.DisplayBullets[0] != null)
						{
							gameObject4.GetComponent<FVRFireArmRound>().BeginAnimationFrom(this.leftMag.DisplayBullets[0].transform.position, this.leftMag.DisplayBullets[0].transform.rotation);
						}
					}
				}
			}
			//Debug.Log("Mag oof 4");

			if (this.rightMag.CanManuallyEjectRounds && this.rightMag.RoundEjectionPos != null && this.rightMag.HasARound())
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
					if (this.FireArm != null)
					{
						this.FireArm.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound, 1f);
					}
					else
					{
						SM.PlayGenericSound(this.Profile.MagazineEjectRound, base.transform.position);
					}
					if (hand.OtherHand.CurrentInteractable == null && hand.OtherHand.Input.IsGrabbing && Vector3.Distance(this.rightMag.RoundEjectionPos.position, hand.OtherHand.Input.Pos) < 0.15f)
					{
						GameObject original = this.rightMag.RemoveRound(false);
						GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original, this.rightMag.RoundEjectionPos.position, this.rightMag.RoundEjectionPos.rotation);
						FVRFireArmRound component2 = gameObject2.GetComponent<FVRFireArmRound>();
						component2.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						hand.OtherHand.ForceSetInteractable(component2);
						component2.BeginInteraction(hand.OtherHand);
					}
					else if (hand.OtherHand.CurrentInteractable is FVRFireArmRound && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).RoundType == this.rightMag.RoundType && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).ProxyRounds.Count < ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).MaxPalmedAmount && Vector3.Distance(hand.Input.Pos, hand.OtherHand.Input.Pos) < 0.15f)
					{
						FireArmRoundClass lr_Class = this.rightMag.LoadedRounds[this.rightMag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper = this.rightMag.LoadedRounds[this.rightMag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).AddProxy(lr_Class, lr_ObjectWrapper);
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).UpdateProxyDisplay();
						this.rightMag.RemoveRound();
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject == null)
					{
						GameObject original2 = this.rightMag.RemoveRound(false);
						GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(original2, this.rightMag.RoundEjectionPos.position, this.rightMag.RoundEjectionPos.rotation);
						FVRFireArmRound component3 = gameObject3.GetComponent<FVRFireArmRound>();
						component3.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						component3.SetQuickBeltSlot(hand.CurrentHoveredQuickbeltSlotDirty);
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject is FVRFireArmRound && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).RoundType == this.rightMag.RoundType && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).ProxyRounds.Count < ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).MaxPalmedAmount)
					{
						FireArmRoundClass lr_Class2 = this.rightMag.LoadedRounds[this.rightMag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper2 = this.rightMag.LoadedRounds[this.rightMag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).AddProxy(lr_Class2, lr_ObjectWrapper2);
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).UpdateProxyDisplay();
						this.rightMag.RemoveRound();
					}
					else
					{
						GameObject original3 = this.rightMag.RemoveRound(false);
						GameObject gameObject4 = UnityEngine.Object.Instantiate<GameObject>(original3, this.rightMag.RoundEjectionPos.position, this.rightMag.RoundEjectionPos.rotation);
						gameObject4.GetComponent<FVRFireArmRound>().SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						gameObject4.GetComponent<Rigidbody>().AddForce(gameObject4.transform.forward * 0.5f);
						if (this.rightMag.DisplayBullets.Length > 0 && this.rightMag.DisplayBullets[0] != null)
						{
							gameObject4.GetComponent<FVRFireArmRound>().BeginAnimationFrom(this.rightMag.DisplayBullets[0].transform.position, this.rightMag.DisplayBullets[0].transform.rotation);
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
			for (int i = 0; i < Mathf.Min(this.leftMag.LoadedRounds.Length, leftComponent.LoadedRounds.Length); i++)
			{
				if (this.leftMag.LoadedRounds[i] != null && this.leftMag.LoadedRounds[i].LR_Mesh != null)
				{
					leftComponent.LoadedRounds[i].LR_Class = this.leftMag.LoadedRounds[i].LR_Class;
					leftComponent.LoadedRounds[i].LR_Mesh = this.leftMag.LoadedRounds[i].LR_Mesh;
					leftComponent.LoadedRounds[i].LR_Material = this.leftMag.LoadedRounds[i].LR_Material;
					leftComponent.LoadedRounds[i].LR_ObjectWrapper = this.leftMag.LoadedRounds[i].LR_ObjectWrapper;
				}
			}
			leftComponent.m_numRounds = this.leftMag.m_numRounds;
			leftComponent.UpdateBulletDisplay();

			FVRFireArmMagazine rightComponent = mag.rightMag.GetComponent<FVRFireArmMagazine>();
			for (int i = 0; i < Mathf.Min(this.rightMag.LoadedRounds.Length, rightComponent.LoadedRounds.Length); i++)
			{
				if (this.rightMag.LoadedRounds[i] != null && this.rightMag.LoadedRounds[i].LR_Mesh != null)
				{
					rightComponent.LoadedRounds[i].LR_Class = this.rightMag.LoadedRounds[i].LR_Class;
					rightComponent.LoadedRounds[i].LR_Mesh = this.rightMag.LoadedRounds[i].LR_Mesh;
					rightComponent.LoadedRounds[i].LR_Material = this.rightMag.LoadedRounds[i].LR_Material;
					rightComponent.LoadedRounds[i].LR_ObjectWrapper = this.rightMag.LoadedRounds[i].LR_ObjectWrapper;
				}
			}
			rightComponent.m_numRounds = this.rightMag.m_numRounds;
			rightComponent.UpdateBulletDisplay();

			return gameObject;
		}
#endif 
	}
}
