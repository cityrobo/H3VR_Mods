using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using ModularWorkshop;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.UniversalModularSkinSystem", "Universal Modular Skin System", "1.0.0")]
    class UniversalModularSkinSystem : BaseUnityPlugin
    {
        private const string ASSET_BUNDLE_NAME = "workshop_ui";
        private const string PREFAB_NAME = "WorkshopUI";
        private GameObject _UIPrefab;

#if !DEBUG
        public void Awake()
        {
            string pluginPath = Path.GetDirectoryName(Info.Location);
            AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(pluginPath, ASSET_BUNDLE_NAME));

            _UIPrefab = bundle.LoadAsset<GameObject>(PREFAB_NAME);

            On.FistVR.FVRPhysicalObject.Awake += FVRPhysicalObject_Awake;
        }

        public void OnDestroy()
        {
            On.FistVR.FVRPhysicalObject.Awake -= FVRPhysicalObject_Awake;
        }

        private void FVRPhysicalObject_Awake(On.FistVR.FVRPhysicalObject.orig_Awake orig, FistVR.FVRPhysicalObject self)
        {
            ReceiverSkinSystem skinSystem = self.GetComponent<ReceiverSkinSystem>();
            if (self.GetComponent<IModularWeapon>() == null && skinSystem == null && self.GetComponent<ModularFVRPhysicalObject>() == null)
            {
                self.gameObject.SetActive(false);

                skinSystem = self.gameObject.AddComponent<ReceiverSkinSystem>();
                skinSystem.MainObject = self;
                skinSystem.UIPrefab = _UIPrefab;

                self.gameObject.SetActive(true);
            }
            orig(self);
        }
#endif
    }
}