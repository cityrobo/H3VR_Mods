using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;


namespace Cityrobo
{
    public class AttachmentPicatinnyRailForwardStop : MonoBehaviour
    {
        public FVRFireArmAttachment Attachment;

        public void Awake()
        {
            AttachmentMountPicatinnyRail.ExistingForwardStops.Add(Attachment, this);
        }

        public void OnDestroy()
        {
            AttachmentMountPicatinnyRail.ExistingForwardStops.Remove(Attachment);
        }
    }
}
