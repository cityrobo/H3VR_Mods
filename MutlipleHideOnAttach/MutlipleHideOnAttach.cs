using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;
namespace Cityrobo
{
    public class MutlipleHideOnAttach : MonoBehaviour
    {
        public FVRFireArmAttachmentMount attachmentMount;

        public List<GameObject> object_to_hide;

        public void Awake()
        {
            if (attachmentMount.DisableOnHover == null)
            {
                attachmentMount.DisableOnHover = object_to_hide[0];
            }

        }

        public void Update()
        {
            if (attachmentMount.DisableOnHover.activeInHierarchy == false)
            {
                foreach (GameObject gameObject in object_to_hide)
                {
                    gameObject.SetActive(false);
                }
            }
            else foreach (GameObject gameObject in object_to_hide)
                {
                    gameObject.SetActive(true);
                }
        }
    }
}
