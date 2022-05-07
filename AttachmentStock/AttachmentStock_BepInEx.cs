#if!DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.attachment_stock", "OpenScripts: AttachmentStock Script", "1.0.0")]
    class AttachmentStock_BepInEx : BaseUnityPlugin
    {
        public AttachmentStock_BepInEx()
        {
            //Logger.LogInfo("OpenScripts: AttachmentStock Script loaded!");
        }
    }
}
#endif