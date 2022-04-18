#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.attachment_mount_parent_to_this", "OpenScripts: AttachmentMountParentToThis Script", "1.0.0")]
    class AttachmentMountParentToThis_BepInEx : BaseUnityPlugin
    {
        public AttachmentMountParentToThis_BepInEx()
        {
            Logger.LogInfo("OpenScripts: AttachmentMountParentToThis loaded!");
        }
    }
}
#endif