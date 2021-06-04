using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Cityrobo
{ 
    public class Attachable_Magazine_Hooks
    {
        public Attachable_Magazine_Hooks()
        {

        }

        public void Hook()
        {
            On.FistVR.FVRFireArmMagazine.Awake += FVRFireArmMagazine_Awake;
            On.FistVR.FVRFireArmAttachment.Awake += FVRFireArmAttachment_Awake;
        }
        public void Unhook()
        {
            On.FistVR.FVRFireArmMagazine.Awake -= FVRFireArmMagazine_Awake;
            On.FistVR.FVRFireArmAttachment.Awake -= FVRFireArmAttachment_Awake;
        }

        private void FVRFireArmAttachment_Awake(On.FistVR.FVRFireArmAttachment.orig_Awake orig, FistVR.FVRFireArmAttachment self)
        {
            orig(self);
            Attachable_Magazine attachable_Magazine = self.gameObject.GetComponent<Attachable_Magazine>();
            if (attachable_Magazine != null)
            {
                attachable_Magazine.attachment_Ready = true;
                Debug.Log("Attachment ready!");
            } 
        }

        private void FVRFireArmMagazine_Awake(On.FistVR.FVRFireArmMagazine.orig_Awake orig, FistVR.FVRFireArmMagazine self)
        {
            orig(self);
            Attachable_Magazine attachable_Magazine = null;
            Transform parent = self.gameObject.transform.parent;
            if (parent != null)
            {
                Debug.Log("Mag has parent!");
                attachable_Magazine = parent.gameObject.GetComponent<Attachable_Magazine>();
                if (attachable_Magazine != null)
                {
                    attachable_Magazine.mag_Ready = true;
                    Debug.Log("Mag ready!");
                }
                else Debug.Log("ERROR: Parent is missing Attachable_Magazine component!");
            }
        }
    }
}
