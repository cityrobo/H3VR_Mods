using FistVR;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx;

namespace Cityrobo
{
#if !(DEBUG || UNITY_EDITOR || UNITY_5)
    [BepInPlugin("h3vr.cityrobo.prefab_replacer", "PrefabReplacer", "1.0.1")]
    [BepInDependency("h3vr.otherloader", "1.1.3")]
    [BepInDependency("nrgill28.Sodalite", "1.2.0")]
    public class PrefabReplacer : BaseUnityPlugin
    {
        public Dictionary<string, PrefabReplacerID> LoadedPrefabReplacerIDs;

        public static float Progress
        {
            get { return _progress; }
        }
        //private bool otherloaderReady = false;
        private List<PrefabReplacerID> _PRIDs;
        private static readonly string s_sideloaderPath = Path.Combine(Paths.BepInExRootPath, "Sideloader");

        private static float _progress = 0f;
        private float _percentagePerPRID;
        public PrefabReplacer()
        {
            //Logger.LogInfo("PrefabReplacer Script loaded!");

            LoadedPrefabReplacerIDs = new Dictionary<string, PrefabReplacerID>();

            LoadPrefabReplacerAssets();

            StartCoroutine(WaitForOtherloader());
        }

        void LoadPrefabReplacerAssets()
        {
            if (!Directory.Exists(s_sideloaderPath))
            {
                Logger.LogWarning("Sideloader folder not found. Skipping loading of standalone PrefabReplacer packages!");
                return;
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(s_sideloaderPath);
            FileInfo[] filesInDir = directoryInfo.GetFiles("pr_*");

            if (filesInDir.Length > 0)
            {
                Logger.LogInfo($"Loading {filesInDir.Length} standalone PrefabReplacer asset bundles!");
                foreach (FileInfo foundFile in filesInDir)
                {
                    Logger.LogInfo($"Loading standalone PrefabReplacer asset bundle {foundFile.Name}.");
                    Sodalite.Api.GameAPI.PreloadAllAssets(foundFile.FullName);
                }
                Logger.LogInfo($"Standalone PrefabReplacer asset bundle loading complete!");
            }
        }

        // Wait for Otherloader to finish loading assets before starting prefab replacement.
        IEnumerator WaitForOtherloader()
        {
            while (OtherLoader.LoaderStatus.GetLoaderProgress() != 1)
            {
                yield return null;
            }

            PrefabReplacement();
        }

        private void PrefabReplacement()
        {
            Logger.LogInfo("Starting Prefab Replacement!");
            _PRIDs = new List<PrefabReplacerID>(Resources.FindObjectsOfTypeAll<PrefabReplacerID>());

            if (_PRIDs.Count != 0) _percentagePerPRID = 1f / _PRIDs.Count;
            foreach (var PRID in _PRIDs)
            {
                if (PRID.PrefabReplacerIDActivated == true) ReplacePrefabViaID(PRID);
                _progress += _percentagePerPRID;
            }
            if (_PRIDs.Count == 0) Logger.LogInfo("No PrefabReplacerIDs found. Have a nice day!");
            _progress = 1f;
        }

        private void ReplacePrefabViaID(PrefabReplacerID PRID)
        {
            

            LoadedPrefabReplacerIDs.Add(PRID.originalItemID, PRID);
            

            // FVRObject Replacement
            FVRObject origFVRObject = null;
            IM.OD.TryGetValue(PRID.originalItemID, out origFVRObject);
            if (origFVRObject == null)
            {
                Logger.LogError($"Could not find original FVRObject with ItemID {PRID.originalItemID}.");
                return;
            }

            FVRObject replacementFVRObject = null;
            if (PRID.replacementFVRObject != null) replacementFVRObject = PRID.replacementFVRObject;
            else IM.OD.TryGetValue(PRID.replacementItemID, out replacementFVRObject);

            if (replacementFVRObject == null)
            {
                Logger.LogError($"Could not find replacement FVRObject with ItemID {PRID.replacementItemID}.");
                return;
            }

            FVRObjectReplacement(PRID, origFVRObject, replacementFVRObject);

            // ItemSpawnerID Replacement
            ItemSpawnerID replacementISID = null;
            if (PRID.replacementItemSpawnerID != null) replacementISID = PRID.replacementItemSpawnerID;
            else ManagerSingleton<IM>.Instance.SpawnerIDDic.TryGetValue(replacementFVRObject.SpawnedFromId, out replacementISID);

            ItemSpawnerID origISID = null;

            ISIDReplacement(PRID, origISID, replacementISID, origFVRObject);

            // ItemSpawnerEntry Replacement
            OtherLoader.ItemSpawnerEntry replacementSpawnerEntry = null;
            if (PRID.ReplacementItemSpawnerEntry != null) replacementSpawnerEntry = PRID.ReplacementItemSpawnerEntry;
            else OtherLoader.OtherLoader.SpawnerEntriesByID.TryGetValue(replacementFVRObject.SpawnedFromId, out replacementSpawnerEntry);

            OtherLoader.ItemSpawnerEntry origSpawnerEntry = null;

            ItemSpawnerEntryReplacement(PRID, origSpawnerEntry, replacementSpawnerEntry, origFVRObject);

            // Additional Operations
            if (PRID.DisableReplacementObjectInItemSpawner)
            { 
                if (replacementISID != null) replacementISID.IsDisplayedInMainEntry = false;
                if (replacementSpawnerEntry != null) replacementSpawnerEntry.IsDisplayedInMainEntry = false;
            }
            if (PRID.DisableReplacementObjectInTnH)
            {
                if (replacementFVRObject != null) replacementFVRObject.OSple = false;
            }

            Logger.LogInfo($"PrefabRepacerID {PRID.name} replaced {PRID.originalItemID} with {PRID.replacementItemID}. {_percentagePerPRID + _progress}% done.");
        }


        private void FVRObjectReplacement(PrefabReplacerID PRID, FVRObject origFVRObject, FVRObject replacementFVRObject)
        {
            origFVRObject.m_anvilPrefab.Guid = replacementFVRObject.m_anvilPrefab.Guid;
            origFVRObject.m_anvilPrefab.Bundle = replacementFVRObject.m_anvilPrefab.Bundle;
            origFVRObject.m_anvilPrefab.AssetName = replacementFVRObject.m_anvilPrefab.AssetName;
            if (replacementFVRObject.DisplayName != string.Empty) origFVRObject.DisplayName = replacementFVRObject.DisplayName;

            if (PRID.copyTags)
            {
                if (replacementFVRObject.TagEra != 0) origFVRObject.TagEra = replacementFVRObject.TagEra;
                if (replacementFVRObject.TagSet != 0) origFVRObject.TagSet = replacementFVRObject.TagSet;
                if (replacementFVRObject.TagFirearmSize != 0) origFVRObject.TagFirearmSize = replacementFVRObject.TagFirearmSize;
                if (replacementFVRObject.TagFirearmAction != 0) origFVRObject.TagFirearmAction = replacementFVRObject.TagFirearmAction;
                if (replacementFVRObject.TagFirearmRoundPower != 0) origFVRObject.TagFirearmRoundPower = replacementFVRObject.TagFirearmRoundPower;
                if (replacementFVRObject.TagFirearmCountryOfOrigin != 0) origFVRObject.TagFirearmCountryOfOrigin = replacementFVRObject.TagFirearmCountryOfOrigin;
                if (replacementFVRObject.TagFirearmSize != 0) origFVRObject.TagFirearmSize = replacementFVRObject.TagFirearmSize;
                if (replacementFVRObject.TagFirearmFirstYear != 0) origFVRObject.TagFirearmFirstYear = replacementFVRObject.TagFirearmFirstYear;
                if (replacementFVRObject.TagFirearmFiringModes.Count != 0 && !PRID.replaceFiringModes) origFVRObject.TagFirearmFiringModes.AddRange(replacementFVRObject.TagFirearmFiringModes);
                else if (replacementFVRObject.TagFirearmFiringModes.Count != 0 && PRID.replaceFiringModes) origFVRObject.TagFirearmFiringModes = replacementFVRObject.TagFirearmFiringModes;
                if (replacementFVRObject.TagFirearmFeedOption.Count != 0 && !PRID.replaceFeedOptions) origFVRObject.TagFirearmFeedOption.AddRange(replacementFVRObject.TagFirearmFeedOption);
                else if (replacementFVRObject.TagFirearmFeedOption.Count != 0 && PRID.replaceFeedOptions) origFVRObject.TagFirearmFeedOption = replacementFVRObject.TagFirearmFeedOption;
                if (replacementFVRObject.TagFirearmMounts.Count != 0 && !PRID.replaceMounts) origFVRObject.TagFirearmMounts.AddRange(replacementFVRObject.TagFirearmMounts);
                else if (replacementFVRObject.TagFirearmMounts.Count != 0 && PRID.replaceMounts) origFVRObject.TagFirearmMounts = replacementFVRObject.TagFirearmMounts;

                if (replacementFVRObject.TagAttachmentMount != 0) origFVRObject.TagAttachmentMount = replacementFVRObject.TagAttachmentMount;
                if (replacementFVRObject.TagAttachmentFeature != 0) origFVRObject.TagAttachmentFeature = replacementFVRObject.TagAttachmentFeature;
                if (replacementFVRObject.TagMeleeStyle != 0) origFVRObject.TagMeleeStyle = replacementFVRObject.TagMeleeStyle;
                if (replacementFVRObject.TagMeleeHandedness != 0) origFVRObject.TagMeleeHandedness = replacementFVRObject.TagMeleeHandedness;
                if (replacementFVRObject.TagThrownType != 0) origFVRObject.TagThrownType = replacementFVRObject.TagThrownType;
                if (replacementFVRObject.TagThrownDamageType != 0) origFVRObject.TagThrownDamageType = replacementFVRObject.TagThrownDamageType;
            }
            if (PRID.copyRelatedAssets)
            {
                if (replacementFVRObject.MagazineType != 0) origFVRObject.MagazineType = replacementFVRObject.MagazineType;
                if (replacementFVRObject.ClipType != 0) origFVRObject.ClipType = replacementFVRObject.ClipType;
                if (replacementFVRObject.UsesRoundTypeFlag != origFVRObject.UsesRoundTypeFlag) origFVRObject.UsesRoundTypeFlag = replacementFVRObject.UsesRoundTypeFlag;
                if (replacementFVRObject.RoundType != 0) origFVRObject.RoundType = replacementFVRObject.RoundType;

                if (replacementFVRObject.CompatibleMagazines.Count != 0 && !PRID.replaceCompatibleMagazines) origFVRObject.CompatibleMagazines.AddRange(replacementFVRObject.CompatibleMagazines);
                else if (replacementFVRObject.CompatibleMagazines.Count != 0 && PRID.replaceCompatibleMagazines) origFVRObject.CompatibleMagazines = replacementFVRObject.CompatibleMagazines;
                if (replacementFVRObject.CompatibleClips.Count != 0 && !PRID.replaceCompatibleClips) origFVRObject.CompatibleClips.AddRange(replacementFVRObject.CompatibleClips);
                else if (replacementFVRObject.CompatibleClips.Count != 0 && PRID.replaceCompatibleClips) origFVRObject.CompatibleClips = replacementFVRObject.CompatibleClips;
                if (replacementFVRObject.CompatibleSpeedLoaders.Count != 0 && !PRID.replaceCompatibleSpeedLoaders) origFVRObject.CompatibleSpeedLoaders.AddRange(replacementFVRObject.CompatibleSpeedLoaders);
                else if (replacementFVRObject.CompatibleSpeedLoaders.Count != 0 && PRID.replaceCompatibleSpeedLoaders) origFVRObject.CompatibleSpeedLoaders = replacementFVRObject.CompatibleSpeedLoaders;
                if (replacementFVRObject.CompatibleSingleRounds.Count != 0 && !PRID.replaceCompatibleSingleRounds) origFVRObject.CompatibleSingleRounds.AddRange(replacementFVRObject.CompatibleSingleRounds);
                else if (replacementFVRObject.CompatibleSingleRounds.Count != 0 && PRID.replaceCompatibleSingleRounds) origFVRObject.CompatibleSingleRounds = replacementFVRObject.CompatibleSingleRounds;
                if (replacementFVRObject.BespokeAttachments.Count != 0 && !PRID.replaceBespokeAttachments) origFVRObject.BespokeAttachments.AddRange(replacementFVRObject.BespokeAttachments);
                else if (replacementFVRObject.BespokeAttachments.Count != 0 && PRID.replaceBespokeAttachments) origFVRObject.BespokeAttachments = replacementFVRObject.BespokeAttachments;
                if (replacementFVRObject.RequiredSecondaryPieces.Count != 0 && !PRID.replaceRequiredSecondaryPieces) origFVRObject.RequiredSecondaryPieces.AddRange(replacementFVRObject.RequiredSecondaryPieces);
                else if (replacementFVRObject.RequiredSecondaryPieces.Count != 0 && PRID.replaceRequiredSecondaryPieces) origFVRObject.RequiredSecondaryPieces = replacementFVRObject.RequiredSecondaryPieces;

                if (replacementFVRObject.MinCapacityRelated != -1) origFVRObject.MinCapacityRelated = replacementFVRObject.MinCapacityRelated;
                if (replacementFVRObject.MaxCapacityRelated != -1) origFVRObject.MaxCapacityRelated = replacementFVRObject.MaxCapacityRelated;
            }

            if (PRID.updateOSple) origFVRObject.OSple = replacementFVRObject.OSple;
        }
        private void ISIDReplacement(PrefabReplacerID PRID, ItemSpawnerID origISID, ItemSpawnerID replacementISID, FVRObject origFVRObject)
        {
            if (replacementISID != null)
            {
                //Debug.Log(replacementISID);
                if (origFVRObject.SpawnedFromId != string.Empty && ManagerSingleton<IM>.Instance.SpawnerIDDic.TryGetValue(origFVRObject.SpawnedFromId, out origISID))
                {
                    if (replacementISID.DisplayName != string.Empty) origISID.DisplayName = replacementISID.DisplayName;
                    if (replacementISID.SubHeading != string.Empty) origISID.SubHeading = replacementISID.SubHeading;
                    if (replacementISID.Description != string.Empty) origISID.Description = replacementISID.Description;
                    if (replacementISID.Sprite != null) origISID.Sprite = replacementISID.Sprite;
                    if (replacementISID.Infographic != null) origISID.Infographic = replacementISID.Infographic;
                    if (replacementISID.MainObject != null) origISID.MainObject = replacementISID.MainObject;
                    if (replacementISID.SecondObject != null) origISID.SecondObject = replacementISID.SecondObject;
                    if (PRID.replaceSecondariesInstead) origISID.Secondaries = replacementISID.Secondaries;
                    else origISID.Secondaries = origISID.Secondaries.Concat(replacementISID.Secondaries).ToArray();
                    if (PRID.replaceSecondariesStringIDInstead) origISID.Secondaries_ByStringID = replacementISID.Secondaries_ByStringID;
                    else origISID.Secondaries_ByStringID.AddRange(replacementISID.Secondaries_ByStringID);
                    if (PRID.replaceModTagsInstead) origISID.ModTags = replacementISID.ModTags;
                    else origISID.ModTags.AddRange(replacementISID.ModTags);
                    if (PRID.replaceTutorialBlocksInstead) origISID.TutorialBlocks = replacementISID.TutorialBlocks;
                    else origISID.TutorialBlocks.AddRange(replacementISID.TutorialBlocks);
                    if (replacementISID.UnlockCost != 0) origISID.UnlockCost = replacementISID.UnlockCost;
                    if (PRID.updateCheckboxes)
                    {
                        if (origISID.UsesLargeSpawnPad != replacementISID.UsesLargeSpawnPad) origISID.UsesLargeSpawnPad = replacementISID.UsesLargeSpawnPad;
                        if (origISID.UsesHugeSpawnPad != replacementISID.UsesHugeSpawnPad) origISID.UsesHugeSpawnPad = replacementISID.UsesHugeSpawnPad;
                        if (origISID.IsUnlockedByDefault != replacementISID.IsUnlockedByDefault) origISID.IsUnlockedByDefault = replacementISID.IsUnlockedByDefault;
                        if (origISID.IsReward != replacementISID.IsReward) origISID.IsReward = replacementISID.IsReward;
                    }
                }
                else
                {
                    origFVRObject.SpawnedFromId = replacementISID.ItemID;
                }
            }
        }

        private void ItemSpawnerEntryReplacement(PrefabReplacerID PRID, OtherLoader.ItemSpawnerEntry origSpawnerEntry, OtherLoader.ItemSpawnerEntry replacementSpawnerEntry, FVRObject origFVRObject)
        {
            if (replacementSpawnerEntry != null)
            {
                if (origFVRObject.SpawnedFromId != string.Empty && OtherLoader.OtherLoader.SpawnerEntriesByID.TryGetValue(origFVRObject.SpawnedFromId, out origSpawnerEntry))
                {
                    if (replacementSpawnerEntry.EntryIcon != null) origSpawnerEntry.EntryIcon = replacementSpawnerEntry.EntryIcon;
                    if (replacementSpawnerEntry.DisplayName != string.Empty) origSpawnerEntry.DisplayName = replacementSpawnerEntry.DisplayName;
                    if (PRID.UpdateCheckboxesEntry)
                    {
                        origSpawnerEntry.IsDisplayedInMainEntry = replacementSpawnerEntry.IsDisplayedInMainEntry;
                        origSpawnerEntry.UsesLargeSpawnPad = replacementSpawnerEntry.UsesLargeSpawnPad;
                        origSpawnerEntry.IsReward = replacementSpawnerEntry.IsReward;
                    }
                    if (PRID.ReplaceSpawnWithIDsInstead) origSpawnerEntry.SpawnWithIDs = replacementSpawnerEntry.SpawnWithIDs;
                    else origSpawnerEntry.SpawnWithIDs.AddRange(replacementSpawnerEntry.SpawnWithIDs);
                    if (PRID.ReplaceSecondaryObjectIDsInstead) origSpawnerEntry.SecondaryObjectIDs = replacementSpawnerEntry.SecondaryObjectIDs;
                    else origSpawnerEntry.SecondaryObjectIDs.AddRange(replacementSpawnerEntry.SecondaryObjectIDs);
                    if (PRID.UpdateEntryPath)
                    {
                        origSpawnerEntry.EntryPath = replacementSpawnerEntry.EntryPath;
                        origSpawnerEntry.Page = replacementSpawnerEntry.Page;
                        origSpawnerEntry.SubCategory = replacementSpawnerEntry.SubCategory;
                    }

                    if (PRID.ReplaceModTagsEntryInstead) origSpawnerEntry.ModTags = replacementSpawnerEntry.ModTags;
                    else origSpawnerEntry.ModTags.AddRange(replacementSpawnerEntry.ModTags);
                    if (PRID.ReplaceTutorialBlocksEntryInstead) origSpawnerEntry.TutorialBlockIDs = replacementSpawnerEntry.TutorialBlockIDs;
                    else origSpawnerEntry.TutorialBlockIDs.AddRange(replacementSpawnerEntry.TutorialBlockIDs);
                }
                else
                {
                    origFVRObject.SpawnedFromId = replacementSpawnerEntry.MainObjectID;
                }
            }
        }
    }
#endif
}
