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

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.HighlightHiddenRails", "Highlight Hidden Rails", "1.0.0")]
    public class HighlightHiddenRails_BepInEx : BaseUnityPlugin
    {
        public static Material _highlightMaterial;

        public static ConfigEntry<bool> HighlightMountsWithoutAHiddenMesh;
        public static ConfigEntry<bool> OnlyHighlightOnHeldObject;

        public void Awake()
        {
            HighlightMountsWithoutAHiddenMesh = Config.Bind("Highlight Hidden Rails", "Highlight mounts without a hidden mesh", true, "If a mount doesn't have a hidden mesh, still highlight it to potentially show places you didn't knwo you could mount it too. Useful for guns created before showing parts was added.");
            OnlyHighlightOnHeldObject = Config.Bind("Highlight Hidden Rails", "Only highlight on held object", true, "Only highlight the mounts on the currently held item.");
        }

        public void Update()
        {
            if (_highlightMaterial == null && GM.Instance != null && GM.Instance.QuickbeltConfigurations != null && GM.Instance.QuickbeltConfigurations.Length > 0)
            {
                // grabbbing the default QBSlot highlight material for use as a highlight material
                _highlightMaterial = GM.Instance.QuickbeltConfigurations[0].GetComponentInChildren<FVRQuickBeltSlot>().HoverGeo.GetComponent<MeshRenderer>().material;
            }
        }

#if !DEBUG
        // Patching the Awake method of FVRPhysicalObject to add RailHighlightController to mounts which have EnablePieces.
        static HighlightHiddenRails_BepInEx()
        {
            On.FistVR.FVRPhysicalObject.Awake += FVRPhysicalObject_Awake;
        }

        private static void FVRPhysicalObject_Awake(On.FistVR.FVRPhysicalObject.orig_Awake orig, FVRPhysicalObject self)
        {
            orig(self);

            // Start coroutine that waits for the object to completely initialize
            self.StartCoroutine(WaitForStart(self));
        }

        private static IEnumerator WaitForStart(FVRPhysicalObject self)
        {
            yield return null;

            FVRFireArmAttachmentMount[] mountsWithEnableOnHoverPiece = self.AttachmentMounts.Where(m => m != null && m.HasHoverEnablePiece && m.EnableOnHover != null).ToArray();
            foreach (var validMount in mountsWithEnableOnHoverPiece)
            {
                validMount.gameObject.AddComponent<RailHighlightController>();
            }

            MultipleHideOnAttach[] multipleHideOnAttaches = self.GetComponentsInChildren<MultipleHideOnAttach>(true).Where(m => m.ShowOnAttach).ToArray();
            foreach (var hideOnAttach in multipleHideOnAttaches)
            {
                hideOnAttach.gameObject.AddComponent<RailHighlightController>();
            }

            if (HighlightMountsWithoutAHiddenMesh.Value)
            {
                FVRFireArmAttachmentMount[] mountsWithoutEnableOnHoverPiece = self.AttachmentMounts.Where(m => m != null && !m.HasHoverEnablePiece).ToArray();
                foreach (var validMount in mountsWithoutEnableOnHoverPiece)
                {
                    if (validMount.GetComponent<RailHighlightController>() == null) validMount.gameObject.AddComponent<RailHighlightController>();
                }
            }
        }
#endif
    }
}