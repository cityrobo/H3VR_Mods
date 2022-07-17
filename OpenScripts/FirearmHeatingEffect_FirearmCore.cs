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

        private float _averageBoltForwardSpeed;
        private float _averageBoltRearwardSpeed;
        private float _averageBoltSpring;
#if !(DEBUG)

        public void Awake()
        {
			FireArm = GetComponent<FVRFireArm>();
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


                    foreach (FirearmHeatingEffect effect in FirearmHeatingEffects)
                    {
                        CombinedHeatMultiplier *= effect.HeatMultiplier;
                    }

                    _lastListCount = FirearmHeatingEffects.Count;
                }

                _averageBoltForwardSpeed = 0f;
                _averageBoltRearwardSpeed = 0f;
                _averageBoltSpring = 0f;

                int BoltBool = 0;
                foreach (FirearmHeatingEffect effect in FirearmHeatingEffects)
                {
                    CombinedHeatMultiplier *= effect.HeatMultiplier;

                    if (effect.DoesHeatAffectBoltSpeed)
                    {
                        _averageBoltForwardSpeed += effect.CurrentBoltForwardSpeed;
                        _averageBoltRearwardSpeed += effect.CurrentBoltRearwardSpeed;
                        _averageBoltSpring += effect.CurrentBoltSpring;
                        BoltBool++;
                    }
                }
                if (BoltBool != 0)
                {
                    _averageBoltForwardSpeed /= BoltBool;
                    _averageBoltRearwardSpeed /= BoltBool;
                    _averageBoltSpring /= BoltBool;
                    switch (FireArm)
                    {
                        case ClosedBoltWeapon w:
                            w.Bolt.Speed_Forward = _averageBoltForwardSpeed;
                            w.Bolt.Speed_Rearward = _averageBoltRearwardSpeed;
                            w.Bolt.SpringStiffness = _averageBoltSpring;
                            break;
                        case OpenBoltReceiver w:
                            w.Bolt.BoltSpeed_Forward = _averageBoltForwardSpeed;
                            w.Bolt.BoltSpeed_Rearward = _averageBoltRearwardSpeed;
                            w.Bolt.BoltSpringStiffness = _averageBoltSpring;
                            break;
                        case Handgun w:
                            w.Slide.Speed_Forward = _averageBoltForwardSpeed;
                            w.Slide.Speed_Rearward = _averageBoltRearwardSpeed;
                            w.Slide.SpringStiffness = _averageBoltSpring;
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
