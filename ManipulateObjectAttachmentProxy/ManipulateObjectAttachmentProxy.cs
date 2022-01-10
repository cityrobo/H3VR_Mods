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


        private FVRPhysicalObject weapon;
        private Transform proxy;

        private bool proxySet = false;
        
        public void Start()
        {
        }
#if !(UNITY_EDITOR || UNITY_5)

        public void Update()
        {
            if (attachment.curMount != null)
            {
                weapon = attachment.curMount.GetRootMount().MyObject;
                if (!proxySet)
                {
                    switch (weapon)
                    {
                        case OpenBoltReceiver s:
                            SetProxy(s);
                            break;
                        case ClosedBoltWeapon s:
                            SetProxy(s);
                            break;
                        case Handgun s:
                            SetProxy(s);
                            break;
                        case TubeFedShotgun s:
                            SetProxy(s);
                            break;
                        case BoltActionRifle s:
                            SetProxy(s);
                            break;
                        default:
                            break;
                    }
                    proxySet = true;
                }
                this.transform.localPosition = proxy.localPosition;
                this.transform.localRotation = proxy.localRotation;
                this.transform.localScale = proxy.localScale;
            }
            else proxySet = false;
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
                    proxy = s.Safety;
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
    }
}
