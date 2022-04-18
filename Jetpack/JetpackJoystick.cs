using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class JetpackJoystick : FVRInteractiveObject
    {
        public Jetpack jetpack;
		public Transform Joystick;
		public Transform JoystickBase;
#if !(UNITY_EDITOR || UNITY_5)
		public override void UpdateInteraction(FVRViveHand hand)
		{
			base.UpdateInteraction(hand);
			Vector3 vector = Vector3.Slerp(hand.Input.FilteredForward, hand.Input.FilteredUp, 0.5f);
			Vector3 vector2 = Vector3.Slerp(hand.Input.FilteredForward, -hand.Input.FilteredUp, 0.5f);
			Vector3 rhs = Vector3.ProjectOnPlane(vector, this.JoystickBase.right);
			Vector3 rhs2 = Vector3.ProjectOnPlane(vector, this.JoystickBase.forward);
			Vector3 rhs3 = Vector3.ProjectOnPlane(vector2, this.JoystickBase.up);
			float num = Mathf.Atan2(Vector3.Dot(this.JoystickBase.right, Vector3.Cross(this.JoystickBase.up, rhs)), Vector3.Dot(this.JoystickBase.up, rhs)) * 57.29578f;
			float num2 = Mathf.Atan2(Vector3.Dot(this.JoystickBase.forward, Vector3.Cross(this.JoystickBase.up, rhs2)), Vector3.Dot(this.JoystickBase.up, rhs2)) * 57.29578f;
			float num3 = Mathf.Atan2(Vector3.Dot(this.JoystickBase.up, Vector3.Cross(this.JoystickBase.forward, rhs3)), Vector3.Dot(this.JoystickBase.forward, rhs3)) * 57.29578f;
			num = Mathf.Clamp(num, -30f, 30f) / 30f;
			num2 = Mathf.Clamp(num2, -30f, 30f) / 30f;
			num3 = Mathf.Clamp(num3, -30f, 30f) / 30f;
			if (Mathf.Abs(num) <= 0.2f)
			{
				num = 0f;
			}
			if (Mathf.Abs(num2) <= 0.2f)
			{
				num2 = 0f;
			}
			if (Mathf.Abs(num3) <= 0.2f)
			{
				num3 = 0f;
			}
			num = num * num * Mathf.Sign(num);
			num2 = num2 * num2 * Mathf.Sign(num2);
			num3 = num3 * num3 * Mathf.Sign(num3);
			this.Joystick.localEulerAngles = new Vector3(num * 30f, num3 * 30f, num2 * 30f);

			jetpack.Boost(hand.Input.TriggerFloat);

			if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45f)
			{
				jetpack.IsHoveringMode = !jetpack.IsHoveringMode;
            }
		}

		public override void EndInteraction(FVRViveHand hand)
		{
			base.EndInteraction(hand);
			this.Joystick.localEulerAngles = new Vector3(0f, 0f, 0f);
		}
#endif
	}
}
