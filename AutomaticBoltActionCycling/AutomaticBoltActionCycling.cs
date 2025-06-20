using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using FistVR;
using BepInEx;
using BepInEx.Configuration;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.AutomaticBoltActionCycling", "Automatic BoltAction Cycling", "0.0.1")]
    public class AutomaticBoltActionCycling : BaseUnityPlugin
    {
        public static ConfigEntry<float> CyclingSpeedConfig;
        public static ConfigEntry<bool> AutomaticReloadingConfig;
        public static ConfigEntry<bool> AutomaticChamberingConfig;
        public static ConfigEntry<bool> FullAutoModeConfig;

        private static readonly List<BoltActionRifle> _cyclingRifles = new();

        public void Awake()
        {
            CyclingSpeedConfig = Config.Bind("Automatic BoltAction Cycling", "Cycling Speed", 0.5f, "The speed at which the bolt cycles automatically, in seconds.");
            AutomaticReloadingConfig = Config.Bind("Automatic BoltAction Cycling", "Automatic Reloading", false, "Whether a clip should be automatically emptied into the magazine.");
            AutomaticChamberingConfig = Config.Bind("Automatic BoltAction Cycling", "Automatic Chambering", false, "Whether a round should be automatically chambered after a full magazine was detected on an open bolt.");
            FullAutoModeConfig = Config.Bind("Automatic BoltAction Cycling", "Full Auto Mode", false, "Whether bolt action firearms should fire in full auto mode.");
        }

#if !DEBUG
        static AutomaticBoltActionCycling()
        {
            On.FistVR.BoltActionRifle.Fire += BoltActionRifle_Fire;
            On.FistVR.BoltActionRifle.DropHammer += BoltActionRifle_DropHammer;
            On.FistVR.BoltActionRifle.UpdateInteraction += BoltActionRifle_UpdateInteraction;
        }

        private static void BoltActionRifle_UpdateInteraction(On.FistVR.BoltActionRifle.orig_UpdateInteraction orig, BoltActionRifle self, FVRViveHand hand)
        {
            orig(self, hand);

            if (AutomaticReloadingConfig.Value && self.Clip != null && self.Magazine != null && self.Clip.HasARound() && !self.Magazine.IsFull())
            {
                self.Clip.LoadOneRoundFromClipToMag();
            }
            
            if (AutomaticChamberingConfig.Value && self.Magazine != null && self.Magazine.IsFull() && Mathf.Approximately(self.BoltHandle.fakeBoltDrive, 1f))
            {
                self.StartCoroutine(AutomaticChambering(self));
            }

            //if (FullAutoModeConfig.Value && !_cyclingRifles.Contains(self) && Mathf.Approximately(self.BoltHandle.fakeBoltDrive, 0f))
            //{
            //    self.DropHammer();
            //}
        }

        private static void BoltActionRifle_DropHammer(On.FistVR.BoltActionRifle.orig_DropHammer orig, BoltActionRifle self)
        {
            if (!_cyclingRifles.Contains(self))
            {
                orig(self);
            }
        }

        private static bool BoltActionRifle_Fire(On.FistVR.BoltActionRifle.orig_Fire orig, BoltActionRifle self)
        {
            bool hasFired = orig(self);
            if (hasFired)
            {
                self.StartCoroutine(AutomaticCycling(self));
                self.m_hasTriggerCycled = FullAutoModeConfig.Value;
            }
            return hasFired;
        }
#endif
        private static IEnumerator AutomaticCycling(BoltActionRifle self)
        {
            _cyclingRifles.Add(self);
            float currentLerp = 0f;
            self.BoltHandle.fakeBoltDrive = 0f;
            while (currentLerp <= 1f)
            {
                if (!self.IsHeld || self.BoltHandle.IsHeld)
                {
                    _cyclingRifles.Remove(self);
                    yield break;
                }

                float amount =  Time.deltaTime / (CyclingSpeedConfig.Value / 2f);
                self.BoltHandle.DriveBolt(amount);
                currentLerp += amount;
                yield return null;
            }
            self.BoltHandle.DriveBolt(2f);
            yield return null;
            if (self.Magazine == null || !self.Magazine.HasARound())
            {
                _cyclingRifles.Remove(self);
                yield break;
            }

            while (currentLerp >= 0f)
            {
                if (!self.IsHeld || self.BoltHandle.IsHeld)
                {
                    _cyclingRifles.Remove(self);
                    yield break;
                }

                float amount = - Time.deltaTime / (CyclingSpeedConfig.Value / 2f);
                self.BoltHandle.DriveBolt(amount);
                currentLerp += amount;
                yield return null;
            }
            self.BoltHandle.DriveBolt(-2f);
            yield return null;
            _cyclingRifles.Remove(self);
        }

        private static IEnumerator AutomaticChambering(BoltActionRifle self)
        {
            _cyclingRifles.Add(self);
            float currentLerp = 1f;
            self.BoltHandle.fakeBoltDrive = 1f;
            while (currentLerp >= 0f)
            {
                if (!self.IsHeld || self.BoltHandle.IsHeld)
                {
                    _cyclingRifles.Remove(self);
                    yield break;
                }

                float amount = -Time.deltaTime / (CyclingSpeedConfig.Value / 2f);
                self.BoltHandle.DriveBolt(amount);
                currentLerp += amount;
                yield return null;
            }
            self.BoltHandle.DriveBolt(-2f);
            yield return null;
            _cyclingRifles.Remove(self);
        }
    }
}
