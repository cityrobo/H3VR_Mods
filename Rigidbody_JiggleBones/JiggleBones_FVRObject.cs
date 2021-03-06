using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace Rigidbody_JiggleBones
{
	public class JiggleBones_FVRObject : MonoBehaviour
	{
		public FVRPhysicalObject fvrPhysicalObject;
		public Rigidbody rootObject;
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
			Transform currentParent = rootBone;
			FixedJoint fJoint = rootBone.gameObject.AddComponent<FixedJoint>();
			fJoint.connectedBody = rootObject;
			/*
			if (fvrPhysicalObject != null)
			{
				Rigidbody[] DependantRBs = fvrPhysicalObject.DependantRBs;
				int DependantRBsCount = fvrPhysicalObject.DependantRBs.Length;

				Array.Resize(ref DependantRBs, DependantRBsCount + 1);
				DependantRBs[DependantRBsCount] = rootBone.GetComponent<Rigidbody>();
				fvrPhysicalObject.DependantRBs = DependantRBs;
			}
			*/
			while (currentParent != null)
			{

				if (currentParent.childCount == 0)
				{
					currentParent = null;
					break;
				}

				Transform child = currentParent.GetChild(0);
				Rigidbody body = child.gameObject.AddComponent<Rigidbody>();
				body.useGravity = false;
				//body = StaticExtras.GetCopyOf(body, referenceRigidbody);

				/*
				if (fvrPhysicalObject != null)
				{
					Rigidbody[] DependantRBs = fvrPhysicalObject.DependantRBs;
					int DependantRBsCount = fvrPhysicalObject.DependantRBs.Length;

					Array.Resize(ref DependantRBs, DependantRBsCount + 1);
					DependantRBs[DependantRBsCount] = body;
					fvrPhysicalObject.DependantRBs = DependantRBs;
				}
				*/
				ConfigurableJoint joint = child.gameObject.AddComponent<ConfigurableJoint>();
				configureJoint(joint, currentParent.gameObject.GetComponent<Rigidbody>());

				CapsuleCollider collider = child.gameObject.AddComponent<CapsuleCollider>();
				collider.direction = 1;
				collider.radius = collider_Radius;
				collider.height = collider_Height;

				currentParent = child;
			}

			var allChildren = rootBone.GetComponentsInChildren<Transform>();
			foreach (Transform child in allChildren)
			{
				if (child != rootBone.transform)
					child.SetParent(null);


			}

			Destroy(referenceRigidbody);
		}

		public void configureJoint(ConfigurableJoint joint, Rigidbody parent)
		{
			joint.connectedBody = parent;
			joint.axis = new Vector3(0, 1, 0);

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
	}
}