using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;
using System.ComponentModel;

namespace Cityrobo
{
    public class MagazineTapeMK2 : MonoBehaviour
    {
        [Tooltip("Main Magazine")]
        public FVRFireArmMagazine primaryMagazine;
        [Tooltip("Attached Magazine")]
        public FVRFireArmMagazine secondaryMagazine;
        [Tooltip("Tape visuals and colliders (not a requirement)")]
        public GameObject tape = null;

        [Header("Relative Mag Positions (Use Context Menu to calculate)")]
        [Tooltip("Primary mag position when parented to secondary mag.")]
        [ReadOnly] public Vector3 primary2SecondaryPos;
        [Tooltip("Primary mag rotation when parented to secondary mag.")]
        [ReadOnly] public Quaternion primary2SecondaryRot;

        [Tooltip("Secondary mag position when parented to primary mag.")]
        [ReadOnly] public Vector3 secondary2PrimaryPos;
        [Tooltip("Primary mag rotation when parented to primary mag.")]
        [ReadOnly] public Quaternion secondary2PrimaryRot;

        [ContextMenu("Calculate Relative Mag Positions")]
        public void CalculateReltativeMagPositions()
        {
            secondary2PrimaryPos = primaryMagazine.transform.InverseTransformPoint(secondaryMagazine.transform.position);
            secondary2PrimaryRot = Quaternion.Inverse(primaryMagazine.transform.rotation) * secondaryMagazine.transform.rotation;

            primary2SecondaryPos = secondaryMagazine.transform.InverseTransformPoint(primaryMagazine.transform.position);
            primary2SecondaryRot = Quaternion.Inverse(secondaryMagazine.transform.rotation) * primaryMagazine.transform.rotation;
        }

        private enum ActiveMagazine
        {
            primary,
            secondary
        }

        private enum AttachedMagazine
        {
            none,
            primary,
            secondary
        }

        private ActiveMagazine activeMagazine = ActiveMagazine.primary;
        private AttachedMagazine attachedMagazine = AttachedMagazine.none;

#if !(DEBUG || MEATKIT)
        public void Start()
        {
            if (primaryMagazine.State == FVRFireArmMagazine.MagazineState.Locked)
            {
                attachedMagazine = AttachedMagazine.primary;
                secondaryMagazine.StoreAndDestroyRigidbody();
                secondaryMagazine.gameObject.layer = LayerMask.NameToLayer("NoCol");
            }
            else if (secondaryMagazine.State == FVRFireArmMagazine.MagazineState.Locked)
            {
                attachedMagazine = AttachedMagazine.secondary;
                activeMagazine = ActiveMagazine.secondary;

                primaryMagazine.StoreAndDestroyRigidbody();
                primaryMagazine.gameObject.layer = LayerMask.NameToLayer("NoCol");
            }
            else if (primaryMagazine.transform.parent == secondaryMagazine.transform)
            {
                activeMagazine = ActiveMagazine.secondary;

                primaryMagazine.StoreAndDestroyRigidbody();
                primaryMagazine.gameObject.layer = LayerMask.NameToLayer("NoCol");
            }
            else
            {
                activeMagazine = ActiveMagazine.primary;

                secondaryMagazine.StoreAndDestroyRigidbody();
                secondaryMagazine.gameObject.layer = LayerMask.NameToLayer("NoCol");
            }

            Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }
        public void Update()
        {
            try
            {
                if (primaryMagazine.State == FVRFireArmMagazine.MagazineState.Locked && attachedMagazine == AttachedMagazine.none && activeMagazine == ActiveMagazine.secondary)
                {
                    attachedMagazine = AttachedMagazine.primary;
                    secondaryMagazine.ForceBreakInteraction();
                    secondaryMagazine.IsHeld = false;
                    secondaryMagazine.gameObject.layer = LayerMask.NameToLayer("NoCol");
                    primaryMagazine.gameObject.layer = LayerMask.NameToLayer("Interactable");
                    UsePrimaryAsParent(primaryMagazine.transform.parent);
                    secondaryMagazine.StoreAndDestroyRigidbody();
                }
                else if (secondaryMagazine.State == FVRFireArmMagazine.MagazineState.Locked && attachedMagazine == AttachedMagazine.none && activeMagazine == ActiveMagazine.primary)
                {
                    attachedMagazine = AttachedMagazine.secondary;
                    primaryMagazine.ForceBreakInteraction();
                    primaryMagazine.IsHeld = false;
                    primaryMagazine.gameObject.layer = LayerMask.NameToLayer("NoCol");
                    secondaryMagazine.gameObject.layer = LayerMask.NameToLayer("Interactable");
                    UseSecondaryAsParent(secondaryMagazine.transform.parent);
                    primaryMagazine.StoreAndDestroyRigidbody();
                }
                else if (primaryMagazine.State == FVRFireArmMagazine.MagazineState.Free && secondaryMagazine.State == FVRFireArmMagazine.MagazineState.Free)
                {
                    attachedMagazine = AttachedMagazine.none;
                }

                if (activeMagazine == ActiveMagazine.primary) UpdateSecondaryMagTransform();
                else if (activeMagazine == ActiveMagazine.secondary) UpdatePrimaryMagTransform();
            }
            catch (Exception e)
            {
                if (primaryMagazine == null || secondaryMagazine == null)
                {
                    Destroy(tape);
                    Destroy(this.GetComponent<MagazineTapeMK2>());
                }
                else
                {
                    Debug.LogError("Error in MagazineTapeMK2 Script!");
                    Debug.LogException(e);
                }
                
            }

            if (activeMagazine == ActiveMagazine.primary)
            {
                if (primaryMagazine.m_hand != null)
                {
                    FVRViveHand hand = primaryMagazine.m_hand;
                    if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f)
                    {
                        ChangeActiveToSecondary(hand);
                    }
                }
            }
            else if (activeMagazine == ActiveMagazine.secondary)
            {
                if (secondaryMagazine.m_hand != null)
                {
                    FVRViveHand hand = secondaryMagazine.m_hand;
                    if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f)
                    {
                        ChangeActiveToPrimary(hand);
                    }
                }
            }
        }

        private void UsePrimaryAsParent(Transform parent = null)
        {
            activeMagazine = ActiveMagazine.primary;
            primaryMagazine.transform.SetParent(parent);
            secondaryMagazine.transform.SetParent(primaryMagazine.transform);
        }

        private void UseSecondaryAsParent(Transform parent = null)
        {
            activeMagazine = ActiveMagazine.secondary;
            secondaryMagazine.transform.SetParent(parent);
            primaryMagazine.transform.SetParent(secondaryMagazine.transform);
        }

        private void UpdateSecondaryMagTransform()
        {
            secondaryMagazine.transform.localPosition = secondary2PrimaryPos;
            secondaryMagazine.transform.localRotation = secondary2PrimaryRot;
        }

        private void UpdatePrimaryMagTransform()
        {
            primaryMagazine.transform.localPosition = primary2SecondaryPos;
            primaryMagazine.transform.localRotation = primary2SecondaryRot;
        }

        private void ChangeActiveToPrimary(FVRViveHand hand)
        {
            secondaryMagazine.ForceBreakInteraction();
            secondaryMagazine.IsHeld = false;
            secondaryMagazine.gameObject.layer = LayerMask.NameToLayer("NoCol");
            primaryMagazine.RecoverRigidbody();
            UsePrimaryAsParent();

            secondaryMagazine.StoreAndDestroyRigidbody();
            hand.ForceSetInteractable(primaryMagazine);
            primaryMagazine.BeginInteraction(hand);
            primaryMagazine.gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        private void ChangeActiveToSecondary(FVRViveHand hand)
        {
            primaryMagazine.ForceBreakInteraction();
            primaryMagazine.IsHeld = false;
            primaryMagazine.gameObject.layer = LayerMask.NameToLayer("NoCol");
            secondaryMagazine.RecoverRigidbody();
            UseSecondaryAsParent();

            primaryMagazine.StoreAndDestroyRigidbody();
            hand.ForceSetInteractable(secondaryMagazine);
            secondaryMagazine.BeginInteraction(hand);
            secondaryMagazine.gameObject.layer = LayerMask.NameToLayer("Interactable");
        }

        void Unhook()
        {
            //On.FistVR.FVRFireArmMagazine.ReloadMagWithType -= FVRFireArmMagazine_ReloadMagWithType;
            On.FistVR.FVRFireArmMagazine.DuplicateFromSpawnLock -= FVRFireArmMagazine_DuplicateFromSpawnLock;
            On.FistVR.FVRWristMenu.CleanUpScene_Empties -= FVRWristMenu_CleanUpScene_Empties;
        }

        void Hook()
        {
            //On.FistVR.FVRFireArmMagazine.ReloadMagWithType += FVRFireArmMagazine_ReloadMagWithType;
            On.FistVR.FVRFireArmMagazine.DuplicateFromSpawnLock += FVRFireArmMagazine_DuplicateFromSpawnLock;
            On.FistVR.FVRWristMenu.CleanUpScene_Empties += FVRWristMenu_CleanUpScene_Empties;
        }

        private void FVRWristMenu_CleanUpScene_Empties(On.FistVR.FVRWristMenu.orig_CleanUpScene_Empties orig, FVRWristMenu self)
        {
            self.Aud.PlayOneShot(self.AudClip_Engage, 1f);
            if (!self.askConfirm_CleanupEmpties)
            {
                self.ResetConfirm();
                self.AskConfirm_CleanupEmpties();
                return;
            }
            self.ResetConfirm();
            FVRFireArmMagazine[] array = UnityEngine.Object.FindObjectsOfType<FVRFireArmMagazine>();
            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (!array[i].IsHeld && array[i].QuickbeltSlot == null && array[i].FireArm == null && array[i].m_numRounds == 0 && array[i].GetComponentInChildren<MagazineTapeMK2>() == null)
                {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                }
            }
            FVRFireArmRound[] array2 = UnityEngine.Object.FindObjectsOfType<FVRFireArmRound>();
            for (int j = array2.Length - 1; j >= 0; j--)
            {
                if (!array2[j].IsHeld && array2[j].QuickbeltSlot == null && array2[j].RootRigidbody != null)
                {
                    UnityEngine.Object.Destroy(array2[j].gameObject);
                }
            }
            FVRFireArmClip[] array3 = UnityEngine.Object.FindObjectsOfType<FVRFireArmClip>();
            for (int k = array3.Length - 1; k >= 0; k--)
            {
                if (!array3[k].IsHeld && array3[k].QuickbeltSlot == null && array3[k].FireArm == null && array3[k].m_numRounds == 0)
                {
                    UnityEngine.Object.Destroy(array3[k].gameObject);
                }
            }
            Speedloader[] array4 = UnityEngine.Object.FindObjectsOfType<Speedloader>();
            for (int l = array4.Length - 1; l >= 0; l--)
            {
                if (!array4[l].IsHeld && array4[l].QuickbeltSlot == null)
                {
                    UnityEngine.Object.Destroy(array4[l].gameObject);
                }
            }
        }

        private GameObject FVRFireArmMagazine_DuplicateFromSpawnLock(On.FistVR.FVRFireArmMagazine.orig_DuplicateFromSpawnLock orig, FVRFireArmMagazine self, FVRViveHand hand)
        {
            GameObject gameObject = orig(self, hand);

            if (self == primaryMagazine)
            {
                MagazineTapeMK2 tape = gameObject.GetComponent<MagazineTapeMK2>();

                FVRFireArmMagazine component = tape.secondaryMagazine;
                for (int i = 0; i < Mathf.Min(this.secondaryMagazine.LoadedRounds.Length, component.LoadedRounds.Length); i++)
                {
                    if (this.secondaryMagazine.LoadedRounds[i] != null && this.secondaryMagazine.LoadedRounds[i].LR_Mesh != null)
                    {
                        component.LoadedRounds[i].LR_Class = this.secondaryMagazine.LoadedRounds[i].LR_Class;
                        component.LoadedRounds[i].LR_Mesh = this.secondaryMagazine.LoadedRounds[i].LR_Mesh;
                        component.LoadedRounds[i].LR_Material = this.secondaryMagazine.LoadedRounds[i].LR_Material;
                        component.LoadedRounds[i].LR_ObjectWrapper = this.secondaryMagazine.LoadedRounds[i].LR_ObjectWrapper;
                    }
                }
                component.m_numRounds = this.secondaryMagazine.m_numRounds;
                component.UpdateBulletDisplay();
                return gameObject;
            }
            else if (self == secondaryMagazine)
            {
                MagazineTapeMK2 tape = gameObject.GetComponentInChildren<MagazineTapeMK2>();

                FVRFireArmMagazine component = tape.primaryMagazine;
                for (int i = 0; i < Mathf.Min(this.primaryMagazine.LoadedRounds.Length, component.LoadedRounds.Length); i++)
                {
                    if (this.primaryMagazine.LoadedRounds[i] != null && this.primaryMagazine.LoadedRounds[i].LR_Mesh != null)
                    {
                        component.LoadedRounds[i].LR_Class = this.primaryMagazine.LoadedRounds[i].LR_Class;
                        component.LoadedRounds[i].LR_Mesh = this.primaryMagazine.LoadedRounds[i].LR_Mesh;
                        component.LoadedRounds[i].LR_Material = this.primaryMagazine.LoadedRounds[i].LR_Material;
                        component.LoadedRounds[i].LR_ObjectWrapper = this.primaryMagazine.LoadedRounds[i].LR_ObjectWrapper;
                    }
                }
                component.m_numRounds = this.primaryMagazine.m_numRounds;
                component.UpdateBulletDisplay();
                return gameObject;
            }
            else return gameObject;
        }

        private void FVRFireArmMagazine_ReloadMagWithType(On.FistVR.FVRFireArmMagazine.orig_ReloadMagWithType orig, FVRFireArmMagazine self, FireArmRoundClass rClass)
        {
            if (self == primaryMagazine || self == secondaryMagazine)
            {
                primaryMagazine.m_numRounds = 0;
                for (int i = 0; i < primaryMagazine.m_capacity; i++)
                {
                    primaryMagazine.AddRound(rClass, false, false);
                }
                primaryMagazine.UpdateBulletDisplay();

                secondaryMagazine.m_numRounds = 0;
                for (int i = 0; i < secondaryMagazine.m_capacity; i++)
                {
                    secondaryMagazine.AddRound(rClass, false, false);
                }
                secondaryMagazine.UpdateBulletDisplay();
            }
            else orig(self, rClass);
        }
#endif
    }
}

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}
