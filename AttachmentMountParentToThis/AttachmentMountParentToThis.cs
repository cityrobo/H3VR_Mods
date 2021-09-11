using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class AttachmentMountParentToThis : MonoBehaviour
    {
        public FVRFireArmAttachmentMount mount;

#if!DEBUG
        public void Awake()
        {
            mount.ParentToThis = true;
            On.FistVR.FVRFireArmAttachmentMount.GetRootMount += FVRFireArmAttachmentMount_GetRootMount;
        }

        public void OnDestoy()
        {
            On.FistVR.FVRFireArmAttachmentMount.GetRootMount -= FVRFireArmAttachmentMount_GetRootMount;
        }
        private FVRFireArmAttachmentMount FVRFireArmAttachmentMount_GetRootMount(On.FistVR.FVRFireArmAttachmentMount.orig_GetRootMount orig, FVRFireArmAttachmentMount self)
        {
            if (self == mount)
            {
                return self;
            }
            else return orig(self);
        }
#endif
    }
}
