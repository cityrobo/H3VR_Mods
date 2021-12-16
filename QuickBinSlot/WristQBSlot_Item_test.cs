using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class WristQBSlot_Item_test : WristItem
    {
        [Header("WristQBSlot Config")]
        public MeshRenderer render;

#if !(UNITY_EDITOR || UNITY_5)
        public override void FVRUpdate()
        {
            base.FVRUpdate();

            if (WristHand != null)
            {
                render.material.color = new Color(WristHand.Input.TriggerFloat, 0, 0, 1);
            }
            else render.material.color = new Color(0, 0, 0, 1);
        }

#endif
    }
}
