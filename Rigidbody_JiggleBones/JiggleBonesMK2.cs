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
		public FVRPhysicalObject mainObject;
		public Transform rootBone;
		public Rigidbody referenceRigidbody;

		public float spring_strength;
		public float spring_dampening;

		public float twistLimit;
		public float angleLimit;

		public float collider_Radius;
		public float collider_Height;

		public List<Collider> addedColliders;
		public List<Rigidbody> addedRBs;
		public List<Joint> addedJoints;
		public List<Joint> rootJoints;
		public List<Vector3> rootJointsPos;
		public List<Quaternion> rootJointsRot;

		public bool useConfigurableJoints = false;

		private bool mainRBWasNull = false;
		private bool isDebug = false;

		public void Start()
		{			
            //if (!CreateJointsOnChildren(rootBone, parentRigidbody)) Debug.LogError("No Children for JiggleBones found!");

			Destroy(referenceRigidbody);

			mainObject.RootRigidbody.centerOfMass = Vector3.zero;
        }

		public void Update()
        {
			if (!mainRBWasNull && mainObject.RootRigidbody == null)
			{
				DebugMessage("attached");
				SetJiggleboneRootRB(FindNewRigidbody(mainObject.transform.parent));
				mainRBWasNull = true;
			}
			else if (mainRBWasNull && mainObject.RootRigidbody != null) 
			{
				DebugMessage("detached");
				SetJiggleboneRootRB(mainObject.RootRigidbody);
				mainRBWasNull = false;
			}
        }

		public void configureJoint(ConfigurableJoint joint, Rigidbody parent)
		{
			joint.enablePreprocessing = false;
			joint.projectionMode = JointProjectionMode.PositionAndRotation;

			joint.connectedBody = parent;
			joint.axis = new Vector3(0, 1, 0);

			//joint.autoConfigureConnectedAnchor = false;

			//joint.connectedAnchor = new Vector3(0,0,0);

			//joint.anchor = joint.transform.InverseTransformPoint(parent.transform.position);

			joint.anchor = Vector3.zero;

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


		public void configureJoint(CharacterJoint joint, Rigidbody parent)
		{
			joint.enablePreprocessing = false;

			joint.connectedBody = parent;
			joint.axis = new Vector3(0, 1, 0);

			//joint.autoConfigureConnectedAnchor = false;

			//joint.connectedAnchor = new Vector3(0,0,0);

			//joint.anchor = joint.transform.InverseTransformPoint(parent.transform.position);

			joint.anchor = Vector3.zero;

			SoftJointLimitSpring spring = new SoftJointLimitSpring();
			spring.spring = spring_strength;
			spring.damper = spring_dampening;


			joint.twistLimitSpring = spring;
			joint.swingLimitSpring = spring;

			SoftJointLimit high_twistlimit = new SoftJointLimit();
			high_twistlimit.limit = 0.1f;
			high_twistlimit.bounciness = joint.lowTwistLimit.bounciness;
			high_twistlimit.contactDistance = joint.lowTwistLimit.contactDistance;
			joint.lowTwistLimit = high_twistlimit;

			SoftJointLimit low_twistlimit = new SoftJointLimit();
			low_twistlimit.limit = -0.1f;
			low_twistlimit.bounciness = joint.highTwistLimit.bounciness;
			low_twistlimit.contactDistance = joint.highTwistLimit.contactDistance;
			joint.highTwistLimit = low_twistlimit;

			SoftJointLimit anglelimit = new SoftJointLimit();
			anglelimit.limit = 0.1f;
			anglelimit.bounciness = joint.swing1Limit.bounciness;
			anglelimit.contactDistance = joint.swing1Limit.contactDistance;
			joint.swing1Limit = anglelimit;
			joint.swing2Limit = anglelimit;
		}

		private bool CreateJointsOnChildren(Transform parent, Rigidbody connectedRB)
        {	
			Transform[] immediateChildren = parent.GetComponentsInDirectChildren<Transform>();

			if (immediateChildren.Length == 0)
			{
				return false;
			}

			foreach (var child in immediateChildren)
			{
                if (child.childCount > 0)
                {
					

					Rigidbody RB = child.gameObject.AddComponent<Rigidbody>();
					CopyRigidBody(RB);
					addedRBs.Add(RB);
					//RB = StaticExtras.GetCopyOf(RB, referenceRigidbody);
					//Rigidbody RB = StaticExtras.CopyComponent(referenceRigidbody, child.gameObject);

					if (useConfigurableJoints) 
					{
						ConfigurableJoint joint = child.gameObject.AddComponent<ConfigurableJoint>();
						configureJoint(joint, connectedRB);
						addedJoints.Add(joint);
					}
					else
                    {
						CharacterJoint joint = child.gameObject.AddComponent<CharacterJoint>();
						configureJoint(joint, connectedRB);
						addedJoints.Add(joint);
					}

					Transform[] immediateGrandChildren = child.GetComponentsInDirectChildren<Transform>();

                    for (int i = 0; i < immediateGrandChildren.Length; i++)
                    {
						GameObject childColliderGO = new GameObject(child.name + "_Collider_" + i);
						childColliderGO.transform.SetParent(child);
						childColliderGO.transform.localPosition = immediateGrandChildren[i].localPosition;
						childColliderGO.transform.localRotation = Quaternion.identity;

						CapsuleCollider collider = childColliderGO.gameObject.AddComponent<CapsuleCollider>();
						collider.direction = 1;
						collider.radius = collider_Radius;
						collider.height = collider_Height;
						addedColliders.Add(collider);
					}
					CreateJointsOnChildren(child, RB);
					//child.SetParent(null);
				}
			}
			return true;
		}

		private Rigidbody FindNewRigidbody(Transform parent)
        {
			if (parent == null)
            {
                Debug.LogError("Couldn't find new Rigidbody to connect jiggle bones to!");
				return null;
            }

			Rigidbody RB = parent.GetComponent<Rigidbody>();

			if (RB == null) return FindNewRigidbody(parent.parent);
			else return RB;
		}

		private void SetJiggleboneRootRB(Rigidbody RB)
        {
			DebugMessage(RB.gameObject.name);
			DebugMessage(rootJoints.Count.ToString());
			ResetRootJointsTransform();

			foreach (var rootJoint in rootJoints)
            {
				rootJoint.connectedBody = RB;
            }
		}

		private void SetRootJoints()
        {
			rootJoints = new List<Joint>(rootBone.GetComponentsInDirectChildren<Joint>());

			rootJointsPos.Clear();
			rootJointsRot.Clear();
            foreach (var rootJoint in rootJoints)
            {
				rootJointsPos.Add(rootJoint.transform.localPosition);
				rootJointsRot.Add(rootJoint.transform.localRotation);

				/*
				Collider collider = rootJoint.GetComponent<Collider>();
				collider.enabled = false;
				*/
            }
		}

		private void ResetRootJointsTransform()
        {
            for (int i = 0; i < rootJoints.Count; i++)
            {
				rootJoints[i].transform.localPosition = rootJointsPos[i];
				rootJoints[i].transform.localRotation = rootJointsRot[i];
			}
        }

		[ContextMenu("Create Jigglebones")]
		public void CreateJigglebones()
        {
			if (addedColliders == null) addedColliders = new List<Collider>();
            foreach (var col in addedColliders)
            {
				DestroyImmediate(col.gameObject);
            }
			addedColliders.Clear();
			if (addedJoints == null) addedJoints = new List<Joint>();
            foreach (var joint in addedJoints)
            {
				DestroyImmediate(joint);
            }
			addedJoints.Clear();
			if (rootJoints == null) rootJoints = new List<Joint>();
			foreach (var joint in rootJoints)
			{
				DestroyImmediate(joint);
			}
			rootJoints.Clear();
			if (addedRBs == null) addedRBs = new List<Rigidbody>();
            foreach (var RB in addedRBs)
            {
				DestroyImmediate(RB);
            }
			addedRBs.Clear();

			if (!CreateJointsOnChildren(rootBone, mainObject.GetComponent<Rigidbody>())) Debug.LogError("No Children for JiggleBones found!");

			SetRootJoints();
		}

		private void DebugMessage(string message)
        {
			if (!isDebug) return;
            Debug.Log(message);
        }
		private void CopyRigidBody(Rigidbody RB)
        {
			RB.mass = referenceRigidbody.mass;
			RB.drag = referenceRigidbody.drag;
			RB.angularDrag = referenceRigidbody.angularDrag;
			RB.useGravity = referenceRigidbody.useGravity;
			RB.isKinematic = referenceRigidbody.isKinematic;
			RB.interpolation = referenceRigidbody.interpolation;
			RB.collisionDetectionMode = referenceRigidbody.collisionDetectionMode;
			RB.constraints = referenceRigidbody.constraints;
        }
	}
}
