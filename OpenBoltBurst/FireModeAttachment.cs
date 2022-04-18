using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Cityrobo

{
    public class FireModeAttachment : MonoBehaviour
    {
        public FVRFireArmAttachment attachment;

        public ClosedBoltWeapon.FireSelectorModeType[] FireSelectorModeTypes;
        public int[] burstAmounts;

        public bool addsSelectorPositions = false;
        public float[] selectorPositions;

        private bool attached = false;
        private FVRFireArm firearm;
        private Handgun.FireSelectorMode[] originalHandgunFireModes;
        private ClosedBoltWeapon.FireSelectorMode[] originalClosedBoltFireModes;
        private OpenBoltReceiver.FireSelectorMode[] originalOpenBoltFireModes;
        private List<H3VRUtils.OpenBoltBurstFire> openBoltBursts;

        private bool handgunHadFireSelectorButton = false;

        public void Awake()
        {
            openBoltBursts = new List<H3VRUtils.OpenBoltBurstFire>();
        }

        public void Update()
        {
            if (!attached && attachment.curMount != null)
            {
                //Debug.Log(attachment.curMount);
                ChangeFireMode(true);
                attached = true;
            }

            if (attached && attachment.curMount == null)
            {
                //Debug.Log(attachment.curMount);
                ChangeFireMode(false);
                attached = false;

                firearm = null;
            }
        }
        public void ChangeFireMode(bool activate)
        {
            if (attachment.curMount != null) firearm = attachment.curMount.GetRootMount().MyObject as FVRFireArm;

            if (firearm is OpenBoltReceiver) ChangeFireMode(firearm as OpenBoltReceiver, activate);
            else if (firearm is ClosedBoltWeapon) ChangeFireMode(firearm as ClosedBoltWeapon, activate);
            else if (firearm is Handgun) ChangeFireMode(firearm as Handgun, activate);
        }

        public void ChangeFireMode(OpenBoltReceiver openBoltReceiver, bool activate)
        {
            if (activate)
            {
                originalOpenBoltFireModes = openBoltReceiver.FireSelector_Modes;
                int burstIndex = 0;
                int selectorPosIndex = 0;

                foreach (var FireSelectorModeType in FireSelectorModeTypes)
                {
                    OpenBoltReceiver.FireSelectorMode newFireSelectorMode = new OpenBoltReceiver.FireSelectorMode();
                    switch (FireSelectorModeType)
                    {
                        case ClosedBoltWeapon.FireSelectorModeType.Safe:
                            newFireSelectorMode.ModeType = OpenBoltReceiver.FireSelectorModeType.Safe;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.Single:
                            newFireSelectorMode.ModeType = OpenBoltReceiver.FireSelectorModeType.Single;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.Burst:
                            newFireSelectorMode.ModeType = OpenBoltReceiver.FireSelectorModeType.FullAuto;
                            /*
                            GameObject openBoltBurstGM = new GameObject("openBoltBurstGM");
                            openBoltBurstGM.transform.SetParent(openBoltReceiver.transform);
                            openBoltBurstGM.transform.localPosition = Vector3.zero;
                            openBoltBurstGM.transform.localRotation = Quaternion.identity;
                            openBoltBurstGM.SetActive(false);

                            H3VRUtils.OpenBoltBurstFire openBoltBurst = openBoltBurstGM.AddComponent<H3VRUtils.OpenBoltBurstFire>();
                            */
                            H3VRUtils.OpenBoltBurstFire openBoltBurst = openBoltReceiver.gameObject.AddComponent<H3VRUtils.OpenBoltBurstFire>();
                            openBoltBursts.Add(openBoltBurst);
                            openBoltBurst.Receiver = openBoltReceiver;
                            openBoltBurst.SelectorSetting = openBoltReceiver.FireSelector_Modes.Length;
                            openBoltBurst.BurstAmt = burstAmounts[burstIndex];
                            //openBoltBurstGM.SetActive(true);

                            burstIndex++;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.FullAuto:
                            newFireSelectorMode.ModeType = OpenBoltReceiver.FireSelectorModeType.FullAuto;
                            break;
                        default:
                            Debug.LogError("FireSelectorMode not supported: " + FireSelectorModeType);
                            continue;
                    }

                    if (!addsSelectorPositions) newFireSelectorMode.SelectorPosition = originalOpenBoltFireModes[originalOpenBoltFireModes.Length - 1].SelectorPosition;
                    else newFireSelectorMode.SelectorPosition = selectorPositions[selectorPosIndex];
                    openBoltReceiver.FireSelector_Modes = openBoltReceiver.FireSelector_Modes.Concat(new OpenBoltReceiver.FireSelectorMode[] { newFireSelectorMode }).ToArray();

                    selectorPosIndex++;
                }
            }
            else
            {
                openBoltReceiver.m_fireSelectorMode = originalOpenBoltFireModes.Length - 1;
                openBoltReceiver.FireSelector_Modes = originalOpenBoltFireModes;
                if (openBoltBursts.Count != 0)
                {
                    foreach (var openBoltBurst in openBoltBursts)
                    {
                        Destroy(openBoltBurst);
                    }

                    openBoltBursts.Clear();
                }
            }
        }

        public void ChangeFireMode(ClosedBoltWeapon closedBoltWeapon, bool activate)
        {
            if (activate)
            {
                originalClosedBoltFireModes = closedBoltWeapon.FireSelector_Modes;

                int burstIndex = 0;
                int selectorPosIndex = 0;
                foreach (var FireSelectorModeType in FireSelectorModeTypes)
                {
                    ClosedBoltWeapon.FireSelectorMode newFireSelectorMode = new ClosedBoltWeapon.FireSelectorMode();
                    switch (FireSelectorModeType)
                    {
                        case ClosedBoltWeapon.FireSelectorModeType.Safe:
                            newFireSelectorMode.ModeType = ClosedBoltWeapon.FireSelectorModeType.Safe;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.Single:
                            newFireSelectorMode.ModeType = ClosedBoltWeapon.FireSelectorModeType.Single;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.Burst:
                            newFireSelectorMode.ModeType = ClosedBoltWeapon.FireSelectorModeType.Burst;
                            newFireSelectorMode.BurstAmount = burstAmounts[burstIndex];
                            burstIndex++;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.FullAuto:
                            newFireSelectorMode.ModeType = ClosedBoltWeapon.FireSelectorModeType.FullAuto;
                            break;
                        default:
                            Debug.LogError("FireSelectorMode not supported: " + FireSelectorModeType);
                            continue;
                    }
                    if (!addsSelectorPositions) newFireSelectorMode.SelectorPosition = originalClosedBoltFireModes[originalClosedBoltFireModes.Length - 1].SelectorPosition;
                    else newFireSelectorMode.SelectorPosition = selectorPositions[selectorPosIndex];
                    closedBoltWeapon.FireSelector_Modes = originalClosedBoltFireModes.Concat(new ClosedBoltWeapon.FireSelectorMode[] { newFireSelectorMode }).ToArray();

                    selectorPosIndex++;
                }
            }
            else
            {
                closedBoltWeapon.m_fireSelectorMode = originalClosedBoltFireModes.Length - 1;
                closedBoltWeapon.FireSelector_Modes = originalClosedBoltFireModes;
            }
        }

        public void ChangeFireMode(Handgun handgun, bool activate)
        {
            if (activate)
            {
                handgunHadFireSelectorButton = handgun.HasFireSelector;

                originalHandgunFireModes = handgun.FireSelectorModes;
                handgun.HasFireSelector = true;

                if (handgun.FireSelector == null) handgun.FireSelector = new GameObject().transform;
                int burstIndex = 0;
                int selectorPosIndex = 0;

                foreach (var FireSelectorModeType in FireSelectorModeTypes)
                {
                    Handgun.FireSelectorMode newFireSelectorMode = new Handgun.FireSelectorMode();
                    switch (FireSelectorModeType)
                    {
                        case ClosedBoltWeapon.FireSelectorModeType.Safe:
                            newFireSelectorMode.ModeType = Handgun.FireSelectorModeType.Safe;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.Single:
                            newFireSelectorMode.ModeType = Handgun.FireSelectorModeType.Single;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.Burst:
                            newFireSelectorMode.ModeType = Handgun.FireSelectorModeType.Burst;
                            newFireSelectorMode.BurstAmount = burstAmounts[burstIndex];
                            burstIndex++;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.FullAuto:
                            newFireSelectorMode.ModeType = Handgun.FireSelectorModeType.FullAuto;
                            break;
                        default:
                            Debug.LogError("FireSelectorMode not supported: " + FireSelectorModeType);
                            continue;
                    }

                    if (!addsSelectorPositions) newFireSelectorMode.SelectorPosition = originalHandgunFireModes[originalHandgunFireModes.Length - 1].SelectorPosition;
                    else newFireSelectorMode.SelectorPosition = selectorPositions[selectorPosIndex];
                    handgun.FireSelectorModes = handgun.FireSelectorModes.Concat(new Handgun.FireSelectorMode[] { newFireSelectorMode }).ToArray();

                    selectorPosIndex++;
                }
            }
            else
            {
                handgun.m_fireSelectorMode = originalHandgunFireModes.Length - 1;
                handgun.FireSelectorModes = originalHandgunFireModes;

                if (!handgunHadFireSelectorButton)
                {
                    Destroy(handgun.FireSelector.gameObject);
                    handgun.HasFireSelector = false;
                }
            }
        }
        /*
        public void ChangeFireMode(TubeFedShotgun shotgun, bool activate)
        {
            if (activate)
            {
                originalClosedBoltFireModes = shotgun.FireSelector_Modes;

                int burstIndex = 0;
                int selectorPosIndex = 0;
                foreach (var FireSelectorModeType in FireSelectorModeTypes)
                {
                    ClosedBoltWeapon.FireSelectorMode newFireSelectorMode = new ClosedBoltWeapon.FireSelectorMode();
                    switch (FireSelectorModeType)
                    {
                        case ClosedBoltWeapon.FireSelectorModeType.Safe:
                            newFireSelectorMode.ModeType = ClosedBoltWeapon.FireSelectorModeType.Safe;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.Single:
                            newFireSelectorMode.ModeType = ClosedBoltWeapon.FireSelectorModeType.Single;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.Burst:
                            newFireSelectorMode.ModeType = ClosedBoltWeapon.FireSelectorModeType.Burst;
                            newFireSelectorMode.BurstAmount = burstAmounts[burstIndex];
                            burstIndex++;
                            break;
                        case ClosedBoltWeapon.FireSelectorModeType.FullAuto:
                            newFireSelectorMode.ModeType = ClosedBoltWeapon.FireSelectorModeType.FullAuto;
                            break;
                        default:
                            Debug.LogError("FireSelectorMode not supported: " + FireSelectorModeType);
                            continue;
                    }
                    if (!addsSelectorPositions) newFireSelectorMode.SelectorPosition = originalClosedBoltFireModes[originalClosedBoltFireModes.Length - 1].SelectorPosition;
                    else newFireSelectorMode.SelectorPosition = selectorPositions[selectorPosIndex];
                    shotgun.FireSelector_Modes = originalClosedBoltFireModes.Concat(new ClosedBoltWeapon.FireSelectorMode[] { newFireSelectorMode }).ToArray();

                    selectorPosIndex++;
                }
            }
            else
            {
                shotgun.m_fireSelectorMode = originalClosedBoltFireModes.Length - 1;
                shotgun.FireSelector_Modes = originalClosedBoltFireModes;
            }
        }
        */
    }
}
