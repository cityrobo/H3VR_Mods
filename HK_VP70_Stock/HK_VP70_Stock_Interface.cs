using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class HK_VP70_Stock_Interface : AttachableStock
    {

#if !(DEBUG || MEATKIT)
        public override void OnAttach()
        {
            base.OnAttach();

            (Attachment as HK_VP70_Stock).AddBurst();
        }

        public override void OnDetach()
        {
            base.OnDetach();

            (Attachment as HK_VP70_Stock).RemoveBurst();
        }
#endif
    }
}
