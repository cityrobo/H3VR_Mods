using UnityEngine;
using FistVR;
using OpenScripts2;

namespace Cityrobo
{
    public class MagMove : MonoBehaviour
    {
        public FVRFireArm fireArm = null;

        private bool posChanged;
#if!DEBUG
        public void Awake()
        {
            gameObject.SetActive(false);
            ForceMagazineMountingToMagMountPos forceMagazineMountingToMagMountPos = gameObject.AddComponent<ForceMagazineMountingToMagMountPos>();
            forceMagazineMountingToMagMountPos.FireArm = fireArm;
            gameObject.SetActive(true);

            Destroy(this);
        }

        //public void Update()
        //{
        //    if (fireArm.Magazine == null)
        //    {
        //        if (posChanged) posChanged = false;
        //    }
        //    else if (!posChanged)
        //    {
        //        fireArm.Magazine.SetParentage(fireArm.MagazineMountPos.transform);
        //        posChanged = true;
        //    }
        //}
#endif
    }
}
