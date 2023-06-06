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

        //private static readonly List<FVRFireArmAttachmentMount> _exisingAttachmentMountParentToThis = new List<FVRFireArmAttachmentMount>();

#if!(DEBUG || MEATKIT)
        public void Awake()
        {
            mount.gameObject.AddComponent<OpenScripts2.AttachmentMountParentToThis>();

            Destroy(this);
            //_exisingAttachmentMountParentToThis.Add(mount);
            //mount.ParentToThis = true;
        }

        //public void OnDestoy()
        //{
        //    _exisingAttachmentMountParentToThis.Remove(mount);
        //}

        //static AttachmentMountParentToThis()
        //{
        //    On.FistVR.FVRFireArmAttachment.AttachToMount += FVRFireArmAttachment_AttachToMount;
        //}

        //private static void FVRFireArmAttachment_AttachToMount(On.FistVR.FVRFireArmAttachment.orig_AttachToMount orig, FVRFireArmAttachment self, FVRFireArmAttachmentMount m, bool playSound)
        //{
        //    orig(self, m, playSound);

        //    if (_exisingAttachmentMountParentToThis.Contains(m))
        //    {
        //        self.SetParentage(m.transform);
        //    }
        //    else if (_exisingAttachmentMountParentToThis.Count > 0 && m.MyObject is FVRFireArmAttachment parentAttachment)
        //    {
        //        do
        //        {
        //            if (_exisingAttachmentMountParentToThis.Contains(parentAttachment.curMount))
        //            {
        //                self.SetParentage(parentAttachment.curMount.transform);
        //                break;
        //            }
        //            parentAttachment = parentAttachment.curMount.MyObject as FVRFireArmAttachment;
        //        } while (parentAttachment != null);
        //    }
        //}
#endif
    }
}
