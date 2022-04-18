using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
	public class HingeForegrip : FVRAlternateGrip
	{
#if !(UNITY_EDITOR || UNITY_5)

		public override void Awake()
		{
			base.Awake();
			this.localPosStart = this.Hinge.transform.localPosition;
			this.RB = this.Hinge.gameObject.GetComponent<Rigidbody>();
			this.physObj = this.Hinge.connectedBody.gameObject.GetComponent<FVRPhysicalObject>();
		}

		public override void FVRUpdate()
		{
			base.FVRUpdate();
			if (Vector3.Distance(this.Hinge.transform.localPosition, this.localPosStart) > 0.01f)
			{
				this.Hinge.transform.localPosition = this.localPosStart;
			}
		}

		public override void FVRFixedUpdate()
		{
			base.FVRFixedUpdate();
			if (this.physObj.IsHeld && this.physObj.IsAltHeld)
			{
				this.RB.mass = 0.001f;
			}
			else
			{
				this.RB.mass = 0.1f;
			}
		}

		public override bool IsInteractable()
		{
			return true;
		}

		public override void UpdateInteraction(FVRViveHand hand)
		{
			base.UpdateInteraction(hand);
			Vector3 vector = hand.Input.Pos - this.Hinge.transform.position;
			Vector3 from = Vector3.ProjectOnPlane(vector, this.ObjectBase.right);
			if (Vector3.Angle(from, -this.ObjectBase.up) > 90f)
			{
				from = this.ObjectBase.forward;
			}
			if (Vector3.Angle(from, this.ObjectBase.forward) > 90f)
			{
				from = -this.ObjectBase.up;
			}
			float value = Vector3.Angle(from, this.ObjectBase.forward);
			JointSpring spring = this.Hinge.spring;
			spring.spring = 10f;
			spring.damper = 0f;
			spring.targetPosition = Mathf.Clamp(value, 0f, this.Hinge.limits.max);
			this.Hinge.spring = spring;
			this.Hinge.transform.localPosition = this.localPosStart;
		}

		public override void EndInteraction(FVRViveHand hand)
		{
			JointSpring spring = this.Hinge.spring;
			spring.spring = 0.5f;
			spring.damper = 0.05f;
			spring.targetPosition = 45f;
			this.Hinge.spring = spring;
			base.EndInteraction(hand);
		}
#endif
		public Transform ObjectBase;
		public HingeJoint Hinge;

		private Vector3 localPosStart;
		private Rigidbody RB;
		private FVRPhysicalObject physObj;
	}
}
