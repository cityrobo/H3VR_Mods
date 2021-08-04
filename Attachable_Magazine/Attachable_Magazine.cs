using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
	public class Attachable_Magazine : MonoBehaviour
	{
		public FistVR.FVRFireArmMagazine mag;

		public FistVR.FVRFireArmAttachment attachment;

		private bool magAttached = false;

		private Vector3 base_magPos;
		private Vector3 base_magEuler;

		private Vector3 secondary_magPos;
		private Vector3 secondary_magEuler;

		[HideInInspector] public bool mag_Ready = false;
		[HideInInspector] public bool attachment_Ready = false;

		public void Start()
        {	
			if (mag.transform.parent == attachment.transform)
			{
				Transform attachmentParent = attachment.transform.parent;
				SetBasePos();
				UseSecondaryParenting();
				SetSecondaryMagPos();
				UseBaseParenting(attachmentParent);
				UseNormalTransform();
			}
			else if (attachment.transform.parent == mag.transform)
			{
				Transform magParent;
				magParent = mag.transform.parent;
				SetSecondaryMagPos();
				UseBaseParenting();
				SetBasePos();
				UseSecondaryParenting();
				UseSecondaryTransform();
				mag.SetParentage(magParent);
			}

			//Debug.Log("Attachable_Magazine waiting for mag and attachment to completly awake!");
			//StartCoroutine("Wait");
			mag.StoreAndDestroyRigidbody();

			Collider collider = mag.GetComponent<Collider>();

			if (collider != null) Destroy(collider);
			if (attachment.transform.parent == mag.transform)
			{
				attachment.StoreAndDestroyRigidbody();
			}
		}
		
        public void Update()
        {
			if (mag.State == FistVR.FVRFireArmMagazine.MagazineState.Locked && !magAttached)
            {
				magAttached = true;

				attachment.ForceBreakInteraction();
				attachment.IsHeld = false;
				attachment.StoreAndDestroyRigidbody();
				attachment.SetParentage(mag.transform);
				List<Collider> collider_list = new List<Collider>();
				collider_list.Add(attachment.GetComponent<Collider>());
				attachment.SetCollidersToLayer(collider_list,true,"Default");
            }
			else if (mag.State == FistVR.FVRFireArmMagazine.MagazineState.Free && magAttached)
            {
				magAttached = false;

				attachment.SetParentage(null);
				attachment.RecoverRigidbody();
				FistVR.FVRViveHand hand = null;
				hand = mag.m_hand;
				mag.ForceBreakInteraction();
				mag.SetParentage(attachment.transform);
				mag.StoreAndDestroyRigidbody();
				if (hand != null)
				{
					hand.ForceSetInteractable(attachment);
					attachment.BeginInteraction(hand);
				}
				List<Collider> collider_list = new List<Collider>();
				collider_list.Add(attachment.GetComponent<Collider>());
				attachment.SetCollidersToLayer(collider_list, true, "Interactable");
            }
			if (magAttached) UseSecondaryTransform();
			else
			{
				UseNormalTransform();

				if (attachment.m_hand != null) MagEjectRound();
			}
			
		}

		private void SetBasePos()
        {
			base_magPos = mag.transform.localPosition;
			base_magEuler = mag.transform.localEulerAngles;
		}
		private void SetSecondaryMagPos()
        {
			secondary_magPos = attachment.transform.localPosition;
			secondary_magEuler = attachment.transform.localEulerAngles;
        }

		private void UseNormalTransform()
        {
			mag.transform.localPosition = base_magPos;
			mag.transform.localEulerAngles = base_magEuler;
		}

		private void UseSecondaryTransform()
        {
			attachment.transform.localPosition = secondary_magPos;
			attachment.transform.localEulerAngles = secondary_magEuler;
		}

		private void UseBaseParenting(Transform parent = null)
        {
			attachment.transform.SetParent(parent);
			mag.transform.SetParent(attachment.transform);
		}

		private void UseSecondaryParenting()
        {
			mag.transform.SetParent(null);
			attachment.SetParentage(mag.transform);
		}

		public void MagEjectRound()
        {
			if (mag.CanManuallyEjectRounds && mag.RoundEjectionPos != null && mag.HasARound())
			{
				bool flag = false;
				if (attachment.m_hand.IsInStreamlinedMode && attachment.m_hand.Input.BYButtonDown)
				{
					flag = true;
				}
				else if (!attachment.m_hand.IsInStreamlinedMode && attachment.m_hand.Input.TouchpadDown && Vector2.Angle(attachment.m_hand.Input.TouchpadAxes, Vector2.up) < 45f)
				{
					flag = true;
				}
				if (flag)
				{
					if (mag.FireArm != null)
					{
						mag.FireArm.PlayAudioEvent(FirearmAudioEventType.MagazineEjectRound, 1f);
					}
					else
					{
						SM.PlayGenericSound(mag.Profile.MagazineEjectRound, this.transform.position);
					}
					if (attachment.m_hand.OtherHand.CurrentInteractable == null && attachment.m_hand.OtherHand.Input.IsGrabbing && Vector3.Distance(mag.RoundEjectionPos.position, attachment.m_hand.OtherHand.Input.Pos) < 0.15f)
					{
						GameObject original = mag.RemoveRound(false);
						GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original, mag.RoundEjectionPos.position, mag.RoundEjectionPos.rotation);
						FVRFireArmRound component = gameObject.GetComponent<FVRFireArmRound>();
						component.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						attachment.m_hand.OtherHand.ForceSetInteractable(component);
						component.BeginInteraction(attachment.m_hand.OtherHand);
					}
					else if (attachment.m_hand.OtherHand.CurrentInteractable is FVRFireArmRound && ((FVRFireArmRound)attachment.m_hand.OtherHand.CurrentInteractable).RoundType == mag.RoundType && ((FVRFireArmRound)attachment.m_hand.OtherHand.CurrentInteractable).ProxyRounds.Count < ((FVRFireArmRound)attachment.m_hand.OtherHand.CurrentInteractable).MaxPalmedAmount && Vector3.Distance(attachment.m_hand.Input.Pos, attachment.m_hand.OtherHand.Input.Pos) < 0.15f)
					{
						FireArmRoundClass lr_Class = mag.LoadedRounds[mag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper = mag.LoadedRounds[mag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)attachment.m_hand.OtherHand.CurrentInteractable).AddProxy(lr_Class, lr_ObjectWrapper);
						((FVRFireArmRound)attachment.m_hand.OtherHand.CurrentInteractable).UpdateProxyDisplay();
						mag.RemoveRound();
					}
					else if (attachment.m_hand.CurrentHoveredQuickbeltSlotDirty != null && attachment.m_hand.CurrentHoveredQuickbeltSlotDirty.HeldObject == null)
					{
						GameObject original2 = mag.RemoveRound(false);
						GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(original2, mag.RoundEjectionPos.position, mag.RoundEjectionPos.rotation);
						FVRFireArmRound component2 = gameObject2.GetComponent<FVRFireArmRound>();
						component2.SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						component2.SetQuickBeltSlot(attachment.m_hand.CurrentHoveredQuickbeltSlotDirty);
					}
					else if (attachment.m_hand.CurrentHoveredQuickbeltSlotDirty != null && attachment.m_hand.CurrentHoveredQuickbeltSlotDirty.HeldObject is FVRFireArmRound && ((FVRFireArmRound)attachment.m_hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).RoundType == mag.RoundType && ((FVRFireArmRound)attachment.m_hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).ProxyRounds.Count < ((FVRFireArmRound)attachment.m_hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).MaxPalmedAmount)
					{
						FireArmRoundClass lr_Class2 = mag.LoadedRounds[mag.m_numRounds - 1].LR_Class;
						FVRObject lr_ObjectWrapper2 = mag.LoadedRounds[mag.m_numRounds - 1].LR_ObjectWrapper;
						((FVRFireArmRound)attachment.m_hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).AddProxy(lr_Class2, lr_ObjectWrapper2);
						((FVRFireArmRound)attachment.m_hand.CurrentHoveredQuickbeltSlotDirty.HeldObject).UpdateProxyDisplay();
						mag.RemoveRound();
					}
					else
					{
						GameObject original3 = mag.RemoveRound(false);
						GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(original3, mag.RoundEjectionPos.position, mag.RoundEjectionPos.rotation);
						gameObject3.GetComponent<FVRFireArmRound>().SetIFF(GM.CurrentPlayerBody.GetPlayerIFF());
						gameObject3.GetComponent<Rigidbody>().AddForce(gameObject3.transform.forward * 0.5f);
					}
				}
			}
		}
		IEnumerator Wait()
        {
			while (!mag_Ready || !attachment_Ready) yield return null;

			//Debug.Log("Attachable_Magazine awoken!");
			mag.StoreAndDestroyRigidbody();

			Collider collider = mag.GetComponent<Collider>();

			if (collider != null) Destroy(collider);
			if (attachment.transform.parent == mag.transform) 
            {
				attachment.StoreAndDestroyRigidbody();
            }
		}
	}
}
