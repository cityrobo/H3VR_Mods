using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class TopAttackSystem : MonoBehaviour
    {
		public FVRFireArm FireArm;

        public LayerMask TargetMask;
		public float MaxRange = 2000f;

        private Vector3? _targetPoint = null;
		private Rigidbody _targetRB;

		public TopAttackProjectile.EAttackMode AttackMode;

		public float MinRangeTopAttackMode = 150f;
		public float MinRangeFrontalAttackMode = 65f;

		[Header("Mode Text Config")]
		public Text ModeTextField;
		public string TopAttackModeText = "Top";
		public string FrontalAttackModeText = "Frontal";

		[Header("Rangefinder Text Config")]
		public Text RangeTextField;
		public string OutOfRangeText = "INF";

		[Header("Target Text Config")]
		public Text TargetTextField;
		public string NoTargetText = "No Target";
		public string PositionTargetText = "Target Position: {0:F0}X {1:F0}Y {2:F0}Z";
		public string RigidbodyTargetText = "Target Object: {0}";

		private string[] _modeTexts;
		private const string _removeFromName = "(Clone)";

#if !(DEBUG || MEATKIT)

		public void Awake()
		{
			Hook();
			_modeTexts = new string[]{ TopAttackModeText, FrontalAttackModeText };

		}
		public void OnDestroy()
		{
			Unhook();
		}

		public void Update()
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, MaxRange, TargetMask,QueryTriggerInteraction.Collide))
            {
				RangeTextField.text = string.Format("{0:F0}m", hit.distance);

                if (AttackMode == TopAttackProjectile.EAttackMode.Top && hit.distance > MinRangeTopAttackMode)
                {
					_targetPoint = hit.point;
					_targetRB = hit.rigidbody;
				}
				else if (AttackMode == TopAttackProjectile.EAttackMode.Direct && hit.distance > MinRangeFrontalAttackMode)
                {
					_targetPoint = hit.point;
					_targetRB = hit.rigidbody;
				}

				if (_targetRB != null)
				{
					string targetName = _targetRB.name.Replace(_removeFromName, "");
					TargetTextField.text = string.Format(RigidbodyTargetText, targetName);
				}
				else
				{
					TargetTextField.text = string.Format(PositionTargetText, hit.point.x, hit.point.y, hit.point.z);
				}
			}
            else
            {
				RangeTextField.text = OutOfRangeText;
				TargetTextField.text = NoTargetText;

				_targetPoint = null;
				_targetRB = null;
			}
        }
		public void Unhook()
		{
			On.FistVR.FVRFireArm.Fire -= FVRFireArm_Fire; 
			On.FistVR.FVRPhysicalObject.UpdateInteraction -= FVRPhysicalObject_UpdateInteraction;
		}
		public void Hook()
		{
			On.FistVR.FVRFireArm.Fire += FVRFireArm_Fire;
            On.FistVR.FVRPhysicalObject.UpdateInteraction += FVRPhysicalObject_UpdateInteraction;
		}

        private void FVRPhysicalObject_UpdateInteraction(On.FistVR.FVRPhysicalObject.orig_UpdateInteraction orig, FVRPhysicalObject self, FVRViveHand hand)
        {
            orig(self, hand);
			if (self == FireArm)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes,Vector2.left)<45f)
                {
					ChangeMode();
                }
            }
        }

		private void ChangeMode()
        {
			switch (AttackMode)
			{
				case TopAttackProjectile.EAttackMode.Top:
					AttackMode = TopAttackProjectile.EAttackMode.Direct;
					break;
				case TopAttackProjectile.EAttackMode.Direct:
					AttackMode = TopAttackProjectile.EAttackMode.Top;
					break;
				default:
					break;
			}
			if (ModeTextField != null)
			{
				ModeTextField.text = _modeTexts[(int)AttackMode];
			}
		}

        private void FVRFireArm_Fire(On.FistVR.FVRFireArm.orig_Fire orig, FVRFireArm self, FVRFireArmChamber chamber, Transform muzzle, bool doBuzz, float velMult, float rangeOverride)
		{
			if (self == FireArm)
			{
				if (doBuzz && self.m_hand != null)
				{
					self.m_hand.Buzz(self.m_hand.Buzzer.Buzz_GunShot);
					if (self.AltGrip != null && self.AltGrip.m_hand != null)
					{
						self.AltGrip.m_hand.Buzz(self.m_hand.Buzzer.Buzz_GunShot);
					}
				}
				GM.CurrentSceneSettings.OnShotFired(self);
				if (self.IsSuppressed())
				{
					GM.CurrentPlayerBody.VisibleEvent(0.1f);
				}
				else
				{
					GM.CurrentPlayerBody.VisibleEvent(2f);
				}
				float chamberVelMult = AM.GetChamberVelMult(chamber.RoundType, Vector3.Distance(chamber.transform.position, muzzle.position));
				float num = self.GetCombinedFixedDrop(self.AccuracyClass) * 0.0166667f;
				Vector2 vector = self.GetCombinedFixedDrift(self.AccuracyClass) * 0.0166667f;
				for (int i = 0; i < chamber.GetRound().NumProjectiles; i++)
				{
					float d = chamber.GetRound().ProjectileSpread + self.m_internalMechanicalMOA + self.GetCombinedMuzzleDeviceAccuracy();
					if (chamber.GetRound().BallisticProjectilePrefab != null)
					{
						Vector3 b = muzzle.forward * 0.005f;
						GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(chamber.GetRound().BallisticProjectilePrefab, muzzle.position - b, muzzle.rotation);
						Vector2 vector2 = (UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle + UnityEngine.Random.insideUnitCircle) * 0.33333334f * d;
						gameObject.transform.Rotate(new Vector3(vector2.x + vector.y + num, vector2.y + vector.x, 0f));
						BallisticProjectile component = gameObject.GetComponent<BallisticProjectile>();
						component.Fire(component.MuzzleVelocityBase * chamber.ChamberVelocityMultiplier * velMult * chamberVelMult, gameObject.transform.forward, self, true);

						TopAttackProjectile smartProjectile = gameObject.GetComponent<TopAttackProjectile>();
						if (smartProjectile != null && _targetPoint != new Vector3(float.MaxValue, float.MaxValue, float.MaxValue))
						{
							if (_targetRB == null) smartProjectile.TargetPoint = _targetPoint;
							else  smartProjectile.TargetRB = _targetRB;

							smartProjectile.AttackMode = AttackMode;
						}
						if (rangeOverride > 0f)
						{
							component.ForceSetMaxDist(rangeOverride);
						}
					}
				}
			}
			else orig(self, chamber, muzzle, doBuzz, velMult, rangeOverride);
		}
#endif
	}
}
