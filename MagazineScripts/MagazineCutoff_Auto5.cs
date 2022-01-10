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
    public class MagazineCutoff_Auto5 : FVRInteractiveObject
    {
        [Header("Magazine Cutoff Config")]
        public TubeFedShotgun fireArm;
        public Transform cutoffLever;

        public float startLimit;
        public float stopLimit;
        public float speed;
        public enum TranslationType
        {
            Translation,
            Rotation
        }

        public enum Axis
        {
            X,
            Y,
            Z
        }

        public TranslationType translationType;
        public Axis axis;

        [Header("Sound")]
        public AudioEvent sounds;
        /*
        private FVRFireArmMagazine empty_mag;
        private FVRFireArmMagazine orig_mag;
        */
        private Vector3 startPos;
        private Vector3 stopPos;

        private Quaternion startRot;
        private Quaternion stopRot;

        private bool isMoving = false;
        private bool isActive = false;
#if !(UNITY_EDITOR || UNITY_5)
        public override void Start()
        {
            base.Start();

            CalculatePositions();
            /*
            empty_mag = FVRFireArmMagazine.Instantiate(fireArm.Magazine);
            empty_mag.LoadedRounds = new FVRLoadedRound[0];
            empty_mag.m_numRounds = 0;
            */
        }

        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);

            isActive = !isActive;

            SM.PlayGenericSound(sounds, cutoffLever.position);
            switch (translationType)
            {
                case TranslationType.Translation:
                    if (isMoving) StopAllCoroutines();
                    StartCoroutine(Activate_Translation());
                    break;
                case TranslationType.Rotation:
                    if (isMoving) StopAllCoroutines();
                    StartCoroutine(Activate_Rotation());
                    break;
                default:
                    break;
            }
        }

        void CalculatePositions()
        {
            switch (translationType)
            {
                case TranslationType.Translation:
                    switch (axis)
                    {
                        case Axis.X:
                            startPos = new Vector3(startLimit, cutoffLever.localPosition.y, cutoffLever.localPosition.z);
                            stopPos = new Vector3(stopLimit, cutoffLever.localPosition.y, cutoffLever.localPosition.z);
                            break;
                        case Axis.Y:
                            startPos = new Vector3(cutoffLever.localPosition.x, startLimit, cutoffLever.localPosition.z);
                            stopPos = new Vector3(cutoffLever.localPosition.x, stopLimit, cutoffLever.localPosition.z);
                            break;
                        case Axis.Z:
                            startPos = new Vector3(cutoffLever.localPosition.x, cutoffLever.localPosition.y, startLimit);
                            stopPos = new Vector3(cutoffLever.localPosition.x, cutoffLever.localPosition.y, stopLimit);
                            break;
                        default:
                            startPos = new Vector3();
                            stopPos = new Vector3();
                            break;
                    }
                    cutoffLever.localPosition = startPos;
                    break;
                case TranslationType.Rotation:
                    switch (axis)
                    {
                        case Axis.X:
                            startRot = Quaternion.Euler(startLimit, cutoffLever.localEulerAngles.y, cutoffLever.localEulerAngles.z);
                            stopRot = Quaternion.Euler(stopLimit, cutoffLever.localEulerAngles.y, cutoffLever.localEulerAngles.z);
                            break;
                        case Axis.Y:
                            startRot = Quaternion.Euler(cutoffLever.localEulerAngles.x, startLimit, cutoffLever.localEulerAngles.z);
                            stopRot = Quaternion.Euler(cutoffLever.localEulerAngles.x, stopLimit, cutoffLever.localEulerAngles.z);
                            break;
                        case Axis.Z:
                            startRot = Quaternion.Euler(cutoffLever.localEulerAngles.x, cutoffLever.localEulerAngles.y, startLimit);
                            stopRot = Quaternion.Euler(cutoffLever.localEulerAngles.x, cutoffLever.localEulerAngles.y, stopLimit);
                            break;
                        default:
                            startRot = Quaternion.identity;
                            stopRot = Quaternion.identity;
                            break;
                    }
                    cutoffLever.localRotation = startRot;
                    break;
                default:
                    break;
            }
        }

        IEnumerator Activate_Translation()
        {
            isMoving = true;

            Vector3 target = isActive ? stopPos : startPos;

            while (cutoffLever.localPosition != target)
            {
                cutoffLever.localPosition = Vector3.MoveTowards(cutoffLever.localPosition, target, speed * Time.deltaTime);
                yield return null;
            }

            Activate_Magazine();
            isMoving = false;
        }

        IEnumerator Activate_Rotation()
        {
            isMoving = true;

            Quaternion target = isActive ? stopRot : startRot;

            while (cutoffLever.localRotation != target)
            {
                cutoffLever.localRotation = Quaternion.RotateTowards(cutoffLever.localRotation, target, speed * Time.deltaTime);
                yield return null;
            }

            Activate_Magazine();
            isMoving = false;
        }

        void Activate_Magazine()
        {
            if (isActive)
            {
                /*
                orig_mag = fireArm.Magazine;
                fireArm.Magazine = empty_mag;
                */
                fireArm.Magazine.IsExtractable = false;
            }
            else
            {
                /*
                fireArm.Magazine = orig_mag;
                */

                fireArm.Magazine.IsExtractable = true;
                if (fireArm.Bolt.m_isBoltLocked)
                {
                    fireArm.ExtractRound();
                    fireArm.TransferShellToUpperTrack();
                    fireArm.Bolt.ReleaseBolt();
                }
            }
        }
#endif
    }
}
