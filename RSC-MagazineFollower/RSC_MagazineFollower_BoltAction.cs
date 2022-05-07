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
    public class RSC_MagazineFollower_BoltAction : FVRInteractiveObject
    {
        [Header("RSC-MagazineFollower Config")]
        public BoltActionRifle fireArm;
        public GameObject magazineWell;
        public GameObject magazineGrabTrigger;
        public Transform follower;

        [Tooltip("Index number the safety position is at (starts at 0!).")]
        public int safetyIndex;

        public enum Axis
        {
            x,
            y,
            z
        }

        public Axis rotationalAxis;
        public float rotSpeed = 100f;

        [Tooltip("first entry is Angle when open to load magazine, all following are for the ammount loaded in the magazine, starting with full mag. Last pos is follower with empty mag.")]
        public float[] rotationalAngles;


        private bool open = true;
        private bool lockedSafety = false;
        private int origSafetyPos;
        private float lastRot;

#if!(UNITY_EDITOR || UNITY_5)
        public override void Start()
        {
            base.Start();
            StartCoroutine(SetFollowerRot(rotationalAngles[0]));
        }


        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);
            switch (open)
            {
                case false:
                    open = true;
                    fireArm.PlayAudioEvent(FirearmAudioEventType.BreachOpen);
                    break;
                case true:
                    open = false;
                    fireArm.PlayAudioEvent(FirearmAudioEventType.BreachClose);
                    break;
                default:
                    break;
            }

            if (open)
            {
                StopAllCoroutines();
                StartCoroutine(SetFollowerRot(rotationalAngles[0]));
            }
            else if (!open)
            {
                magazineWell.SetActive(false);
                magazineGrabTrigger.SetActive(false);
                //Debug.Log("magazine inactive");

                if (fireArm.Magazine == null)
                {
                    //Debug.Log("magazine no inserted");
                    StopAllCoroutines();
                    StartCoroutine(SetFollowerRot(rotationalAngles[rotationalAngles.Length-1]));
                }
            }
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();

            if (open && !lockedSafety)
            {
                origSafetyPos = fireArm.m_fireSelectorMode;
                fireArm.m_fireSelectorMode = safetyIndex;

                lockedSafety = true;
            }
            else if (open && lockedSafety)
            {
                fireArm.m_fireSelectorMode = safetyIndex;
            }
            else if (!open && lockedSafety)
            {
                fireArm.m_fireSelectorMode = origSafetyPos;
                lockedSafety = false;
            }
            FVRFireArmMagazine magazine = fireArm.Magazine;
            if (!open && fireArm.Magazine != null)
            {
                int roundCount = magazine.m_numRounds;
                int magCap = magazine.m_capacity;

                int rotIndex = magCap - roundCount + 1;

                if (lastRot != rotationalAngles[rotIndex])
                {
                    StopAllCoroutines();
                    StartCoroutine(SetFollowerRot(rotationalAngles[rotIndex]));
                }
            }
        }


        IEnumerator SetFollowerRot(float rot)
        {
            bool rotDone = false;
            //lastRot = rot;
            lastRot = rot;

            Quaternion targetRotation;

            switch (rotationalAxis)
            {
                case Axis.x:
                    targetRotation = Quaternion.Euler(rot, 0, 0);
                    break;
                case Axis.y:
                    targetRotation = Quaternion.Euler(0, rot, 0);
                    break;
                case Axis.z:
                    targetRotation = Quaternion.Euler(0, 0, rot);
                    break;
                default:
                    targetRotation = Quaternion.Euler(0, 0, 0);
                    break;
            }

            while (!rotDone)
            {
                follower.localRotation = Quaternion.RotateTowards(follower.localRotation, targetRotation, rotSpeed * Time.deltaTime);
                rotDone = follower.localRotation == targetRotation;
                yield return null;
            }
            if (open)
            {
                magazineWell.SetActive(true);
                magazineGrabTrigger.SetActive(true);
                //Debug.Log("magazine active");
            }
        }
#endif
    }
}
