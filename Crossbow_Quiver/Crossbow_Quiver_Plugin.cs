using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Cityrobo
{
    public class Crossbow_Quiver_Plugin : MonoBehaviour
    {
#if !DEBUG
        Crossbow_Quiver_Hooks _hooks;
        public void Awake()
        {
            _hooks = new Crossbow_Quiver_Hooks();
            _hooks.Hook();
        }

    public void OnDestroy()
        {
            _hooks?.Unhook();
        }
#endif
    }
}