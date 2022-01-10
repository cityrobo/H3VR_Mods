using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
	public class Attachable_Magazine_MK2 : MonoBehaviour
	{
		public FVRFireArmMagazine mag;
		
		public FVRFireArmAttachment attachment;

		public bool attachInstantly;

		private bool attachmentLocked = false;
		private FVRFireArmReloadTriggerMag reloadTriggerMag;


		private Vector3 base_attachmentPos;
		private Vector3 base_attachmentEuler;

		private Vector3 secondary_attachmentPos;
		private Vector3 secondary_attachmentEuler;
#if !DEBUG

		public void Start()
        {
			Hook();

			if (attachment.transform.parent == mag.transform)
			{
				Transform magParent = mag.transform.parent;
				SetBaseTransform();
				UseSecondaryParenting();
				SetSecondaryTransform();
				UseBaseParenting(magParent);
				UseBaseTransform();

                //Debug.Log("Mag based Setup complete!");

            }
			else if (mag.transform.parent == attachment.transform)
			{
				Transform attachmentParent = attachment.transform.parent;
				SetSecondaryTransform();
				UseBaseParenting();
				SetBaseTransform();
				UseSecondaryParenting(attachmentParent);
				UseSecondaryTransform();
				attachmentLocked = true;
				attachment.Sensor.CurHoveredMount = attachment.curMount;
				//Debug.Log("Attachment based Setup complete!");
			}
            else
            {
				Debug.LogError("Attachable Mag Setup failed!");
            }


			if (mag.transform.parent == attachment.transform)
			{
				mag.StoreAndDestroyRigidbody();
				mag.gameObject.layer = LayerMask.NameToLayer("NoCol");
			}
            else
            {
				attachment.StoreAndDestroyRigidbody();
				attachment.gameObject.layer = LayerMask.NameToLayer("NoCol");
			}

			reloadTriggerMag = mag.GetComponentInChildren<FVRFireArmReloadTriggerMag>();
		}
		
        public void Update()
        {
			if (attachment.Sensor.CurHoveredMount != null && !attachmentLocked)
            {
				//StartHoverMode();
				/*attachmentLocked = true;
				attachment.RecoverRigidbody();
				attachment.AttachmentInterface.SetAllCollidersToLayer(true, "Interactable");
				FVRViveHand hand = mag.m_hand;
				mag.ForceBreakInteraction();
				hand.ForceSetInteractable(attachment);
				attachment.BeginInteraction(hand);
				mag.StoreAndDestroyRigidbody();
				mag.SetParentage(attachment.transform);

				mag.gameObject.layer = LayerMask.NameToLayer("NoCol");*/
			}
			else if (attachment.Sensor.CurHoveredMount == null && attachmentLocked)
            {
				attachmentLocked = false;
				attachment.gameObject.layer = LayerMask.NameToLayer("NoCol");
				mag.SetParentage(null);
				mag.RecoverRigidbody();
				FVRViveHand hand = attachment.m_hand;
				attachment.ForceBreakInteraction();
				attachment.SetParentage(mag.transform);
				attachment.StoreAndDestroyRigidbody();
				if (hand != null)
				{
					hand.ForceSetInteractable(mag);
					mag.BeginInteraction(hand);
				}
				mag.gameObject.layer = LayerMask.NameToLayer("Interactable");
			}
			if (attachmentLocked) UseSecondaryTransform();
			else
			{
				UseBaseTransform();
			}

            if (mag.State == FVRFireArmMagazine.MagazineState.Locked)
            {
				attachment.Sensor.gameObject.layer = LayerMask.NameToLayer("NoCol");
			}
			else if (attachment.IsHovered)
            {
				reloadTriggerMag.gameObject.layer = LayerMask.NameToLayer("NoCol");
			}
            else
            {
				attachment.Sensor.gameObject.layer = LayerMask.NameToLayer("Interactable");
				reloadTriggerMag.gameObject.layer = LayerMask.NameToLayer("Interactable");
			}
		}

		private void SetBaseTransform()
        {
			base_attachmentPos = attachment.transform.localPosition;
			base_attachmentEuler = attachment.transform.localEulerAngles;
		}
		private void SetSecondaryTransform()
        {
			secondary_attachmentPos = mag.transform.localPosition;
			secondary_attachmentEuler = mag.transform.localEulerAngles;
        }

		private void UseBaseTransform()
        {
			attachment.transform.localPosition = base_attachmentPos;
			attachment.transform.localEulerAngles = base_attachmentEuler;
		}

		private void UseSecondaryTransform()
        {
			mag.transform.localPosition = secondary_attachmentPos;
			mag.transform.localEulerAngles = secondary_attachmentEuler;
		}

		private void UseBaseParenting(Transform magParent = null)
        {
			mag.SetParentage(magParent);
			attachment.SetParentage(mag.transform);
		}

		private void UseSecondaryParenting(Transform attachmentParent = null)
        {
			attachment.SetParentage(attachmentParent);
			mag.SetParentage(attachment.transform);
		}

		private void Hook()
        {

			On.FistVR.FVRFireArmAttachmentSensor.OnTriggerEnter += FVRFireArmAttachmentSensor_OnTriggerEnter;

		}

		private void FVRFireArmAttachmentSensor_OnTriggerEnter(On.FistVR.FVRFireArmAttachmentSensor.orig_OnTriggerEnter orig, FVRFireArmAttachmentSensor self, Collider collider)
        {
			if (self == attachment.Sensor)
            {
				if (self.CurHoveredMount == null && self.Attachment.CanAttach() && collider.gameObject.tag == "FVRFireArmAttachmentMount")
				{
					FVRFireArmAttachmentMount component = collider.gameObject.GetComponent<FVRFireArmAttachmentMount>();
					if (component.Type == self.Attachment.Type && component.isMountableOn(self.Attachment))
					{
						if (!attachInstantly)
						{
							if (!attachmentLocked) StartHoverMode();
							self.SetHoveredMount(component);
							component.BeginHover();
						}
                        else
                        {
							self.SetHoveredMount(component);
							if (!attachmentLocked) InstantlyAttachToMount(component);
						}
					}
				}
			}
			else 
			orig(self, collider);
        }


		private void StartHoverMode()
        {
			attachmentLocked = true;
			attachment.RecoverRigidbody();
			attachment.AttachmentInterface.gameObject.layer = LayerMask.NameToLayer("Interactable");
			FVRViveHand hand = mag.m_hand;
			mag.ForceBreakInteraction();
			hand.ForceSetInteractable(attachment);
			attachment.BeginInteraction(hand);
			mag.StoreAndDestroyRigidbody();
			mag.SetParentage(attachment.transform);
			mag.gameObject.layer = LayerMask.NameToLayer("NoCol");
		}

		private void InstantlyAttachToMount(FVRFireArmAttachmentMount mount)
        {
			attachmentLocked = true;
			mag.ForceBreakInteraction();
			mag.StoreAndDestroyRigidbody();
			attachment.RecoverRigidbody();
			attachment.AttachmentInterface.gameObject.layer = LayerMask.NameToLayer("Interactable");
			UseSecondaryParenting();
            attachment.AttachToMount(mount, true);
			mag.gameObject.layer = LayerMask.NameToLayer("NoCol");
		}
#endif
	}
}
