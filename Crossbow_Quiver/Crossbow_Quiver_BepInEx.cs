#if !DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.CrossbowQuiver", "Crossbow Quiver Scripts", "1.1.0")]
    class Crossbow_Quiver_BepInEx : BaseUnityPlugin
    {
        public Crossbow_Quiver_BepInEx()
        {
            //Logger.LogInfo("Crossbow Quiver Scripts loaded!");
            On.FistVR.FVRFireArmChamber.EjectRound_Vector3_Vector3_Vector3_bool += FVRFireArmChamber_EjectRound;
        }
        private FistVR.FVRFireArmRound FVRFireArmChamber_EjectRound(On.FistVR.FVRFireArmChamber.orig_EjectRound_Vector3_Vector3_Vector3_bool orig, FistVR.FVRFireArmChamber self, UnityEngine.Vector3 EjectionPosition, UnityEngine.Vector3 EjectionVelocity, UnityEngine.Vector3 EjectionAngularVelocity, bool ForceCaseLessEject)
        {
            ForceCaseLessEject = true;
            return orig(self, EjectionPosition, EjectionVelocity, EjectionAngularVelocity, ForceCaseLessEject);
        }
        public void OnDestroy()
        {
            On.FistVR.FVRFireArmChamber.EjectRound_Vector3_Vector3_Vector3_bool -= FVRFireArmChamber_EjectRound;
        }
    }
}
#endif