using BepInEx;
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
    [BepInPlugin("h3vr.cityrobo.UnlimitedPointLights", "Unlimited Point Lights", "1.0.0")]
    public class UnlimitedPointLights : BaseUnityPlugin
    {
        public void Update()
        {
            QualitySettings.pixelLightCount = int.MaxValue;
        }
#if !DEBUG

#endif
    }
}
