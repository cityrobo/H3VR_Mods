using UnityEngine;
using FistVR;

namespace Cityrobo
{
    class MagSafety : MonoBehaviour
    {
        public OpenBoltReceiver openFireArm = null;
        public ClosedBoltWeapon closedFireArm = null;
        public int SafetyFireModePosition = 0;

        private int lastFireMode;
        private bool magSafetyEngaged;

        public void Update()
        {
            if (openFireArm != null)
            {
                if (openFireArm.Magazine == null && openFireArm.m_fireSelectorMode != SafetyFireModePosition)
                {
                    lastFireMode = openFireArm.m_fireSelectorMode;
                    openFireArm.m_fireSelectorMode = SafetyFireModePosition;
                    magSafetyEngaged = true;
                }
                else if (openFireArm.Magazine != null && magSafetyEngaged)
                {
                    openFireArm.m_fireSelectorMode = lastFireMode;
                    magSafetyEngaged = false;
                }
            }
            else if (closedFireArm != null)
            {
                if (closedFireArm.Magazine == null && closedFireArm.m_fireSelectorMode != SafetyFireModePosition)
                {
                    lastFireMode = closedFireArm.m_fireSelectorMode;
                    closedFireArm.m_fireSelectorMode = SafetyFireModePosition;
                    magSafetyEngaged = true;
                }
                else if (closedFireArm.Magazine != null && magSafetyEngaged)
                {
                    closedFireArm.m_fireSelectorMode = lastFireMode;
                    magSafetyEngaged = false;
                }
            }
        }
    }
}
