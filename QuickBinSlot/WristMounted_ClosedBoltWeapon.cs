using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class WristMounted_ClosedBoltWeapon : MonoBehaviour
    {
        public ClosedBoltWeapon closedBoltWeapon;
        public bool requiresEmptyHand = false;

#if !(UNITY_EDITOR || UNITY_5)
        public void Start()
        {
            Hook();
        }

		public void OnDestroy()
        {
            Unhook();
		}

        public void Update()
        {
            if (closedBoltWeapon.m_quickbeltSlot != null)
            {
                WristQBSlot wristQBSlot = closedBoltWeapon.m_quickbeltSlot as WristQBSlot;
                if (wristQBSlot != null && wristQBSlot.Hand != null)
                {
                    if (requiresEmptyHand && wristQBSlot.Hand.CurrentInteractable != null) return;
                    if (wristQBSlot.Hand.Input.TriggerFloat < 0.15f)
                    {
                        closedBoltWeapon.m_hasTriggeredUpSinceBegin = true;
                    }
                    closedBoltWeapon.UpdateInputAndAnimate(wristQBSlot.Hand);
                }
            }
        }

        void Unhook()
        {
            
        }

        void Hook()
        {
            
        }
#endif
    }
}
