#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.scope_shader_zoom", "ScopeShaderZoom Script", "1.0.0")]
    class ScopeShaderZoom_BepInEx : BaseUnityPlugin
    {
        public ScopeShaderZoom_BepInEx()
        {
            Logger.LogInfo("ScopeShaderZoom Script loaded!");
        }
    }
}
#endif