using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class LeftHandModeIgnoreQBSlot : FVRQuickBeltSlot
    {
#if !(UNITY_EDITOR || UNITY_5)
        public virtual void Start()
        {
            if (GM.Options.QuickbeltOptions.QuickbeltHandedness > 0)
            {
                Vector3 vector = PoseOverride.forward;
                Vector3 vector2 = PoseOverride.up;
                vector = Vector3.Reflect(vector, -transform.right);
                vector2 = Vector3.Reflect(vector2, -transform.right);
                PoseOverride.rotation = Quaternion.LookRotation(vector, vector2);

                transform.localPosition = new Vector3(-transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
            }
        }
#endif
	}
}
