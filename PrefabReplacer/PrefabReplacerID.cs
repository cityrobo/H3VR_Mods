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
    [CreateAssetMenu(fileName = "New PrefabReplacerID", menuName = "PrefabReplacer/PrefabReplacerID", order = 0)]
    public class PrefabReplacerID : ScriptableObject
    {
        [Header("Prefab Replacer Identifier")]
        public string originalItemID = "";
        public string replacementItemID = "";
        [Header("FVRObject Options")]
        [Tooltip("Place Replacement FVRObject here. (not strictly required because the internal systems allow fetching by ItemID)")]
        public FVRObject replacementFVRObject;
        [Tooltip("Only copies tags that are different from the default one. (\"none\")")]
        public bool copyTags = false;
        [Tooltip("If true, mounts will be replaced, else they will be added to the existing ones.")]
        public bool replaceFiringModes = false;
        [Tooltip("If true, feed options will be replaced, else they will be added to the existing ones.")]
        public bool replaceFeedOptions = false;
        [Tooltip("If true, mounts will be replaced, else they will be added to the existing ones.")]
        public bool replaceMounts = false;
        [Tooltip("Only copies related assets that are different from the default one. (\"none\")")]
        public bool copyRelatedAssets = false;
        [Tooltip("If true, compatible magazines will be replaced, else they will be added to the existing ones.")]
        public bool replaceCompatibleMagazines = false;
        [Tooltip("If true, compatible magazines will be replaced, else they will be added to the existing ones.")]
        public bool replaceCompatibleClips = false;
        [Tooltip("If true, compatible magazines will be replaced, else they will be added to the existing ones.")]
        public bool replaceCompatibleSpeedLoaders = false;
        [Tooltip("If true, compatible magazines will be replaced, else they will be added to the existing ones.")]
        public bool replaceCompatibleSingleRounds = false;
        [Tooltip("If true, compatible magazines will be replaced, else they will be added to the existing ones.")]
        public bool replaceBespokeAttachments = false;
        [Tooltip("If true, compatible magazines will be replaced, else they will be added to the existing ones.")]
        public bool replaceRequiredSecondaryPieces = false;
        [Tooltip("If true, will replace existing OSple.")]
        public bool updateOSple = false;


        [Header("ItemSpawnerID Options")]
        [Tooltip("Optional replacement ItemspawnerID, options will only get copied if not empty or for checkboxes, different from the original. (Also not strictly required, can be fetched from \"spawnedByID\"). Or, if original item does not have a ISID, it will use this one.")]
        public ItemSpawnerID replacementItemSpawnerID;
        public PrefabReplacerID(string orig, string replacement)
        {
            originalItemID = orig;
            replacementItemID = replacement;
        }
        [Tooltip("If true, secondaries list will be replaced with the one in the ItemspawnerID, else they will be added.")]
        public bool replaceSecondariesInstead = false;
        [Tooltip("If true, secondaries by String ID list will be replaced with the one in the ItemspawnerID instead of being added instead.")]
        public bool replaceSecondariesStringIDInstead = false;
        [Tooltip("If true, mod tags list will be replaced with the one in the ItemspawnerID instead of being added instead.")]
        public bool replaceModTagsInstead = false;
        [Tooltip("If true, mod tags list will be replaced with the one in the ItemspawnerID instead of being added instead.")]
        public bool replaceTutorialBlocksInstead = false;
        [Tooltip("If true, mod tags list will be replaced with the one in the ItemspawnerID instead of being added instead.")]
        public bool updateCheckboxes = false;
    }
}
