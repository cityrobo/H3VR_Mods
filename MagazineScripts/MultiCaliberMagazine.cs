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
    public class MultiCaliberMagazine : MonoBehaviour
    {
        public FVRFireArmMagazine magazine;

        public CaliberDefinition[] caliberDefinitions;

        [Header("Text field that shows current selected caliber on switching calibers manually.")]
        public GameObject canvas;
        public Text text;
        public bool alwaysShowText;

        public int currentCaliberDefinition = 0;
        [Serializable]
        public class CaliberDefinition
        {
            [SearchableEnum]
            public FireArmRoundType RoundType;
            public int capacity;
            public GameObject[] DisplayBullets;
            public MeshFilter[] DisplayMeshFilters;
            public Renderer[] DisplayRenderers;
            [SearchableEnum]
            public FVRFireArmMechanicalAccuracyClass accuracyClass;
            public FVRFireArmRecoilProfile recoilProfile;
            public FVRFireArmRecoilProfile recoilProfileStocked;
            [Tooltip("Only replaces firing sounds!")]
            public FVRFirearmAudioSet ReplacementFiringSounds;
            public FVRObject objectWrapper;
        }
        /*
        [SearchableEnum]
        public FireArmRoundClass defaultRoundClass;
        */
        [Tooltip("Only allows insertion of mag into firearm if the caliber of the mag and the gun are equal")]
        public bool checksFirearmCompatibility;

        //private CaliberDefinition originalCaliberDefinition;

        [Header("MeatKit required stuff. Use ContextMenu to populate.")]
        [SearchableEnum]
        public FireArmRoundType[] roundTypes;
        public int[] capacities;
        public GameObject[][] DisplayBulletss;
        public MeshFilter[][] DisplayMeshFilterss;
        public Renderer[][] DisplayRendererss;
        [SearchableEnum]
        public FVRFireArmMechanicalAccuracyClass[] accuracyClasses;
        public FVRFireArmRecoilProfile[] recoilProfiles;
        public FVRFireArmRecoilProfile[] recoilProfilesStocked;
        [Tooltip("Only replaces firing sounds!")]
        public FVRFirearmAudioSet[] ReplacementFiringSoundss;
        public FVRObject[] objectWrappers;

        public bool isMeatKit = false;

        private FVRFireArm _fireArm = null;
        private List<CaliberDefinition> _caliberDefinitionsList;

        private FVRFireArmMechanicalAccuracyClass _origAccuracyClass;

        private FVRFireArmRecoilProfile _origRecoilProfile;
        private FVRFireArmRecoilProfile _origRecoilProfileStocked;

        private AudioEvent _origFiringSounds;
        private AudioEvent _origSuppressedSounds;
        private AudioEvent _origLowPressureSounds;

        private bool _isDebug = true;

        [ContextMenu("Populate MeatKit Lists")]
        public void PopulateMeatKitLists()
        {
            int definitionCount = caliberDefinitions.Length;

            roundTypes = new FireArmRoundType[definitionCount];
            capacities = new int[definitionCount];
            DisplayBulletss = new GameObject[definitionCount][];
            DisplayMeshFilterss = new MeshFilter[definitionCount][];
            DisplayRendererss = new Renderer[definitionCount][];
            accuracyClasses = new FVRFireArmMechanicalAccuracyClass[definitionCount];
            recoilProfiles = new FVRFireArmRecoilProfile[definitionCount];
            recoilProfilesStocked = new FVRFireArmRecoilProfile[definitionCount];
            ReplacementFiringSoundss = new FVRFirearmAudioSet[definitionCount];
            objectWrappers = new FVRObject[definitionCount];

            for (int i = 0; i < definitionCount; i++)
            {
                roundTypes[i] = caliberDefinitions[i].RoundType;
                capacities[i] = caliberDefinitions[i].capacity;
                DisplayBulletss[i] = caliberDefinitions[i].DisplayBullets;
                DisplayMeshFilterss[i] = caliberDefinitions[i].DisplayMeshFilters;
                DisplayRendererss[i] = caliberDefinitions[i].DisplayRenderers;
                accuracyClasses[i] = caliberDefinitions[i].accuracyClass;
                recoilProfiles[i] = caliberDefinitions[i].recoilProfile;
                recoilProfilesStocked[i] = caliberDefinitions[i].recoilProfileStocked;
                ReplacementFiringSoundss[i] = caliberDefinitions[i].ReplacementFiringSounds;

                objectWrappers[i] = caliberDefinitions[i].objectWrapper;
            }

            if (_isDebug)
            {
                foreach (var DisplayBullets in DisplayBulletss)
                {
                    foreach (var DisplayBullet in DisplayBullets)
                    {
                        Debug.Log("DisplayBullets: " + DisplayBullet.name);
                    }
                }
            }

            isMeatKit = true;
        }

#if!(UNITY_EDITOR || UNITY_5)
        public void Awake()
        {
            Hook();
        }

        public void Start()
        {
            if (!isMeatKit) _caliberDefinitionsList = new List<CaliberDefinition>(caliberDefinitions);
            else _caliberDefinitionsList = CreateListFromMeatKitDefines();

            PrepareCaliberDefinitions();
            if (!alwaysShowText && canvas != null) canvas.SetActive(false);
        }

        public void OnDestroy()
        {
            Unhook();
        }

        public void Update()
        {
            FVRViveHand hand = magazine.m_hand;
            if (hand != null)
            {
                if (magazine.m_numRounds == 0)
                {
                    if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f)
                    {
                        NextCartridge();
                        if (!alwaysShowText && text != null)
                        {
                            StopCoroutine("ShowCaliberText");
                            StartCoroutine("ShowCaliberText");
                        }
                    }
                }
            }

            if (magazine.State == FVRFireArmMagazine.MagazineState.Locked && _fireArm == null)
            {
                _fireArm = magazine.FireArm;

                _origAccuracyClass = _fireArm.AccuracyClass;

                _origRecoilProfile = _fireArm.RecoilProfile;
                _origRecoilProfileStocked = _fireArm.RecoilProfileStocked;

                _origFiringSounds = _fireArm.AudioClipSet.Shots_Main;
                _origSuppressedSounds = _fireArm.AudioClipSet.Shots_Suppressed;
                _origLowPressureSounds = _fireArm.AudioClipSet.Shots_LowPressure;

                if (_caliberDefinitionsList[currentCaliberDefinition].accuracyClass != FVRFireArmMechanicalAccuracyClass.None)
                    _fireArm.AccuracyClass = _caliberDefinitionsList[currentCaliberDefinition].accuracyClass;
                if (_caliberDefinitionsList[currentCaliberDefinition].recoilProfile != null)
                    _fireArm.RecoilProfile = _caliberDefinitionsList[currentCaliberDefinition].recoilProfile;
                if (_caliberDefinitionsList[currentCaliberDefinition].recoilProfileStocked != null)
                    _fireArm.RecoilProfileStocked = _caliberDefinitionsList[currentCaliberDefinition].recoilProfileStocked;

                if (_caliberDefinitionsList[currentCaliberDefinition].ReplacementFiringSounds != null) ReplaceFiringSounds(_caliberDefinitionsList[currentCaliberDefinition].ReplacementFiringSounds);

            }
            else if (magazine.State == FVRFireArmMagazine.MagazineState.Free && _fireArm != null)
            {
                _fireArm.AccuracyClass = _origAccuracyClass;
                _fireArm.RecoilProfile = _origRecoilProfile;
                _fireArm.RecoilProfileStocked = _origRecoilProfileStocked;

                _fireArm.AudioClipSet.Shots_Main = _origFiringSounds;
                _fireArm.AudioClipSet.Shots_Suppressed = _origSuppressedSounds;
                _fireArm.AudioClipSet.Shots_LowPressure = _origLowPressureSounds;

                _fireArm = null;
            }
            else if (magazine.State == FVRFireArmMagazine.MagazineState.Locked && _fireArm != null && _fireArm.RoundType != _caliberDefinitionsList[currentCaliberDefinition].RoundType)
            {
                if (!SetCartridge(_fireArm.RoundType) && magazine.m_numRounds == 0 && checksFirearmCompatibility)
                {
                    _fireArm.EjectMag();
                }
            }

            if (alwaysShowText && text != null)
            {
                FireArmRoundType roundType = _caliberDefinitionsList[currentCaliberDefinition].RoundType;
                if (AM.SRoundDisplayDataDic.ContainsKey(roundType))
                {
                    string name = AM.SRoundDisplayDataDic[roundType].DisplayName;

                    text.text = name;
                }
            }
        }

        public void NextCartridge()
        {
            currentCaliberDefinition++;
            if (currentCaliberDefinition >= _caliberDefinitionsList.Count)
            {
                currentCaliberDefinition = 0;
            }

            ConfigureMagazine(currentCaliberDefinition);
        }

        public bool SetCartridge(FireArmRoundType fireArmRoundType)
        {
            if (magazine.m_numRounds != 0) return false;
            
            int chosenDefinition = 0;
            foreach (var caliberDefinition in _caliberDefinitionsList)
            {
                if (caliberDefinition.RoundType != fireArmRoundType)
                {
                    chosenDefinition++;
                }
                else break;
            }

            if (chosenDefinition == _caliberDefinitionsList.Count)
            {
                return false;
            }
            else 
            {
                ConfigureMagazine(chosenDefinition);
                currentCaliberDefinition = chosenDefinition;
                return true;
            }
        }

        public void ConfigureMagazine(int CaliberDefinitionIndex)
        {
            magazine.RoundType = _caliberDefinitionsList[CaliberDefinitionIndex].RoundType;
            if (_caliberDefinitionsList[CaliberDefinitionIndex].capacity > 0)
                magazine.m_capacity = _caliberDefinitionsList[CaliberDefinitionIndex].capacity;
            if (_caliberDefinitionsList[CaliberDefinitionIndex].DisplayBullets.Length > 0)
            {
                magazine.m_roundInsertionTarget.localPosition = _caliberDefinitionsList[CaliberDefinitionIndex].DisplayBullets[0].transform.localPosition;
                magazine.m_roundInsertionTarget.localRotation = _caliberDefinitionsList[CaliberDefinitionIndex].DisplayBullets[0].transform.localRotation;
                magazine.DisplayBullets = _caliberDefinitionsList[CaliberDefinitionIndex].DisplayBullets;
                magazine.DisplayMeshFilters = _caliberDefinitionsList[CaliberDefinitionIndex].DisplayMeshFilters;
                magazine.DisplayRenderers = _caliberDefinitionsList[CaliberDefinitionIndex].DisplayRenderers;

                magazine.m_DisplayStartPositions = new Vector3[_caliberDefinitionsList[CaliberDefinitionIndex].DisplayBullets.Length];
                for (int i = 0; i < _caliberDefinitionsList[CaliberDefinitionIndex].DisplayBullets.Length; i++)
                {
                    if (_caliberDefinitionsList[CaliberDefinitionIndex].DisplayBullets[i] != null)
                    {
                        magazine.m_DisplayStartPositions[i] = _caliberDefinitionsList[CaliberDefinitionIndex].DisplayBullets[i].transform.localPosition;
                    }
                }
            }
            magazine.ObjectWrapper = _caliberDefinitionsList[CaliberDefinitionIndex].objectWrapper;
        }
        public void ReplaceFiringSounds(FVRFirearmAudioSet set)
        {
            if (set.Shots_Main.Clips.Count > 0) _fireArm.AudioClipSet.Shots_Main = set.Shots_Main;
            if (set.Shots_Suppressed.Clips.Count > 0) _fireArm.AudioClipSet.Shots_Suppressed = set.Shots_Suppressed;
            if (set.Shots_LowPressure.Clips.Count > 0) _fireArm.AudioClipSet.Shots_LowPressure = set.Shots_LowPressure;
        }
        public void Unhook()
        {
#if !(MEATKIT)
            On.FistVR.FVRFireArmRound.OnTriggerEnter -= FVRFireArmRound_OnTriggerEnter;
            //On.FistVR.FVRFireArmMagazine.ReloadMagWithList -= FVRFireArmMagazine_ReloadMagWithList;

            if (checksFirearmCompatibility)
            {
                On.FistVR.FVRFireArmReloadTriggerMag.OnTriggerEnter -= FVRFireArmReloadTriggerMag_OnTriggerEnter;
            }
#endif
        }

        public void Hook()
        {
#if !(MEATKIT)
            On.FistVR.FVRFireArmRound.OnTriggerEnter += FVRFireArmRound_OnTriggerEnter;
            //On.FistVR.FVRFireArmMagazine.ReloadMagWithList += FVRFireArmMagazine_ReloadMagWithList;

            if (checksFirearmCompatibility)
            {
                On.FistVR.FVRFireArmReloadTriggerMag.OnTriggerEnter += FVRFireArmReloadTriggerMag_OnTriggerEnter;
            }
#endif
        }
        /*
        private void FVRFireArmMagazine_ReloadMagWithList(On.FistVR.FVRFireArmMagazine.orig_ReloadMagWithList orig, FVRFireArmMagazine self, List<FireArmRoundClass> list)
        {
            if (magazine == self)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = defaultRoundClass;
                }
            }
            orig(self, list);
        }
        */
#if !(MEATKIT)
        private void FVRFireArmReloadTriggerMag_OnTriggerEnter(On.FistVR.FVRFireArmReloadTriggerMag.orig_OnTriggerEnter orig, FVRFireArmReloadTriggerMag self, Collider collider)
        {
            if (this.magazine == self.Magazine)
            {
                if (!(self.Magazine != null) || !(self.Magazine.FireArm == null) || !(self.Magazine.QuickbeltSlot == null) || !(collider.gameObject.tag == "FVRFireArmReloadTriggerWell"))
                    return;
                FVRFireArmReloadTriggerWell component = collider.gameObject.GetComponent<FVRFireArmReloadTriggerWell>();
                bool flag = false;
                if (component != null && !self.Magazine.IsBeltBox && component.FireArm.HasBelt)
                    flag = true;
                if (!(component != null) || component.IsBeltBox != self.Magazine.IsBeltBox || !(component.FireArm != null) || !(component.FireArm.Magazine == null) || flag)
                    return;
                FireArmMagazineType fireArmMagazineType = component.FireArm.MagazineType;
                if (component.UsesTypeOverride)
                    fireArmMagazineType = component.TypeOverride;
                if (fireArmMagazineType != self.Magazine.MagazineType || (double)component.FireArm.EjectDelay > 0.0 && !(self.Magazine != component.FireArm.LastEjectedMag) || !(component.FireArm.Magazine == null))
                    return;
                if (checksFirearmCompatibility && magazine.RoundType != component.FireArm.RoundType)
                    return;
                self.Magazine.Load(component.FireArm);
            }
            else orig(self, collider);
        }

        private void FVRFireArmRound_OnTriggerEnter(On.FistVR.FVRFireArmRound.orig_OnTriggerEnter orig, FVRFireArmRound self, Collider collider)
        {
            if (self.IsSpent)
                return;
            if (self.isManuallyChamberable && !self.IsSpent && (UnityEngine.Object)self.HoveredOverChamber == (UnityEngine.Object)null && (UnityEngine.Object)self.m_hoverOverReloadTrigger == (UnityEngine.Object)null && !self.IsSpent && collider.gameObject.CompareTag("FVRFireArmChamber"))
            {
                FVRFireArmChamber component = collider.gameObject.GetComponent<FVRFireArmChamber>();
                if (component.RoundType == self.RoundType && component.IsManuallyChamberable && component.IsAccessible && !component.IsFull)
                    self.HoveredOverChamber = component;
            }
            if (self.isMagazineLoadable && (UnityEngine.Object)self.HoveredOverChamber == (UnityEngine.Object)null && !self.IsSpent && collider.gameObject.CompareTag("FVRFireArmMagazineReloadTrigger"))
            {
                FVRFireArmMagazineReloadTrigger component = collider.gameObject.GetComponent<FVRFireArmMagazineReloadTrigger>();
                if (component.IsClipTrigger)
                {
                    if ((UnityEngine.Object)component != (UnityEngine.Object)null && (UnityEngine.Object)component.Clip != (UnityEngine.Object)null && component.Clip.RoundType == self.RoundType && !component.Clip.IsFull() && ((UnityEngine.Object)component.Clip.FireArm == (UnityEngine.Object)null || component.Clip.IsDropInLoadable))
                        self.m_hoverOverReloadTrigger = component;
                }
                else if (component.IsSpeedloaderTrigger)
                {
                    if (!component.SpeedloaderChamber.IsLoaded)
                        self.m_hoverOverReloadTrigger = component;
                }
                else if ((UnityEngine.Object)component != (UnityEngine.Object)null && (UnityEngine.Object)component.Magazine != (UnityEngine.Object)null && component.Magazine.RoundType == self.RoundType && !component.Magazine.IsFull() && ((UnityEngine.Object)component.Magazine.FireArm == (UnityEngine.Object)null || component.Magazine.IsDropInLoadable))
                    self.m_hoverOverReloadTrigger = component;
                else if ((UnityEngine.Object)component != (UnityEngine.Object)null && component.Magazine == magazine && !component.Magazine.IsFull() && ((UnityEngine.Object)component.Magazine.FireArm == (UnityEngine.Object)null || component.Magazine.IsDropInLoadable))
                {
                    MultiCaliberMagazine multiCaliberMagazine = component.Magazine.GetComponent<MultiCaliberMagazine>();
                    if (multiCaliberMagazine.SetCartridge(self.RoundType))
                    {
                        self.m_hoverOverReloadTrigger = component;
                    }
                }
            }
            if (!self.isPalmable || self.ProxyRounds.Count >= self.MaxPalmedAmount || self.IsSpent || !collider.gameObject.CompareTag(nameof(FVRFireArmRound)))
                return;
            FVRFireArmRound component1 = collider.gameObject.GetComponent<FVRFireArmRound>();
            if (component1.RoundType != self.RoundType || component1.IsSpent || !((UnityEngine.Object)component1.QuickbeltSlot == (UnityEngine.Object)null))
                return;
            self.HoveredOverRound = component1;
        }
#endif
        public IEnumerator ShowCaliberText()
        {
            FireArmRoundType roundType = _caliberDefinitionsList[currentCaliberDefinition].RoundType;
            if (AM.SRoundDisplayDataDic.ContainsKey(roundType))
            {
                string name = AM.SRoundDisplayDataDic[roundType].DisplayName;

                text.text = name;
                canvas.SetActive(true);
                yield return new WaitForSeconds(1f);
            }

            canvas.SetActive(false);
            yield return null;
        }

        void PrepareCaliberDefinitions()
        {
            foreach (var caliberDefinition in caliberDefinitions)
            {
                for (int i = 0; i < caliberDefinition.DisplayMeshFilters.Length; i++)
                {
                    if (!magazine.DisplayMeshFilters.Contains(caliberDefinition.DisplayMeshFilters[i])) caliberDefinition.DisplayMeshFilters[i].mesh = null;
                }
                for (int i = 0; i < caliberDefinition.DisplayRenderers.Length; i++)
                {
                    if (!magazine.DisplayRenderers.Contains(caliberDefinition.DisplayRenderers[i])) caliberDefinition.DisplayRenderers[i].material = null;
                }
            }
        }

        List<CaliberDefinition> CreateListFromMeatKitDefines()
        {
            List<CaliberDefinition> caliberDefinitions = new List<CaliberDefinition>();
            for (int i = 0; i < roundTypes.Length; i++)
            {
                CaliberDefinition caliberDefinition = new CaliberDefinition();
                caliberDefinition.RoundType = roundTypes[i];
                caliberDefinition.capacity = capacities[i];
                caliberDefinition.DisplayBullets = DisplayBulletss[i];
                caliberDefinition.DisplayMeshFilters = DisplayMeshFilterss[i];
                caliberDefinition.DisplayRenderers = DisplayRendererss[i];
                caliberDefinition.accuracyClass = accuracyClasses[i];
                caliberDefinition.recoilProfile = recoilProfiles[i];
                caliberDefinition.recoilProfileStocked = recoilProfilesStocked[i];
                caliberDefinition.objectWrapper = objectWrappers[i];
            }

            return caliberDefinitions;
        }
#endif
    }
}
