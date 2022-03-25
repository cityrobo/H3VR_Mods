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
    public class ForcedBurst : MonoBehaviour
    {
        public FVRFireArm FireArm;
        public float CooldownPeriod = 0.25f;

        private bool _isHooked = false;

        private bool _isBurstFiring = false;
        private int _burstAmount = 0;
        private bool _isCoolingDown = false;
        private bool _shouldCoolDown = false;
#if !(DEBUG || MEATKIT)
        void Awake()
        {
            if (!_isHooked)
                switch (FireArm)
                {
                    case ClosedBoltWeapon w:
                        HookClosedBolt();
                        _isHooked = true;
                        break;
                    case Handgun w:
                        HookHandgun();
                        _isHooked = true;
                        break;
                    default:
                        Debug.LogWarning("ForcedBurst: Firearm type not supported!");
                        break;
                }
        }

        void OnDestroy()
        {
            if (_isHooked)
                switch (FireArm)
                {
                    case ClosedBoltWeapon w:
                        UnhookClosedBolt();
                        _isHooked = false;
                        break;
                    case Handgun w:
                        UnhookHandgun();
                        _isHooked = false;
                        break;
                    default:
                        break;
                }
        }

        // ClosedBoltWeapon Hooks and Coroutine
        void UnhookClosedBolt()
        {
            On.FistVR.ClosedBoltWeapon.DropHammer -= ClosedBoltWeapon_DropHammer;
            On.FistVR.ClosedBoltWeapon.FVRUpdate -= ClosedBoltWeapon_FVRUpdate;
        }
        void HookClosedBolt()
        {
            On.FistVR.ClosedBoltWeapon.DropHammer += ClosedBoltWeapon_DropHammer;
            On.FistVR.ClosedBoltWeapon.FVRUpdate += ClosedBoltWeapon_FVRUpdate;
        }

        private void ClosedBoltWeapon_FVRUpdate(On.FistVR.ClosedBoltWeapon.orig_FVRUpdate orig, ClosedBoltWeapon self)
        {
            orig(self);

            if (self == FireArm)
            {
                ClosedBoltWeapon.FireSelectorModeType modeType = self.FireSelector_Modes[self.m_fireSelectorMode].ModeType;
                if (_isBurstFiring && _burstAmount > 0 && (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold) && self.Bolt.CurPos == ClosedBolt.BoltPos.Forward && modeType == ClosedBoltWeapon.FireSelectorModeType.Burst)
                {
                    self.DropHammer();
                }
                else if (modeType != ClosedBoltWeapon.FireSelectorModeType.Burst || ((self.Magazine == null || !self.Magazine.HasARound()) && !self.Chamber.IsFull && !self.m_proxy.IsFull))
                {
                    _isBurstFiring = false;
                }
                else if (_shouldCoolDown && !_isCoolingDown) StartCoroutine(Cooldown());
            }
        }

        private void ClosedBoltWeapon_DropHammer(On.FistVR.ClosedBoltWeapon.orig_DropHammer orig, ClosedBoltWeapon self)
        {
            if (_shouldCoolDown || _isCoolingDown) return;

            orig(self);

            if (self == FireArm)
            {
                ClosedBoltWeapon.FireSelectorModeType modeType = self.FireSelector_Modes[self.m_fireSelectorMode].ModeType;
                if (!_isBurstFiring && modeType == ClosedBoltWeapon.FireSelectorModeType.Burst)
                {
                    _isBurstFiring = true;
                    _burstAmount = self.m_CamBurst - 1;
                }
                else if (_isBurstFiring && modeType == ClosedBoltWeapon.FireSelectorModeType.Burst && _burstAmount > 0)
                {
                    _burstAmount--;
                }

                if (_burstAmount == 0)
                {
                    _shouldCoolDown = true;
                    _isBurstFiring = false;
                }
            }
        }

        IEnumerator Cooldown()
        {
            _isCoolingDown = true;
            yield return new WaitForSeconds(CooldownPeriod);
            _isCoolingDown = false;
            _shouldCoolDown = false;
        }

        // Handgun Hooks and Coroutine
        void UnhookHandgun()
        {
            On.FistVR.Handgun.ReleaseSeer -= Handgun_ReleaseSeer;
            On.FistVR.Handgun.FVRUpdate -= Handgun_FVRUpdate;
        }
        void HookHandgun()
        {
            On.FistVR.Handgun.ReleaseSeer += Handgun_ReleaseSeer;
            On.FistVR.Handgun.FVRUpdate += Handgun_FVRUpdate;
        }

        private void Handgun_FVRUpdate(On.FistVR.Handgun.orig_FVRUpdate orig, Handgun self)
        {
            orig(self);

            if (self == FireArm)
            {
                Handgun.FireSelectorModeType modeType = self.FireSelectorModes[self.m_fireSelectorMode].ModeType;
                if (_isBurstFiring && _burstAmount > 0 && (!self.IsHeld || self.m_hand.Input.TriggerFloat < self.TriggerResetThreshold) && self.Slide.CurPos == HandgunSlide.SlidePos.Forward && modeType == Handgun.FireSelectorModeType.Burst)
                {
                    self.ReleaseSeer();
                }
                else if (modeType != Handgun.FireSelectorModeType.Burst || ((self.Magazine == null || !self.Magazine.HasARound()) && !self.Chamber.IsFull && !self.m_proxy.IsFull))
                {
                    _isBurstFiring = false;
                }
                else if (_shouldCoolDown && !_isCoolingDown) StartCoroutine(Cooldown());
            }
        }

        private void Handgun_ReleaseSeer(On.FistVR.Handgun.orig_ReleaseSeer orig, Handgun self)
        {
            if (_shouldCoolDown || _isCoolingDown) return;

            if (self == FireArm && self.m_isHammerCocked && self.m_isSeerReady)
            {
                Handgun.FireSelectorModeType modeType = self.FireSelectorModes[self.m_fireSelectorMode].ModeType;
                if (!_isBurstFiring && modeType == Handgun.FireSelectorModeType.Burst)
                {
                    _isBurstFiring = true;
                    _burstAmount = self.m_CamBurst - 1;
                }
                else if (_isBurstFiring && modeType == Handgun.FireSelectorModeType.Burst && _burstAmount > 0)
                {
                    _burstAmount--;
                }

                if (_burstAmount == 0)
                {
                    _shouldCoolDown = true;
                    _isBurstFiring = false;
                }
            }

            orig(self);
        }
#endif
    }
}
