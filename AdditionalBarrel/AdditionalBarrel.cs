using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
    public class AdditionalBarrel : MonoBehaviour
    {
        public FVRFireArm fireArm;

        public Transform muzzle;


    #if !DEBUG
        public void Start()
        {
            On.FistVR.FVRFireArm.Fire += FVRFireArm_Fire;
        }

        public void OnDestroy()
        {
            On.FistVR.FVRFireArm.Fire -= FVRFireArm_Fire;
        }

        private void FVRFireArm_Fire(On.FistVR.FVRFireArm.orig_Fire orig, FVRFireArm self, FVRFireArmChamber chamber, Transform muzzle, bool doBuzz, float velMult, float rangeOverride)
        {
            orig(self, chamber, muzzle, doBuzz, velMult, rangeOverride);
            if (this.fireArm == self)
            {
                float chamberVelMult = AM.GetChamberVelMult(chamber.RoundType, Vector3.Distance(chamber.transform.position, muzzle.position));
                float num = fireArm.GetCombinedFixedDrop(fireArm.AccuracyClass) * 0.0166667f;
                Vector2 vector = fireArm.GetCombinedFixedDrift(fireArm.AccuracyClass) * 0.0166667f;

                for (int i = 0; i < chamber.GetRound().NumProjectiles; i++)
                {
                    float d = chamber.GetRound().ProjectileSpread + fireArm.m_internalMechanicalMOA + fireArm.GetCombinedMuzzleDeviceAccuracy();
                    if (chamber.GetRound().BallisticProjectilePrefab != null)
                    {
                        Vector3 b = muzzle.forward * 0.005f;
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(chamber.GetRound().BallisticProjectilePrefab, this.muzzle.position - b, this.muzzle.rotation);
                        Vector2 vector2 = (UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle) * 0.33333334f * d;
                        gameObject.transform.Rotate(new Vector3(vector2.x + vector.y + num, vector2.y + vector.x, 0f));
                        BallisticProjectile component = gameObject.GetComponent<BallisticProjectile>();
                        component.Fire(component.MuzzleVelocityBase * chamber.ChamberVelocityMultiplier * velMult * chamberVelMult, gameObject.transform.forward, fireArm);
                    }
                }
            }
        }
    #endif
    }
}
