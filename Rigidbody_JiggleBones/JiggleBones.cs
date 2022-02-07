using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JiggleBones : MonoBehaviour
{
	public Rigidbody rootObject;
	public Transform rootBone;
	public float spring_strength;
	public float spring_dampening;

	public float angleLimit;

	public float collider_Radius;
	public float collider_Height;

	public void Start()
	{
		Transform currentParent = rootBone;
		FixedJoint fJoint = rootBone.gameObject.AddComponent<FixedJoint>();
		fJoint.connectedBody = rootObject;

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
	}

	public void configureJoint(ConfigurableJoint joint, Rigidbody parent)
	{
		joint.connectedBody = parent;
		joint.axis = new Vector3(0, 1, 0);

		joint.xMotion = ConfigurableJointMotion.Locked;
		joint.yMotion = ConfigurableJointMotion.Locked;
		joint.zMotion = ConfigurableJointMotion.Locked;

		joint.angularXMotion = ConfigurableJointMotion.Free;
		joint.angularYMotion = ConfigurableJointMotion.Limited;
		joint.angularZMotion = ConfigurableJointMotion.Limited;

		JointDrive drive = new JointDrive();
		drive.positionSpring = spring_strength;
		drive.positionDamper = spring_dampening;
		drive.maximumForce = joint.angularYZDrive.maximumForce;

		joint.angularXDrive = drive;
		joint.angularYZDrive = drive;

		SoftJointLimit limit = new SoftJointLimit();
		limit.limit = angleLimit;
		limit.bounciness = joint.angularYLimit.bounciness;
		limit.contactDistance = joint.angularYLimit.contactDistance;
		joint.angularYLimit = limit;
		joint.angularZLimit = limit;
	}
}
