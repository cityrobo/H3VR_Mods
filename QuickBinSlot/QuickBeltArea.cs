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

		private Dictionary<GameObject, StandaloneQBSlot> _quickBeltSlots;

#if !(UNITY_EDITOR || UNITY_5)
        public override void Start()
        {
            base.Start();

            _quickBeltSlots = new Dictionary<GameObject, StandaloneQBSlot>();
            SubQBSlotPrefab.SetActive(false);
        }

        public void CreateNewQBSlotPos(FVRPhysicalObject physicalObject)
        {
            Vector3 pos = physicalObject.transform.position;
            Quaternion rot = physicalObject.transform.rotation;
            if (physicalObject.QBPoseOverride != null)
            {
                pos = physicalObject.QBPoseOverride.position;
                rot = physicalObject.QBPoseOverride.rotation;
            }
            else if (physicalObject.PoseOverride_Touch != null && (GM.HMDMode == ControlMode.Oculus || GM.HMDMode == ControlMode.Index))
            {
                pos = physicalObject.PoseOverride_Touch.position;
                rot = physicalObject.PoseOverride_Touch.rotation;
            }
            else if (physicalObject.PoseOverride != null)
            {
                pos = physicalObject.PoseOverride.position;
                rot = physicalObject.PoseOverride.rotation;
            }


            GameObject slotGameObject = Instantiate(SubQBSlotPrefab, pos, rot, this.transform.parent);
            slotGameObject.name = "QuickBeltAreaSubSlot_" + _quickBeltSlots.Count;
            StandaloneQBSlot slot = slotGameObject.GetComponent<StandaloneQBSlot>();
            slotGameObject.SetActive(true);

            physicalObject.ForceObjectIntoInventorySlot(slot);

            _quickBeltSlots.Add(slotGameObject,slot);

            
            slot.PoseOverride.rotation = rot;
        }

        public void LateUpdate()
        {
            if (CurObject != null)
            {
                CreateNewQBSlotPos(CurObject);
            }


            List<GameObject> slotsToDelete = new List<GameObject>();

            foreach (var quickBeltSlot in _quickBeltSlots)
            {
                StandaloneQBSlot slot = quickBeltSlot.Value;

                if (slot.CurObject == null) slotsToDelete.Add(quickBeltSlot.Key);
                else if (slot.CurObject != null && !slot.CurObject.m_isSpawnLock && !slot.CurObject.m_isHardnessed) slot.HoverGeo.SetActive(false);
            }

            foreach (var slotToDelete in slotsToDelete)
            {
                _quickBeltSlots.Remove(slotToDelete);

                FVRPhysicalObject physicalObject = slotToDelete.GetComponentInChildren<FVRPhysicalObject>();
                if (physicalObject != null && physicalObject.m_hand != null)
                {
                    physicalObject.SetParentage(physicalObject.m_hand.WholeRig);
                }

                Destroy(slotToDelete);
            }
        }
#endif
    }
}
