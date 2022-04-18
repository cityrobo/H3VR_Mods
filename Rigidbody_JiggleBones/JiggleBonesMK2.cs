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
		[Tooltip("Script will ignore these bones and their children when creating joints.")]
		public List<Transform> boneBlackList;

		public enum Axis
		{
			X,
			Y,
			Z
		}

		public bool usesSprings = true;
		public Axis jointAxis = Axis.Y;

		public float spring_strength;
		public float spring_dampening;

		public float twistLimitX;
		public float angleLimitY;
		public float angleLimitZ;

		public List<Collider> addedColliders;
		public List<Rigidbody> addedRBs;
		public List<Joint> addedJoints;
		public List<Joint> rootJoints;
		public List<Vector3> rootJointsPos;
		public List<Quaternion> rootJointsRot;

		public bool useConfigurableJoints = true;
		public bool hasEndBones = true;
		[Tooltip("You will want to use this setting for pretty much anything gravity enabled that is only attached on one end. Having all RBs act on gravity gave weird results in testing.")]
		public bool onlyLastRigidbodyUsesGravity = true;
		public bool addBasicColliders = true;
		public float collider_Radius;



		public Axis colliderAxis = Axis.Y;
		//public float collider_Height;

		private bool mainRBWasNull = false;

		private bool IsDebug
        {
			get { return false; }
        }

		public void Start()
		{
			referenceRigidbody.gameObject.SetActive(false);
			
			FixParenting();
        }

		public void Update()
        {
			if (!mainRBWasNull && mainObject.RootRigidbody == null)
			{
				SetJiggleboneRootRB(FindNewRigidbody(mainObject.transform.parent));
				mainRBWasNull = true;
			}
			else if (mainRBWasNull && mainObject.RootRigidbody != null) 
			{
				SetJiggleboneRootRB(mainObject.RootRigidbody);
				mainRBWasNull = false;
			}
        }

		public void configureJoint(ConfigurableJoint joint, Rigidbody parent)
		{
			joint.enablePreprocessing = false;
			joint.projectionMode = JointProjectionMode.PositionAndRotation;

			joint.connectedBody = parent;
            switch (jointAxis)
            {
                case Axis.X:
					joint.axis = new Vector3(1, 0, 0);
					break;
                case Axis.Y:
					joint.axis = new Vector3(0, 1, 0);
					break;
                case Axis.Z:
					joint.axis = new Vector3(0, 0, 1);
					break;
                default:
                    break;
            }

			//joint.autoConfigureConnectedAnchor = false;

			//joint.connectedAnchor = new Vector3(0,0,0);

			//joint.anchor = joint.transform.InverseTransformPoint(parent.transform.position);

			joint.anchor = Vector3.zero;

			joint.xMotion = ConfigurableJointMotion.Locked;
			joint.yMotion = ConfigurableJointMotion.Locked;
			joint.zMotion = ConfigurableJointMotion.Locked;

            if (usesSprings)
            {
				joint.angularXMotion = ConfigurableJointMotion.Limited;
				joint.angularYMotion = ConfigurableJointMotion.Limited;
				joint.angularZMotion = ConfigurableJointMotion.Limited;
			}
            else
            {
				joint.angularXMotion = ConfigurableJointMotion.Free;
				joint.angularYMotion = ConfigurableJointMotion.Free;
				joint.angularZMotion = ConfigurableJointMotion.Free;
			}

			JointDrive drive = new JointDrive();
			drive.positionSpring = spring_strength;
			drive.positionDamper = spring_dampening;
			drive.maximumForce = joint.angularYZDrive.maximumForce;

			joint.angularXDrive = drive;
			joint.angularYZDrive = drive;

			SoftJointLimit high_twistlimit = new SoftJointLimit();
			high_twistlimit.limit = twistLimitX;
			high_twistlimit.bounciness = joint.highAngularXLimit.bounciness;
			high_twistlimit.contactDistance = joint.highAngularXLimit.contactDistance;
			joint.highAngularXLimit = high_twistlimit;

			SoftJointLimit low_twistlimit = new SoftJointLimit();
			low_twistlimit.limit = -twistLimitX;
			low_twistlimit.bounciness = joint.lowAngularXLimit.bounciness;
			low_twistlimit.contactDistance = joint.lowAngularXLimit.contactDistance;
			joint.lowAngularXLimit = low_twistlimit;

			SoftJointLimit anglelimitY = new SoftJointLimit();
			anglelimitY.limit = angleLimitY;
			anglelimitY.bounciness = joint.angularYLimit.bounciness;
			anglelimitY.contactDistance = joint.angularYLimit.contactDistance;
			joint.angularYLimit = anglelimitY;


			SoftJointLimit anglelimitZ = new SoftJointLimit();
			anglelimitZ.limit = angleLimitZ;
			anglelimitZ.bounciness = joint.angularZLimit.bounciness;
			anglelimitZ.contactDistance = joint.angularZLimit.contactDistance;
			joint.angularZLimit = anglelimitZ;
		}


		public void configureJoint(CharacterJoint joint, Rigidbody parent)
		{
			joint.enablePreprocessing = false;

			joint.connectedBody = parent;
			switch (jointAxis)
			{
				case Axis.X:
					joint.axis = new Vector3(1, 0, 0);
					break;
				case Axis.Y:
					joint.axis = new Vector3(0, 1, 0);
					break;
				case Axis.Z:
					joint.axis = new Vector3(0, 0, 1);
					break;
				default:
					break;
			}

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
				if ( boneBlackList.Contains(child) || child.GetComponent<Collider>() != null) continue;
                if (child.childCount > 0 || !hasEndBones)
                {
					Rigidbody RB = child.gameObject.AddComponent<Rigidbody>();
					CopyRigidBody(RB);
					if (onlyLastRigidbodyUsesGravity) RB.useGravity = false;
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

                    if (immediateGrandChildren.Length == 1)
                    {
						RB.centerOfMass = immediateGrandChildren[0].localPosition;

					}

					if (addBasicColliders)
					{
						for (int i = 0; i < immediateGrandChildren.Length; i++)
						{
							GameObject childColliderGO = new GameObject(child.name + "_Collider_" + i);
							childColliderGO.transform.SetParent(child);
							childColliderGO.transform.position = Vector3.Lerp(child.position, immediateGrandChildren[i].position, 0.5f);
							childColliderGO.transform.localRotation = Quaternion.identity;

							CapsuleCollider collider = childColliderGO.gameObject.AddComponent<CapsuleCollider>();
							collider.direction = (int) colliderAxis;
							collider.radius = collider_Radius;
							collider.height = Vector3.Distance(child.position, immediateGrandChildren[i].position);
							addedColliders.Add(collider);
						}
					}
                    if (onlyLastRigidbodyUsesGravity)
                    {
						bool isLastBone = true;
                        foreach (var grandchild in immediateGrandChildren)
                        {
							if (grandchild.GetComponent<Collider>() == null) isLastBone = false;
                        }
						if (isLastBone && referenceRigidbody.useGravity) RB.useGravity = true;
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

		private void FixParenting()
        {
            foreach (var RB in addedRBs)
            {
				RB.transform.SetParent(rootBone);
            }
        }
		[ContextMenu("Create JiggleBones")]
		public void CreateJiggleBones()
        {
			ClearJiggleBones();

			if (!CreateJointsOnChildren(rootBone, mainObject.GetComponent<Rigidbody>())) Debug.LogError("No Children for JiggleBones found!");

			SetRootJoints();
		}

		[ContextMenu("Clear JiggleBones")]
		public void ClearJiggleBones()
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

			rootJoints.Clear();
			rootJointsPos.Clear();
			rootJointsRot.Clear();

		}

		private void DebugMessage(string message)
        {
			if (!IsDebug) return;
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
