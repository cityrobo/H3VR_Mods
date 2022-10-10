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
    public class MagazineCutoff : FVRInteractiveObject
    {
        [Header("Magazine Cutoff Config")]
        public FVRFireArm fireArm;
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
        private Vector3 _startPos;
        private Vector3 _stopPos;

        private Quaternion _startRot;
        private Quaternion _stopRot;

        private bool _magazineCuttoffActive = false;

        private FVRFireArmMagazine _mag;

        private Quaternion _targetRotation;
        private Vector3 _targetPosition;

#if !(UNITY_EDITOR || UNITY_5)
        public override void Start()
        {
            base.Start();

            CalculatePositions();


        }

        public override void SimpleInteraction(FVRViveHand hand)
        {
            base.SimpleInteraction(hand);

            _magazineCuttoffActive = !_magazineCuttoffActive;

            SM.PlayGenericSound(sounds, cutoffLever.position);
            switch (translationType)
            {
                case TranslationType.Translation:
                    _targetPosition = _magazineCuttoffActive ? _stopPos : _startPos;
                    break;
                case TranslationType.Rotation:
                    _targetRotation = _magazineCuttoffActive ? _stopRot : _startRot;
                    break;
            }
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();

            if (_magazineCuttoffActive && _mag != null)
            {
                if (_mag.FireArm == fireArm)
                {
                    _mag.IsExtractable = false;
                }
                else
                {
                    _mag.IsExtractable = true;
                    _mag = null;
                }
            }
            else if (!_magazineCuttoffActive && _mag != null)
            {
                _mag.IsExtractable = true;
            }

            _mag = fireArm.Magazine;


            if (translationType == TranslationType.Rotation && cutoffLever.localRotation != _targetRotation)
            {
                cutoffLever.localRotation = Quaternion.RotateTowards(cutoffLever.localRotation, _targetRotation, speed * Time.deltaTime);
            }
            else if (translationType == TranslationType.Translation && cutoffLever.localPosition != _targetPosition)
            {
                cutoffLever.localPosition = Vector3.MoveTowards(cutoffLever.localPosition, _targetPosition, speed * Time.deltaTime);
            }
        }

        private void CalculatePositions()
        {
            switch (translationType)
            {
                case TranslationType.Translation:
                    switch (axis)
                    {
                        case Axis.X:
                            _startPos = new Vector3(startLimit, cutoffLever.localPosition.y, cutoffLever.localPosition.z);
                            _stopPos = new Vector3(stopLimit, cutoffLever.localPosition.y, cutoffLever.localPosition.z);
                            break;
                        case Axis.Y:
                            _startPos = new Vector3(cutoffLever.localPosition.x, startLimit, cutoffLever.localPosition.z);
                            _stopPos = new Vector3(cutoffLever.localPosition.x, stopLimit, cutoffLever.localPosition.z);
                            break;
                        case Axis.Z:
                            _startPos = new Vector3(cutoffLever.localPosition.x, cutoffLever.localPosition.y, startLimit);
                            _stopPos = new Vector3(cutoffLever.localPosition.x, cutoffLever.localPosition.y, stopLimit);
                            break;
                        default:
                            _startPos = new Vector3();
                            _stopPos = new Vector3();
                            break;
                    }
                    cutoffLever.localPosition = _startPos;
                    break;
                case TranslationType.Rotation:
                    switch (axis)
                    {
                        case Axis.X:
                            _startRot = Quaternion.Euler(startLimit, cutoffLever.localEulerAngles.y, cutoffLever.localEulerAngles.z);
                            _stopRot = Quaternion.Euler(stopLimit, cutoffLever.localEulerAngles.y, cutoffLever.localEulerAngles.z);
                            break;
                        case Axis.Y:
                            _startRot = Quaternion.Euler(cutoffLever.localEulerAngles.x, startLimit, cutoffLever.localEulerAngles.z);
                            _stopRot = Quaternion.Euler(cutoffLever.localEulerAngles.x, stopLimit, cutoffLever.localEulerAngles.z);
                            break;
                        case Axis.Z:
                            _startRot = Quaternion.Euler(cutoffLever.localEulerAngles.x, cutoffLever.localEulerAngles.y, startLimit);
                            _stopRot = Quaternion.Euler(cutoffLever.localEulerAngles.x, cutoffLever.localEulerAngles.y, stopLimit);
                            break;
                        default:
                            _startRot = Quaternion.identity;
                            _stopRot = Quaternion.identity;
                            break;
                    }
                    cutoffLever.localRotation = _startRot;
                    break;
                default:
                    break;
            }
        }
#endif
    }
}
