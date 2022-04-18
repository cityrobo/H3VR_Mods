#if!DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.cap_and_ball_revolver", "OpenScripts: CapAndBallRevolver Scripts", "1.0.0")]
    class CapAndBallRevolver_BepInEx : BaseUnityPlugin
    {
        public CapAndBallRevolver_BepInEx()
        {
            Logger.LogInfo("OpenScripts: CapAndBallRevolver Scripts loaded!");
        }
    }
}
#endif