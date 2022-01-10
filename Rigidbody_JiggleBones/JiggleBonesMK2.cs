using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace Rigidbody_JiggleBones
{
	public class JiggleBonesMK2 : MonoBehaviour
	{

		public FVRPhysicalObject fvrPhysicalObject;
		public Rigidbody parentRigidbody;
		public Transform rootBone;
		public Rigidbody referenceRigidbody;

		public float spring_strength;
		public float spring_dampening;

		public float twistLimit;
		public float angleLimit;

		public float collider_Radius;
		public float collider_Height;

		public void Start()
		{			
            if (!CreateJointsOnChildren(rootBone, parentRigidbody)) Debug.LogError("No Children for JiggleBones found!");
        }

		public void configureJoint(ConfigurableJoint joint, Rigidbody parent)
		{
			joint.connectedBody = parent;
			joint.axis = new Vector3(0, 1, 0);

			joint.autoConfigureConnectedAnchor = false;

			joint.connectedAnchor = new Vector3(0,0,0);

			joint.anchor = joint.transform.InverseTransformPoint(parent.transform.position);

			joint.xMotion = ConfigurableJointMotion.Locked;
			joint.yMotion = ConfigurableJointMotion.Locked;
			joint.zMotion = ConfigurableJointMotion.Locked;

			joint.angularXMotion = ConfigurableJointMotion.Limited;
			joint.angularYMotion = ConfigurableJointMotion.Limited;
			joint.angularZMotion = ConfigurableJointMotion.Limited;

			JointDrive drive = new JointDrive();
			drive.positionSpring = spring_strength;
			drive.positionDamper = spring_dampening;
			drive.maximumForce = joint.angularYZDrive.maximumForce;

			joint.angularXDrive = drive;
			joint.angularYZDrive = drive;

			SoftJointLimit high_twistlimit = new SoftJointLimit();
			high_twistlimit.limit = twistLimit;
			high_twistlimit.bounciness = joint.highAngularXLimit.bounciness;
			high_twistlimit.contactDistance = joint.highAngularXLimit.contactDistance;
			joint.highAngularXLimit = high_twistlimit;

			SoftJointLimit low_twistlimit = new SoftJointLimit();
			low_twistlimit.limit = twistLimit;
			low_twistlimit.bounciness = joint.lowAngularXLimit.bounciness;
			low_twistlimit.contactDistance = joint.lowAngularXLimit.contactDistance;
			joint.lowAngularXLimit = low_twistlimit;

			SoftJointLimit anglelimit = new SoftJointLimit();
			anglelimit.limit = angleLimit;
			anglelimit.bounciness = joint.angularYLimit.bounciness;
			anglelimit.contactDistance = joint.angularYLimit.contactDistance;
			joint.angularYLimit = anglelimit;
			joint.angularZLimit = anglelimit;
		}

		/*public void SetJiggleData(JiggleBoneData data)
        {
			this.fvrPhysicalObject = data.fvrPhysicalObject;
			this.parentRigidbody = data.parentRigidbody;
			this.parent = data.parent;
			this.referenceRigidbody = data.referenceRigidbody;

			this.spring_strength = data.spring_strength;
			this.spring_dampening = data.spring_dampening;

			this.twistLimit	= data.twistLimit;
			this.angleLimit	= data.angleLimit;

			this.collider_Radius = data.collider_Radius;
			this.collider_Height = data.collider_Height;
		}
		
		private void ConvertData()
		{
			fvrPhysicalObject = this.fvrPhysicalObject;
			parentRigidbody = this.parentRigidbody;
			parent = this.parent;
			referenceRigidbody = this.referenceRigidbody;


			spring_strength = this.spring_strength;
			spring_dampening = this.spring_dampening;

			twistLimit = this.twistLimit;
			angleLimit = this.angleLimit;

			collider_Radius = this.collider_Radius;
			collider_Height = this.collider_Height;
		}
		*/

		private bool CreateJointsOnChildren(Transform parent, Rigidbody connectedRB)
        {	
			Transform[] immediateChildren = parent.GetComponentsInDirectChildren<Transform>();

			if (immediateChildren.Length == 0)
			{
				return false;
			}

			foreach (var child in immediateChildren)
			{
				Rigidbody RB = child.gameObject.AddComponent<Rigidbody>();
				//RB = StaticExtras.GetCopyOf(RB, referenceRigidbody);
				//Rigidbody RB = StaticExtras.CopyComponent(referenceRigidbody, child.gameObject);

				ConfigurableJoint joint = child.gameObject.AddComponent<ConfigurableJoint>();

				configureJoint(joint, connectedRB);

				CapsuleCollider collider = child.gameObject.AddComponent<CapsuleCollider>();
				collider.direction = 1;
				collider.radius = collider_Radius;
				collider.height = collider_Height;

				if (fvrPhysicalObject != null)
				{
					Rigidbody[] DependantRBs = fvrPhysicalObject.DependantRBs;
					int DependantRBsCount = fvrPhysicalObject.DependantRBs.Length;

					Array.Resize(ref DependantRBs, DependantRBsCount + 1);
					DependantRBs[DependantRBsCount] = RB;
					fvrPhysicalObject.DependantRBs = DependantRBs;
				}

				CreateJointsOnChildren(child,RB);

				child.SetParent(null);
			}
			return true;
		}
	}
}
