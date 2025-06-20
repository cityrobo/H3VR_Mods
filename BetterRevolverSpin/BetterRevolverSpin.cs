using BepInEx;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using OpenScripts2;
using BepInEx.Configuration;
using static UnityEngine.UnityEngineExtensions;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.BetterRevolverSpin", "Better Revolver Spin", "1.0.0")]
    public class BetterRevolverSpin : BaseUnityPlugin
    {
        public ConfigEntry<float> SpinReleaseDelayTime;
        public ConfigEntry<bool> SpinToggle;
        public ConfigEntry<bool> SpinGrabHelper;
        public ConfigEntry<float> SpinGrabHelperScale;

        private readonly Dictionary<FVRPhysicalObject, bool> _revolverSpinToggleState = new();

#if !DEBUG
        public void Awake()
        {
            //On.FistVR.Revolver.UpdateSpinning += Revolver_UpdateSpinning;
            On.FistVR.Revolver.BeginInteraction += Revolver_BeginInteraction;
            On.FistVR.Revolver.UpdateInteraction += Revolver_UpdateInteraction;
            On.FistVR.Revolver.EndInteraction += Revolver_EndInteraction;

            On.FistVR.SingleActionRevolver.BeginInteraction += SingleActionRevolver_BeginInteraction;
            On.FistVR.SingleActionRevolver.UpdateInteraction += SingleActionRevolver_UpdateInteraction;
            On.FistVR.SingleActionRevolver.EndInteraction += SingleActionRevolver_EndInteraction;

            SpinReleaseDelayTime = Config.Bind("Better Revolver Spin", "Spin Release Delay Time", 0.25f, "Delay between letting go of the spin input and stopping to spin.");
            SpinToggle = Config.Bind("Better Revolver Spin", "Spin Toggle", false, "This option lets you toggle spinning. Great when you wanna catch the revolver while spining!");
            SpinGrabHelper = Config.Bind("Better Revolver Spin", "Spin Grab Helper", true, "If this option is true, the grab collider of the revolver will be scaled up by below value to help with grabbing after tossing the gun.");
            SpinGrabHelperScale = Config.Bind("Better Revolver Spin", "Spin Grab Helper Scale", 3f);

            On.FistVR.FVRPhysicalObject.Awake += FVRPhysicalObject_Awake;
            On.FistVR.FVRPhysicalObject.OnDestroy += FVRPhysicalObject_OnDestroy;
        }

        private void FVRPhysicalObject_Awake(On.FistVR.FVRPhysicalObject.orig_Awake orig, FVRPhysicalObject self)
        {
            orig(self);

            if (self is Revolver || self is SingleActionRevolver) _revolverSpinToggleState.Add(self, false);
        }

        private void FVRPhysicalObject_OnDestroy(On.FistVR.FVRPhysicalObject.orig_OnDestroy orig, FVRPhysicalObject self)
        {
            if (self is Revolver || self is SingleActionRevolver) _revolverSpinToggleState.Remove(self);

            orig(self);
        }

        private void SingleActionRevolver_BeginInteraction(On.FistVR.SingleActionRevolver.orig_BeginInteraction orig, SingleActionRevolver self, FVRViveHand hand)
        {
            if (SpinToggle.Value)
            {
                self.m_isSpinning = _revolverSpinToggleState[self];
            }
            else if (!SpinToggle.Value && hand.Input.TouchpadPressed && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
            {
                self.m_isSpinning = true;
                self.StopAllCoroutines();
                self.StartCoroutine(SpinReleaseDelay(self));
            }

            if (self.m_isSpinning)
            {
                Vector3 spinSpeed = self.transform.InverseTransformDirection(self.RootRigidbody.angularVelocity);

                self.xSpinVel = -spinSpeed.x * 10f;

                self.xSpinRot = Vector3Utils.SignedAngle(self.PoseOverride.forward, hand.Input.Forward, self.transform.right);
            }

            if (SpinGrabHelper.Value && self.m_isSpinning)
            {
                ScaleColliderDown(self);
            }

            orig(self, hand);
        }

        private void SingleActionRevolver_UpdateInteraction(On.FistVR.SingleActionRevolver.orig_UpdateInteraction orig, SingleActionRevolver self, FVRViveHand hand)
        {
            orig(self, hand);

            if (!self.IsAltHeld && !hand.IsInStreamlinedMode)
            {
                if (!SpinToggle.Value && hand.Input.TouchpadPressed && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
                {
                    self.m_isSpinning = true;
                    self.StopAllCoroutines();
                    self.StartCoroutine(SpinReleaseDelay(self));
                }

                if (SpinToggle.Value && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
                {
                    _revolverSpinToggleState[self] = !_revolverSpinToggleState[self];
                }

                if (SpinToggle.Value)
                {
                    self.m_isSpinning = _revolverSpinToggleState[self];
                }

                if (self.m_isSpinning) self.UseGrabPointChild = false;
                else self.UseGrabPointChild = true;
            }
        }

        private void SingleActionRevolver_EndInteraction(On.FistVR.SingleActionRevolver.orig_EndInteraction orig, SingleActionRevolver self, FVRViveHand hand)
        {
            Action<FVRViveHand> baseMethod = OpenScripts2_BasePlugin.GetBaseAction<FVRViveHand>(typeof(FVRFireArm), nameof(FVRFireArm.EndInteraction), self);
            baseMethod(hand);

            self.RootRigidbody.AddRelativeTorque(new Vector3(-self.xSpinVel / 10f, 0f, 0f), ForceMode.VelocityChange);

            if (SpinGrabHelper.Value && self.m_isSpinning)
            {
                ScaleColliderUp(self);
            }
        }

        private void Revolver_BeginInteraction(On.FistVR.Revolver.orig_BeginInteraction orig, Revolver self, FVRViveHand hand)
        {
            if (SpinToggle.Value)
            {
                self.m_isSpinning = _revolverSpinToggleState[self];
            }
            else if (!SpinToggle.Value && hand.Input.TouchpadPressed && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
            {
                self.m_isSpinning = true;
                self.StopAllCoroutines();
                self.StartCoroutine(SpinReleaseDelay(self));
            }

            if (self.m_isSpinning)
            {
                Vector3 spinSpeed = self.transform.InverseTransformDirection(self.RootRigidbody.angularVelocity);

                self.xSpinVel = -spinSpeed.x * 10f;

                self.xSpinRot = Vector3Utils.SignedAngle(self.PoseOverride.forward, hand.Input.Forward, self.transform.right);
            }

            if (SpinGrabHelper.Value && self.m_isSpinning)
            {
                ScaleColliderDown(self);
            }

            orig(self, hand);
        }

        private void Revolver_UpdateInteraction(On.FistVR.Revolver.orig_UpdateInteraction orig, Revolver self, FVRViveHand hand)
        {
            orig(self, hand);

            if (!self.IsAltHeld && !hand.IsInStreamlinedMode)
            {
                if (!SpinToggle.Value && hand.Input.TouchpadPressed && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
                {
                    self.m_isSpinning = true;
                    self.StopAllCoroutines();
                    self.StartCoroutine(SpinReleaseDelay(self));
                }

                if (SpinToggle.Value && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
                {
                    _revolverSpinToggleState[self] = !_revolverSpinToggleState[self];
                }

                if (SpinToggle.Value)
                {
                    self.m_isSpinning = _revolverSpinToggleState[self];
                }

                if (self.m_isSpinning) self.UseGrabPointChild = false;
                else self.UseGrabPointChild = true;
            }
        }

        private void Revolver_EndInteraction(On.FistVR.Revolver.orig_EndInteraction orig, Revolver self, FVRViveHand hand)
        {
            Action<FVRViveHand> baseMethod = OpenScripts2_BasePlugin.GetBaseAction<FVRViveHand>(typeof(FVRFireArm), nameof(FVRFireArm.EndInteraction), self);
            baseMethod(hand);

            self.RootRigidbody.AddRelativeTorque(new Vector3(-self.xSpinVel / 10f, 0f, 0f), ForceMode.VelocityChange);

            if (SpinGrabHelper.Value && self.m_isSpinning)
            {
                ScaleColliderUp(self);
            }
        }

        private void ScaleColliderUp(FVRPhysicalObject self)
        {
            float colliderScale = SpinGrabHelperScale.Value;
            Collider collider = self.GetComponent<Collider>();
            collider.Analyze(out Vector3 _, out Vector3 size, out var type, out var _);

            size *= colliderScale;
            switch (type)
            {
                case EColliderType.Sphere:
                    (collider as SphereCollider).radius = size.x;
                    break;
                case EColliderType.Capsule:
                    (collider as CapsuleCollider).radius = size.x;
                    (collider as CapsuleCollider).height = size.y;
                    break;
                case EColliderType.Box:
                    (collider as BoxCollider).size = size;
                    break;
            }
        }

        private void ScaleColliderDown(FVRPhysicalObject self)
        {
            float colliderScale = SpinGrabHelperScale.Value;
            Collider collider = self.GetComponent<Collider>();
            collider.Analyze(out Vector3 _, out Vector3 size, out var type, out var _);

            size /= colliderScale;
            switch (type)
            {
                case EColliderType.Sphere:
                    (collider as SphereCollider).radius = size.x;
                    break;
                case EColliderType.Capsule:
                    (collider as CapsuleCollider).radius = size.x;
                    (collider as CapsuleCollider).height = size.y;
                    break;
                case EColliderType.Box:
                    (collider as BoxCollider).size = size;
                    break;
            }
        }

        private IEnumerator SpinReleaseDelay(Revolver revolver)
        {
            for (float i = 0; i < SpinReleaseDelayTime.Value; i+=Time.deltaTime)
            {
                revolver.m_isSpinning = true;
                yield return null;
            }
        }

        private IEnumerator SpinReleaseDelay(SingleActionRevolver revolver)
        {
            for (float i = 0; i < SpinReleaseDelayTime.Value; i += Time.deltaTime)
            {
                revolver.m_isSpinning = true;
                yield return null;
            }
        }
#endif
    }
}
