using System;
using System.IO;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using FistVR;
using OpenScripts2;
using UnityEngine;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.VanillaClickyRailsEnabler", "Vanilla Clicky Rails Enabler", "1.0.0")]
    [BepInDependency("h3vr.OpenScripts2")]
    public class VanillaClickyRailsEnabler : BaseUnityPlugin
    {
        public static ConfigEntry<float> PicatinnySlotDistance;

        private const string ASSET_BUNDLE_NAME = "picatinny_sounds";
        private const string PREFAB_NAME = "PicatinnyRailPrefab";

        private readonly AttachmentMountPicatinnyRail _prefabRail;
        public VanillaClickyRailsEnabler()
        {
            PicatinnySlotDistance = Config.Bind("Vanilla Clicky Rails Enabler", "Picatinny slot distance", 0.01f);

            On.FistVR.FVRFireArmAttachmentMount.Awake += FVRFireArmAttachmentMount_Awake;

            string pluginPath = Path.GetDirectoryName(Info.Location);
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(pluginPath, ASSET_BUNDLE_NAME));

            GameObject prefab = bundle.LoadAsset<GameObject>(PREFAB_NAME);

            _prefabRail = prefab.GetComponent<AttachmentMountPicatinnyRail>();
        }

        public void OnDestroy()
        {
            On.FistVR.FVRFireArmAttachmentMount.Awake -= FVRFireArmAttachmentMount_Awake;
        }

        private void FVRFireArmAttachmentMount_Awake(On.FistVR.FVRFireArmAttachmentMount.orig_Awake orig, FVRFireArmAttachmentMount self)
        {
            orig(self);

            if (self.Type == FVRFireArmAttachementMountType.Picatinny && self.GetComponent<AttachmentMountPicatinnyRail>() == null)
            {
                self.gameObject.SetActive(false);
                AttachmentMountPicatinnyRail picatinnyRail = self.gameObject.AddComponent<AttachmentMountPicatinnyRail>();

                float railSize = Vector3.Distance(self.Point_Front.position, self.Point_Rear.position);
                int numberOfSlots = Mathf.RoundToInt(railSize / PicatinnySlotDistance.Value);

                picatinnyRail.NumberOfPicatinnySlots = numberOfSlots;
                picatinnyRail.Mount = self;
                picatinnyRail.SlotSound = _prefabRail.SlotSound;

                if (numberOfSlots == 0) Destroy(picatinnyRail);
                self.gameObject.SetActive(true);
            }
        }

        private void SettingsChanged(object sender, EventArgs e)
        {

        }
    }
}
