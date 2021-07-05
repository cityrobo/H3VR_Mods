#if !DEBUG
using UnityEngine;
using FistVR;
namespace Cityrobo
{
    class AdditionalBarrel_hooks
    {
        AdditionalBarrel barrel;
        
        AdditionalBarrel_hooks(AdditionalBarrel b)
        {
            barrel = b;
        }
        public void Hook()
        {
            On.FistVR.FVRFireArm.Fire += FVRFireArm_Fire;
        }

        private void FVRFireArm_Fire(On.FistVR.FVRFireArm.orig_Fire orig, FistVR.FVRFireArm self, FistVR.FVRFireArmChamber chamber, UnityEngine.Transform muzzle, bool doBuzz, float velMult)
        {
            orig(self,chamber,muzzle,doBuzz,velMult);
            float chamberVelMult = AM.GetChamberVelMult(chamber.RoundType, Vector3.Distance(chamber.transform.position, muzzle.position));
            float num = self.GetCombinedFixedDrop(self.AccuracyClass) * 0.0166667f;
            Vector2 vector = self.GetCombinedFixedDrift(self.AccuracyClass) * 0.0166667f;

            for (int i = 0; i < chamber.GetRound().NumProjectiles; i++)
            {
                float d = chamber.GetRound().ProjectileSpread + self.m_internalMechanicalMOA + self.GetCombinedMuzzleDeviceAccuracy();
                if (chamber.GetRound().BallisticProjectilePrefab != null)
                {
                    Vector3 b = muzzle.forward * 0.005f;
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(chamber.GetRound().BallisticProjectilePrefab, barrel.muzzle.position - b, barrel.muzzle.rotation);
                    Vector2 vector2 = (UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle) * 0.33333334f * d;
                    gameObject.transform.Rotate(new Vector3(vector2.x + vector.y + num, vector2.y + vector.x, 0f));
                    BallisticProjectile component = gameObject.GetComponent<BallisticProjectile>();
                    component.Fire(component.MuzzleVelocityBase * chamber.ChamberVelocityMultiplier * velMult * chamberVelMult, gameObject.transform.forward, self);
                }
            }
        }
    }
}
#endif