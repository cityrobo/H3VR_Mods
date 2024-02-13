using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;
namespace Cityrobo
{
    public class MultipleHideOnAttach : MonoBehaviour
    {
        [Header("Mount to monitor for attachments:")]
        public FVRFireArmAttachmentMount attachmentMount;

        [Header("List of GameObjects to affect:")]
        public List<GameObject> objectToHideOrShow;

        [Header("Enable to show Objects instead:")]
        public bool showOnAttach = false;

        public void Awake()
        {
            gameObject.SetActive(false);
            OpenScripts2.MultipleHideOnAttach newComponent = gameObject.AddComponent<OpenScripts2.MultipleHideOnAttach>();
            newComponent.attachmentMount = attachmentMount;
            newComponent.ShowOnAttach = showOnAttach;
            newComponent.ObjectToHideOrShow = objectToHideOrShow;
            gameObject.SetActive(true);

            Destroy(this);
            //attachmentMount.HasHoverDisablePiece = true;
            //if (attachmentMount.DisableOnHover == null)
            //{
            //    attachmentMount.DisableOnHover = new GameObject("MultipleHideOnAttach_Proxy");
            //}
        }
#if !DEBUG
        //public void Update()
        //{

        //    if (attachmentMount.DisableOnHover.activeInHierarchy == false)
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
