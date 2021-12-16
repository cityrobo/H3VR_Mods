using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class WristItem : FVRPhysicalObject
    {
        [Header("WristItem Config")]
        public bool requiresEmptyHand = false;
#if !(UNITY_EDITOR || UNITY_5)
        private WristQBSlot wristQBSlot = null;
        private FVRViveHand m_wristHand;
        public WristQBSlot WristQBSlot
        {
            get
            {
                return wristQBSlot;
            }
        }

        public FVRViveHand WristHand
        {
            get
            {
                return m_wristHand;
            }
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();

            wristQBSlot = m_quickbeltSlot as WristQBSlot;

            if (wristQBSlot != null)
            {
                if (requiresEmptyHand && wristQBSlot.Hand.CurrentInteractable != null)
                {
                    m_wristHand = null;
                    return;
                }
                m_wristHand = wristQBSlot.Hand;
            }
            else m_wristHand = null;

        }
#endif
    }
}
