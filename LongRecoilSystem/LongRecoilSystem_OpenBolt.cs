using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class LongRecoilSystem_OpenBolt : MonoBehaviour
    {
        public OpenBoltReceiverBolt originalBolt;

        public GameObject newBolt;
        public Transform newBoltForwardPos;
        public Transform newBoltLockingPos;
        public Transform newBoltRearwardPos;

        public GameObject barrel;
        public Transform barrelForwardPos;
        public Transform barrelLockingPos;
        public Transform barrelRearwardPos;
        [Range(0.01f, 0.99f)]
        public float barrelForwardThreshhold = 0.9f;

        [Header("Sound")]
        public AudioEvent barrelHitForward;

        private bool _wasHeld = false;
        private float _currentZ;
        private float _lastZ;

        private float _boltLerp;
        private Vector3 _lerpPosBolt;
        private Vector3 _lerpPosBarrel;

        private bool _soundPlayed = false;

        private string _lastMessage;

        public void Start()
        {
            _currentZ = originalBolt.transform.localPosition.z;
            _lastZ = _currentZ;
        }
        public void Update()
        {
            _currentZ = originalBolt.transform.localPosition.z;
            if (originalBolt.IsHeld || _wasHeld)
            {
                _boltLerp = GetBoltLerpBetweenRearAndFore();
                _lerpPosBolt = Vector3.Lerp(newBoltRearwardPos.localPosition, newBoltForwardPos.localPosition, _boltLerp);
                newBolt.transform.localPosition = _lerpPosBolt;
                if (originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.Forward) _wasHeld = false;
                else _wasHeld = true;
            }
            else if (!_wasHeld)
            {
                if (originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.ForwardToMid && _currentZ < _lastZ)
                {
                    _boltLerp = originalBolt.GetBoltLerpBetweenLockAndFore();

                    if (_boltLerp >= (1f - barrelForwardThreshhold))
                    {
                        float inverseLerp = Mathf.InverseLerp((1f - barrelForwardThreshhold), 1f, _boltLerp);
                        _lerpPosBolt = Vector3.Lerp(newBoltRearwardPos.localPosition, newBoltForwardPos.localPosition, inverseLerp);
                        _lerpPosBarrel = Vector3.Lerp(barrelRearwardPos.localPosition, barrelForwardPos.localPosition, inverseLerp);

                        newBolt.transform.localPosition = _lerpPosBolt;
                        barrel.transform.localPosition = _lerpPosBarrel;
                    }
                    else if (_boltLerp < (1f - barrelForwardThreshhold))
                    {
                        float inverseLerp = Mathf.InverseLerp((1f - barrelForwardThreshhold), 0f, _boltLerp);
                        _lerpPosBarrel = Vector3.Lerp(barrelRearwardPos.localPosition, barrelLockingPos.localPosition, inverseLerp);

                        newBolt.transform.localPosition = newBoltRearwardPos.localPosition;
                        barrel.transform.localPosition = _lerpPosBarrel;
                    }
                }
                else if (originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.Locked && _currentZ < _lastZ)
                {
                    newBolt.transform.localPosition = newBoltRearwardPos.localPosition;
                    barrel.transform.localPosition = barrelLockingPos.localPosition;
                }
                else if (originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.LockedToRear && _currentZ < _lastZ)
                {
                    _boltLerp = GetBoltLerpBetweenLockAndRear();
                    _lerpPosBarrel = Vector3.Lerp(barrelLockingPos.localPosition, barrelForwardPos.localPosition, _boltLerp);

                    newBolt.transform.localPosition = newBoltRearwardPos.localPosition;
                    barrel.transform.localPosition = _lerpPosBarrel;
                }
                else if (originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.Rear && (_currentZ < _lastZ || _currentZ == _lastZ))
                {
                    newBolt.transform.localPosition = newBoltRearwardPos.localPosition;
                    barrel.transform.localPosition = barrelForwardPos.localPosition;
                }
                else if ((originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.LockedToRear || originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.ForwardToMid) && _currentZ > _lastZ)
                {
                    _boltLerp = GetBoltLerpBetweenRearAndFore();
                    _lerpPosBolt = Vector3.Lerp(newBoltRearwardPos.localPosition, newBoltForwardPos.localPosition, _boltLerp);

                    newBolt.transform.localPosition = _lerpPosBolt;
                    barrel.transform.localPosition = barrelForwardPos.localPosition;
                }
                else if (originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.Forward && (_currentZ > _lastZ || _currentZ == _lastZ))
                {
                    newBolt.transform.localPosition = newBoltForwardPos.localPosition;
                    barrel.transform.localPosition = barrelForwardPos.localPosition;

                    _soundPlayed = false;
                }
                else if (originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.Locked && _currentZ == _lastZ)
                {
                    newBolt.transform.localPosition = newBoltLockingPos.localPosition;
                    barrel.transform.localPosition = barrelForwardPos.localPosition;
                }
                // Sound
                if (!_soundPlayed && ((originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.Rear && (_currentZ < _lastZ || _currentZ == _lastZ)) || (originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.LockedToRear && _currentZ > _lastZ) || (originalBolt.CurPos == OpenBoltReceiverBolt.BoltPos.ForwardToMid && _currentZ > _lastZ)))
                {
                    SM.PlayGenericSound(barrelHitForward, transform.position);
                    _soundPlayed = true;
                }
            }
            _lastZ = _currentZ;
        }

        private float GetBoltLerpBetweenRearAndFore()
        {
            return Mathf.InverseLerp(originalBolt.m_boltZ_rear, originalBolt.m_boltZ_forward, originalBolt.transform.localPosition.z);
        }

        private float GetBoltLerpBetweenLockAndRear()
        {
            return Mathf.InverseLerp(originalBolt.m_boltZ_lock, originalBolt.m_boltZ_rear, originalBolt.transform.localPosition.z);
        }
    }
}
