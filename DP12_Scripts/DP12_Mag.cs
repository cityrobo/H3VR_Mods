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
    public class DP12_Mag : FVRFireArmMagazine
    {
        [Header("DP-12 Mag Config")]
        public FVRFireArmMagazine SecondMagazine;

#if !(DEBUG || MEATKIT)
		public override void Start()
		{
			base.Start();

			SecondMagazine.StoreAndDestroyRigidbody();
			SecondMagazine.gameObject.layer = LayerMask.NameToLayer("Default");

			Hook();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();

			Unhook();
		}

		public void Unhook()
		{
			On.FistVR.FVRFireArmMagazine.ReloadMagWithType -= FVRFireArmMagazine_ReloadMagWithType;
			On.FistVR.FVRFireArmRound.DuplicateFromSpawnLock -= FVRFireArmRound_DuplicateFromSpawnLock;
			On.FistVR.FVRFireArmRound.GetNumRoundsPulled -= FVRFireArmRound_GetNumRoundsPulled;

		}
		public void Hook()
		{
			On.FistVR.FVRFireArmMagazine.ReloadMagWithType += FVRFireArmMagazine_ReloadMagWithType;
            On.FistVR.FVRFireArmRound.DuplicateFromSpawnLock += FVRFireArmRound_DuplicateFromSpawnLock;
            On.FistVR.FVRFireArmRound.GetNumRoundsPulled += FVRFireArmRound_GetNumRoundsPulled;
		}
		// Patching FVRFireArmRound.GetNumRoundsPulled to make smart palming work correctly with the DP-12 magazine system
		private int FVRFireArmRound_GetNumRoundsPulled(On.FistVR.FVRFireArmRound.orig_GetNumRoundsPulled orig, FVRFireArmRound self, FVRViveHand hand)
        {
			if (hand.OtherHand.CurrentInteractable is DP12_Mag mag)
			{
				int num = 0;
				if (mag.RoundType == self.RoundType)
				{
					num = mag.m_capacity - mag.m_numRounds;
					num += mag.SecondMagazine.m_capacity - mag.SecondMagazine.m_numRounds;
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
				if (hand.OtherHand.CurrentInteractable is DP12_Mag mag)
				{
					if (mag.RoundType == self.RoundType)
					{
						num = mag.m_capacity - mag.m_numRounds;
						num += mag.SecondMagazine.m_capacity - mag.SecondMagazine.m_numRounds;
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

        private void FVRFireArmMagazine_ReloadMagWithType(On.FistVR.FVRFireArmMagazine.orig_ReloadMagWithType orig, FVRFireArmMagazine self, FireArmRoundClass rClass)
		{
			if (self == this)
			{
				m_numRounds = 0;
				for (int i = 0; i < this.m_capacity; i++)
				{
					AddRound(rClass, false, false);
				}
				SecondMagazine.ReloadMagWithType(rClass);
				UpdateBulletDisplay();
			}
			else orig(self, rClass);
		}

		public override void UpdateInteraction(FVRViveHand hand)
		{
			base.UpdateInteraction(hand);

			if (SecondMagazine.CanManuallyEjectRounds && SecondMagazine.RoundEjectionPos != null && SecondMagazine.HasARound())
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
					if (hand.OtherHand.CurrentInteractable == null && hand.OtherHand.Input.IsGrabbing && Vector3.Distance(SecondMagazine.RoundEjectionPos.position, hand.OtherHand.Input.Pos) < 0.15f)
					{
						GameObject original = SecondMagazine.RemoveRound(false);
						GameObject gameObject2 = Instantiate(original, SecondMagazine.RoundEjectionPos.position, SecondMagazine.RoundEjectionPos.rotation);
						FVRFireArmRound component2 = gameObject2.GetComponent<FVRFireArmRound>();
						component2.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						hand.OtherHand.ForceSetInteractable(component2);
						component2.BeginInteraction(hand.OtherHand);
					}
					else if (hand.OtherHand.CurrentInteractable is FVRFireArmRound && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).RoundType == SecondMagazine.RoundType && ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).ProxyRounds.Count < ((FVRFireArmRound)hand.OtherHand.CurrentInteractable).MaxPalmedAmount && Vector3.Distance(hand.Input.Pos, hand.OtherHand.Input.Pos) < 0.15f)
					{
						FireArmRoundClass lr_Class = SecondMagazine.LoadedRounds[SecondMagazine.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper = SecondMagazine.LoadedRounds[SecondMagazine.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).AddProxy(lr_Class, lr_ObjectWrapper);
						((FVRFireArmRound)hand.OtherHand.CurrentInteractable).UpdateProxyDisplay();
						SecondMagazine.RemoveRound();
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject == null)
					{
						GameObject original2 = SecondMagazine.RemoveRound(false);
						GameObject gameObject3 = Instantiate(original2, SecondMagazine.RoundEjectionPos.position, SecondMagazine.RoundEjectionPos.rotation);
						FVRFireArmRound component3 = gameObject3.GetComponent<FVRFireArmRound>();
						component3.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						component3.SetQuickBeltSlot(hand.CurrentHoveredQuickbeltSlotDirty);
					}
					else if (hand.CurrentHoveredQuickbeltSlotDirty != null && hand.CurrentHoveredQuickbeltSlotDirty.HeldObject is FVRFireArmRound && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).RoundType == SecondMagazine.RoundType && ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).ProxyRounds.Count < ((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).MaxPalmedAmount)
					{
						FireArmRoundClass lr_Class2 = SecondMagazine.LoadedRounds[SecondMagazine.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper2 = SecondMagazine.LoadedRounds[SecondMagazine.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).AddProxy(lr_Class2, lr_ObjectWrapper2);
						((FVRFireArmRound)hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).UpdateProxyDisplay();
						SecondMagazine.RemoveRound();
					}
					else
					{
						GameObject original3 = SecondMagazine.RemoveRound(false);
						GameObject gameObject4 = Instantiate(original3, SecondMagazine.RoundEjectionPos.position, SecondMagazine.RoundEjectionPos.rotation);
						gameObject4.GetComponent<FVRFireArmRound>().SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						gameObject4.GetComponent<Rigidbody>().AddForce(gameObject4.transform.forward * 0.5f);
						if (SecondMagazine.DisplayBullets.Length > 0 && SecondMagazine.DisplayBullets[0] != null)
						{
							gameObject4.GetComponent<FVRFireArmRound>().BeginAnimationFrom(SecondMagazine.DisplayBullets[0].transform.position, SecondMagazine.DisplayBullets[0].transform.rotation);
						}
					}
				}
			}
		}

		public override GameObject DuplicateFromSpawnLock(FVRViveHand hand)
		{
			GameObject copyGameObject = base.DuplicateFromSpawnLock(hand);
			DP12_Mag copyDP12_Mag = copyGameObject.GetComponent<DP12_Mag>();

			FVRFireArmMagazine copySecondMagazine = copyDP12_Mag.SecondMagazine;
			for (int i = 0; i < Mathf.Min(SecondMagazine.LoadedRounds.Length, copySecondMagazine.LoadedRounds.Length); i++)
			{
				if (SecondMagazine.LoadedRounds[i] != null && SecondMagazine.LoadedRounds[i].LR_Mesh != null)
				{
					copySecondMagazine.LoadedRounds[i].LR_Class = SecondMagazine.LoadedRounds[i].LR_Class;
					copySecondMagazine.LoadedRounds[i].LR_Mesh = SecondMagazine.LoadedRounds[i].LR_Mesh;
					copySecondMagazine.LoadedRounds[i].LR_Material = SecondMagazine.LoadedRounds[i].LR_Material;
					copySecondMagazine.LoadedRounds[i].LR_ObjectWrapper = SecondMagazine.LoadedRounds[i].LR_ObjectWrapper;
				}
			}
			copySecondMagazine.m_numRounds = SecondMagazine.m_numRounds;
			copySecondMagazine.UpdateBulletDisplay();

			return copyGameObject;
		}
#endif
		[Tooltip("Use this if you're working with a FVRFireArmMagazine prefab and don't feel like repopulating all the field of this script manually. Use the context menu after placing the FVRFireArmMagazine here.")]
		public  FVRFireArmMagazine CopyFVRFireArmMagazine;
		[ContextMenu("Copy existing Magazine Parameters")]
		public void CopyFVRFireArmMagazineParameters()
		{
			System.Type type = CopyFVRFireArmMagazine.GetType();
			Component copy = this;
			System.Reflection.FieldInfo[] fields = type.GetFields();
			foreach (System.Reflection.FieldInfo field in fields)
			{
				field.SetValue(copy, field.GetValue(CopyFVRFireArmMagazine));
			}
		}
	}
}
