using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class HolsterStockQBSlot : StandaloneQBSlot
	{
#if !(UNITY_EDITOR || UNITY_5)

        [Header("Holster Config")]
        public FVRPhysicalObject PhysicalObject;

        public Transform StockCap;
        public float CapClosed;
        public float CapOpen;

        public enum Axis
        {
            X,
            Y,
            Z
        }
        public Axis axis;
        public override void Start()
        {
            base.Start();

            Hook();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Unhook();
        }

        void Unhook()
        {
            On.FistVR.FVRQuickBeltSlot.Update -= FVRQuickBeltSlot_Update;
        }
        void Hook()
        {
            On.FistVR.FVRQuickBeltSlot.Update += FVRQuickBeltSlot_Update;
        }

        private void FVRQuickBeltSlot_Update(On.FistVR.FVRQuickBeltSlot.orig_Update orig, FVRQuickBeltSlot self)
        {
            orig(self);

            if (this.HeldObject != null && PhysicalObject.QuickbeltSlot != null)
            {
                PhysicalObject.gameObject.layer = LayerMask.NameToLayer("Default");
                this.gameObject.layer = LayerMask.NameToLayer("Interactable");
                PhysicalObject.QuickbeltSlot.IsSelectable = false;
            }
            else if (this.HeldObject == null && PhysicalObject.QuickbeltSlot != null)
            {
                PhysicalObject.gameObject.layer = LayerMask.NameToLayer("Interactable");
                this.gameObject.layer = LayerMask.NameToLayer("Interactable");
                PhysicalObject.QuickbeltSlot.IsSelectable = true;
            }
            else gameObject.layer = LayerMask.NameToLayer("Interactable");

            switch (axis)
            {
                case Axis.X:
                    if (StockCap.localRotation == Quaternion.Euler(CapOpen, 0, 0)) this.IsSelectable = true;
                    else this.IsSelectable = false;
                    break;
                case Axis.Y:
                    if (StockCap.localRotation == Quaternion.Euler(0, CapOpen, 0)) this.IsSelectable = true;
                    else this.IsSelectable = false;
                    break;
                case Axis.Z:
                    if (StockCap.localRotation == Quaternion.Euler(0, 0, CapOpen)) this.IsSelectable = true;
                    else this.IsSelectable = false;
                    break;
                default:
                    break;
            }
        }
#endif
    }
}
