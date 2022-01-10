using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace Cityrobo
{
    public class RecoilPercentage : MonoBehaviour
    {
        public FVRFireArm fireArm;
        public float multiplier;
#if!(UNITY_EDITOR || UNITY_5)
        void Start()
        {
            FVRFireArmRecoilProfile origRecoilProfile = fireArm.RecoilProfile;
            FVRFireArmRecoilProfile origRecoilProfileStocked = fireArm.RecoilProfileStocked;

            FVRFireArmRecoilProfile recoilProfile = FVRFireArmRecoilProfile.Instantiate(fireArm.RecoilProfile);
            FVRFireArmRecoilProfile recoilProfileStocked = FVRFireArmRecoilProfile.Instantiate(fireArm.RecoilProfileStocked);


        }
        void AdjustRecoilProfile(Component original)
        {
            System.Type type = original.GetType();
            // Copied fields can be restricted with BindingFlags
            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(original);

                if (value is float)
                {
                    field.SetValue(original, ((float) value) * multiplier);
                }
                else if (value is Quaternion)
                {
                    Quaternion quaternion = (Quaternion) value;
                    quaternion.x = 1 / multiplier;
                    quaternion.y = 1 / multiplier;
                    quaternion.z = 1 / multiplier;
                    quaternion.w = 1 / multiplier;
                    field.SetValue(original, quaternion);
                }


            }
        }
#endif
    }
}