using UnityEngine;
using FistVR;

namespace Cityrobo
{
    public class MagMove : MonoBehaviour
    {
        public FVRFireArm fireArm = null;

        private bool posChanged;
        public void Update()
        {
            if (fireArm.Magazine == null)
            {
                if (posChanged) posChanged = false;
            }
            else if (!posChanged)
            {
                fireArm.Magazine.SetParentage(fireArm.MagazineMountPos.transform);
            }
        }
    }
}
