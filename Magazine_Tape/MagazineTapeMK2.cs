using System;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

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
        public Vector3 primary2SecondaryPos;
        [Tooltip("Primary mag rotation when parented to secondary mag.")]
        public Quaternion primary2SecondaryRot;

        [Tooltip("Secondary mag position when parented to primary mag.")]
        public Vector3 secondary2PrimaryPos;
        [Tooltip("Primary mag rotation when parented to primary mag.")]
        public Quaternion secondary2PrimaryRot;

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


#if!DEBUG
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
            else
            {
                secondaryMagazine.StoreAndDestroyRigidbody();
                secondaryMagazine.gameObject.layer = LayerMask.NameToLayer("NoCol");
            }
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
#endif
    }
}
