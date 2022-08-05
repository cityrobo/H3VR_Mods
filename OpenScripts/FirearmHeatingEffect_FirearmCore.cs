using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using FistVR;

namespace Cityrobo
{
    public class FirearmHeatingEffect_FirearmCore : MonoBehaviour
    {
        public FVRFireArm FireArm = null;
        public float CombinedHeatMultiplier = 1f;
        public List<FirearmHeatingEffect> FirearmHeatingEffects = new List<FirearmHeatingEffect>();
        private int _lastListCount = 0;

        private float _origBoltForwardSpeed;
        private float _origBoltRearwardSpeed;
        private float _origBoltSpringStiffness;

        private float _averageBoltForwardSpeedMultiplier;
        private float _averageBoltRearwardSpeedMultiplier;
        private float _averageBoltSpringStiffnessMultiplier;

#if !(DEBUG)
        public void Awake()
        {
			FireArm = GetComponent<FVRFireArm>();

            switch (FireArm)
            {
                case ClosedBoltWeapon w:
                    _origBoltForwardSpeed = w.Bolt.Speed_Forward;
                    _origBoltRearwardSpeed = w.Bolt.Speed_Rearward;
                    _origBoltSpringStiffness = w.Bolt.SpringStiffness;
                    break;
                case OpenBoltReceiver w:
                    _origBoltForwardSpeed = w.Bolt.BoltSpeed_Forward;
                    _origBoltRearwardSpeed = w.Bolt.BoltSpeed_Rearward;
                    _origBoltSpringStiffness = w.Bolt.BoltSpringStiffness;
                    break;
                case Handgun w:
                    _origBoltForwardSpeed = w.Slide.Speed_Forward;
                    _origBoltRearwardSpeed = w.Slide.Speed_Rearward;
                    _origBoltSpringStiffness = w.Slide.SpringStiffness;
                    break;
                default:
                    break;
            }
        }
		public void OnDestroy()
        {

        }

        public void Update()
        {
            if (FireArm != null)
            {
                if (_lastListCount != FirearmHeatingEffects.Count)
                {
                    CombinedHeatMultiplier = 1f;

                    foreach (FirearmHeatingEffect effect in FirearmHeatingEffects) CombinedHeatMultiplier *= effect.HeatMultiplier;

                    _lastListCount = FirearmHeatingEffects.Count;
                }

                _averageBoltForwardSpeedMultiplier = 0f;
                _averageBoltRearwardSpeedMultiplier = 0f;
                _averageBoltSpringStiffnessMultiplier = 0f;
                
                int BoltBool = 0;
                foreach (FirearmHeatingEffect effect in FirearmHeatingEffects)
                {
                    if (effect.DoesHeatAffectBoltSpeed)
                    {
                        _averageBoltForwardSpeedMultiplier += effect.CurrentBoltForwardSpeedMultiplier;
                        _averageBoltRearwardSpeedMultiplier += effect.CurrentBoltRearwardSpeedMultiplier;
                        _averageBoltSpringStiffnessMultiplier += effect.CurrentBoltSpringMultiplier;
                        BoltBool++;
                    }
                }
                if (BoltBool != 0)
                {
                    _averageBoltForwardSpeedMultiplier /= BoltBool;
                    _averageBoltRearwardSpeedMultiplier /= BoltBool;
                    _averageBoltSpringStiffnessMultiplier /= BoltBool;
                    switch (FireArm)
                    {
                        case ClosedBoltWeapon w:
                            w.Bolt.Speed_Forward = _averageBoltForwardSpeedMultiplier * _origBoltForwardSpeed;
                            w.Bolt.Speed_Rearward = _averageBoltRearwardSpeedMultiplier * _origBoltRearwardSpeed;
                            w.Bolt.SpringStiffness = _averageBoltSpringStiffnessMultiplier * _origBoltSpringStiffness;
                            break;
                        case OpenBoltReceiver w:
                            w.Bolt.BoltSpeed_Forward = _averageBoltForwardSpeedMultiplier * _origBoltForwardSpeed;
                            w.Bolt.BoltSpeed_Rearward = _averageBoltRearwardSpeedMultiplier * _origBoltRearwardSpeed;
                            w.Bolt.BoltSpringStiffness = _averageBoltSpringStiffnessMultiplier * _origBoltSpringStiffness;
                            break;
                        case Handgun w:
                            w.Slide.Speed_Forward = _averageBoltForwardSpeedMultiplier * _origBoltForwardSpeed;
                            w.Slide.Speed_Rearward = _averageBoltRearwardSpeedMultiplier * _origBoltRearwardSpeed;
                            w.Slide.SpringStiffness = _averageBoltSpringStiffnessMultiplier * _origBoltSpringStiffness;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
#endif
    }
}
