using UnityEngine;
using FistVR;

namespace Cityrobo
{
    class MagLock : MonoBehaviour
    {
        public OpenBoltReceiver openBoltWeapon;
        public int safetyMode = 0;
        //public ClosedBolt closedBoltWeapon;
        public bool safetyLock = false;
        public GameObject lockHandle;
        public GameObject MagReloadTrigger;
        public Vector3 start;
        public Vector3 end;

        public float wiggleroom = 0.05f;
        public enum dirtype
        {
            x = 0,
            y = 1,
            z = 2,
            w = 3
        }

        public dirtype direction;
        public bool is_rotation;

        private bool isLocked;
        private int lastFireMode;

#if!DEBUG
        public void Update()
        {
            float pos = Mathf.InverseLerp(start[(int)direction], end[(int)direction], lockHandle.transform.localPosition[(int)direction]);

            if (safetyLock)
            {
                if ((0f + wiggleroom) <= pos)
                {
                    if(!isLocked) lastFireMode = openBoltWeapon.m_fireSelectorMode;
                    openBoltWeapon.m_fireSelectorMode = safetyMode;
                    isLocked = true;
                }
                else if ((0f + wiggleroom) >= pos && isLocked)
                {
                    openBoltWeapon.m_fireSelectorMode = lastFireMode;
                    isLocked = false;
                }
            }
            if ((1f - wiggleroom) <= pos) MagReloadTrigger.SetActive(true);
            else MagReloadTrigger.SetActive(false);
            /*if (openBoltWeapon != null)
            {

            }
            else if (closedBoltWeapon != null)
            {

            }*/


        }
#endif
    }
}
