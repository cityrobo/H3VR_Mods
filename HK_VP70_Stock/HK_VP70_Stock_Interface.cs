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

            HK_VP70_Stock stock = Attachment as HK_VP70_Stock;
            stock.AddBurst();
            //stock.SightFlipper.gameObject.layer = LayerMask.NameToLayer("NoCol");
        }

        public override void OnDetach()
        {
            base.OnDetach();

            HK_VP70_Stock stock = Attachment as HK_VP70_Stock;
            stock.RemoveBurst();
            //stock.SightFlipper.gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
#endif
    }
}
