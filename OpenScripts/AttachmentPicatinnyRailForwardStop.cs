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
#if !(DEBUG)
        public void Start()
        {
            gameObject.SetActive(false);

            OpenScripts2.AttachmentPicatinnyRailForwardStop newRail = gameObject.AddComponent<OpenScripts2.AttachmentPicatinnyRailForwardStop>();

            newRail.Attachment = Attachment;

            gameObject.SetActive(true);

            Destroy(this);
            // AttachmentMountPicatinnyRail.ExistingForwardStops.Add(Attachment, this);
        }


        public void OnDestroy()
        {
            // AttachmentMountPicatinnyRail.ExistingForwardStops.Remove(Attachment);
        }
#endif
    }
}