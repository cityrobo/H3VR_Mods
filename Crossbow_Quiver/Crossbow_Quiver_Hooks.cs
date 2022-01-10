#if !DEBUG
namespace Cityrobo
{
    class Crossbow_Quiver_Hooks
    {
        public void Hook()
        {
            On.FistVR.FVRFireArmChamber.EjectRound += FVRFireArmChamber_EjectRound;
        }

        private FistVR.FVRFireArmRound FVRFireArmChamber_EjectRound(On.FistVR.FVRFireArmChamber.orig_EjectRound orig, FistVR.FVRFireArmChamber self, UnityEngine.Vector3 EjectionPosition, UnityEngine.Vector3 EjectionVelocity, UnityEngine.Vector3 EjectionAngularVelocity, bool ForceCaseLessEject)
        {
            ForceCaseLessEject = true;
            return orig(self, EjectionPosition, EjectionVelocity, EjectionAngularVelocity, ForceCaseLessEject);
        }

        public void Unhook()
        {
            On.FistVR.FVRFireArmChamber.EjectRound -= FVRFireArmChamber_EjectRound;
        }
    }
}
#endif