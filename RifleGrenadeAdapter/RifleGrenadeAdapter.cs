using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class RifleGrenadeAdapter : MuzzleDevice
    {
		[Header("Rifle Grenade Adapter Config")]
        public FVRFireArmChamber GrenadeChamber;
		public Transform GrenadeMuzzle;

		public float VelocityMultiplier = 1f;
		public float RangeOverride = -1f;

		public AudioEvent GrenadeShot;

        private Vector3 _origMuzzlePos;
		private Quaternion _origMuzzleRot;
        private FVRFireArm _fireArm;

#if !(UNITY_EDITOR || UNITY_5)
		public override void Awake()
        {
            base.Awake();

            _origMuzzlePos = this.Muzzle.localPosition;
			_origMuzzleRot = this.Muzzle.localRotation;
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();

			if (GrenadeChamber.IsFull)
            {
				this.Muzzle.position = Vector3.down * 3 + this.transform.TransformPoint(_origMuzzlePos);
				this.Muzzle.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);

				if (GrenadeChamber.IsSpent && GrenadeChamber.GetRound().IsCaseless) GrenadeChamber.Unload();
            }
            else
            {
				this.Muzzle.localPosition = _origMuzzlePos;
				this.Muzzle.localRotation = _origMuzzleRot;
			}
        }

        public override void AttachToMount(FVRFireArmAttachmentMount m, bool playSound)
        {
            base.AttachToMount(m, playSound);

			_fireArm = m.GetRootMount().MyObject as FVRFireArm;
        }

        public override void OnShot(FVRFireArm f, FVRTailSoundClass tailClass)
        {
            base.OnShot(f, tailClass);

            FireGrenade();
        }

        private void FireGrenade()
        {
            if (!GrenadeChamber.Fire() || _fireArm == null) return;

			//float chamberVelMult = AM.GetChamberVelMult(Chamber.RoundType, Vector3.Distance(Chamber.transform.position, GrenadeMuzzle.position));
			float num = _fireArm.GetCombinedFixedDrop(this.MechanicalAccuracy) * 0.0166667f;
			Vector2 vector = _fireArm.GetCombinedFixedDrift(this.MechanicalAccuracy) * 0.0166667f;

			for (int i = 0; i < GrenadeChamber.GetRound().NumProjectiles; i++)
			{
				float d = GrenadeChamber.GetRound().ProjectileSpread + this.m_mechanicalAccuracy;
				if (GrenadeChamber.GetRound().BallisticProjectilePrefab != null)
				{
					Vector3 b = GrenadeMuzzle.forward * 0.005f;
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GrenadeChamber.GetRound().BallisticProjectilePrefab, GrenadeMuzzle.position - b, GrenadeMuzzle.rotation);
					Vector2 vector2 = (UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle) * 0.33333334f * d;
					gameObject.transform.Rotate(new Vector3(vector2.x + vector.y + num, vector2.y + vector.x, 0f));
					BallisticProjectile component = gameObject.GetComponent<BallisticProjectile>();
					component.Fire(component.MuzzleVelocityBase * GrenadeChamber.ChamberVelocityMultiplier * VelocityMultiplier /* * chamberVelMult*/, gameObject.transform.forward, _fireArm, true);
					if (RangeOverride > 0f)
					{
						component.ForceSetMaxDist(RangeOverride);
					}
				}
			}

			SM.PlayCoreSound(FVRPooledAudioType.GunShot, GrenadeShot, GrenadeMuzzle.position);
		}
#endif
	}
}
