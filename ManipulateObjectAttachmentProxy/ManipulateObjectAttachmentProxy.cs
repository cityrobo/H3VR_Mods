using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class ManipulateObjectAttachmentProxy : MonoBehaviour
    {
        public FVRFireArmAttachment attachment;

        public enum TargetType
        {
            Bolt,
            Trigger,
            BoltHandle,
            Safety,
            FireSelector,
            MagazineRelease,
            BoltRelease,
            Hammer
        }
        
        public TargetType targetType;

        [Header("Alternative target by name:")]
        public bool useAlternativeMethod;
        [Tooltip("If the part you wanna monitor doesn't exist as a type, you can put in the exact path of the part (without the parent) that you wanna proxy and it will get that one on the gun instead.")]
        public string targetPath;

        private FVRPhysicalObject weapon;
        private Transform proxy = null;

        private bool debug = false;

#if !(UNITY_EDITOR || UNITY_5 || DEBUG)

        public void Update()
        {
            if (attachment.curMount != null && !useAlternativeMethod)
            {
                if (proxy == null)
                {
                    DebugMessage("Grabbing mounted item.");

                    weapon = attachment.curMount.GetRootMount().MyObject;

                    DebugMessage("Mounted Item: " + weapon.name);

                    switch (weapon)
                    {
                        case OpenBoltReceiver s:
                            DebugMessage("OpenBoltReceiver found!");
                            SetProxy(s);
                            break;
                        case ClosedBoltWeapon s:
                            DebugMessage("ClosedBoltWeapon found!");
                            SetProxy(s);
                            break;
                        case Handgun s:
                            DebugMessage("Handgun found!");
                            SetProxy(s);
                            break;
                        case TubeFedShotgun s:
                            DebugMessage("TubeFedShotgun found!");
                            SetProxy(s);
                            break;
                        case BoltActionRifle s:
                            DebugMessage("BoltActionRifle found!");
                            SetProxy(s);
                            break;
                        default:
                            Debug.LogWarning("ManipulateObjectAttachmentProxy: Parent object is not a supported firearm!");
                            break;
                    }
                }
                if (proxy != null)
                {
                    this.transform.localPosition = proxy.localPosition;
                    this.transform.localRotation = proxy.localRotation;
                    this.transform.localScale = proxy.localScale;
                }

            }
            else if (attachment.curMount != null && useAlternativeMethod)
            {
                if (proxy == null)
                {
                    DebugMessage("Grabbing mounted item.");

                    weapon = attachment.curMount.GetRootMount().MyObject;

                    DebugMessage("Mounted Item: " + weapon.name);

                    proxy = weapon.transform.Find(targetPath);
                }
                if (proxy != null)
                {
                    this.transform.localPosition = proxy.localPosition;
                    this.transform.localRotation = proxy.localRotation;
                    this.transform.localScale = proxy.localScale;
                }
                else
                {
                    Debug.LogWarning("ManipulateObjectAttachmentProxy: Could not find target with alternative mode path!");
                }
            }
            else
            {
                proxy = null;
            }
        }

#endif
        private void SetProxy(OpenBoltReceiver s)
        {
            switch (targetType)
            {
                case TargetType.Bolt:
                    proxy = s.Bolt.transform;
                    break;
                case TargetType.Trigger:
                    proxy = s.Trigger;
                    break;
                case TargetType.BoltHandle:
                    OpenBoltChargingHandle openBoltChargingHandle = s.GetComponentInChildren<OpenBoltChargingHandle>();
                    proxy = openBoltChargingHandle.transform;
                    break;
                case TargetType.Safety:
                    proxy = s.FireSelectorSwitch;
                    break;
                case TargetType.FireSelector:
                    proxy = s.FireSelectorSwitch2;
                    break;
                case TargetType.MagazineRelease:
                    proxy = s.MagReleaseButton;
                    break;
                default:
                    Debug.LogWarning("ManipulateObjectAttachmentProxy: TargetType not available for this type of FireArm!");
                    break;
            }
        }
        private void SetProxy(ClosedBoltWeapon s)
        {
            switch (targetType)
            {
                case TargetType.Bolt:
                    proxy = s.Bolt.transform;
                    break;
                case TargetType.Trigger:
                    proxy = s.Trigger;
                    break;
                case TargetType.BoltHandle:
                    proxy = s.Handle.transform;
                    break;
                case TargetType.Safety:
                    proxy = s.FireSelectorSwitch;
                    break;
                case TargetType.FireSelector:
                    proxy = s.FireSelectorSwitch2;
                    break;
                case TargetType.Hammer:
                    proxy = s.Bolt.Hammer;
                    break;
                default:
                    Debug.LogWarning("ManipulateObjectAttachmentProxy: TargetType not available for this type of FireArm!");
                    break;
            }
        }
        private void SetProxy(Handgun s)
        {
            switch (targetType)
            {
                case TargetType.Bolt:
                    proxy = s.Slide.transform;
                    break;
                case TargetType.Trigger:
                    proxy = s.Trigger;
                    break;
                case TargetType.MagazineRelease:
                    proxy = s.MagazineReleaseButton;
                    break;
                case TargetType.Safety:
                    if (debug && s.Safety == null) Debug.LogWarning("ManipulateObjectAttachmentProxy: Handgun.Safety == null");
                    if (debug) DebugMessage("Safety: " + s.Safety);
                    proxy = s.Safety;
                    if (debug) DebugMessage("proxy: " + proxy);
                    break;
                case TargetType.FireSelector:
                    proxy = s.FireSelector;
                    break;
                case TargetType.BoltRelease:
                    proxy = s.SlideRelease;
                    break;
                case TargetType.Hammer:
                    proxy = s.Hammer;
                    break;
                default:
                    Debug.LogWarning("ManipulateObjectAttachmentProxy: TargetType not available for this type of FireArm!");
                    break;
            }
            if (debug && proxy == null) Debug.LogWarning("ManipulateObjectAttachmentProxy: Proxy should be set but isn't!");
        }
        private void SetProxy(TubeFedShotgun s)
        {
            switch (targetType)
            {
                case TargetType.Bolt:
                    proxy = s.Bolt.transform;
                    break;
                case TargetType.Trigger:
                    proxy = s.Trigger;
                    break;
                case TargetType.Safety:
                    proxy = s.Safety;
                    break;
                case TargetType.Hammer:
                    proxy = s.Bolt.Hammer;
                    break;
                default:
                    Debug.LogWarning("ManipulateObjectAttachmentProxy: TargetType not available for this type of FireArm!");
                    break;
            }
        }
        private void SetProxy(BoltActionRifle s)
        {
            switch (targetType)
            {
                case TargetType.Bolt:
                    proxy = s.BoltHandle.transform;
                    break;
                case TargetType.Trigger:
                    proxy = s.Trigger_Display.transform;
                    break;
                case TargetType.Safety:
                    proxy = s.FireSelector_Display;
                    break;
                case TargetType.Hammer:
                    proxy = s.Hammer;
                    break;
                default:
                    Debug.LogWarning("ManipulateObjectAttachmentProxy: TargetType not available for this type of FireArm!");
                    break;
            }
        }

        private void DebugMessage(string message)
        {
            if (debug) Debug.Log("ManipulateObjectAttachmentProxy: " + message);
        }
    }
}
