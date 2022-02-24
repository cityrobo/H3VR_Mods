using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BepInEx;

namespace Cityrobo
{
#if !(DEBUG || UNITY_EDITOR || UNITY_5)
    [BepInPlugin("h3vr.cityrobo.prefab_replacer", "PrefabReplacer Script", "1.0.0")]
    public class PrefabReplacer : BaseUnityPlugin
    {
        public string origItemID = "G17";
        public string replancementItemID = "gl";
        public List<PrefabReplacerID> PRIDs;

        //private bool otherloaderReady = false;
        private Dictionary<string, PrefabReplacerID> loadedPrefabReplacerIDs;
        public PrefabReplacer()
        {
            Logger.LogInfo("PrefabReplacer Script loaded!");

            loadedPrefabReplacerIDs = new Dictionary<string, PrefabReplacerID>();
            StartCoroutine(WaitForOtherloader());
            
        }

        /*
        public void Update()
        {
            
            if (OtherLoader.LoaderStatus.GetLoaderProgress() == 1)
            {
                Logger.LogInfo("Starting PrefabReplacement!");
                PRIDs = new List<PrefabReplacerID>(Resources.FindObjectsOfTypeAll<PrefabReplacerID>());
                PRIDs.Add(new PrefabReplacerID("FTW.50BMGFAKE.SLAP", "FTW.50BMG.SLAP"));
                PRIDs.Add(new PrefabReplacerID(origItemID, replancementItemID));
                foreach (var PRID in PRIDs)
                {
                    ReplacePrefabViaID(PRID);
                }
            }
            
        }
        */

        IEnumerator WaitForOtherloader()
        {
            while (OtherLoader.LoaderStatus.GetLoaderProgress() != 1)
            {
                yield return null;
            }

            Logger.LogInfo("Starting PrefabReplacement!");
            PRIDs = new List<PrefabReplacerID>(Resources.FindObjectsOfTypeAll<PrefabReplacerID>());
            //PRIDs.Add(new PrefabReplacerID("FTW.50BMGFAKE.SLAP", "FTW.50BMG.SLAP"));
            //PRIDs.Add(new PrefabReplacerID(origItemID, replancementItemID));
            foreach (var PRID in PRIDs)
            {
                ReplacePrefabViaID(PRID);
            }
        }

        void ReplacePrefabViaID(PrefabReplacerID PRID)
        {
            loadedPrefabReplacerIDs.Add(PRID.originalItemID, PRID);

            FVRObject origFVRObject = IM.OD[PRID.originalItemID];
            FVRObject replacementFVRObject;
            if (PRID.replacementFVRObject != null) replacementFVRObject = PRID.replacementFVRObject;
            else replacementFVRObject = IM.OD[PRID.replacementItemID];

            

            origFVRObject.m_anvilPrefab.Guid = replacementFVRObject.m_anvilPrefab.Guid;
            origFVRObject.m_anvilPrefab.Bundle = replacementFVRObject.m_anvilPrefab.Bundle;
            origFVRObject.m_anvilPrefab.AssetName = replacementFVRObject.m_anvilPrefab.AssetName;
            if (replacementFVRObject.DisplayName != String.Empty) origFVRObject.DisplayName = replacementFVRObject.DisplayName;

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
            ItemSpawnerID replacementISID;
            if (PRID.replacementItemSpawnerID != null) replacementISID = PRID.replacementItemSpawnerID;
            else ManagerSingleton<IM>.Instance.SpawnerIDDic.TryGetValue(replacementFVRObject.SpawnedFromId, out replacementISID);

            ItemSpawnerID origISID = null;
            if (replacementISID != null)
            {
                Debug.Log(replacementISID);
                if (origFVRObject.SpawnedFromId != string.Empty && ManagerSingleton<IM>.Instance.SpawnerIDDic.TryGetValue(origFVRObject.SpawnedFromId, out origISID))
                {
                    if (replacementISID.DisplayName != String.Empty) origISID.DisplayName = replacementISID.DisplayName;
                    if (replacementISID.SubHeading != String.Empty) origISID.SubHeading = replacementISID.SubHeading;
                    if (replacementISID.Description != String.Empty) origISID.Description = replacementISID.Description;
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

            if (PRID.updateOSple) origFVRObject.OSple = replacementFVRObject.OSple;

            Logger.LogInfo("PrefabRepacerID " + PRID.name + " replaced " + PRID.originalItemID + " with " + PRID.replacementItemID);
        }
    }
#endif
}
