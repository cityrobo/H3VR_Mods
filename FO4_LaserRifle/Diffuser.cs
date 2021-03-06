using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
    public class Diffuser : MonoBehaviour
    {
        public FVRFireArmAttachment attachment;


        public Transform[] muzzles;

        private FVRFireArm fireArm;
        private bool attached = false;
#if !DEBUG

        public void Update()
        {
            if (attachment.curMount != null && !attached)
            {
                fireArm = attachment.curMount.GetRootMount().MyObject as FVRFireArm;
                Hook();
                attached = true;
            }
            else if (attachment.curMount == null && attached)
            {
                fireArm = null;
                Unhook();
                attached = false;
            }
        }



        public void Hook()
        {
            On.FistVR.FVRFireArm.Fire += FVRFireArm_Fire;
        }

        public void Unhook()
        {
            On.FistVR.FVRFireArm.Fire -= FVRFireArm_Fire;
        }

        private void FVRFireArm_Fire(On.FistVR.FVRFireArm.orig_Fire orig, FistVR.FVRFireArm self, FistVR.FVRFireArmChamber chamber, UnityEngine.Transform muzzle_orig, bool doBuzz, float velMult, float rangeOverride)
        {
            if (this.fireArm == self)
            {
                foreach (Transform muzzle in muzzles)
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
                            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(chamber.GetRound().BallisticProjectilePrefab, muzzle.position - b, muzzle.rotation);
                            Vector2 vector2 = (UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle) * 0.33333334f * d;
                            gameObject.transform.Rotate(new Vector3(vector2.x + vector.y + num, vector2.y + vector.x, 0f));
                            BallisticProjectile component = gameObject.GetComponent<BallisticProjectile>();
                            component.Fire(component.MuzzleVelocityBase * chamber.ChamberVelocityMultiplier * (velMult/muzzles.Length) * chamberVelMult, gameObject.transform.forward, fireArm);
                        }
                    }
                }
            }
            else orig(self, chamber, muzzle_orig, doBuzz, velMult, rangeOverride);
        }
    #endif
    }
}
