using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    [RequireComponent(typeof(FVRQuickBeltSlot))]
    public class CustomQBSlotSounds : MonoBehaviour
    {
		public AudioEvent insertSound;
		public AudioEvent extractSound;

		private FVRQuickBeltSlot slot;
		private bool slotHasItem = false;
        private bool isHooked = false;
#if !(UNITY_EDITOR || UNITY_5)
		public void Start()
        {
			slot = gameObject.GetComponent<FVRQuickBeltSlot>();
            slotHasItem = false;
        }

		public void Update()
        {
            if (!slotHasItem && (slot.HeldObject != null || slot.CurObject != null))
            {
                slotHasItem = true;
                SM.PlayGenericSound(insertSound, slot.transform.position);
            }
            else if (slotHasItem && (slot.HeldObject == null && slot.CurObject == null))
            {
                slotHasItem = false;
                SM.PlayGenericSound(extractSound, slot.transform.position);
            }

            if (!isHooked && slotHasItem && slot.CurObject.m_isSpawnLock == true)
            {
                Hook();
                isHooked = true;
            }
            else if (isHooked && (!slotHasItem || slot.CurObject.m_isSpawnLock == false))
            {
                Unhook();
                isHooked = false;
            }
        }

        void Unhook()
        {
            On.FistVR.FVRPhysicalObject.DuplicateFromSpawnLock -= FVRPhysicalObject_DuplicateFromSpawnLock;
        }

        void Hook()
        {
            On.FistVR.FVRPhysicalObject.DuplicateFromSpawnLock += FVRPhysicalObject_DuplicateFromSpawnLock;
        }

        private GameObject FVRPhysicalObject_DuplicateFromSpawnLock(On.FistVR.FVRPhysicalObject.orig_DuplicateFromSpawnLock orig, FVRPhysicalObject self, FVRViveHand hand)
        {
            GameObject temp = orig(self, hand);
            if (self == slot.CurObject || self == slot.HeldObject)
            {
                SM.PlayGenericSound(extractSound, slot.transform.position);
            }
            return temp;
        }
#endif
    }
}
