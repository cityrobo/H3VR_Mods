using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Deli.Setup;
using UnityEngine;

namespace Cityrobo
{
    public class BrighterFlashlights_Plugin : DeliBehaviour
    {
        private readonly BrighterFlashlights_Hooks _hooks;

        public BrighterFlashlights_Plugin()
        {
            Logger.LogInfo("BrighterFlashlights loaded!");
            _hooks = new BrighterFlashlights_Hooks();
            _hooks.Hook();
        }

        private void OnDestroy()
        {
            _hooks?.Unhook();
        }

    }
}
