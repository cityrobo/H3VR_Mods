using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using BepInEx.Configuration;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using HarmonyLib;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.SlowMotionMode", "Slow Motion Mode", "1.1.0")]
    public class SlowMotionMode : BaseUnityPlugin
    {
        // Config Options:
        public static ConfigEntry<float> SlowMotionTime;
        public static ConfigEntry<float> SlowMotionRechargeTime;
        public static ConfigEntry<float> SlowDownFadeTime;
        public static ConfigEntry<float> MinimumTimeScale;
        public static ConfigEntry<bool> ExternallyTriggered;
        public static ConfigEntry<EHand> Hand;
        public static ConfigEntry<EButtonInput> ButtonInput;
        public static ConfigEntry<bool> AffectMovementSpeed;
        public static ConfigEntry<bool> AffectInteractionSpeed;

        public static SlowMotionMode Instance;
        public float SlowMotionCharge = 1f;

        private static readonly float s_fixedDeltaTime = -1f;
        private float _currentTimeScale = 1f;
        private float _deltaTime = 0f;

        private const float INTENSITY_EXPONENT = 1.1f;
        private const float INTERP_EXPONENT = 2.5f;

        public enum EHand
        {
            Left,
            Right,
            OffHand
        }

        public enum EButtonInput
        {
            BYButton,
            AXButton,
            Trigger,
            Touchpad,
            TouchpadCenter,
            TouchpadUp,
            TouchpadDown,
            TouchpadLeft,
            TouchpadRight,
            IndexAnalogStickClick
        }

        //private Dictionary<FVRViveHand, HeldObject> _lastHeldObjects = new();

        private Dictionary<FVRPhysicalObject,HeldObject> _physicalObjectsInScene = new();

        private FVRSceneSettings _currentSceneSettings;
        //private Vector3 worldTPAxis;
        
        private enum ESlowDownState
        {
            normal,
            slow,
            slowingDown,
            speedingUp
        }

        private ESlowDownState _state = ESlowDownState.normal;
        static SlowMotionMode()
        {
            s_fixedDeltaTime = Time.fixedDeltaTime;
        }

        public SlowMotionMode()
        {
            SlowMotionTime = Config.Bind("Slow Motion Mode", "Slow Motion Time", 10f, "How long can slow motion stay active in seconds (0 or negative number means infinite)?");
            SlowMotionRechargeTime = Config.Bind("Slow Motion Mode", "Slow Motion Recharge Time", 5f, "How long how long does it take to fully recharge the slow motion ability?");
            SlowDownFadeTime = Config.Bind("Slow Motion Mode", "Slow Down Delay", 1f, "How quickly is the slow motion effect applied?");
            MinimumTimeScale = Config.Bind("Slow Motion Mode", "Minimum Time Scale", 0.25f, "How small can the time scale become?");
            Hand = Config.Bind("Slow Motion Mode", "Slow Motion Hand", EHand.Right, "Input which hand should toggle slow motion? Or should the slow motion be toggled with a hand that doesn't hold an item?");
            ButtonInput = Config.Bind("Slow Motion Mode", "Slow Motion Button", EButtonInput.BYButton, "Which Button should activate slow motion?");
            AffectMovementSpeed = Config.Bind("Slow Motion Mode", "Affect Movement Speed", true, "If true, movement speed will be increased to counter slow motion effect.");
            AffectInteractionSpeed = Config.Bind("Slow Motion Mode", "Affect Interaction Speed", true, "If true, object interaction speed will be increased to counter slow motion effect.");
            ExternallyTriggered = Config.Bind("Slow Motion Mode", "Externally Triggered", false, "If true, disables global activation command and only reacts to other external triggers.");

            Instance = this;
        }

        public void Awake()
        {
            if (AffectMovementSpeed.Value)
            {
                IL.FistVR.FVRMovementManager.UpdateSmoothLocomotion += FVRMovementManager_UpdateSmoothLocomotion;
                IL.FistVR.FVRMovementManager.HandUpdateTwinstick += FVRMovementManager_HandUpdateTwinstick;
            }
            if (AffectInteractionSpeed.Value)
            {
                On.FistVR.FVRPhysicalObject.Awake += FVRPhysicalObject_Awake;
                On.FistVR.FVRPhysicalObject.OnDestroy += FVRPhysicalObject_OnDestroy;
                On.FistVR.FVRPhysicalObject.FVRUpdate += FVRPhysicalObject_FVRUpdate;
            }
            AffectMovementSpeed.SettingChanged += AffectMovementSpeed_SettingChanged;
            AffectInteractionSpeed.SettingChanged += AffectInteractionSpeed_SettingChanged;

            Harmony.CreateAndPatchAll(typeof(SlowMotionMode));
        }

        private void AffectMovementSpeed_SettingChanged(object sender, EventArgs e)
        {
            if (AffectMovementSpeed.Value)
            {
                IL.FistVR.FVRMovementManager.UpdateSmoothLocomotion -= FVRMovementManager_UpdateSmoothLocomotion;
                IL.FistVR.FVRMovementManager.HandUpdateTwinstick -= FVRMovementManager_HandUpdateTwinstick;
                IL.FistVR.FVRMovementManager.UpdateSmoothLocomotion += FVRMovementManager_UpdateSmoothLocomotion;
                IL.FistVR.FVRMovementManager.HandUpdateTwinstick += FVRMovementManager_HandUpdateTwinstick;
            }
            else
            {
                IL.FistVR.FVRMovementManager.UpdateSmoothLocomotion -= FVRMovementManager_UpdateSmoothLocomotion;
                IL.FistVR.FVRMovementManager.HandUpdateTwinstick -= FVRMovementManager_HandUpdateTwinstick;
            }
        }

        private void AffectInteractionSpeed_SettingChanged(object sender, EventArgs e)
        {
            if (AffectInteractionSpeed.Value)
            {
                On.FistVR.FVRPhysicalObject.Awake -= FVRPhysicalObject_Awake;
                On.FistVR.FVRPhysicalObject.OnDestroy -= FVRPhysicalObject_OnDestroy;
                On.FistVR.FVRPhysicalObject.FVRUpdate -= FVRPhysicalObject_FVRUpdate;
                On.FistVR.FVRPhysicalObject.Awake += FVRPhysicalObject_Awake;
                On.FistVR.FVRPhysicalObject.OnDestroy += FVRPhysicalObject_OnDestroy;
                On.FistVR.FVRPhysicalObject.FVRUpdate += FVRPhysicalObject_FVRUpdate;
            }
            else
            {
                On.FistVR.FVRPhysicalObject.Awake -= FVRPhysicalObject_Awake;
                On.FistVR.FVRPhysicalObject.OnDestroy -= FVRPhysicalObject_OnDestroy;
                On.FistVR.FVRPhysicalObject.FVRUpdate -= FVRPhysicalObject_FVRUpdate;
            }
        }

        public void OnDestroy()
        {
            IL.FistVR.FVRMovementManager.UpdateSmoothLocomotion -= FVRMovementManager_UpdateSmoothLocomotion;
            IL.FistVR.FVRMovementManager.HandUpdateTwinstick -= FVRMovementManager_HandUpdateTwinstick;
            On.FistVR.FVRPhysicalObject.Awake -= FVRPhysicalObject_Awake;
            On.FistVR.FVRPhysicalObject.OnDestroy -= FVRPhysicalObject_OnDestroy;
            On.FistVR.FVRPhysicalObject.FVRUpdate -= FVRPhysicalObject_FVRUpdate;

            AffectMovementSpeed.SettingChanged -= AffectMovementSpeed_SettingChanged;
            AffectInteractionSpeed.SettingChanged -= AffectInteractionSpeed_SettingChanged;

            if (_currentSceneSettings != null) _currentSceneSettings.PlayerDeathEvent -= DisableSlowMotion;
        }

        private void FVRPhysicalObject_Awake(On.FistVR.FVRPhysicalObject.orig_Awake orig, FVRPhysicalObject self)
        {
            orig(self);
            _physicalObjectsInScene.Add(self, new HeldObject(self, self.PositionInterpSpeed, self.RotationInterpSpeed, self.MoveIntensity, self.RotIntensity));

        }
        private void FVRPhysicalObject_OnDestroy(On.FistVR.FVRPhysicalObject.orig_OnDestroy orig, FVRPhysicalObject self)
        {
            if (_physicalObjectsInScene.ContainsKey(self))
            {
                _physicalObjectsInScene.Remove(self);
            }

            orig(self);
        }

        private void FVRPhysicalObject_FVRUpdate(On.FistVR.FVRPhysicalObject.orig_FVRUpdate orig, FVRPhysicalObject self)
        {
            orig(self);

            HeldObject heldObject;
            if (_currentTimeScale < 1f)
            {
                if (_physicalObjectsInScene.TryGetValue(self, out heldObject))
                {
                    self.PositionInterpSpeed = heldObject.PositionInterpSpeed * Mathf.Pow(1f / _currentTimeScale, INTERP_EXPONENT);
                    self.RotationInterpSpeed = heldObject.RotationInterpSpeed * Mathf.Pow(1f / _currentTimeScale, INTERP_EXPONENT);

                    self.MoveIntensity = heldObject.MoveIntensity * Mathf.Pow(1f / _currentTimeScale, INTENSITY_EXPONENT);
                    self.RotIntensity = heldObject.RotIntensity * Mathf.Pow(1f / _currentTimeScale, INTENSITY_EXPONENT);
                }
            }
            else
            {
                if (_physicalObjectsInScene.TryGetValue(self, out heldObject))
                {
                    self.PositionInterpSpeed = heldObject.PositionInterpSpeed;
                    self.RotationInterpSpeed = heldObject.RotationInterpSpeed;

                    self.MoveIntensity = heldObject.MoveIntensity;
                    self.RotIntensity = heldObject.RotIntensity;
                }
            }
        }

        private void FVRMovementManager_HandUpdateTwinstick(ILContext il)
        {
            ILCursor c = new(il);
            c.GotoNext(
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<FVRMovementManager>(nameof(FVRMovementManager.worldTPAxis)),
                i => i.MatchCall<GM>("get_Options"),
                i => i.MatchLdfld<GameOptions>(nameof(GameOptions.MovementOptions)),
                i => i.MatchLdfld<MovementOptions>(nameof(MovementOptions.TPLocoSpeeds)),
                i => i.MatchCall<GM>("get_Options"),
                i => i.MatchLdfld<GameOptions>(nameof(GameOptions.MovementOptions)),
                i => i.MatchLdfld<MovementOptions>(nameof(MovementOptions.TPLocoSpeedIndex)),
                i => i.MatchLdelemR4()
            );
            c.Emit(OpCodes.Ldc_R4, 1f);
            c.Emit<SlowMotionMode>(OpCodes.Ldsfld, nameof(SlowMotionMode.Instance));
            c.Emit<SlowMotionMode>(OpCodes.Ldfld, nameof(_currentTimeScale));
            c.Emit(OpCodes.Div);
            c.Emit(OpCodes.Mul);
        }
        private void FVRMovementManager_UpdateSmoothLocomotion(ILContext il)
        {
            ILCursor c = new(il);
            c.GotoNext(
                MoveType.After,
                i => i.MatchLdarg(0),
                i => i.MatchLdfld<FVRMovementManager>(nameof(FVRMovementManager.m_curArmSwingerImpetus)),
                i => i.MatchCall<Vector3>("op_Multiply"),
                i => i.MatchLdcR4(1.5f)
            );
            c.Emit(OpCodes.Ldc_R4, 1f);
            c.Emit<SlowMotionMode>(OpCodes.Ldsfld, nameof(SlowMotionMode.Instance));
            c.Emit<SlowMotionMode>(OpCodes.Ldfld, nameof(_currentTimeScale));
            c.Emit(OpCodes.Div);
            c.Emit(OpCodes.Mul);

            c.GotoNext(
                MoveType.After,
                i => i.MatchLdfld<MovementOptions>(nameof(MovementOptions.SmoothTurnMagnitudeIndex)),
                i => i.MatchLdelemR4(),
                i => i.MatchCall<Time>("get_deltaTime"),
                i => i.MatchMul()
            );
            c.Emit(OpCodes.Ldc_R4, 1f);
            c.Emit<SlowMotionMode>(OpCodes.Ldsfld, nameof(SlowMotionMode.Instance));
            c.Emit<SlowMotionMode>(OpCodes.Ldfld, nameof(_currentTimeScale));
            c.Emit(OpCodes.Div);
            c.Emit(OpCodes.Mul);
        }

        public void Update()
        {
            // Vector3 a = new();
            // a = a * GM.CurrentMovementManager.m_curArmSwingerImpetus * 1.5f * (1f / SlowMotionMode.Instance._currentTimeScale);

            if (GM.CurrentSceneSettings != null && GM.CurrentSceneSettings != _currentSceneSettings)
            {
                _currentSceneSettings = GM.CurrentSceneSettings;
                GM.CurrentSceneSettings.PlayerDeathEvent -= DisableSlowMotion;
                GM.CurrentSceneSettings.PlayerDeathEvent += DisableSlowMotion;
            }

            if (!ExternallyTriggered.Value && Hand.Value != EHand.OffHand)
            {
                FVRViveHand hand = Hand.Value == EHand.Left ? GetLeftHand() : GetRightHand();
                if (hand != null && CorrectButtonPressed(hand))
                {
                    ToggleSlowMotionInternal();
                }
            }
            else if (!ExternallyTriggered.Value && Hand.Value == EHand.OffHand)
            {
                FVRViveHand[] FVRViveHands = GM.CurrentMovementManager.Hands;
                foreach (var hand in FVRViveHands)
                {
                    if (hand.CurrentInteractable != null && hand.CurrentInteractable is FVRPhysicalObject && (hand.OtherHand.CurrentInteractable == null || !(hand.OtherHand.CurrentInteractable is FVRPhysicalObject)) && hand.OtherHand.CurrentPointable == null && hand.OtherHand.m_grabityHoveredObject == null && hand.OtherHand.m_selectedObj == null && CorrectButtonPressed(hand.OtherHand))
                    {
                        ToggleSlowMotionInternal();
                        break;
                    }
                    else if (hand.CurrentInteractable == null && hand.OtherHand.CurrentInteractable == null && hand.CurrentPointable == null && hand.m_grabityHoveredObject == null && hand.m_selectedObj == null && CorrectButtonPressed(hand))
                    {
                        ToggleSlowMotionInternal();
                        break;
                    }
                }
            }

            //if (true && _currentTimeScale < 1f)
            //{
            //    foreach (var qbSlot in GM.CurrentPlayerBody.QuickbeltSlots)
            //    {
            //        if (qbSlot.CurObject != null)
            //        {
            //            qbSlot.CurObject.transform.position = qbSlot.PoseOverride.position;
            //            qbSlot.CurObject.transform.rotation = qbSlot.PoseOverride.rotation;
            //        }
            //    }
            //}

            // Charge Mechanic
            if (SlowMotionTime.Value > 0f && (_state == ESlowDownState.slow || _state == ESlowDownState.slowingDown))
            {
                SlowMotionCharge -= (Time.deltaTime * (1f / _currentTimeScale)) / SlowMotionTime.Value;
                SlowMotionCharge = Mathf.Clamp01(SlowMotionCharge);
            }
            else if (SlowMotionTime.Value > 0f && SlowMotionCharge != 1f)
            {
                SlowMotionCharge += (Time.deltaTime * (1f / _currentTimeScale)) / SlowMotionRechargeTime.Value;
                SlowMotionCharge = Mathf.Clamp01(SlowMotionCharge);
            }
            if (SlowMotionCharge <= 0f)
            {
                DisableSlowMotion();
            }

            //Logger.LogInfo(SlowMotionCharge);
        }
        
        public static void ToggleSlowMotion()
        {
            if (ExternallyTriggered.Value && Instance != null)
            {
                Instance.ToggleSlowMotionInternal();
            }
        }

        private bool CorrectButtonPressed(FVRViveHand hand)
        {
            return ButtonInput.Value switch
            {
                EButtonInput.BYButton => hand.Input.BYButtonDown,
                EButtonInput.AXButton => hand.Input.AXButtonDown,
                EButtonInput.Trigger => hand.Input.TriggerDown,
                EButtonInput.Touchpad => hand.Input.TouchpadDown,
                EButtonInput.TouchpadCenter => hand.Input.TouchpadDown && hand.Input.TouchpadAxes.magnitude < 0.5f,
                EButtonInput.TouchpadUp => hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f,
                EButtonInput.TouchpadDown => hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.down) < 45f,
                EButtonInput.TouchpadLeft => hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f,
                EButtonInput.TouchpadRight => hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f,
                EButtonInput.IndexAnalogStickClick => hand.Input.Secondary2AxisInputDown,
                _ => false,
            };
        }

        private void ToggleSlowMotionInternal()
        {
            switch (_state)
            {
                case ESlowDownState.normal:
                    _deltaTime = 0f;
                    StartCoroutine(SlowDownTime());
                    break;
                case ESlowDownState.slow:
                    _deltaTime = SlowDownFadeTime.Value;
                    StartCoroutine(SpeedUpTime());
                    break;
                case ESlowDownState.slowingDown:
                    StopAllCoroutines();
                    StartCoroutine(SpeedUpTime());
                    break;
                case ESlowDownState.speedingUp:
                    StopAllCoroutines();
                    StartCoroutine(SlowDownTime());
                    break;
            }
        }

        private void DisableSlowMotion(bool killedSelf = false)
        {
            switch (_state)
            {
                case ESlowDownState.slow:
                    _deltaTime = SlowDownFadeTime.Value;
                    StartCoroutine(SpeedUpTime());
                    break;
                case ESlowDownState.slowingDown:
                    StopAllCoroutines();
                    StartCoroutine(SpeedUpTime());
                    break;
            }
        }
        
        private IEnumerator SlowDownTime()
        {
            _state = ESlowDownState.slowingDown;
            float inverseLerp;
            while (_deltaTime < SlowDownFadeTime.Value)
            {
                inverseLerp = Mathf.InverseLerp(0f, SlowDownFadeTime.Value, _deltaTime);
                ChangeTimeScale(Mathf.Lerp(1f, MinimumTimeScale.Value, inverseLerp));
                yield return null;
                _deltaTime += Time.deltaTime * (1f / _currentTimeScale);
            }
            ChangeTimeScale(MinimumTimeScale.Value);
            _state = ESlowDownState.slow;
        }

        private IEnumerator SpeedUpTime()
        {
            _state = ESlowDownState.speedingUp;
            float inverseLerp;
            while (_deltaTime > 0f)
            {
                inverseLerp = Mathf.InverseLerp(0f, SlowDownFadeTime.Value, _deltaTime);
                ChangeTimeScale(Mathf.Lerp(1f, MinimumTimeScale.Value,  inverseLerp));
                yield return null;
                _deltaTime -= Time.deltaTime * (1f / _currentTimeScale);
            }
            ChangeTimeScale(1f);
            _state = ESlowDownState.normal;
        }

        private void ChangeTimeScale(float scale)
        {
            Time.timeScale = Mathf.Clamp(scale, MinimumTimeScale.Value, 1.0f);
            Time.fixedDeltaTime = Time.timeScale * s_fixedDeltaTime;

            _currentTimeScale = scale;
        }
        private FVRViveHand GetLeftHand()
        {
            FVRViveHand[] FVRViveHands = GM.CurrentMovementManager.Hands;
            if (FVRViveHands[0].IsThisTheRightHand) return FVRViveHands[1];
            else return FVRViveHands[0];
        }

        private FVRViveHand GetRightHand()
        {
            FVRViveHand[] FVRViveHands = GM.CurrentMovementManager.Hands;
            if (!FVRViveHands[0].IsThisTheRightHand) return FVRViveHands[1];
            else return FVRViveHands[0];
        }

        [HarmonyPatch(typeof(AudioSource), "pitch", (MethodType)2)]
        [HarmonyPrefix]
        public static void FixPitch(ref float value)
        {
            if (Instance._currentTimeScale != 1f)
            {
                value *= Mathf.Pow(Instance._currentTimeScale, 0.25f);
            }
            else
            {
                value *= 1f;
            }
        }

        private class HeldObject
        {
            public readonly FVRInteractiveObject InteractiveObject;
            public readonly float PositionInterpSpeed;
            public readonly float RotationInterpSpeed;
            public readonly float MoveIntensity;
            public readonly float RotIntensity;

            public HeldObject(FVRInteractiveObject i, float p, float r)
            {
                InteractiveObject = i;
                PositionInterpSpeed = p;
                RotationInterpSpeed = r;
                MoveIntensity = 1f;
                RotIntensity = 1f;
            }

            public HeldObject(FVRInteractiveObject i, float p, float r, float mi, float ri)
            {
                InteractiveObject = i;
                PositionInterpSpeed = p;
                RotationInterpSpeed = r;
                MoveIntensity = mi;
                RotIntensity = ri;
            }
        }
    }
}
