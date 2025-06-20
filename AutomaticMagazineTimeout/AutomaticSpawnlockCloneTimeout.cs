using BepInEx;
using BepInEx.Configuration;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.AutomaticSpawnlockCloneTimeout", "Automatic Spawnlock Clone Timeout", "1.0.0")]
    public class AutomaticSpawnlockCloneTimeout : BaseUnityPlugin
    {
        public static ConfigEntry<float> CloneTimeout;
        public static ConfigEntry<bool> LimitedToTakeAndHold;

        private static readonly IntPtr _baseFVRFireArmRoundEndInteractionFunctionPointer;

        public void Awake()
        {
            CloneTimeout = Config.Bind("Automatic Spawnlock Clone Timeout", "Clone Timeout Time", 10f, "Time in seconds after which spawnlock-spawned magazines/speedloader/clip/loose round will time out when sitting in the world.");
            LimitedToTakeAndHold = Config.Bind("Automatic Spawnlock Clone Timeout", "Limited to Take and Hold", false, "Limits the timeout to Take and Hold.");
        }

#if !DEBUG
        static AutomaticSpawnlockCloneTimeout()
        {
            On.FistVR.FVRPhysicalObject.DuplicateFromSpawnLock += FVRPhysicalObject_DuplicateFromSpawnLock;
            On.FistVR.FVRFireArmRound.EndInteraction += FVRFireArmRound_EndInteraction;
            On.FistVR.FVRFireArmChamber.EjectRound_Vector3_Vector3_Vector3_bool += FVRFireArmChamber_EjectRound_Vector3_Vector3_Vector3_bool;
            On.FistVR.FVRFireArmChamber.EjectRound_Vector3_Vector3_Vector3_Vector3_Quaternion_bool += FVRFireArmChamber_EjectRound_Vector3_Vector3_Vector3_Vector3_Quaternion_bool;

            MethodInfo _methodInfo = typeof(FVRPhysicalObject).GetMethod(nameof(FVRPhysicalObject.EndInteraction), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            _baseFVRFireArmRoundEndInteractionFunctionPointer = _methodInfo.MethodHandle.GetFunctionPointer();

            //On.FistVR.FVRFireArmMagazine.DuplicateFromSpawnLock += FVRFireArmMagazine_DuplicateFromSpawnLock;
            //On.FistVR.FVRFireArmClip.DuplicateFromSpawnLock += FVRFireArmClip_DuplicateFromSpawnLock;
            //On.FistVR.Speedloader.DuplicateFromSpawnLock += Speedloader_DuplicateFromSpawnLock;
            //On.FistVR.FVRFireArmRound.DuplicateFromSpawnLock += FVRFireArmRound_DuplicateFromSpawnLock;
        }

        private static GameObject FVRPhysicalObject_DuplicateFromSpawnLock(On.FistVR.FVRPhysicalObject.orig_DuplicateFromSpawnLock orig, FVRPhysicalObject self, FVRViveHand hand)
        {
            GameObject copy = orig(self, hand);
            if (GM.TNH_Manager != null || !LimitedToTakeAndHold.Value)
            {
                FVRPhysicalObject physicalObject = copy.GetComponent<FVRPhysicalObject>();
                SpawnlockCloneTimeoutController controller = copy.GetComponent<SpawnlockCloneTimeoutController>() ?? copy.AddComponent<SpawnlockCloneTimeoutController>();
                switch (physicalObject)
                {
                    case FVRFireArmMagazine:
                        controller.Magazine = copy.GetComponent<FVRFireArmMagazine>();
                        controller.Timout = CloneTimeout.Value;
                        break;
                    case FVRFireArmClip:
                        controller.Clip = copy.GetComponent<FVRFireArmClip>();
                        controller.Timout = CloneTimeout.Value;
                        break;
                    case Speedloader:
                        controller.Speedloader = copy.GetComponent<Speedloader>();
                        controller.Timout = CloneTimeout.Value;
                        break;
                    case FVRFireArmRound:
                        controller.Round = copy.GetComponent<FVRFireArmRound>();
                        controller.Timout = CloneTimeout.Value;
                        break;
                    case StingerLauncher:
                        controller.PhysicalObject = copy.GetComponent<StingerLauncher>();
                        controller.Timout = CloneTimeout.Value;
                        break;
                    default:
                        break;
                }
            }
            return copy;
        }

        private static FVRFireArmRound FVRFireArmChamber_EjectRound_Vector3_Vector3_Vector3_Vector3_Quaternion_bool(On.FistVR.FVRFireArmChamber.orig_EjectRound_Vector3_Vector3_Vector3_Vector3_Quaternion_bool orig, FVRFireArmChamber self, Vector3 EjectionPosition, Vector3 EjectionVelocity, Vector3 EjectionAngularVelocity, Vector3 AnimationPosStart, Quaternion AnimationStartRot, bool ForceCaseLessEject)
        {
            FVRFireArmRound round = orig(self, EjectionPosition, EjectionVelocity, EjectionAngularVelocity, AnimationPosStart, AnimationStartRot, ForceCaseLessEject);
            if (round != null && GM.CurrentSceneSettings.IsSpawnLockingEnabled && (GM.TNH_Manager != null || !LimitedToTakeAndHold.Value))
            {
                SpawnlockCloneTimeoutController controller = round.GetComponent<SpawnlockCloneTimeoutController>() ?? round.gameObject.AddComponent<SpawnlockCloneTimeoutController>();
                controller.Round = round;
                controller.Timout = CloneTimeout.Value;
            }
            
            return round;
        }

        private static FVRFireArmRound FVRFireArmChamber_EjectRound_Vector3_Vector3_Vector3_bool(On.FistVR.FVRFireArmChamber.orig_EjectRound_Vector3_Vector3_Vector3_bool orig, FVRFireArmChamber self, Vector3 EjectionPosition, Vector3 EjectionVelocity, Vector3 EjectionAngularVelocity, bool ForceCaseLessEject)
        {
            FVRFireArmRound round = orig(self, EjectionPosition, EjectionVelocity, EjectionAngularVelocity, ForceCaseLessEject);
            if (round != null && GM.CurrentSceneSettings.IsSpawnLockingEnabled && (GM.TNH_Manager != null || !LimitedToTakeAndHold.Value))
            {
                SpawnlockCloneTimeoutController controller = round.GetComponent<SpawnlockCloneTimeoutController>() ?? round.gameObject.AddComponent<SpawnlockCloneTimeoutController>();
                controller.Round = round;
                controller.Timout = CloneTimeout.Value;
            }

            return round;
        }

        private static void FVRFireArmRound_EndInteraction(On.FistVR.FVRFireArmRound.orig_EndInteraction orig, FVRFireArmRound self, FVRViveHand hand)
        {
            if (self.GetComponent<SpawnlockCloneTimeoutController>() != null)
            {
                Action<FVRViveHand> baseMethod = (Action<FVRViveHand>)Activator.CreateInstance(typeof(Action<FVRViveHand>), self, _baseFVRFireArmRoundEndInteractionFunctionPointer);
                baseMethod(hand);

                if (self.ProxyRounds.Count > 0 && self.m_proxyDumpFlag)
                {
                    for (int i = 0; i < self.ProxyRounds.Count; i++)
                    {
                        GameObject gameObject = Instantiate(self.ProxyRounds[i].ObjectWrapper.GetGameObject(), self.ProxyRounds[i].GO.transform.position, self.ProxyRounds[i].GO.transform.rotation);
                        gameObject.GetComponent<Rigidbody>().velocity = self.RootRigidbody.velocity;

                        SpawnlockCloneTimeoutController controller = gameObject.AddComponent<SpawnlockCloneTimeoutController>();
                        controller.Round = gameObject.GetComponent<FVRFireArmRound>();

                        Destroy(self.ProxyRounds[i].GO);
                        self.ProxyRounds[i].GO = null;
                        self.ProxyRounds[i].Filter = null;
                        self.ProxyRounds[i].Renderer = null;
                        self.ProxyRounds[i].ObjectWrapper = null;
                    }
                    self.ProxyRounds.Clear();
                }
            }
            else orig(self, hand);
        }

        #region Old Patches
        private static GameObject Speedloader_DuplicateFromSpawnLock(On.FistVR.Speedloader.orig_DuplicateFromSpawnLock orig, Speedloader self, FVRViveHand hand)
        {
            GameObject copySpeedloader = orig(self, hand);
            if (GM.TNH_Manager != null || !LimitedToTakeAndHold.Value)
            {
                SpawnlockCloneTimeoutController controller = copySpeedloader.GetComponent<SpawnlockCloneTimeoutController>() ?? copySpeedloader.AddComponent<SpawnlockCloneTimeoutController>();
                controller.Speedloader = copySpeedloader.GetComponent<Speedloader>();
                controller.Timout = CloneTimeout.Value;
            }
            return copySpeedloader;
        }

        private static GameObject FVRFireArmRound_DuplicateFromSpawnLock(On.FistVR.FVRFireArmRound.orig_DuplicateFromSpawnLock orig, FVRFireArmRound self, FVRViveHand hand)
        {
            GameObject copyRound = orig(self, hand);
            if (GM.TNH_Manager != null || !LimitedToTakeAndHold.Value)
            {
                SpawnlockCloneTimeoutController controller = copyRound.GetComponent<SpawnlockCloneTimeoutController>() ?? copyRound.AddComponent<SpawnlockCloneTimeoutController>();
                controller.Round = copyRound.GetComponent<FVRFireArmRound>();
                controller.Timout = CloneTimeout.Value;
            }
            return copyRound;
        }

        private static GameObject FVRFireArmClip_DuplicateFromSpawnLock(On.FistVR.FVRFireArmClip.orig_DuplicateFromSpawnLock orig, FVRFireArmClip self, FVRViveHand hand)
        {
            GameObject copyClip = orig(self, hand);
            if (GM.TNH_Manager != null || !LimitedToTakeAndHold.Value)
            {
                SpawnlockCloneTimeoutController controller = copyClip.GetComponent<SpawnlockCloneTimeoutController>() ?? copyClip.AddComponent<SpawnlockCloneTimeoutController>();
                controller.Clip = copyClip.GetComponent<FVRFireArmClip>();
                controller.Timout = CloneTimeout.Value;
            }
            return copyClip;
        }

        private static GameObject FVRFireArmMagazine_DuplicateFromSpawnLock(On.FistVR.FVRFireArmMagazine.orig_DuplicateFromSpawnLock orig, FVRFireArmMagazine self, FVRViveHand hand)
        {
            GameObject copyMag = orig(self, hand);
            if (GM.TNH_Manager != null || !LimitedToTakeAndHold.Value)
            {
                SpawnlockCloneTimeoutController controller = copyMag.GetComponent<SpawnlockCloneTimeoutController>() ?? copyMag.AddComponent<SpawnlockCloneTimeoutController>();
                controller.Magazine = copyMag.GetComponent<FVRFireArmMagazine>();
                controller.Timout = CloneTimeout.Value;
            }
            return copyMag;
        }
        #endregion
#endif
    }
}