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
    public class HK_P7_SqueezeCocker : MonoBehaviour
    {
        public Handgun P7;
        public Transform CockingLever;
        public enum Axis
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        public float CockingLeverLowerLimit;
        public float CockingLeverUpperLimit;
        public Axis CockingLeverAxis;
        public Transform Striker;
        public float StrikerLowerLimit;
        public float StrikerMiddleLimit;
        public float StrikerUpperLimit;

        private float _strikerSlideForwardThreshold;
        public Axis StrikerAxis;

        private bool _wasManuallyCocked = false;
        public void Start()
        {
            CockingLever.localRotation = Quaternion.Euler(CockingLeverLowerLimit * GetDir(CockingLeverAxis));
            MoveStriker(StrikerLowerLimit);

            _strikerSlideForwardThreshold = Mathf.InverseLerp(P7.Slide.m_slideZ_lock, P7.Slide.m_slideZ_forward, P7.Slide.m_slideZ_forward - (Math.Abs(StrikerMiddleLimit - StrikerLowerLimit)));
        }

        public void Update()
        {
            if (P7.m_hand != null)
            {
                if (P7.m_hand.CMode == ControlMode.Index) IndexMode(P7.m_hand);
                else NonIndexMode(P7.m_hand);
            }
        }


        void IndexMode(FVRViveHand hand)
        {
            if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f)
            {
                P7.EngageSlideRelease();
            }

            if (hand.Input.GripPressed)
            {
                if (!_wasManuallyCocked)
                {
                    P7.CockHammer(false);
                    _wasManuallyCocked = true;

                    if (hand.Input.TriggerFloat > P7.TriggerBreakThreshold && P7.Magazine != null) P7.DropHammer(false);

                    P7.DropSlideRelease();
                }

                CockingLever.localRotation = Quaternion.Euler(CockingLeverUpperLimit * GetDir(CockingLeverAxis));
            }
            else
            { 
                P7.DeCockHammer(false, false);
                _wasManuallyCocked = false;

                CockingLever.localRotation = Quaternion.Euler(CockingLeverLowerLimit * GetDir(CockingLeverAxis));
            }

            if (P7.m_isHammerCocked)
            {
                float lerp = P7.Slide.GetSlideLerpBetweenLockAndFore();

                if (lerp > _strikerSlideForwardThreshold)
                {
                    float inverseLerp = Mathf.InverseLerp(_strikerSlideForwardThreshold, 1f, lerp);
                    float pos = Mathf.Lerp(StrikerLowerLimit, StrikerMiddleLimit, inverseLerp);
                    if (hand.Input.TriggerFloat > 0f && P7.HasTriggerReset)
                    {
                        float triggerLerp = Mathf.InverseLerp(0f, P7.TriggerBreakThreshold, hand.Input.TriggerFloat);

                        float additionalPos = Mathf.Lerp(0f, StrikerUpperLimit - StrikerMiddleLimit, triggerLerp);
                        pos += additionalPos;
                    }
                    MoveStriker(pos);
                }
                else MoveStriker(StrikerLowerLimit);
            }
            else
            {
                MoveStriker(StrikerLowerLimit);
            }
        }

        void NonIndexMode(FVRViveHand hand)
        {
            if (!hand.IsInStreamlinedMode)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f)
                {
                    P7.EngageSlideRelease();
                }

                if (hand.Input.TouchpadPressed && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f)
                {
                    if (!_wasManuallyCocked)
                    {
                        P7.CockHammer(false);
                        _wasManuallyCocked = true;

                        if (hand.Input.TriggerFloat > P7.TriggerBreakThreshold && P7.Magazine != null) P7.DropHammer(false);

                        P7.DropSlideRelease();
                    }

                    CockingLever.localRotation = Quaternion.Euler(CockingLeverUpperLimit * GetDir(CockingLeverAxis));
                }
                else
                {
                    P7.DeCockHammer(false, false);
                    _wasManuallyCocked = false;

                    CockingLever.localRotation = Quaternion.Euler(CockingLeverLowerLimit * GetDir(CockingLeverAxis));
                }

                if (P7.m_isHammerCocked)
                {
                    float lerp = P7.Slide.GetSlideLerpBetweenLockAndFore();

                    if (lerp > _strikerSlideForwardThreshold)
                    {
                        float inverseLerp = Mathf.InverseLerp(_strikerSlideForwardThreshold, 1f, lerp);
                        float pos = Mathf.Lerp(StrikerLowerLimit, StrikerMiddleLimit, inverseLerp);
                        if (hand.Input.TriggerFloat > 0f && P7.HasTriggerReset)
                        {
                            float triggerLerp = Mathf.InverseLerp(0f, P7.TriggerBreakThreshold, hand.Input.TriggerFloat);

                            float additionalPos = Mathf.Lerp(0f, StrikerUpperLimit - StrikerMiddleLimit, triggerLerp);
                            pos += additionalPos;
                        }
                        MoveStriker(pos);
                    }
                    else MoveStriker(StrikerLowerLimit);
                }
                else
                {
                    MoveStriker(StrikerLowerLimit);
                }
            }
            else
            {
                if (hand.Input.BYButtonPressed)
                {
                    if (!_wasManuallyCocked)
                    {
                        P7.CockHammer(false);
                        _wasManuallyCocked = true;

                        if (hand.Input.TriggerFloat > P7.TriggerBreakThreshold && P7.Magazine != null) P7.DropHammer(false);

                        P7.DropSlideRelease();
                    }

                    CockingLever.localRotation = Quaternion.Euler(CockingLeverUpperLimit * GetDir(CockingLeverAxis));
                }
                else
                {
                    P7.DeCockHammer(false, false);
                    _wasManuallyCocked = false;

                    CockingLever.localRotation = Quaternion.Euler(CockingLeverLowerLimit * GetDir(CockingLeverAxis));
                }

                if (P7.m_isHammerCocked)
                {
                    float lerp = P7.Slide.GetSlideLerpBetweenLockAndFore();

                    if (lerp > _strikerSlideForwardThreshold)
                    {
                        float inverseLerp = Mathf.InverseLerp(_strikerSlideForwardThreshold, 1f, lerp);
                        float pos = Mathf.Lerp(StrikerLowerLimit, StrikerMiddleLimit, inverseLerp);
                        if (hand.Input.TriggerFloat > 0f && P7.HasTriggerReset)
                        {
                            float triggerLerp = Mathf.InverseLerp(0f, P7.TriggerBreakThreshold, hand.Input.TriggerFloat);

                            float additionalPos = Mathf.Lerp(0f, StrikerUpperLimit - StrikerMiddleLimit, triggerLerp);
                            pos += additionalPos;
                        }
                        MoveStriker(pos);
                    }
                    else MoveStriker(StrikerLowerLimit);
                }
                else
                {
                    MoveStriker(StrikerLowerLimit);
                }
            }
        }


        Vector3 GetDir(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return Vector3.right;
                case Axis.Y:
                    return Vector3.up;
                case Axis.Z:
                    return Vector3.forward;
                default:
                    return Vector3.zero;
            }
        }

        void MoveStriker(float value)
        {
            Vector3 pos = Striker.localPosition;
            Vector3 newPos = Vector3.one * value;

            pos[(int)StrikerAxis] = newPos[(int)StrikerAxis];
            Striker.localPosition = pos;
        }
#if !(DEBUG || MEATKIT)

#endif
    }
}
