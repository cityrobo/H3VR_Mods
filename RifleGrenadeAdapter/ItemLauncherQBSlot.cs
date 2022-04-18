using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class ItemLauncherQBSlot : FVRQuickBeltSlot
    {
		[Header("ItemLauncherQBSlot Config")]
		[Tooltip("Should the Launcher duplicate spawn-locked item?")]
		public bool AllowSpawnLock = true;
		[Tooltip("Should the Launcher duplicate harnessed item?")]
		public bool AllowHarnessing = true;
		[Tooltip("Should the Launcher automatically fire a cartridge placed in the slot or just launch it?")]
		public bool FireAmmunition = true;
		[Tooltip("Should the Launcher automatically pull the pin or cap of a grenade?")]
		public bool AutoArmGrenades = true;
		[Tooltip("Should the Launcher automatically align the object in the slot so it points forward?")]
		public bool AutoAlignZAxis = true;

		private bool _isAlinged = false;
#if !(UNITY_EDITOR || UNITY_5)
        public void Start()
        {
			if (GM.CurrentPlayerBody != null)
			{
				this.RegisterQuickbeltSlot();
			}
        }

		public void OnDestroy()
        {
			if (GM.CurrentPlayerBody != null)
			{
				this.DeRegisterQuickbeltSlot();
			}
		}

		public void RegisterQuickbeltSlot()
		{
			if (!GM.CurrentPlayerBody.QuickbeltSlots.Contains(this))
			{
				GM.CurrentPlayerBody.QuickbeltSlots.Add(this);
			}
		}

		public void DeRegisterQuickbeltSlot()
		{
			if (GM.CurrentPlayerBody.QuickbeltSlots.Contains(this))
			{
				GM.CurrentPlayerBody.QuickbeltSlots.Remove(this);
			}
		}

		void LateUpdate()
        {
			if (!AllowHarnessing && CurObject != null && CurObject.m_isHardnessed)
            {
				CurObject.m_isHardnessed = false;
			}
			if (!AllowSpawnLock && CurObject != null && CurObject.m_isSpawnLock)
			{
				CurObject.m_isSpawnLock = false;
			}

			if (!_isAlinged && CurObject != null && AutoAlignZAxis) AlignHeldObject();

			if (_isAlinged && CurObject == null) _isAlinged = false;
		}

		public bool LaunchHeldObject(float speed, Vector3 point)
        {
			if (CurObject == null) return false;

			FVRPhysicalObject physObject;

			if (CurObject.m_isSpawnLock || CurObject.m_isHardnessed)
            {
				physObject = DuplicateFromSpawnLock(CurObject).GetComponent<FVRPhysicalObject>();
			}
			else
            {
				physObject = CurObject;
				CurObject.SetQuickBeltSlot(null);
			}

			physObject.RootRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			physObject.transform.SetParent(null);
			physObject.transform.position = point;
			physObject.transform.rotation = this.transform.rotation;
			physObject.RootRigidbody.velocity = this.transform.forward * speed;

			switch (physObject)
            {
				case PinnedGrenade g:
					if (AutoArmGrenades) PrimeGrenade(g);
					break;
				case FVRCappedGrenade g:
					if (AutoArmGrenades) PrimeGrenade(g);
					break;
				case FVRFireArmRound g:
					if (FireAmmunition) FireRound(g);
					break;
				default:
                    break;
            }

            return true;
        }

		void PrimeGrenade(PinnedGrenade grenade)
        {
			grenade.ReleaseLever();
        }

		void PrimeGrenade(FVRCappedGrenade grenade)
        {
			grenade.m_IsFuseActive = true;
		}

		void FireRound(FVRFireArmRound round)
        {
			round.Splode(1f, false, true);
        }

		GameObject DuplicateFromSpawnLock(FVRPhysicalObject physicalObject)
        {
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(physicalObject.ObjectWrapper.GetGameObject(), physicalObject.Transform.position, physicalObject.Transform.rotation);
			FVRPhysicalObject component = gameObject.GetComponent<FVRPhysicalObject>();
			if (component is FVREntityProxy)
			{
				(component as FVREntityProxy).Data.PrimeDataLists((component as FVREntityProxy).Flags);
			}
			component.SetQuickBeltSlot(null);
			if (physicalObject.MP.IsMeleeWeapon && component.MP.IsThrownDisposable)
			{
				component.MP.IsCountingDownToDispose = true;
				if (component.MP.m_isThrownAutoAim)
				{
					component.MP.SetReadyToAim(true);
					component.MP.SetPose(physicalObject.MP.PoseIndex);
				}
			}
			return gameObject;
		}

		void AlignHeldObject()
        {
			/*
			if (this.CurObject != null && this.CurObject.transform.forward != this.transform.forward)
			{
				Quaternion objectRot = this.CurObject.transform.localRotation;
				this.PoseOverride.transform.localRotation = Quaternion.Inverse(objectRot);
			}
			*/

			Quaternion objectRot = Quaternion.identity;

            if (CurObject.QBPoseOverride != null) objectRot = CurObject.QBPoseOverride.localRotation;
			else if (CurObject.PoseOverride != null) objectRot = CurObject.PoseOverride.localRotation;

			PoseOverride.localRotation = Quaternion.Inverse(objectRot);
			_isAlinged = true;
		}
#endif
	}
}
