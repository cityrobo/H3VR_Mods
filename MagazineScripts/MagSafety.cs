using UnityEngine;
using FistVR;

namespace Cityrobo
{
    class MagSafety : MonoBehaviour
    {
        public FVRFireArm fireArm = null;
        public int SafetyFireModePosition = 0;

        private int lastFireMode;
        private bool magSafetyEngaged;
#if !(UNITY_EDITOR || UNITY_5)
        public void Update()
        {
            switch (fireArm)
            {
                case OpenBoltReceiver s:
                    CheckState(s);
                    break;
                case ClosedBoltWeapon s:
                    CheckState(s);
                    break;
                case BoltActionRifle s:
                    CheckState(s);
                    break;
                default:
                    break;
            }
        }

        void CheckState(OpenBoltReceiver s)
        {
            if (s.Magazine == null && s.m_fireSelectorMode != SafetyFireModePosition)
            {
                lastFireMode = s.m_fireSelectorMode;
                s.m_fireSelectorMode = SafetyFireModePosition;
                magSafetyEngaged = true;
            }
            else if (s.Magazine != null && magSafetyEngaged)
            {
                s.m_fireSelectorMode = lastFireMode;
                magSafetyEngaged = false;
            }
        }
        void CheckState(ClosedBoltWeapon s)
        {
            if (s.Magazine == null && s.m_fireSelectorMode != SafetyFireModePosition)
            {
                lastFireMode = s.m_fireSelectorMode;
                s.m_fireSelectorMode = SafetyFireModePosition;
                magSafetyEngaged = true;
            }
            else if (s.Magazine != null && magSafetyEngaged)
            {
                s.m_fireSelectorMode = lastFireMode;
                magSafetyEngaged = false;
            }
        }
        void CheckState(BoltActionRifle s)
        {
            if (s.Magazine == null && s.m_fireSelectorMode != SafetyFireModePosition)
            {
                lastFireMode = s.m_fireSelectorMode;
                s.m_fireSelectorMode = SafetyFireModePosition;
                magSafetyEngaged = true;
            }
            else if (s.Magazine != null && magSafetyEngaged)
            {
                s.m_fireSelectorMode = lastFireMode;
                magSafetyEngaged = false;
            }
        }
#endif
    }
}
