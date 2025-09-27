using FistVR;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx;
using BepInEx.Configuration;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.UniversalMuzzleAttachmentPoint", "Universal Muzzle Attachment Point", "1.1.0")]
    public class UniversalMuzzleAttachmentPoint : BaseUnityPlugin
    {
        private const string MOUNTNAME = "_PointSuppressorMount";
        private const string LAYERNAME = "Interactable";
        private const string TAG = "FVRFireArmAttachmentMount";

        public ConfigEntry<float> FlintLockScale;

        private string _dictionaryPath;
        private const string DICTIONARYNAME = "RoundScaleDictionary.txt";

        private string _roundScaleDictionaryString;

        private Dictionary<FireArmRoundType, float> _roundScaleDictionary = [];

        private float GetScale(FireArmRoundType roundType)
        {
            if (_roundScaleDictionary.TryGetValue(roundType, out float scale)) return scale;
            else return 1f;
        }

        public void Awake()
        {
            FlintLockScale = Config.Bind("Universal Muzzle Attachment Point", "Flintlock Muzzle Scale", 1.5f, "Since Flintlocks don't use conventional caliber types in the game's ammo sense, I cannot auto configure them by caliber. So this config option exists to negate that.");

            Hook();

            _dictionaryPath = Info.Location;
            _dictionaryPath = Path.GetDirectoryName(_dictionaryPath);

            _dictionaryPath = Path.Combine(_dictionaryPath,DICTIONARYNAME);
            StreamReader reader = new(_dictionaryPath);
            _roundScaleDictionaryString = reader.ReadToEnd();
            reader.Close();

            string[] _roundScaleDictionaryLines = _roundScaleDictionaryString.Split('\n');

            foreach (string _roundScaleDictionaryLine in _roundScaleDictionaryLines)
            {
                string[] _roundScaleDictionaryData = _roundScaleDictionaryLine.Split(';');

                FireArmRoundType type = (FireArmRoundType)Enum.Parse(typeof(FireArmRoundType), _roundScaleDictionaryData[0]);
                float scale = (float)float.Parse(_roundScaleDictionaryData[1]);

                _roundScaleDictionary.Add(type, scale);
            }
        }

        public void OnDestroy()
        {
            Unhook();
        }

        private void Hook()
        {
            On.FistVR.FVRFireArm.Awake += FVRFireArm_Awake;
            On.FistVR.MuzzleDevice.Awake += MuzzleDevice_Awake;
           
        }

        private void Unhook()
        {
            On.FistVR.FVRFireArm.Awake -= FVRFireArm_Awake;
            On.FistVR.MuzzleDevice.Awake -= MuzzleDevice_Awake;
        }

        private void FVRFireArm_Awake(On.FistVR.FVRFireArm.orig_Awake orig, FVRFireArm self)
        {
            orig(self);

            self.AttachmentMounts ??= [];
            bool hasMuzzleMount = false;
            bool hasNullMount = false;

            // Check if Muzzle mount exists
            foreach (var Mount in self.AttachmentMounts)
            {
                // Check for potential null mount
                if (Mount == null) hasNullMount = true;
                else if (Mount.Type == FVRFireArmAttachementMountType.Suppressor) hasMuzzleMount = true;
            }

            if (!hasMuzzleMount)
            {
                GameObject MuzzleMountGameObject = new(MOUNTNAME); ;

                // Setup Mount GameObject
                MuzzleMountGameObject.layer = LayerMask.NameToLayer(LAYERNAME);
                MuzzleMountGameObject.tag = TAG;
                MuzzleMountGameObject.SetActive(false);

                // Setup Mount differently for Multi Barrel Weapons
                FVRFireArmAttachmentMount MountComponent = CreateNewMountComponent(self, MuzzleMountGameObject);

                // Create Trigger
                SphereCollider collider = MuzzleMountGameObject.AddComponent<SphereCollider>();
                collider.radius = 0.01f;
                collider.isTrigger = true;

                // Replace potential null mount with new mount and remove other null mounts
                if (hasNullMount)
                {
                    for (int i = 0; i < self.AttachmentMounts.Count; i++)
                    {
                        if (self.AttachmentMounts[i] == null)
                        {
                            self.AttachmentMounts[i] = MountComponent;
                            break;
                        }
                    }

                    self.AttachmentMounts.RemoveAll(obj => obj == null);
                }
                else self.AttachmentMounts.Add(MountComponent);

                MuzzleMountGameObject.SetActive(true);
            }
        }

        private FVRFireArmAttachmentMount CreateNewMountComponent(FVRFireArm self, GameObject MuzzleMountGameObject)
        {
            FVRFireArmAttachmentMount MountComponent;
            Vector3 MountPos;
            Vector3 MountRot;
            bool parentToThis;

            switch (self)
            {
                case BreakActionWeapon breakAction:
                    MountComponent = MuzzleMountGameObject.AddComponent<OpenScripts2.MultiBarrelMount>();
                    MuzzleMountGameObject.transform.parent = breakAction.Hinge.transform;
                    parentToThis = true;

                    MountPos = ComputeAverage(breakAction.Barrels.Select(barrel => barrel.Muzzle.position));
                    MountRot = ComputeAverage(breakAction.Barrels.Select(barrel => barrel.Muzzle.eulerAngles));

                    MuzzleMountGameObject.transform.position = MountPos;
                    MuzzleMountGameObject.transform.rotation = Quaternion.Euler(MountRot);
                    break;

                case Derringer derringer:
                    MountComponent = MuzzleMountGameObject.AddComponent<OpenScripts2.MultiBarrelMount>();
                    MuzzleMountGameObject.transform.parent = derringer.Hinge;
                    parentToThis = true;

                    MountPos = ComputeAverage(derringer.Barrels.Select(barrel => barrel.MuzzlePoint.position));
                    MountRot = ComputeAverage(derringer.Barrels.Select(barrel => barrel.MuzzlePoint.eulerAngles));

                    MuzzleMountGameObject.transform.position = MountPos;
                    MuzzleMountGameObject.transform.rotation = Quaternion.Euler(MountRot);
                    break;

                case Flaregun flaregun:
                    MountComponent = MuzzleMountGameObject.AddComponent<FVRFireArmAttachmentMount>();
                    MuzzleMountGameObject.transform.parent = flaregun.Hinge;
                    parentToThis = true;

                    MountPos = self.GetMuzzle().position;
                    MountRot = self.GetMuzzle().eulerAngles;

                    MuzzleMountGameObject.transform.position = MountPos;
                    MuzzleMountGameObject.transform.rotation = Quaternion.Euler(MountRot);
                    break;

                case FlintlockWeapon flintlock:
                    MountComponent = MuzzleMountGameObject.AddComponent<FVRFireArmAttachmentMount>();
                    MuzzleMountGameObject.transform.parent = self.transform;
                    parentToThis = false;

                    MountPos = flintlock.GetComponentInChildren<FlintlockBarrel>().Muzzle.position;
                    MountRot = flintlock.GetComponentInChildren<FlintlockBarrel>().Muzzle.eulerAngles;

                    MuzzleMountGameObject.transform.position = MountPos;
                    MuzzleMountGameObject.transform.rotation = Quaternion.Euler(MountRot);
                    break;

                default:
                    MountComponent = MuzzleMountGameObject.AddComponent<FVRFireArmAttachmentMount>();
                    MuzzleMountGameObject.transform.parent = self.transform;
                    parentToThis = false;

                    MountPos = self.GetMuzzle().position;
                    MountRot = self.GetMuzzle().eulerAngles;

                    MuzzleMountGameObject.transform.position = MountPos;
                    MuzzleMountGameObject.transform.rotation = Quaternion.Euler(MountRot);
                    break;
            }

            // Setup Mount Component
            MountComponent.MyObject = self;
            MountComponent.Parent = self;
            MountComponent.Type = FVRFireArmAttachementMountType.Suppressor;
            MountComponent.Point_Front = MountComponent.transform;
            MountComponent.Point_Rear = MountComponent.transform;
            MountComponent.AttachmentsList = [];
            MountComponent.SubMounts = [];
            MountComponent.ParentToThis = parentToThis;
            // Since Flintlocks don't use conventional caliber types in the game's ammo sense, I cannot auto configure them by caliber. So this config option exists to negate that.
            MountComponent.ScaleModifier = self is not FlintlockWeapon ? GetScale(self.RoundType) : FlintLockScale.Value;

            return MountComponent;
        }

        private Vector3 ComputeAverage(IEnumerable<Vector3> ListOfVectors)
        {
            Vector3 average = new();
            int numberOfVectors = ListOfVectors.Count();
            foreach (var vector in ListOfVectors)
            {
                average += vector;
            }
            average /= numberOfVectors;
            return average;
        }

        private void MuzzleDevice_Awake(On.FistVR.MuzzleDevice.orig_Awake orig, MuzzleDevice self)
        {
            orig(self);

            bool hasMuzzleMount = false;
            self.AttachmentMounts ??= [];
            bool hasNullMount = false;

            // Check if Muzzle mount exists
            foreach (var Mount in self.AttachmentMounts)
            {
                // Check for potential null mount
                if (Mount == null) hasNullMount = true;
                else if (Mount.Type == FVRFireArmAttachementMountType.Suppressor) hasMuzzleMount = true;
            }

            if (!hasMuzzleMount)
            {
                // Setup Mount GameObject
                GameObject MuzzleMountGameObject = new(MOUNTNAME);
                MuzzleMountGameObject.transform.parent = self.transform;
                MuzzleMountGameObject.transform.position = self.Muzzle.position;
                MuzzleMountGameObject.transform.rotation = self.Muzzle.rotation;
                MuzzleMountGameObject.layer = LayerMask.NameToLayer(LAYERNAME);
                MuzzleMountGameObject.tag = TAG;
                MuzzleMountGameObject.SetActive(false);

                // Setup Mount Component
                FVRFireArmAttachmentMount MountComponent = MuzzleMountGameObject.AddComponent<FVRFireArmAttachmentMount>();
                MountComponent.MyObject = self;
                MountComponent.Parent = self;
                MountComponent.ScaleModifier = 1f;
                MountComponent.Type = FVRFireArmAttachementMountType.Suppressor;
                MountComponent.Point_Front = MountComponent.transform;
                MountComponent.Point_Rear = MountComponent.transform;
                MountComponent.AttachmentsList = [];
                MountComponent.SubMounts = [];

                // Create Trigger
                SphereCollider collider = MuzzleMountGameObject.AddComponent<SphereCollider>();
                collider.radius = 0.01f;
                collider.isTrigger = true;

                // Replace potential null mount with new mount and remove other null mounts
                if (hasNullMount)
                {
                    for (int i = 0; i < self.AttachmentMounts.Count; i++)
                    {
                        if (self.AttachmentMounts[i] == null)
                        {
                            self.AttachmentMounts[i] = MountComponent;
                            break;
                        }
                    }

                    self.AttachmentMounts.RemoveAll(obj => obj == null);
                }
                else self.AttachmentMounts.Add(MountComponent);

                MuzzleMountGameObject.SetActive(true);
            }
        }

#if !(DEBUG || MEATKIT)

#endif
    }
}
