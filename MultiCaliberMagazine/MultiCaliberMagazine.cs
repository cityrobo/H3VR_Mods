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

        [Serializable]
        public class CaliberDefinition
        {
            [SearchableEnum]
            public FireArmRoundType RoundType;
            public int capacity;
            public GameObject[] DisplayBullets;
            public MeshFilter[] DisplayMeshFilters;
            public Renderer[] DisplayRenderers;
            public FVRFireArmMechanicalAccuracyClass accuracyClass;
            public FVRFireArmRecoilProfile recoilProfile;
            public FVRFireArmRecoilProfile recoilProfileStocked;
        }

        [Tooltip("Only allows insertion of mag into firearm if the caliber of the mag and the gun are equal. Also ejects the magazine if the firearms caliber is changed to something the magazine doesn't support.")]
        public bool checksFirearmCompatibility;

        private CaliberDefinition originalCaliberDefinition;
        private int currentCaliberDefinition = 0;
        private FVRFireArm fireArm = null;
        private List<CaliberDefinition> caliberDefinitionsList;

#if!(UNITY_EDITOR || UNITY_5)
        public void Start()
        {
            //Debug.Log(caliberDefinitions);
            //Debug.Log(caliberDefinitions.Length);

            caliberDefinitionsList = new List<CaliberDefinition>(caliberDefinitions);
            originalCaliberDefinition = new CaliberDefinition();

            originalCaliberDefinition.RoundType = magazine.RoundType;
            originalCaliberDefinition.capacity = magazine.m_capacity;
            originalCaliberDefinition.DisplayBullets = magazine.DisplayBullets;
            originalCaliberDefinition.DisplayMeshFilters = magazine.DisplayMeshFilters;
            originalCaliberDefinition.DisplayRenderers = magazine.DisplayRenderers;

            caliberDefinitionsList.Add(originalCaliberDefinition);

            currentCaliberDefinition = caliberDefinitionsList.Count - 1;
            //Debug.Log("Caliber Definitions: " + caliberDefinitionsList);
            //Debug.Log("Caliber Definitions: " + caliberDefinitionsList.Count);

            //Debug.Log("currentCaliberDefinition: " + currentCaliberDefinition);

            //Debug.Log("OriginalCaliberDefinition: " + caliberDefinitionsList[currentCaliberDefinition].RoundType);


            Hook();

            canvas.SetActive(false);
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
                        if (text != null)
                        {
                            StopCoroutine("ShowCaliberText");
                            StartCoroutine("ShowCaliberText");
                        }
                    }
                }
            }

            if (magazine.State == FVRFireArmMagazine.MagazineState.Locked && fireArm == null)
            {
                fireArm = magazine.FireArm;
                originalCaliberDefinition.accuracyClass = fireArm.AccuracyClass;
                originalCaliberDefinition.recoilProfile = fireArm.RecoilProfile;
                originalCaliberDefinition.recoilProfileStocked = fireArm.RecoilProfileStocked;

                if(caliberDefinitionsList[currentCaliberDefinition].accuracyClass != FVRFireArmMechanicalAccuracyClass.None)
                    fireArm.AccuracyClass = caliberDefinitionsList[currentCaliberDefinition].accuracyClass;
                if (caliberDefinitionsList[currentCaliberDefinition].recoilProfile != null)
                    fireArm.RecoilProfile = caliberDefinitionsList[currentCaliberDefinition].recoilProfile;
                if (caliberDefinitionsList[currentCaliberDefinition].recoilProfileStocked != null)
                    fireArm.RecoilProfileStocked = caliberDefinitionsList[currentCaliberDefinition].recoilProfileStocked;

            }
            else if (magazine.State == FVRFireArmMagazine.MagazineState.Free && fireArm != null)
            {
                fireArm.AccuracyClass = originalCaliberDefinition.accuracyClass;
                fireArm.RecoilProfile = originalCaliberDefinition.recoilProfile;
                fireArm.RecoilProfileStocked = originalCaliberDefinition.recoilProfileStocked;

                fireArm = null;
            }
            else if (magazine.State == FVRFireArmMagazine.MagazineState.Locked && fireArm != null && fireArm.RoundType != caliberDefinitionsList[currentCaliberDefinition].RoundType)
            {
                if (!SetCartridge(fireArm.RoundType) && magazine.m_numRounds == 0 && checksFirearmCompatibility)
                {
                    fireArm.EjectMag();
                }
            }
        }

        public void NextCartridge()
        {
            currentCaliberDefinition++;
            if (currentCaliberDefinition >= caliberDefinitionsList.Count)
            {
                currentCaliberDefinition = 0;
            }

            //Debug.Log(currentCaliberDefinition);

            ConfigureMagazine(currentCaliberDefinition);
        }

        public bool SetCartridge(FireArmRoundType fireArmRoundType)
        {
            if (magazine.m_numRounds != 0) return false;
            
            int chosenDefinition = 0;
            foreach (var caliberDefinition in caliberDefinitionsList)
            {
                if (caliberDefinition.RoundType != fireArmRoundType)
                {
                    chosenDefinition++;
                }
                else break;
            }

            if (chosenDefinition == caliberDefinitionsList.Count)
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
            magazine.RoundType = caliberDefinitionsList[CaliberDefinitionIndex].RoundType;
            magazine.m_capacity = caliberDefinitionsList[CaliberDefinitionIndex].capacity;
            magazine.DisplayBullets = caliberDefinitionsList[CaliberDefinitionIndex].DisplayBullets;
            magazine.DisplayMeshFilters = caliberDefinitionsList[CaliberDefinitionIndex].DisplayMeshFilters;
            magazine.DisplayRenderers = caliberDefinitionsList[CaliberDefinitionIndex].DisplayRenderers;
        }

        public void Unhook()
        {
            On.FistVR.FVRFireArmRound.OnTriggerEnter -= FVRFireArmRound_OnTriggerEnter;

            if (checksFirearmCompatibility)
            {
                On.FistVR.FVRFireArmReloadTriggerMag.OnTriggerEnter -= FVRFireArmReloadTriggerMag_OnTriggerEnter;
            }
        }

        public void Hook()
        {
            On.FistVR.FVRFireArmRound.OnTriggerEnter += FVRFireArmRound_OnTriggerEnter;

            if (checksFirearmCompatibility)
            {
                On.FistVR.FVRFireArmReloadTriggerMag.OnTriggerEnter += FVRFireArmReloadTriggerMag_OnTriggerEnter;
            }
        }

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

        public IEnumerator ShowCaliberText()
        {
            FireArmRoundType roundType = caliberDefinitionsList[currentCaliberDefinition].RoundType;
            if (AM.SRoundDisplayDataDic.ContainsKey(roundType))
            {
                string name = AM.SRoundDisplayDataDic[roundType].DisplayName;

                text.text = name;
                canvas.SetActive(true);
                yield return new WaitForSeconds(1);
            }

            canvas.SetActive(false);
            yield return null;
        }
#endif
    }
}
