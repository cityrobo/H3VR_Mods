using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FistVR
{
	public class CustomBipodInterface : FVRFireArmAttachmentInterface
	{
		public override void SimpleInteraction(FVRViveHand hand)
		{
			this.Bipod.Toggle();
			ToggleRotation();
			base.SimpleInteraction(hand);
		}

		public override void UpdateInteraction(FVRViveHand hand)
		{
			if (hand.IsInStreamlinedMode)
			{
				if (hand.Input.BYButtonDown)
				{
					this.Bipod.NextML();
				}
			}
			else if (hand.Input.TouchpadDown)
			{
				Vector2 touchpadAxes = hand.Input.TouchpadAxes;
				if (touchpadAxes.magnitude > 0.3f)
				{
					if (Vector2.Angle(touchpadAxes, Vector2.left) < 45f)
					{
						this.Bipod.PrevML();
					}
					else if (Vector2.Angle(touchpadAxes, Vector2.right) < 45f)
					{
						this.Bipod.NextML();
					}
				}
			}
			base.UpdateInteraction(hand);
		}

		public override void BeginInteraction(FVRViveHand hand)
		{
			base.BeginInteraction(hand);
			this.Bipod.Toggle();
		}

		public override void OnAttach()
		{
			base.OnAttach();
			this.Attachment.curMount.GetRootMount().Parent.Bipod = this.Bipod;
			this.Bipod.FireArm = this.Attachment.curMount.GetRootMount().Parent;
		}

		public override void OnDetach()
		{
			if (this.Bipod.isActiveAndEnabled)
			{
				this.Bipod.Contract(true);
			}
			this.Attachment.curMount.GetRootMount().Parent.Bipod = null;
			this.Bipod.FireArm = null;
			base.OnDetach();
		}

		[ContextMenu("Config")]
		public void Config()
		{
			this.Bipod = base.transform.GetComponent<FVRFireArmBipod>();
		}


		private void ToggleRotation()
        {
            if (!_is_open)
            {
				StartCoroutine("OpenObject");
				_is_open = true;
            }
			else if (_is_open)
			{
				StartCoroutine("CloseObject");
				_is_open = false;
			}
		}

		private void RotateObject(float f)
		{
			this.Object_to_rotate.localEulerAngles = new Vector3(Mathf.Lerp(this.RotationRange.x, this.RotationRange.y, f), 0f, 0f);
		}

		private IEnumerator OpenObject()
		{
			timeElapsed = 0f;
			while (timeElapsed < rotation_duration)
			{
				timeElapsed += Time.deltaTime;
				RotateObject(timeElapsed / rotation_duration);
				yield return null;
			}
			RotateObject(1f);
		}

		private IEnumerator CloseObject()
		{
			timeElapsed = 0f;
			while (timeElapsed < rotation_duration)
			{
				timeElapsed += Time.deltaTime;
				RotateObject(1f - (timeElapsed / rotation_duration));
				yield return null;
			}
			RotateObject(0f);
		}

		public FVRFireArmBipod Bipod;
		public Transform Object_to_rotate;
		public float rotation_duration;
		public Vector2 RotationRange = new Vector2(0f, 90f);
		private bool _is_open = false;
		private float timeElapsed;


		public static FistVR.CustomBipodInterface CopyFromInterface(FVRFireArmAttachmentInterface original, GameObject target)
		{
			target.gameObject.SetActive(false);
			var real = target.AddComponent<CustomBipodInterface>();
			real.ControlType = original.ControlType;
			real.IsSimpleInteract = original.IsSimpleInteract;
			real.HandlingGrabSound = original.HandlingGrabSound;
			real.HandlingReleaseSound = original.HandlingReleaseSound;
			real.PoseOverride = original.PoseOverride;
			real.QBPoseOverride = original.QBPoseOverride;
			real.PoseOverride_Touch = original.PoseOverride_Touch;
			real.UseGrabPointChild = original.UseGrabPointChild;
			real.UseGripRotInterp = original.UseGripRotInterp;
			real.PositionInterpSpeed = original.PositionInterpSpeed;
			real.RotationInterpSpeed = original.RotationInterpSpeed;
			real.EndInteractionIfDistant = original.EndInteractionIfDistant;
			real.EndInteractionDistance = original.EndInteractionDistance;
			real.UXGeo_Held = original.UXGeo_Held;
			real.UXGeo_Hover = original.UXGeo_Hover;
			real.UseFilteredHandTransform = original.UseFilteredHandTransform;
			real.UseFilteredHandRotation = original.UseFilteredHandRotation;
			real.UseFilteredHandPosition = original.UseFilteredHandPosition;
			real.UseSecondStepRotationFiltering = original.UseSecondStepRotationFiltering;
			real.Attachment = original.Attachment;
			real.IsLocked = original.IsLocked;
			real.ForceInteractable = original.ForceInteractable;
			real.SubMounts = original.SubMounts;
			Destroy(original);
			return real;
		}
	}
}
