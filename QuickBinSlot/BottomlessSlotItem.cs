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
#if !(UNITY_EDITOR || UNITY_5)
    public class BottomlessSlotItem : MonoBehaviour
    {
        public struct BottomlessSlotItemIDs
        {
            public BottomlessSlotItemIDs(int slotID, int itemID)
            {
                this.slotID = slotID;
                this.itemID = itemID;
            }
            public readonly int slotID;
            public readonly int itemID;
        }

        private BottomlessSlotItemIDs iDs;
        private bool valueSet = false;
        public BottomlessSlotItemIDs IDs
        {
            get
            {
                return iDs;
            }
            set
            {
                if (!valueSet)
                {
                    iDs = value;
                    valueSet = false;
                }
                else
                {
                    throw new AccessViolationException();
                }
            }
        }
    }
#endif
}
