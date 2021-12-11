using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class BreakActionWeapon_BarrelSwitch : MonoBehaviour
    {
        public BreakActionWeapon breakActionWeapon;
        public int[] primaryBarrelGroupIndex;
        public int[] secondaryBarrelGroupIndex;

        public bool hasFireSelector = false;
        public GameObject fireSelector;
        public TransformType transformType = TransformType.translation;
        public Axis axis = Axis.x;

        public enum Axis
        {
            x,
            y,
            z
        }

        public enum TransformType
        {
            translation,
            rotation
        }

        public float primaryMode;
        public float secondaryMode;

#if!DEBUG

        private enum SelectedBarrelGroup
        {
            primary,
            secondary
        }

        private class BarrelGroup
        {
            public BarrelGroup()
            {
                this.indexList = new List<int>();
                this.Barrels = new List<BreakActionWeapon.BreakActionBarrel>();
            }
            public List<int> indexList;
            public List<BreakActionWeapon.BreakActionBarrel> Barrels;
        }

        private BarrelGroup primaryBarrelGroup = new BarrelGroup();
        private BarrelGroup secondaryBarrelGroup = new BarrelGroup();

        private SelectedBarrelGroup selectedBarrelGroup = SelectedBarrelGroup.primary;

        private Transform origTransformFireSelector;

        public void Start()
        {
            foreach (int index in primaryBarrelGroupIndex)
            {
                primaryBarrelGroup.indexList.Add(index);
                primaryBarrelGroup.Barrels.Add(breakActionWeapon.Barrels[index]);
            }

            foreach (int index in secondaryBarrelGroupIndex)
            {
                secondaryBarrelGroup.indexList.Add(index);
                secondaryBarrelGroup.Barrels.Add(breakActionWeapon.Barrels[index]);
            }

            origTransformFireSelector = fireSelector.transform;

            Hook();

            if (hasFireSelector) UpdateFireSelector();
        }

        public void OnDestroy()
        {
            Unhook();
        }

        public void NextBarrelGroup()
        {
            switch (selectedBarrelGroup)
            {
                case SelectedBarrelGroup.primary:
                    selectedBarrelGroup = SelectedBarrelGroup.secondary;
                    break;
                case SelectedBarrelGroup.secondary:
                    selectedBarrelGroup = SelectedBarrelGroup.primary;
                    break;
                default:
                    selectedBarrelGroup = SelectedBarrelGroup.primary;
                    break;
            }

            if (hasFireSelector) UpdateFireSelector();
        }

        public void UpdateFireSelector()
        {
            if (!hasFireSelector) return;
            switch (selectedBarrelGroup)
            {
                case SelectedBarrelGroup.primary:
                    switch (transformType)
                    {
                        case TransformType.translation:
                            switch (axis)
                            {
                                case Axis.x:
                                    fireSelector.transform.localPosition = new Vector3(primaryMode, origTransformFireSelector.localPosition.y, origTransformFireSelector.localPosition.z);
                                    break;
                                case Axis.y:
                                    fireSelector.transform.localPosition = new Vector3(origTransformFireSelector.localPosition.x ,primaryMode, origTransformFireSelector.localPosition.z);
                                    break;
                                case Axis.z:
                                    fireSelector.transform.localPosition = new Vector3(origTransformFireSelector.localPosition.x, origTransformFireSelector.localPosition.y, primaryMode);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case TransformType.rotation:
                            switch (axis)
                            {
                                case Axis.x:
                                    fireSelector.transform.localEulerAngles = new Vector3(primaryMode, origTransformFireSelector.localEulerAngles.y, origTransformFireSelector.localEulerAngles.z);
                                    break;
                                case Axis.y:
                                    fireSelector.transform.localEulerAngles = new Vector3(origTransformFireSelector.localEulerAngles.x, primaryMode, origTransformFireSelector.localEulerAngles.z);
                                    break;
                                case Axis.z:
                                    fireSelector.transform.localEulerAngles = new Vector3(origTransformFireSelector.localEulerAngles.x, origTransformFireSelector.localEulerAngles.y, primaryMode);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                    break;
                case SelectedBarrelGroup.secondary:
                    switch (transformType)
                    {
                        case TransformType.translation:
                            switch (axis)
                            {
                                case Axis.x:
                                    fireSelector.transform.localPosition = new Vector3(secondaryMode, origTransformFireSelector.localPosition.y, origTransformFireSelector.localPosition.z);
                                    break;
                                case Axis.y:
                                    fireSelector.transform.localPosition = new Vector3(origTransformFireSelector.localPosition.x, secondaryMode, origTransformFireSelector.localPosition.z);
                                    break;
                                case Axis.z:
                                    fireSelector.transform.localPosition = new Vector3(origTransformFireSelector.localPosition.x, origTransformFireSelector.localPosition.y, secondaryMode);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case TransformType.rotation:
                            switch (axis)
                            {
                                case Axis.x:
                                    fireSelector.transform.localEulerAngles = new Vector3(secondaryMode, origTransformFireSelector.localEulerAngles.y, origTransformFireSelector.localEulerAngles.z);
                                    break;
                                case Axis.y:
                                    fireSelector.transform.localEulerAngles = new Vector3(origTransformFireSelector.localEulerAngles.x, secondaryMode, origTransformFireSelector.localEulerAngles.z);
                                    break;
                                case Axis.z:
                                    fireSelector.transform.localEulerAngles = new Vector3(origTransformFireSelector.localEulerAngles.x, origTransformFireSelector.localEulerAngles.y, secondaryMode);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public void Unhook()
        {
            On.FistVR.BreakActionWeapon.DropHammer -= BreakActionWeapon_DropHammer;
            On.FistVR.BreakActionWeapon.UpdateInputAndAnimate -= BreakActionWeapon_UpdateInputAndAnimate;
        }

        public void Hook()
        {
            On.FistVR.BreakActionWeapon.DropHammer += BreakActionWeapon_DropHammer;
            On.FistVR.BreakActionWeapon.UpdateInputAndAnimate += BreakActionWeapon_UpdateInputAndAnimate;
        }

        private void BreakActionWeapon_UpdateInputAndAnimate(On.FistVR.BreakActionWeapon.orig_UpdateInputAndAnimate orig, BreakActionWeapon self, FVRViveHand hand)
        {
            orig(self, hand);
            if (self == breakActionWeapon)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45f)
                {
                    NextBarrelGroup();
                    self.PlayAudioEvent(FirearmAudioEventType.FireSelector);
                }
            }
        }

        private void BreakActionWeapon_DropHammer(On.FistVR.BreakActionWeapon.orig_DropHammer orig, BreakActionWeapon self)
        {
            if (self == breakActionWeapon)
            {
                if (!self.m_isLatched)
                {
                    return;
                }
                self.firedOneShot = false;

                switch (selectedBarrelGroup)
                {
                    case SelectedBarrelGroup.primary:
                        for (int i = 0; i < primaryBarrelGroup.Barrels.Count; i++)
                        {
                            if (primaryBarrelGroup.Barrels[i].m_isHammerCocked)
                            {
                                self.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
                                primaryBarrelGroup.Barrels[i].m_isHammerCocked = false;
                                self.UpdateVisualHammers();
                                self.Fire(primaryBarrelGroup.indexList[i], self.FireAllBarrels, primaryBarrelGroup.indexList[i]);
                                if (!self.FireAllBarrels)
                                {
                                    break;
                                }
                            }
                        }
                        break;
                    case SelectedBarrelGroup.secondary:
                        for (int i = 0; i < secondaryBarrelGroup.Barrels.Count; i++)
                        {
                            if (secondaryBarrelGroup.Barrels[i].m_isHammerCocked)
                            {
                                self.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
                                secondaryBarrelGroup.Barrels[i].m_isHammerCocked = false;
                                self.UpdateVisualHammers();
                                self.Fire(secondaryBarrelGroup.indexList[i], self.FireAllBarrels, secondaryBarrelGroup.indexList[i]);
                                if (!self.FireAllBarrels)
                                {
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else orig(self);
        }
#endif
    }
}
