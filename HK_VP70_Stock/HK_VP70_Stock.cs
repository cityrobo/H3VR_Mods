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
    public class HK_VP70_Stock : FVRFireArmAttachment
    {
        [Header("Stock Config")]
        public Transform FireSelector;
        public float ModeSingle;
        public float ModeBurst;
        public int BurstAmount = 3;


        public Transform StockLatch;
        public float LatchUnheld;
        public float LatchHeld;

        public Transform StockCap;
        public AR15HandleSightFlipper SightFlipper;

        public FVRQuickBeltSlot QBSlot;

        private Handgun _handgun;
        private Handgun.FireSelectorMode[] _originalHandgunFireModes;
        private bool _handgunHadFireSelectorButton = false;
#if !(DEBUG || MEATKIT)
        public override bool CanDetach()
        {
            if (_handgun != null && _handgun.FireSelectorModeIndex == 1) return false;
            else return base.CanDetach();
        }

        public void AddBurst()
        {
            _handgun = curMount.GetRootMount().MyObject as Handgun;
            if (_handgun != null)
            {
                _originalHandgunFireModes = _handgun.FireSelectorModes;

                _handgunHadFireSelectorButton = _handgun.HasFireSelector;

                if (!_handgunHadFireSelectorButton)
                {
                    _handgun.HasFireSelector = true;
                    _handgun.FireSelector = new GameObject("FireSelector").transform;
                }

                Handgun.FireSelectorMode newFireSelectorMode = new Handgun.FireSelectorMode();
                newFireSelectorMode.ModeType = Handgun.FireSelectorModeType.Burst;
                newFireSelectorMode.BurstAmount = BurstAmount;

                _handgun.FireSelectorModes = _handgun.FireSelectorModes.Concat(new Handgun.FireSelectorMode[] { newFireSelectorMode }).ToArray();
            }
        }

        public void RemoveBurst()
        {
            if (_handgun != null)
            {
                _handgun.m_fireSelectorMode = _originalHandgunFireModes.Length - 1;
                _handgun.FireSelectorModes = _originalHandgunFireModes;

                if (!_handgunHadFireSelectorButton)
                {
                    Destroy(_handgun.FireSelector.gameObject);
                    _handgun.HasFireSelector = false;
                }
            }
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();

            if (_handgun != null)
            {
                switch (_handgun.FireSelectorModeIndex)
                {
                    case 0:
                        FireSelector.localRotation = Quaternion.Euler(ModeSingle, 0, 0);
                        break;
                    case 1:
                        FireSelector.localRotation = Quaternion.Euler(ModeBurst, 0 , 0);
                        break;
                    default:
                        break;
                }
            }

            if (IsHeld) StockLatch.localPosition = new Vector3(StockLatch.localPosition.x, StockLatch.localPosition.y, LatchHeld);
            else if (AttachmentInterface.IsHeld && AttachmentInterface.m_hand.Input.TouchpadPressed && Vector2.Angle(AttachmentInterface.m_hand.Input.TouchpadAxes, Vector2.down) < 45f) StockLatch.localPosition = new Vector3(StockLatch.localPosition.x, StockLatch.localPosition.y, LatchHeld);
            else StockLatch.localPosition = new Vector3(StockLatch.localPosition.x, StockLatch.localPosition.y, LatchUnheld);

            if (QBSlot.HeldObject != null && QuickbeltSlot != null)
            {
                gameObject.layer = LayerMask.NameToLayer("NoCol");
                QBSlot.gameObject.layer = LayerMask.NameToLayer("Interactable");
                QuickbeltSlot.IsSelectable = false;
            }
            else if (QBSlot.HeldObject == null && QuickbeltSlot != null)
            {
                gameObject.layer = LayerMask.NameToLayer("Interactable");
                QBSlot.gameObject.layer = LayerMask.NameToLayer("Interactable");
                QuickbeltSlot.IsSelectable = true;
            }
            else gameObject.layer = LayerMask.NameToLayer("Interactable");

            if (!SightFlipper.m_isLargeAperture)
            {
                QBSlot.IsSelectable = true;
                //Sensor.gameObject.SetActive(false);
                if (QBSlot.HeldObject != null && QBSlot.HeldObject is Handgun handgun && handgun.Slide.gameObject.layer != LayerMask.NameToLayer("Interactable")) handgun.Slide.gameObject.layer = LayerMask.NameToLayer("Interactable");
            }
            else
            {
                QBSlot.IsSelectable = false;
                //Sensor.gameObject.SetActive(true);
                if (QBSlot.HeldObject != null && QBSlot.HeldObject is Handgun handgun && handgun.Slide.gameObject.layer != LayerMask.NameToLayer("NoCol")) handgun.Slide.gameObject.layer = LayerMask.NameToLayer("NoCol");
            }
        }
#endif
    }
}
