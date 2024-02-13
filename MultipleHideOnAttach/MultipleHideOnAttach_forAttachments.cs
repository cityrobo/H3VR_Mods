using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;
namespace Cityrobo
{
    public class MultipleHideOnAttach_forAttachments : MonoBehaviour
    {
        [Header("Attachment to monitor for attachments:")]
        public FVRFireArmAttachment attachment;

        [Header("List of GameObjects to affect:")]
        public List<GameObject> objectToHideOrShow;

        [Header("Enable to show Objects instead:")]
        public bool showOnAttach = false;

        public void Awake()
        {
            gameObject.SetActive(false);
            OpenScripts2.MultipleHideOnAttachForAttachments newComponent = gameObject.AddComponent<OpenScripts2.MultipleHideOnAttachForAttachments>();
            newComponent.Attachment = attachment;
            newComponent.ShowOnAttach = showOnAttach;
            newComponent.ObjectToHideOrShow = objectToHideOrShow;
            gameObject.SetActive(true);

            Destroy(this);
            /*
            if (attachment.DisableOnHover == null)
            {
                attachment.DisableOnHover = new GameObject();
            }
            */
        }
#if !DEBUG
        //public void Update()
        //{

        //    if (attachment.Sensor.CurHoveredMount != null)
        //    {
        //        foreach (GameObject gameObject in objectToHideOrShow)
        //        {
        //            gameObject.SetActive(showOnAttach);
        //        }
        //    }
        //    else
        //    {
        //        foreach (GameObject gameObject in objectToHideOrShow)
        //        {
        //            gameObject.SetActive(!showOnAttach);
        //        }
        //    }
        //}
#endif
    }
}
