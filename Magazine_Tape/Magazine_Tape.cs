using System;
using System.Collections.Generic;
using UnityEngine;

namespace FistVR
{
    public class Magazine_Tape : FVRPhysicalObject
    {
        public FVRFireArmMagazine Magazine_1;
        public FVRFireArmMagazine Magazine_2;

        //Base Transforms
        private Vector3 mag1pos;
        private Vector3 mag2pos;
        private Vector3 mag1euler;
        private Vector3 mag2euler;

        //Mag1 Attached Tape Transforms
        private Vector3 tape1pos;
        private Vector3 tape1euler;

        //Mag2 Attached Tape Transforms

        private Vector3 tape2pos;
        private Vector3 tape2euler;

        private bool mag1_attached;
        private bool mag2_attached;
        

        protected override void Awake()
        {
            base.Awake();

            Magazine_1.StoreAndDestroyRigidbody();
            Magazine_2.StoreAndDestroyRigidbody();
            if (Magazine_1.Transform.IsChildOf(this.Transform) && Magazine_2.Transform.IsChildOf(this.Transform))
            {
                mag1_attached = false;
                mag2_attached = false;

                this.SetStandard_Transform();

                Magazine_1.SetParentage(null);
                this.Use_Mag1_Attached_Parenting();
                this.SetMag1_Attached_Transform();

                Magazine_2.SetParentage(null);
                this.Use_Mag2_Attached_Parenting();
                this.SetMag2_Attached_Transform();

                this.SetParentage(null);
                this.Use_Standard_Parenting();
                this.Use_Standard_Transform();
            }
            else if (!Magazine_1.Transform.IsChildOf(this.Transform) && Magazine_2.Transform.IsChildOf(this.Transform))
            {
                mag1_attached = true;
                mag2_attached = false;
                Transform magParent = Magazine_1.Transform.parent;

                this.SetMag1_Attached_Transform();

                this.SetParentage(null);
                this.Use_Standard_Parenting();
                this.SetStandard_Transform();

                Magazine_2.SetParentage(null);
                this.Use_Mag2_Attached_Parenting();
                this.SetMag2_Attached_Transform();

                Magazine_1.SetParentage(magParent);
                Magazine_1.State = FVRFireArmMagazine.MagazineState.Locked;
                this.Parent_to_mag1();
            }
            else if (Magazine_1.Transform.IsChildOf(this.Transform) && !Magazine_2.Transform.IsChildOf(this.Transform))
            {
                mag1_attached = false;
                mag2_attached = true;
                Transform magParent = Magazine_2.Transform.parent;

                this.SetMag2_Attached_Transform();

                this.SetParentage(null);
                this.Use_Standard_Parenting();
                this.SetStandard_Transform();

                Magazine_1.SetParentage(null);
                this.Use_Mag1_Attached_Parenting();
                this.SetMag1_Attached_Transform();

                Magazine_2.SetParentage(magParent);
                Magazine_2.State = FVRFireArmMagazine.MagazineState.Locked;
                this.Parent_to_mag2();
            }

            /*Debug.Log("mag1pos: " + mag1pos.ToString("F6"));
            Debug.Log("mag1euler: " + mag1euler.ToString("F6"));
            Debug.Log("mag2pos: " + mag2pos.ToString("F6"));
            Debug.Log("mag2euler: " + mag2euler.ToString("F6"));
            Debug.Log("tape1pos: " + tape1pos.ToString("F6"));
            Debug.Log("tape1euler: " + tape1euler.ToString("F6"));
            Debug.Log("tape2pos: " + tape2pos.ToString("F6"));
            Debug.Log("tape2euler: " + tape2euler.ToString("F6"));*/
        }
        protected override void FVRUpdate()
        {
            base.FVRUpdate();
            if (Magazine_1.State == FVRFireArmMagazine.MagazineState.Locked && !mag1_attached) Parent_to_mag1();
            else if (Magazine_2.State == FVRFireArmMagazine.MagazineState.Locked && !mag2_attached) Parent_to_mag2();
            else if ((Magazine_1.State == FVRFireArmMagazine.MagazineState.Free && Magazine_2.State == FVRFireArmMagazine.MagazineState.Free) && (mag1_attached || mag2_attached)) Parent_to_null();

            if (mag1_attached) UseMag1_Attached_Transform();
            else if (mag2_attached) UseMag2_Attached_Transform();
            else Use_Standard_Transform();
        }

        private void Parent_to_mag1()
        {
            mag1_attached = true;
            mag2_attached = false;
            if (this.QuickbeltSlot != null)
            {
                this.SetAllCollidersToLayer(true, "NoCol");
                Magazine_2.SetAllCollidersToLayer(true, "NoCol");
            }
            else
            {
                this.SetAllCollidersToLayer(true, "Default");
                Magazine_2.SetAllCollidersToLayer(true, "Default");
            }
            this.ForceBreakInteraction();
            this.IsHeld = false;
            this.StoreAndDestroyRigidbody();
            this.Use_Mag1_Attached_Parenting();
        }

        private void Parent_to_mag2()
        {
            mag1_attached = false;
            mag2_attached = true;
            if (this.QuickbeltSlot != null)
            {
                this.SetAllCollidersToLayer(true, "NoCol");
                Magazine_1.SetAllCollidersToLayer(true, "NoCol");
            }
            else
            {
                this.SetAllCollidersToLayer(true, "Default");
                Magazine_1.SetAllCollidersToLayer(true, "Default");
            }
            this.ForceBreakInteraction();
            this.IsHeld = false;
            this.StoreAndDestroyRigidbody();
            this.Use_Mag2_Attached_Parenting();
        }

        private void Parent_to_null()
        {
            this.SetParentage(null);
            this.RecoverRigidbody();
            FVRViveHand hand = null;
            if (mag1_attached)
            {
                hand = Magazine_1.m_hand;
                Magazine_1.ForceBreakInteraction();
                Magazine_1.SetParentage(this.Transform);
                Magazine_1.StoreAndDestroyRigidbody();
                mag1_attached = false;
            }
            if (mag2_attached)
            {
                hand = Magazine_2.m_hand;
                Magazine_2.ForceBreakInteraction();
                Magazine_2.SetParentage(this.Transform);
                Magazine_2.StoreAndDestroyRigidbody();
                mag2_attached = false;
            }
            if (hand != null)
            {
                hand.ForceSetInteractable(this);
                this.BeginInteraction(hand);
            }
            Magazine_1.SetAllCollidersToLayer(true, "Interactable");
            Magazine_2.SetAllCollidersToLayer(true, "Interactable");
            this.SetAllCollidersToLayer(true, "Interactable");
            Magazine_1.SetAllCollidersToLayer(false, "Default");
            Magazine_2.SetAllCollidersToLayer(false, "Default");
            this.SetAllCollidersToLayer(false, "Default");
        }

        private void Use_Standard_Parenting()
        {
            Magazine_1.SetParentage(this.Transform);
            Magazine_2.SetParentage(this.Transform);
        }

        private void Use_Mag1_Attached_Parenting()
        {
            this.SetParentage(Magazine_1.Transform);
            Magazine_2.SetParentage(this.Transform);
        }

        private void Use_Mag2_Attached_Parenting()
        {
            this.SetParentage(Magazine_2.Transform);
            Magazine_1.SetParentage(this.Transform);
        }

        private void Use_Standard_Transform()
        {
            Magazine_1.Transform.localPosition = mag1pos;
            Magazine_1.Transform.localEulerAngles = mag1euler;

            Magazine_2.Transform.localPosition = mag2pos;
            Magazine_2.Transform.localEulerAngles = mag2euler;
        }
        
        private void UseMag1_Attached_Transform()
        {
            this.Transform.localPosition = tape1pos;
            this.Transform.localEulerAngles = tape1euler;

            Magazine_2.Transform.localPosition = mag2pos;
            Magazine_2.Transform.localEulerAngles = mag2euler;
        }

        private void UseMag2_Attached_Transform()
        {      
            this.Transform.localPosition = tape2pos;
            this.Transform.localEulerAngles = tape2euler;

            Magazine_1.Transform.localPosition = mag1pos;
            Magazine_1.Transform.localEulerAngles = mag1euler;
        }

        private void SetStandard_Transform()
        {
            mag1pos = Magazine_1.Transform.localPosition;
            mag2pos = Magazine_2.Transform.localPosition;

            mag1euler = Magazine_1.Transform.localEulerAngles;
            mag2euler = Magazine_2.Transform.localEulerAngles;
        }

        private void SetMag1_Attached_Transform()
        {
            tape1pos = this.Transform.localPosition;
            tape1euler = this.Transform.localEulerAngles;
        }

        private void SetMag2_Attached_Transform()
        {
            tape2pos = this.Transform.localPosition;
            tape2euler = this.Transform.localEulerAngles;
        }


        public static FistVR.Magazine_Tape CopyFromObject(FVRPhysicalObject original, GameObject target)
        {
            target.gameObject.SetActive(false);
            var real = target.AddComponent<Magazine_Tape>();
            real.ControlType = original.ControlType;
            real.IsSimpleInteract = original.IsSimpleInteract;
            real.HandlingGrabSound = original.HandlingGrabSound;
            real.HandlingReleaseSound = original.HandlingReleaseSound;
            real.PoseOverride = original.PoseOverride;
            real.QBPoseOverride = original.QBPoseOverride;
            real.PoseOverride_Touch = original.PoseOverride_Touch;
            real.UseGrabPointChild = original.UseGrabPointChild;
            real.UseGripRotInterp = original.UseGripRotInterp;
            real.PositionInterpSpeed = original.PositionInterpSpeed;
            real.RotationInterpSpeed = original.RotationInterpSpeed;
            real.EndInteractionIfDistant = original.EndInteractionIfDistant;
            real.EndInteractionDistance = original.EndInteractionDistance;
            real.UXGeo_Held = original.UXGeo_Held;
            real.UXGeo_Hover = original.UXGeo_Hover;
            real.UseFilteredHandTransform = original.UseFilteredHandTransform;
            real.UseFilteredHandRotation = original.UseFilteredHandRotation;
            real.UseFilteredHandPosition = original.UseFilteredHandPosition;
            real.UseSecondStepRotationFiltering = original.UseSecondStepRotationFiltering;
            real.ObjectWrapper = original.ObjectWrapper;
            real.SpawnLockable = original.SpawnLockable;
            real.Harnessable = original.Harnessable;
            real.HandlingReleaseIntoSlotSound = original.HandlingReleaseIntoSlotSound;
            real.Size = original.Size;
            real.QBSlotType = original.QBSlotType;
            real.ThrowVelMultiplier = original.ThrowVelMultiplier;
            real.ThrowAngMultiplier = original.ThrowAngMultiplier;
            real.UsesGravity = original.UsesGravity;
            real.DependantRBs = original.DependantRBs;
            real.DistantGrabbable = original.DistantGrabbable;
            real.IsDebug = original.IsDebug;
            real.IsAltHeld = original.IsAltHeld;
            real.IsKinematicLocked = original.IsKinematicLocked;
            real.DoesQuickbeltSlotFollowHead = original.DoesQuickbeltSlotFollowHead;
            real.IsInWater = original.IsInWater;
            real.AttachmentMounts = original.AttachmentMounts;
            real.IsAltToAltTransfer = original.IsAltToAltTransfer;
            real.CollisionSound = original.CollisionSound;
            real.IsPickUpLocked = original.IsPickUpLocked;
            real.OverridesObjectToHand = original.OverridesObjectToHand;
            real.MP = original.MP;
            Destroy(original);
            return real;
        }
    }
}
