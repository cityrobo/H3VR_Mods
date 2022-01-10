using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class LongRecoilSystem_TubeFedShotgun : MonoBehaviour
    {
        public TubeFedShotgunBolt originalBolt;

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

        private bool wasHeld = false;
        private float currentZ;
        private float lastZ;

        private bool soundPlayed = false;

        public void Start()
        {
            currentZ = originalBolt.m_boltZ_current;
            lastZ = currentZ;
        }

        public void Update()
        {
            currentZ = originalBolt.m_boltZ_current;
            if (originalBolt.IsHeld)
            {
                float boltLerp = originalBolt.GetBoltLerpBetweenRearAndFore();
                Vector3 lerpPos = Vector3.Lerp(newBoltRearwardPos.localPosition, newBoltForwardPos.localPosition, boltLerp);
                newBolt.transform.localPosition = lerpPos;
                wasHeld = true;
            }

            if (wasHeld)
            {
                float boltLerp = originalBolt.GetBoltLerpBetweenRearAndFore();
                Vector3 lerpPos = Vector3.Lerp(newBoltRearwardPos.localPosition, newBoltForwardPos.localPosition, boltLerp);
                newBolt.transform.localPosition = lerpPos;
                if (originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.Forward) wasHeld = false;
            }

            if (!wasHeld)
            {
                if (originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.ForwardToMid && currentZ < lastZ)
                {
                    float boltLerp = originalBolt.GetBoltLerpBetweenLockAndFore();
                    if (boltLerp >= (1f - barrelForwardThreshhold))
                    {
                        float inverseLerp = Mathf.InverseLerp((1f - barrelForwardThreshhold), 1f, boltLerp);
                        Vector3 lerpPosBolt = Vector3.Lerp(newBoltRearwardPos.localPosition, newBoltForwardPos.localPosition, inverseLerp);
                        Vector3 lerpPosBarrel = Vector3.Lerp(barrelRearwardPos.localPosition, barrelForwardPos.localPosition, inverseLerp);

                        newBolt.transform.localPosition = lerpPosBolt;
                        barrel.transform.localPosition = lerpPosBarrel;
                    }
                    else if (boltLerp < (1f - barrelForwardThreshhold))
                    {
                        float inverseLerp = Mathf.InverseLerp((1f - barrelForwardThreshhold), 0f, boltLerp);
                        Vector3 lerpPosBarrel = Vector3.Lerp(barrelRearwardPos.localPosition, barrelLockingPos.localPosition, inverseLerp);

                        barrel.transform.localPosition = lerpPosBarrel;
                    }
                }
                else if (originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.Locked && currentZ < lastZ)
                {
                    newBolt.transform.localPosition = newBoltRearwardPos.localPosition;
                    barrel.transform.localPosition = barrelLockingPos.localPosition;
                }
                else if (originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.LockedToRear && currentZ < lastZ)
                {
                    float boltLerp = Mathf.InverseLerp(originalBolt.m_boltZ_lock, originalBolt.m_boltZ_rear, originalBolt.m_boltZ_current);
                    Vector3 lerpPosBarrel = Vector3.Lerp(barrelLockingPos.localPosition, barrelForwardPos.localPosition, boltLerp);

                    barrel.transform.localPosition = lerpPosBarrel;
                }
                else if (originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.Rear && (currentZ < lastZ || currentZ == lastZ))
                {
                    newBolt.transform.localPosition = newBoltRearwardPos.localPosition;
                    barrel.transform.localPosition = barrelForwardPos.localPosition;
                }
                else if ((originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.LockedToRear || originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.ForwardToMid) && currentZ > lastZ)
                {
                    float boltLerp = originalBolt.GetBoltLerpBetweenRearAndFore();
                    Vector3 lerpPosBolt = Vector3.Lerp(newBoltRearwardPos.localPosition, newBoltForwardPos.localPosition, boltLerp);

                    newBolt.transform.localPosition = lerpPosBolt;
                }
                else if (originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.Forward && (currentZ > lastZ || currentZ == lastZ))
                {
                    newBolt.transform.localPosition = newBoltForwardPos.localPosition;
                    barrel.transform.localPosition = barrelForwardPos.localPosition;

                    soundPlayed = false;
                }
                else if (originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.Locked && currentZ == lastZ)
                {
                    newBolt.transform.localPosition = newBoltLockingPos.localPosition;
                    barrel.transform.localPosition = barrelForwardPos.localPosition;
                }

                if (!soundPlayed && ((originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.Rear && (currentZ < lastZ || currentZ == lastZ)) || (originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.LockedToRear && currentZ > lastZ) || (originalBolt.CurPos == TubeFedShotgunBolt.BoltPos.ForwardToMid && currentZ > lastZ)))
                {
                    SM.PlayGenericSound(barrelHitForward, transform.position);
                    soundPlayed = true;
                }
            }
            lastZ = currentZ;
        }
    }
}
