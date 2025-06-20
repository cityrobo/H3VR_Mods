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

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.MagazineRoundDisplayRotationRandomization", "Magazine Round Display Rotation Randomization", "1.0.0")]
    public class MagazineRoundDisplayRotationRandomization_BepInEx : BaseUnityPlugin
    {
        private static readonly Dictionary<FVRFireArmMagazine, MagRoundDispRotRand_RoundCountTracker> _existingRoundTrackers = new();
#if !DEBUG
        public void Awake()
        {
            On.FistVR.FVRFireArmMagazine.Awake += FVRFireArmMagazine_Awake;
            On.FistVR.FVRPhysicalObject.OnDestroy += FVRPhysicalObject_OnDestroy;

            On.FistVR.FVRFireArmMagazine.UpdateBulletDisplay += FVRFireArmMagazine_UpdateBulletDisplay;
        }

        private void FVRFireArmMagazine_Awake(On.FistVR.FVRFireArmMagazine.orig_Awake orig, FVRFireArmMagazine self)
        {
            orig(self);

            MagRoundDispRotRand_RoundCountTracker tracker = self.gameObject.AddComponent<MagRoundDispRotRand_RoundCountTracker>();
            tracker.LastRoundCount = self.m_numRounds;
            _existingRoundTrackers.Add(self, tracker);

            for (int i = 0; i < self.DisplayBullets.Length; i++)
            {
                float randomRotation = UnityEngine.Random.Range(-180f, 180f);

                self.DisplayBullets[i].transform.ModifyLocalRotationAxisValue(OpenScripts2_BasePlugin.Axis.Z, randomRotation);
            }
        }

        private void FVRPhysicalObject_OnDestroy(On.FistVR.FVRPhysicalObject.orig_OnDestroy orig, FVRPhysicalObject self)
        {
            if (self is FVRFireArmMagazine mag)
            {
                _existingRoundTrackers.Remove(mag);
            }

            orig(self);
        }

        private void FVRFireArmMagazine_UpdateBulletDisplay(On.FistVR.FVRFireArmMagazine.orig_UpdateBulletDisplay orig, FVRFireArmMagazine self)
        {
            orig(self);

            if (_existingRoundTrackers.TryGetValue(self, out var tracker))
            {
                int lastRoundCount = tracker.LastRoundCount;
                int currentRoundCount = self.m_numRounds;

                int difference = currentRoundCount - lastRoundCount;

                if (difference > 0)
                {
                    for (int i = Mathf.Min((lastRoundCount + difference) - 1, self.DisplayBullets.Length - 1); i > 0; i--)
                    {
                        Transform previousDisplayBullet = self.DisplayBullets[i - difference].transform;
                        Transform nextDisplayBullet = self.DisplayBullets[i].transform;

                        nextDisplayBullet.ModifyLocalRotationAxisValue(OpenScripts2_BasePlugin.Axis.Z, previousDisplayBullet.GetLocalEulerAnglesAxisValue(OpenScripts2_BasePlugin.Axis.Z));
                    }

                    for (int j = 0; j < difference; j++)
                    {
                        float randomRotation = UnityEngine.Random.Range(-180f, 180f);
                        self.DisplayBullets[j].transform.ModifyLocalRotationAxisValue(OpenScripts2_BasePlugin.Axis.Z, randomRotation);
                    }
                }
                else if (difference < 0)
                {
                    for (int i = 0; i < Mathf.Min(lastRoundCount + difference, self.DisplayBullets.Length + difference); i++)
                    {
                        Transform previousDisplayBullet = self.DisplayBullets[i - difference].transform;
                        Transform nextDisplayBullet = self.DisplayBullets[i].transform;

                        nextDisplayBullet.ModifyLocalRotationAxisValue(OpenScripts2_BasePlugin.Axis.Z, previousDisplayBullet.GetLocalEulerAnglesAxisValue(OpenScripts2_BasePlugin.Axis.Z));
                    }
                }

                tracker.LastRoundCount = self.m_numRounds;
            }
        }
#endif
    }
}
