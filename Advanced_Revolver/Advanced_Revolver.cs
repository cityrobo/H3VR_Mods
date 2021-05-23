using System;
using System.Collections.Generic;
using UnityEngine;

namespace FistVR
{
	public class Advanced_Revolver : FVRFireArm
	{
		public bool isCylinderArmLocked
		{
			get
			{
				return this.m_isCylinderArmLocked;
			}
		}

		public int CurChamber
		{
			get
			{
				return this.m_curChamber;
			}
			set
			{
				if (value < 0)
				{
					this.m_curChamber = this.Cylinder.numChambers - 1;
				}
				else
				{
					this.m_curChamber = value % this.Cylinder.numChambers;
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
		}

		public override int GetTutorialState()
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < this.Chambers.Length; i++)
			{
				if (this.Chambers[i].IsFull)
				{
					num++;
					if (!this.Chambers[i].IsSpent)
					{
						num2++;
					}
				}
			}
			if (num <= 0)
			{
				if (this.m_isCylinderArmLocked)
				{
					return 0;
				}
				return 1;
			}
			else if (num2 > 0)
			{
				if (this.m_isCylinderArmLocked)
				{
					return 3;
				}
				return 2;
			}
			else
			{
				if (this.m_isCylinderArmLocked)
				{
					return 0;
				}
				return 4;
			}
		}

		protected override void FVRUpdate()
		{
			base.FVRUpdate();
		}

		public override void BeginInteraction(FVRViveHand hand)
		{
			base.BeginInteraction(hand);
			if (!this.IsAltHeld)
			{
				if (this.m_isInMainpose)
				{
					this.PoseOverride.localPosition = this.Pose_Main.localPosition;
					this.PoseOverride.localRotation = this.Pose_Main.localRotation;
					this.m_grabPointTransform.localPosition = this.Pose_Main.localPosition;
					this.m_grabPointTransform.localRotation = this.Pose_Main.localRotation;
				}
				else
				{
					this.PoseOverride.localPosition = this.Pose_Reloading.localPosition;
					this.PoseOverride.localRotation = this.Pose_Reloading.localRotation;
					this.m_grabPointTransform.localPosition = this.Pose_Reloading.localPosition;
					this.m_grabPointTransform.localRotation = this.Pose_Reloading.localRotation;
				}
			}
			base.RootRigidbody.maxAngularVelocity = 40f;
		}

		public override void EndInteraction(FVRViveHand hand)
		{
			base.EndInteraction(hand);
			base.RootRigidbody.AddRelativeTorque(new Vector3(this.xSpinVel, 0f, 0f), ForceMode.Impulse);
		}

		public override void UpdateInteraction(FVRViveHand hand)
		{
			base.UpdateInteraction(hand);
			if (this.IsAltHeld && !this.m_isInMainpose)
			{
				this.m_isInMainpose = true;
				this.PoseOverride.localPosition = this.Pose_Main.localPosition;
				this.PoseOverride.localRotation = this.Pose_Main.localRotation;
				this.m_grabPointTransform.localPosition = this.Pose_Main.localPosition;
				this.m_grabPointTransform.localRotation = this.Pose_Main.localRotation;
			}
			this.TouchPadAxes = hand.Input.TouchpadAxes;
			this.m_isSpinning = false;
			if (!this.IsAltHeld && !hand.IsInStreamlinedMode)
			{
				if (hand.Input.TouchpadPressed && Vector2.Angle(this.TouchPadAxes, Vector2.up) < 45f)
				{
					this.m_isSpinning = true;
				}
				if (hand.Input.TouchpadDown && Vector2.Angle(this.TouchPadAxes, Vector2.right) < 45f && this.UsesAltPoseSwitch)
				{
					this.m_isInMainpose = !this.m_isInMainpose;
					if (this.m_isInMainpose)
					{
						this.PoseOverride.localPosition = this.Pose_Main.localPosition;
						this.PoseOverride.localRotation = this.Pose_Main.localRotation;
						this.m_grabPointTransform.localPosition = this.Pose_Main.localPosition;
						this.m_grabPointTransform.localRotation = this.Pose_Main.localRotation;
					}
					else
					{
						this.PoseOverride.localPosition = this.Pose_Reloading.localPosition;
						this.PoseOverride.localRotation = this.Pose_Reloading.localRotation;
						this.m_grabPointTransform.localPosition = this.Pose_Reloading.localPosition;
						this.m_grabPointTransform.localRotation = this.Pose_Reloading.localRotation;
					}
				}
			}
			this.UpdateTriggerHammer();
			this.UpdateCylinderRelease();
			if (!base.IsHeld || this.IsAltHeld || base.AltGrip != null)
			{
				this.m_isSpinning = false;
			}
		}

		protected override void FVRFixedUpdate()
		{
			this.UpdateSpinning();
			base.FVRFixedUpdate();
		}

		public void EjectChambers()
		{
			bool flag = false;
			for (int i = 0; i < this.Chambers.Length; i++)
			{
				if (this.Chambers[i].IsFull)
				{
					flag = true;
					if (this.AngInvert)
					{
						this.Chambers[i].EjectRound(this.Chambers[i].transform.position + this.Chambers[i].transform.forward * this.Cylinder.CartridgeLength, this.Chambers[i].transform.forward, UnityEngine.Random.onUnitSphere, true);
					}
					else
					{
						this.Chambers[i].EjectRound(this.Chambers[i].transform.position + -this.Chambers[i].transform.forward * this.Cylinder.CartridgeLength, -this.Chambers[i].transform.forward, UnityEngine.Random.onUnitSphere, true);
					}
				}
			}
			if (flag)
			{
				base.PlayAudioEvent(FirearmAudioEventType.MagazineOut, 1f);
			}
		}

		private void UpdateSpinning()
		{
			if (!base.IsHeld || this.IsAltHeld || base.AltGrip != null)
			{
				this.m_isSpinning = false;
			}
			if (this.m_isSpinning)
			{
				Vector3 vector = Vector3.zero;
				if (this.m_hand != null)
				{
					vector = this.m_hand.Input.VelLinearWorld;
				}
				float num = Vector3.Dot(vector.normalized, base.transform.up);
				num = Mathf.Clamp(num, -vector.magnitude, vector.magnitude);
				if (Mathf.Abs(this.xSpinVel) < 90f)
				{
					this.xSpinVel += num * Time.deltaTime * 600f;
				}
				else if (Mathf.Sign(num) == Mathf.Sign(this.xSpinVel))
				{
					this.xSpinVel += num * Time.deltaTime * 600f;
				}
				if (Mathf.Abs(this.xSpinVel) < 90f)
				{
					if (Vector3.Dot(base.transform.up, Vector3.down) >= 0f && Mathf.Sign(this.xSpinVel) == 1f)
					{
						this.xSpinVel += Time.deltaTime * 50f;
					}
					if (Vector3.Dot(base.transform.up, Vector3.down) < 0f && Mathf.Sign(this.xSpinVel) == -1f)
					{
						this.xSpinVel -= Time.deltaTime * 50f;
					}
				}
				this.xSpinVel = Mathf.Clamp(this.xSpinVel, -500f, 500f);
				this.xSpinRot += this.xSpinVel * Time.deltaTime * 5f;
				this.PoseSpinHolder.localEulerAngles = new Vector3(this.xSpinRot, 0f, 0f);
				this.xSpinVel = Mathf.Lerp(this.xSpinVel, 0f, Time.deltaTime * 0.6f);
			}
			else
			{
				this.xSpinRot = 0f;
				this.xSpinVel = 0f;
			}
		}

		private void UpdateTriggerHammer()
		{
			float num = 0f;
			if (this.m_hasTriggeredUpSinceBegin && !this.m_isSpinning && !this.IsAltHeld && this.isCylinderArmLocked)
			{
				num = this.m_hand.Input.TriggerFloat;
			}
			if (this.m_isHammerLocked)
			{
				num += 0.8f;
				this.m_triggerCurrentRot = Mathf.Lerp(this.m_triggerForwardRot, this.m_triggerBackwardRot, num);
			}
			else
			{
				this.m_triggerCurrentRot = Mathf.Lerp(this.m_triggerForwardRot, this.m_triggerBackwardRot, num);
			}
			if (Mathf.Abs(this.m_triggerCurrentRot - this.lastTriggerRot) > 0.01f)
			{
				if (this.Trigger != null)
				{
					this.Trigger.localEulerAngles = new Vector3(this.m_triggerCurrentRot, 0f, 0f);
				}
				for (int i = 0; i < this.TPieces.Count; i++)
				{
					base.SetAnimatedComponent(this.TPieces[i].TPiece, Mathf.Lerp(this.TPieces[i].TRange.x, this.TPieces[i].TRange.y, num), this.TPieces[i].TInterp, this.TPieces[i].TAxis);
				}
			}
			this.lastTriggerRot = this.m_triggerCurrentRot;
			if (this.m_shouldRecock)
			{
				this.m_shouldRecock = false;
				this.m_isHammerLocked = true;
				base.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
			}
			if (!this.m_hasTriggerCycled || !this.IsDoubleActionTrigger)
			{
				if (num >= 0.98f && (this.m_isHammerLocked || this.IsDoubleActionTrigger) && !this.m_hand.Input.TouchpadPressed)
				{
					if (this.m_isCylinderArmLocked)
					{
						this.m_hasTriggerCycled = true;
						this.m_isHammerLocked = false;
						if (this.IsCylinderRotClockwise)
						{
							this.CurChamber++;
						}
						else
						{
							int curChamber = this.CurChamber - 1;
							this.CurChamber = curChamber;
						}
						this.m_curChamberLerp = 0f;
						this.m_tarChamberLerp = 0f;
						base.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);
						if (this.Chambers[this.CurChamber].IsFull && !this.Chambers[this.CurChamber].IsSpent)
						{
							this.Chambers[this.CurChamber].Fire();
							this.Fire();
							if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
							{
								this.Chambers[this.CurChamber].IsSpent = false;
								this.Chambers[this.CurChamber].UpdateProxyDisplay();
							}
							if (this.DoesFiringRecock)
							{
								this.m_shouldRecock = true;
							}
						}
					}
					else
					{
						this.m_hasTriggerCycled = true;
						this.m_isHammerLocked = false;
					}
				}
				else if ((num <= 0.08f || !this.IsDoubleActionTrigger) && !this.m_isHammerLocked && this.CanManuallyCockHammer && !this.IsAltHeld)
				{
					if (this.m_hand.IsInStreamlinedMode)
					{
						if (this.m_hand.Input.AXButtonDown)
						{
							this.m_isHammerLocked = true;
							base.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
						}
					}
					else if (this.m_hand.Input.TouchpadDown && Vector2.Angle(this.TouchPadAxes, Vector2.down) < 45f)
					{
						this.m_isHammerLocked = true;
						base.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
					}
				}
			}
			else if (this.m_hasTriggerCycled && this.m_hand.Input.TriggerFloat <= 0.08f)
			{
				this.m_hasTriggerCycled = false;
				base.PlayAudioEvent(FirearmAudioEventType.TriggerReset, 1f);
			}
			if (!this.isChiappaHammer)
			{
				if (this.m_hasTriggerCycled || !this.IsDoubleActionTrigger)
				{
					if (this.m_isHammerLocked)
					{
						this.m_hammerCurrentRot = Mathf.Lerp(this.m_hammerCurrentRot, this.m_hammerBackwardRot, Time.deltaTime * 10f);
					}
					else
					{
						this.m_hammerCurrentRot = Mathf.Lerp(this.m_hammerCurrentRot, this.m_hammerForwardRot, Time.deltaTime * 30f);
					}
				}
				else if (this.m_isHammerLocked)
				{
					this.m_hammerCurrentRot = Mathf.Lerp(this.m_hammerCurrentRot, this.m_hammerBackwardRot, Time.deltaTime * 10f);
				}
				else
				{
					this.m_hammerCurrentRot = Mathf.Lerp(this.m_hammerForwardRot, this.m_hammerBackwardRot, num);
				}
			}
			if (this.isChiappaHammer)
			{
				bool flag = false;
				if (this.m_hand.IsInStreamlinedMode && this.m_hand.Input.AXButtonPressed)
				{
					flag = true;
				}
				else if (Vector2.Angle(this.m_hand.Input.TouchpadAxes, Vector2.down) < 45f && this.m_hand.Input.TouchpadPressed)
				{
					flag = true;
				}
				if (num <= 0.02f && !this.IsAltHeld && flag)
				{
					this.m_hammerCurrentRot = Mathf.Lerp(this.m_hammerCurrentRot, this.m_hammerBackwardRot, Time.deltaTime * 15f);
				}
				else
				{
					this.m_hammerCurrentRot = Mathf.Lerp(this.m_hammerCurrentRot, this.m_hammerForwardRot, Time.deltaTime * 6f);
				}
			}
			if (this.Hammer != null)
			{
				this.Hammer.localEulerAngles = new Vector3(this.m_hammerCurrentRot, 0f, 0f);
			}
		}

		private void Fire()
		{
			FVRFireArmChamber fvrfireArmChamber = this.Chambers[this.CurChamber];
			base.Fire(fvrfireArmChamber, this.GetMuzzle(), true, 1f);
			this.FireMuzzleSmoke();
			if (fvrfireArmChamber.GetRound().IsHighPressure)
			{
				bool twoHandStabilized = this.IsTwoHandStabilized();
				bool foregripStabilized = base.AltGrip != null;
				bool shoulderStabilized = this.IsShoulderStabilized();
				this.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
			}
			base.PlayAudioGunShot(fvrfireArmChamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), 1f);
			if (fvrfireArmChamber.GetRound().IsCaseless)
			{
				fvrfireArmChamber.SetRound(null);
			}
		}

		public void AddCylinderCloseVel(float f)
		{
			this.m_CylCloseVel = f;
		}

		private void UpdateCylinderRelease()
		{
			float num = 0f;
			if (this.m_hasTriggeredUpSinceBegin && !this.m_isSpinning && !this.IsAltHeld && this.isCylinderArmLocked)
			{
				num = this.m_hand.Input.TriggerFloat;
			}
			this.m_isCylinderReleasePressed = false;
			if (!this.IsAltHeld && (!this.m_isHammerLocked || this.DoesFiringRecock))
			{
				if (this.m_hand.IsInStreamlinedMode)
				{
					if (this.m_hand.Input.BYButtonPressed)
					{
						this.m_isCylinderReleasePressed = true;
					}
				}
				else if (this.m_hand.Input.TouchpadPressed && Vector2.Angle(this.TouchPadAxes, Vector2.left) < 45f)
				{
					this.m_isCylinderReleasePressed = true;
				}
			}
			if (this.CylinderReleaseButton != null)
			{
				if (this.isCyclinderReleaseARot)
				{
					if (!this.m_isCylinderReleasePressed)
					{
						this.m_curCyclinderReleaseRot = Mathf.Lerp(this.m_curCyclinderReleaseRot, this.CylinderReleaseButtonForwardPos.x, Time.deltaTime * 3f);
					}
					else
					{
						this.m_curCyclinderReleaseRot = Mathf.Lerp(this.m_curCyclinderReleaseRot, this.CylinderReleaseButtonRearPos.x, Time.deltaTime * 3f);
					}
					this.CylinderReleaseButton.localEulerAngles = new Vector3(this.m_curCyclinderReleaseRot, 0f, 0f);
				}
				else if (this.m_isCylinderReleasePressed)
				{
					this.CylinderReleaseButton.localPosition = Vector3.Lerp(this.CylinderReleaseButton.localPosition, this.CylinderReleaseButtonForwardPos, Time.deltaTime * 3f);
				}
				else
				{
					this.CylinderReleaseButton.localPosition = Vector3.Lerp(this.CylinderReleaseButton.localPosition, this.CylinderReleaseButtonRearPos, Time.deltaTime * 3f);
				}
			}
			if (this.m_isCylinderReleasePressed)
			{
				this.m_isCylinderArmLocked = false;
			}
			else
			{
				float f = this.CylinderArm.localEulerAngles.z;
				if (this.IsCylinderArmZ)
				{
					f = this.CylinderArm.localEulerAngles.x;
				}
				if (Mathf.Abs(f) <= 1f && !this.m_isCylinderArmLocked)
				{
					this.m_isCylinderArmLocked = true;
					this.CylinderArm.localEulerAngles = Vector3.zero;
				}
			}
			float num2 = 160f;
			if (!this.GravityRotsCylinderPositive)
			{
				num2 *= -1f;
			}
			if (!this.m_isCylinderArmLocked)
			{
				float num3 = base.transform.InverseTransformDirection(this.m_hand.Input.VelAngularWorld).z;
				float num4 = base.transform.InverseTransformDirection(this.m_hand.Input.VelLinearWorld).x;
				if (this.IsCylinderArmZ)
				{
					num3 = base.transform.InverseTransformDirection(this.m_hand.Input.VelAngularWorld).x;
					num4 = base.transform.InverseTransformDirection(this.m_hand.Input.VelLinearWorld).y;
				}
				if (this.AngInvert)
				{
					num3 = -num3;
					num4 = -num4;
				}
				num2 += num3 * 70f;
				num2 += num4 * -350f;
				num2 += this.m_CylCloseVel;
				this.m_CylCloseVel = 0f;
				float num5 = this.CylinderArmRot + num2 * Time.deltaTime;
				num5 = Mathf.Clamp(num5, this.CylinderRotRange.x, this.CylinderRotRange.y);
				if (num5 != this.CylinderArmRot)
				{
					this.CylinderArmRot = num5;
					if (this.IsCylinderArmZ)
					{
						this.CylinderArm.localEulerAngles = new Vector3(num5, 0f, 0f);
					}
					else
					{
						this.CylinderArm.localEulerAngles = new Vector3(0f, 0f, num5);
					}
				}
			}
			float f2 = this.CylinderArm.localEulerAngles.z;
			if (this.IsCylinderArmZ)
			{
				f2 = this.CylinderArm.localEulerAngles.x;
			}
			if (Mathf.Abs(f2) > 30f)
			{
				for (int i = 0; i < this.Chambers.Length; i++)
				{
					this.Chambers[i].IsAccessible = true;
				}
			}
			else
			{
				for (int j = 0; j < this.Chambers.Length; j++)
				{
					this.Chambers[j].IsAccessible = false;
				}
			}
			if (Mathf.Abs(f2) < 1f && this.IsCylinderArmZ)
			{
				this.m_hasEjectedSinceOpening = false;
			}
			if (Mathf.Abs(f2) > 45f && this.IsCylinderArmZ && !this.m_hasEjectedSinceOpening)
			{
				this.m_hasEjectedSinceOpening = true;
				this.EjectChambers();
			}
			if (!this.IsCylinderArmZ && Mathf.Abs(this.CylinderArm.localEulerAngles.z) > 75f && Vector3.Angle(base.transform.forward, Vector3.up) <= 120f)
			{
				float num6 = base.transform.InverseTransformDirection(this.m_hand.Input.VelLinearWorld).z;
				if (this.AngInvert)
				{
					num6 = -num6;
				}
				if (num6 < -2f)
				{
					this.EjectChambers();
				}
			}
			if (this.m_isCylinderArmLocked && !this.m_wasCylinderArmLocked)
			{
				this.m_curChamber = this.Cylinder.GetClosestChamberIndex();
				this.Cylinder.transform.localRotation = this.Cylinder.GetLocalRotationFromCylinder(this.m_curChamber);
				this.m_curChamberLerp = 0f;
				this.m_tarChamberLerp = 0f;
				base.PlayAudioEvent(FirearmAudioEventType.BreachClose, 1f);
			}
			if (!this.m_isCylinderArmLocked && this.m_wasCylinderArmLocked)
			{
				base.PlayAudioEvent(FirearmAudioEventType.BreachOpen, 1f);
			}
			if (this.m_isHammerLocked)
			{
				this.m_tarChamberLerp = 1f;
			}
			else if (!this.m_hasTriggerCycled && this.IsDoubleActionTrigger)
			{
				this.m_tarChamberLerp = num * 1.4f;
			}
			this.m_curChamberLerp = Mathf.Lerp(this.m_curChamberLerp, this.m_tarChamberLerp, Time.deltaTime * 16f);
			int cylinder;
			if (this.IsCylinderRotClockwise)
			{
				cylinder = (this.CurChamber + 1) % this.Cylinder.numChambers;
			}
			else
			{
				cylinder = (this.CurChamber - 1) % this.Cylinder.numChambers;
			}
			if (this.isCylinderArmLocked)
			{
				this.Cylinder.transform.localRotation = Quaternion.Slerp(this.Cylinder.GetLocalRotationFromCylinder(this.CurChamber), this.Cylinder.GetLocalRotationFromCylinder(cylinder), this.m_curChamberLerp);
			}
			this.m_wasCylinderArmLocked = this.m_isCylinderArmLocked;
		}

		public override List<FireArmRoundClass> GetChamberRoundList()
		{
			bool flag = false;
			List<FireArmRoundClass> list = new List<FireArmRoundClass>();
			for (int i = 0; i < this.Chambers.Length; i++)
			{
				if (this.Chambers[i].IsFull)
				{
					list.Add(this.Chambers[i].GetRound().RoundClass);
					flag = true;
				}
			}
			if (flag)
			{
				return list;
			}
			return null;
		}

		public override void SetLoadedChambers(List<FireArmRoundClass> rounds)
		{
			if (rounds.Count > 0)
			{
				for (int i = 0; i < this.Chambers.Length; i++)
				{
					if (i < rounds.Count)
					{
						this.Chambers[i].Autochamber(rounds[i]);
					}
				}
			}
		}

		public override List<string> GetFlagList()
		{
			return null;
		}

		public override void SetFromFlagList(List<string> flags)
		{
		}

		[Header("Advanced Revolver Config")]
		public bool AllowsSuppressor;

		public bool isChiappa;

		public bool isChiappaHammer;

		public Transform Hammer;

		public bool CanManuallyCockHammer = true;

		public bool IsDoubleActionTrigger = true;

		private float m_hammerForwardRot;

		public float m_hammerBackwardRot = -49f;

		private float m_hammerCurrentRot;

		public Transform Trigger;

		private float m_triggerForwardRot;

		public float m_triggerBackwardRot = 30f;

		private float m_triggerCurrentRot;

		private bool m_isHammerLocked;

		private bool m_hasTriggerCycled;

		public bool DoesFiringRecock;

		public Transform CylinderReleaseButton;

		public bool isCyclinderReleaseARot;

		public Vector3 CylinderReleaseButtonForwardPos;

		public Vector3 CylinderReleaseButtonRearPos;

		private bool m_isCylinderReleasePressed;

		private float m_curCyclinderReleaseRot;

		[Header("Cylinder Config")]
		public bool UsesCylinderArm = true;

		public Transform CylinderArm;

		private bool m_isCylinderArmLocked = true;

		private bool m_wasCylinderArmLocked = true;

		private float CylinderArmRot;

		public bool IsCylinderRotClockwise = true;

		public Vector2 CylinderRotRange = new Vector2(0f, 105f);

		public bool IsCylinderArmZ;

		public bool AngInvert;

		public bool GravityRotsCylinderPositive = true;

		public RevolverCylinder Cylinder;

		private int m_curChamber;

		private float m_tarChamberLerp;

		private float m_curChamberLerp;

		[Header("Chambers Config")]
		public FVRFireArmChamber[] Chambers;

		[Header("Spinning Config")]
		public Transform PoseSpinHolder;

		public bool CanSpin = true;

		private bool m_isSpinning;

		public Transform Muzzle;

		public bool UsesAltPoseSwitch = true;

		public Transform Pose_Main;

		public Transform Pose_Reloading;

		private bool m_isInMainpose = true;

		private Vector2 TouchPadAxes = Vector2.zero;

		private bool m_hasEjectedSinceOpening;

		public List<Revolver.TriggerPiece> TPieces;

		private float xSpinRot;

		private float xSpinVel;

		private float lastTriggerRot;

		private bool m_shouldRecock;

		private float m_CylCloseVel;

		[Serializable]
		public class TriggerPiece
		{
			public Transform TPiece;

			public FVRPhysicalObject.Axis TAxis;

			public Vector2 TRange;

			public FVRPhysicalObject.InterpStyle TInterp;
		}
	}
}
