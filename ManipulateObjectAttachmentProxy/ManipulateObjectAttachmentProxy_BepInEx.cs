#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.manipulate_object_attachment_proxy", "OpenScripts: ManipulateObjectAttachmentProxy Script", "1.0.0")]
    public class ManipulateObjectAttachmentProxy_BepInEx : BaseUnityPlugin
    {
        public ManipulateObjectAttachmentProxy_BepInEx()
        {
            Logger.LogInfo("OpenScripts: ManipulateObjectAttachmentProxy Script loaded!");
        }
    }
}
#endif