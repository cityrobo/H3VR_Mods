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
    [BepInPlugin("h3vr.cityrobo.VanillaClickyPicatinnyMountEnabler", "Vanilla Clicky Picatinny Mount Enabler", "1.0.0")]
    [BepInDependency("h3vr.OpenScripts2")]
    public class VanillaClickyPicatinnyMountEnabler : BaseUnityPlugin
    {
        private const string ASSET_BUNDLE_NAME = "picatinny_sounds";
        private const string PREFAB_NAME = "PicatinnyRailPrefab";
        private const float PICATINNY_SLOT_DISTANCE = 0.01f;

        private AttachmentMountPicatinnyRail _prefabRail;
        public VanillaClickyPicatinnyMountEnabler()
        {
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

                float railSize = Vector3.Distance(self.Point_Front.localPosition, self.Point_Rear.localPosition);

                int numberOfSlots = Mathf.FloorToInt(railSize / PICATINNY_SLOT_DISTANCE);

                picatinnyRail.NumberOfPicatinnySlots = numberOfSlots;
                picatinnyRail.Mount = self;
                picatinnyRail.SlotSound = _prefabRail.SlotSound;

                self.gameObject.SetActive(true);
            }
        }

        private void SettingsChanged(object sender, EventArgs e)
        {

        }

    }
}
