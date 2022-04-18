using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{	
    public class QuickBeltArea : StandaloneQBSlot
	{
        public GameObject SubQBSlotPrefab;
        [Tooltip("Any positive number will cause the Area to limit the number of items it can carry.")]
        public int ItemLimit = 0;
        public bool ObjectsKeepCollision = false;
        public bool CollisionActivatedFreeze = false;
        public QuickBeltAreaCollisionDetector CollisionDetector;
        public bool SetKinematic = false;

        [Header("Advanced Size Options")]
        public bool UsesAdvancedSizeMode = false;
        [Tooltip("Capacity requirement for items of size Small, Medium, Large, Massive, CantCarryBig")]
        public int[] Sizes = { 1, 2, 5, 10, 25 };
        public int TotalCapacity = 50;

        [HideInInspector]
        public bool ItemDidCollide = false;
		private Dictionary<GameObject, StandaloneQBSlot> _quickBeltSlots;
        private FVRPhysicalObject.FVRPhysicalObjectSize[] _sizes =
                    {FVRPhysicalObject.FVRPhysicalObjectSize.Small,
                    FVRPhysicalObject.FVRPhysicalObjectSize.Medium,
                    FVRPhysicalObject.FVRPhysicalObjectSize.Large,
                    FVRPhysicalObject.FVRPhysicalObjectSize.Massive,
                    FVRPhysicalObject.FVRPhysicalObjectSize.CantCarryBig};

        private Dictionary<FVRPhysicalObject.FVRPhysicalObjectSize, int> _SizeRequirements;
        private int _currentLoad = 0;
        private class SubQBSlot
        {
            SubQBSlot(StandaloneQBSlot slot, Vector3 localPos, Quaternion localRot)
            {
                this.slot = slot;
                this.localPos = localPos;
                this.localRot = localRot;
            }

            public StandaloneQBSlot slot;
            public Vector3 localPos;
            public Quaternion localRot;
        }

#if !(UNITY_EDITOR || UNITY_5)
        public override void Start()
        {
            base.Start();

            _quickBeltSlots = new Dictionary<GameObject, StandaloneQBSlot>();
            SubQBSlotPrefab.SetActive(false);

            _SizeRequirements = new Dictionary<FVRPhysicalObject.FVRPhysicalObjectSize, int>();
            for (int i = 0; i < _sizes.Length; i++)
            {
                _SizeRequirements.Add(_sizes[i], Sizes[i]);
            }
        }

        public void LateUpdate()
        {
            if (!CollisionActivatedFreeze && CurObject != null)
            {
                CreateNewQBSlotPos(CurObject);
            }
            else if (CurObject != null)
            {
                StartCoroutine(WaitForCollision(CurObject));
            }


            List<GameObject> slotsToDelete = new List<GameObject>();

            foreach (var quickBeltSlot in _quickBeltSlots)
            {
                StandaloneQBSlot slot = quickBeltSlot.Value;

                if (slot.CurObject == null) slotsToDelete.Add(quickBeltSlot.Key);
                else if (slot.CurObject != null && !slot.CurObject.m_isSpawnLock && !slot.CurObject.m_isHardnessed) slot.HoverGeo.SetActive(false);

                if (ObjectsKeepCollision && slot.CurObject != null) slot.CurObject.SetAllCollidersToLayer(false, "Default");

                if (SetKinematic && slot.CurObject != null && slot.CurObject.transform.localPosition != Vector3.zero) slot.CurObject.transform.localPosition = Vector3.zero;
                if (SetKinematic && slot.CurObject != null && slot.CurObject.transform.localRotation != Quaternion.identity) slot.CurObject.transform.localRotation = Quaternion.identity;
            }

            foreach (var slotToDelete in slotsToDelete)
            {
                _quickBeltSlots.Remove(slotToDelete);

                FVRPhysicalObject physicalObject = slotToDelete.GetComponentInChildren<FVRPhysicalObject>();
                if (physicalObject != null && physicalObject.m_hand != null)
                {
                    physicalObject.SetParentage(physicalObject.m_hand.WholeRig);
                }
                if (UsesAdvancedSizeMode)
                {
                    FVRPhysicalObject.FVRPhysicalObjectSize size = slotToDelete.GetComponent<FVRQuickBeltSlot>().SizeLimit;
                    int sizeRequirement = 0;
                    _SizeRequirements.TryGetValue(size, out sizeRequirement);

                    _currentLoad -= sizeRequirement;
                }
                Destroy(slotToDelete);
            }

            if (ItemLimit != 0)
            {
                if (_quickBeltSlots.Count >= ItemLimit) this.IsSelectable = false;
                else this.IsSelectable = true;
            }

            if (UsesAdvancedSizeMode)
            {
                if (_currentLoad >= TotalCapacity) this.IsSelectable = false;
                else this.IsSelectable = true;
            }
        }

        public void CreateNewQBSlotPos(FVRPhysicalObject physicalObject)
        {
            FVRPhysicalObject.FVRPhysicalObjectSize size = physicalObject.Size;

            Vector3 pos = physicalObject.transform.position;
            Quaternion rot = physicalObject.transform.rotation;
            Quaternion localRot = rot;
            if (!SetKinematic && physicalObject.QBPoseOverride != null)
            {
                pos = physicalObject.QBPoseOverride.position;
                rot = physicalObject.QBPoseOverride.rotation;
                localRot = physicalObject.QBPoseOverride.localRotation;
            }
            else if (!SetKinematic && physicalObject.PoseOverride_Touch != null && (GM.HMDMode == ControlMode.Oculus || GM.HMDMode == ControlMode.Index))
            {
                pos = physicalObject.PoseOverride_Touch.position;
                rot = physicalObject.PoseOverride_Touch.rotation;
                localRot = physicalObject.PoseOverride_Touch.localRotation;
            }
            else if (!SetKinematic && physicalObject.PoseOverride != null)
            {
                pos = physicalObject.PoseOverride.position;
                rot = physicalObject.PoseOverride.rotation;
                localRot = physicalObject.PoseOverride.localRotation;
            }

            if (UsesAdvancedSizeMode)
            {
                int sizeRequirement = 0;
                _SizeRequirements.TryGetValue(size, out sizeRequirement);
                if (_currentLoad + sizeRequirement > TotalCapacity)
                {
                    physicalObject.ForceObjectIntoInventorySlot(null);
                    return;
                }
                else
                {
                    _currentLoad += sizeRequirement;
                }
            }

            GameObject slotGameObject = Instantiate(SubQBSlotPrefab, pos, rot, this.transform.parent);
            slotGameObject.name = "QuickBeltAreaSubSlot_" + _quickBeltSlots.Count;
            StandaloneQBSlot slot = slotGameObject.GetComponent<StandaloneQBSlot>();
            slotGameObject.SetActive(true);
            slot.SizeLimit = size;

            physicalObject.ForceObjectIntoInventorySlot(slot);

            _quickBeltSlots.Add(slotGameObject, slot);

            if (SetKinematic)
            {
                physicalObject.RootRigidbody.isKinematic = true;
                slot.transform.rotation = rot;
            }
            else slot.PoseOverride.rotation = rot;
            //slot.CurObject.transform.rotation = slot.CurObject.transform.rotation * Quaternion.Inverse(localRot);
        }

        IEnumerator WaitForCollision(FVRPhysicalObject physicalObject)
        {
            physicalObject.SetParentage(null);
            physicalObject.SetQuickBeltSlot(null);
            ItemDidCollide = false;
            CollisionDetector.PhysicalObjectToDetect = physicalObject;
            while (!ItemDidCollide) yield return null;
            ItemDidCollide = false;
            CreateNewQBSlotPos(physicalObject);
        }
#endif
    }
}
