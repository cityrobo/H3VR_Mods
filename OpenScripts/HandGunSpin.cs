using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using FistVR;

namespace Cityrobo
{
    public class HandGunSpin : MonoBehaviour
    {
        public Handgun handgun;
		public Transform PoseSpinHolder;

		private float xSpinVel;
		private float xSpinRot;

		private bool m_isSpinning;

#if !(DEBUG || MEATKIT)

		public void Awake()
        {
			Hook();

		}
		public void OnDestroy()
        {
			Unhook();
        }

		void Unhook()
        {
			On.FistVR.Handgun.UpdateInputAndAnimate -= Handgun_UpdateInputAndAnimate;
			On.FistVR.FVRFireArm.FVRFixedUpdate -= FVRFireArm_FVRFixedUpdate;
			On.FistVR.FVRFireArm.EndInteraction -= FVRFireArm_EndInteraction;
			On.FistVR.FVRFireArm.EndInteractionIntoInventorySlot -= FVRFireArm_EndInteractionIntoInventorySlot;
		}

		void Hook()
        {
            On.FistVR.Handgun.UpdateInputAndAnimate += Handgun_UpdateInputAndAnimate;
            On.FistVR.FVRFireArm.FVRFixedUpdate += FVRFireArm_FVRFixedUpdate;
            On.FistVR.FVRFireArm.EndInteraction += FVRFireArm_EndInteraction;
            On.FistVR.FVRFireArm.EndInteractionIntoInventorySlot += FVRFireArm_EndInteractionIntoInventorySlot;
        }

        private void FVRFireArm_EndInteractionIntoInventorySlot(On.FistVR.FVRFireArm.orig_EndInteractionIntoInventorySlot orig, FVRFireArm self, FVRViveHand hand, FVRQuickBeltSlot slot)
        {
			if (self == handgun)
			{
				m_isSpinning = false;
			}
			orig(self, hand, slot);
		}

        private void FVRFireArm_EndInteraction(On.FistVR.FVRFireArm.orig_EndInteraction orig, FVRFireArm self, FVRViveHand hand)
        {
			if (self == handgun)
			{
				m_isSpinning = false;
			}
			orig(self,hand);
		}

        private void FVRFireArm_FVRFixedUpdate(On.FistVR.FVRFireArm.orig_FVRFixedUpdate orig, FVRFireArm self)
        {
			orig(self);

			if (self == handgun)
			{
				UpdateSpinning();
			}
		}

        private void Handgun_UpdateInputAndAnimate(On.FistVR.Handgun.orig_UpdateInputAndAnimate orig, Handgun self, FVRViveHand hand)
        {
			orig(self,hand);

			if (self == handgun)
			{
				if (hand.Input.TouchpadPressed && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) m_isSpinning = true;
				else m_isSpinning = false;
			}
        }

        private void UpdateSpinning()
		{
			if (!handgun.IsHeld || handgun.IsAltHeld || handgun.AltGrip != null)
			{
				m_isSpinning = false;
			}
			if (m_isSpinning)
			{
				Vector3 vector = Vector3.zero;
				if (handgun.m_hand != null)
				{
					vector = handgun.m_hand.Input.VelLinearWorld;
				}
				float num = Vector3.Dot(vector.normalized, handgun.transform.up);
				num = Mathf.Clamp(num, -vector.magnitude, vector.magnitude);
				if (Mathf.Abs(xSpinVel) < 90f)
				{
					this.xSpinVel += num * Time.deltaTime * 600f;
				}
				else if (Mathf.Sign(num) == Mathf.Sign(xSpinVel))
				{
					this.xSpinVel += num * Time.deltaTime * 600f;
				}
				if (Mathf.Abs(xSpinVel) < 90f)
				{
					if (Vector3.Dot(handgun.transform.up, Vector3.down) >= 0f && Mathf.Sign(xSpinVel) == 1f)
					{
						xSpinVel += Time.deltaTime * 50f;
					}
					if (Vector3.Dot(handgun.transform.up, Vector3.down) < 0f && Mathf.Sign(xSpinVel) == -1f)
					{
						xSpinVel -= Time.deltaTime * 50f;
					}
				}
				xSpinVel = Mathf.Clamp(xSpinVel, -500f, 500f);
				xSpinRot += xSpinVel * Time.deltaTime * 5f;
				PoseSpinHolder.localEulerAngles = new Vector3(this.xSpinRot, 0f, 0f);
				xSpinVel = Mathf.Lerp(xSpinVel, 0f, Time.deltaTime * 0.6f);
			}
			else
			{
				xSpinRot = 0f;
				xSpinVel = 0f;
			}
		}
#endif
	}
}
