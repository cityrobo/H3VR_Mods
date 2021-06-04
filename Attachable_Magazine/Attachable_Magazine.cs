using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

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


		public void Awake()
        {
			if (mag.transform.parent == attachment.transform)
			{
				SetBasePos();
				UseSecondaryParenting();
				SetSecondaryMagPos();
				UseBaseParenting();
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

			Debug.Log("Attachable_Magazine waiting for mag and attachment to completly awake!");
			StartCoroutine("Wait");
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
			else UseNormalTransform();
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

		private void UseBaseParenting()
        {
			attachment.transform.SetParent(null);
			mag.transform.SetParent(attachment.transform);
		}

		private void UseSecondaryParenting()
        {
			mag.transform.SetParent(null);
			attachment.SetParentage(mag.transform);
		}

		IEnumerator Wait()
        {
			while (!mag_Ready || !attachment_Ready) yield return null;

			Debug.Log("Attachable_Magazine awoken!");
			mag.StoreAndDestroyRigidbody();
            if (attachment.transform.parent == mag.transform)
            {
				attachment.StoreAndDestroyRigidbody();
            }
		}
	}
}
