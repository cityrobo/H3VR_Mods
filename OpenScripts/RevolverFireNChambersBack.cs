using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using FistVR;

namespace Cityrobo
{
	public class RevolverFireNChambersBack : MonoBehaviour
	{
		public Revolver revolver;
		public int NChambersBack = 3;
		public Transform SecondMuzzle;
		public float FiringDelay = 0.1f;

		private List<MuzzlePSystem> m_muzzleSystems = new List<MuzzlePSystem>();

#if !(DEBUG || MEATKIT)

		public int PrevChamberN(int n)
		{
			int num = revolver.CurChamber - n;
			if (num < 0)
			{
				return revolver.Cylinder.numChambers + num;
			}
			return num;
		}

		public void Awake()
		{
			Hook();

			RegenerateSecondaryMuzzleEffects();
		}
		public void OnDestroy()
		{
			Unhook();
		}

		void Unhook()
		{
			On.FistVR.Revolver.UpdateTriggerHammer -= Revolver_UpdateTriggerHammer;
			On.FistVR.FVRFireArm.RegenerateMuzzleEffects -= FVRFireArm_RegenerateMuzzleEffects;
		}

		void Hook()
		{
			On.FistVR.Revolver.UpdateTriggerHammer += Revolver_UpdateTriggerHammer;
			On.FistVR.FVRFireArm.RegenerateMuzzleEffects += FVRFireArm_RegenerateMuzzleEffects;
		}

		private void FVRFireArm_RegenerateMuzzleEffects(On.FistVR.FVRFireArm.orig_RegenerateMuzzleEffects orig, FVRFireArm self, MuzzleDevice m)
		{
			orig(self, m);
			if (self == revolver) RegenerateSecondaryMuzzleEffects();
		}

		private void Revolver_UpdateTriggerHammer(On.FistVR.Revolver.orig_UpdateTriggerHammer orig, Revolver self)
		{
			if (self == revolver)
			{
				if (self.m_hasTriggeredUpSinceBegin && !self.m_isSpinning && !self.IsAltHeld && self.isCylinderArmLocked)
				{
					self.m_tarTriggerFloat = self.m_hand.Input.TriggerFloat;
					self.m_tarRealTriggerFloat = self.m_hand.Input.TriggerFloat;
				}
				else
				{
					self.m_tarTriggerFloat = 0f;
					self.m_tarRealTriggerFloat = 0f;
				}
				if (self.m_isHammerLocked)
				{
					self.m_tarTriggerFloat += 0.8f;
					self.m_triggerCurrentRot = Mathf.Lerp(self.m_triggerForwardRot, self.m_triggerBackwardRot, self.m_curTriggerFloat);
				}
				else
				{
					self.m_triggerCurrentRot = Mathf.Lerp(self.m_triggerForwardRot, self.m_triggerBackwardRot, self.m_curTriggerFloat);
				}
				self.m_curTriggerFloat = Mathf.MoveTowards(self.m_curTriggerFloat, self.m_tarTriggerFloat, Time.deltaTime * 14f);
				self.m_curRealTriggerFloat = Mathf.MoveTowards(self.m_curRealTriggerFloat, self.m_tarRealTriggerFloat, Time.deltaTime * 14f);
				if (Mathf.Abs(self.m_triggerCurrentRot - self.lastTriggerRot) > 0.01f)
				{
					if (self.Trigger != null)
					{
						self.Trigger.localEulerAngles = new Vector3(self.m_triggerCurrentRot, 0f, 0f);
					}
					for (int i = 0; i < self.TPieces.Count; i++)
					{
						self.SetAnimatedComponent(self.TPieces[i].TPiece, Mathf.Lerp(self.TPieces[i].TRange.x, self.TPieces[i].TRange.y, self.m_curTriggerFloat), self.TPieces[i].TInterp, self.TPieces[i].TAxis);
					}
				}
				self.lastTriggerRot = self.m_triggerCurrentRot;
				if (self.m_shouldRecock)
				{
					self.m_shouldRecock = false;
					self.InitiateRecock();
				}
				if (!self.m_hasTriggerCycled || (!self.IsDoubleActionTrigger && !self.DoesFiringRecock))
				{
					bool flag = false;
					if (self.DoesFiringRecock && self.m_recockingState != Revolver.RecockingState.Forward)
					{
						flag = true;
					}
					if (!flag && self.m_curTriggerFloat >= 0.98f && (self.m_isHammerLocked || self.IsDoubleActionTrigger) && !self.m_hand.Input.TouchpadPressed)
					{
						if (self.m_isCylinderArmLocked)
						{
							self.m_hasTriggerCycled = true;
							self.m_isHammerLocked = false;
							if (self.IsCylinderRotClockwise)
							{
								self.CurChamber++;
							}
							else
							{
								int curChamber = self.CurChamber - 1;
								self.CurChamber = curChamber;
							}
							self.m_curChamberLerp = 0f;
							self.m_tarChamberLerp = 0f;
							self.PlayAudioEvent(FirearmAudioEventType.HammerHit, 1f);

							bool PrimaryChamberFire = self.Chambers[self.CurChamber].Fire();
							bool SecondaryChamberFire = self.Chambers[PrevChamberN(NChambersBack)].Fire();

							if (PrimaryChamberFire && !SecondaryChamberFire)
							{
								self.Fire();
								if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
								{
									self.Chambers[self.CurChamber].IsSpent = false;
									self.Chambers[self.CurChamber].UpdateProxyDisplay();
								}
								if (self.DoesFiringRecock)
								{
									self.m_shouldRecock = true;
								}
							}
							else if (!PrimaryChamberFire && SecondaryChamberFire)
							{
								FireSecondaryChamber();
								if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
								{
									self.Chambers[PrevChamberN(NChambersBack)].IsSpent = false;
									self.Chambers[PrevChamberN(NChambersBack)].UpdateProxyDisplay();
								}
								if (self.DoesFiringRecock)
								{
									self.m_shouldRecock = true;
								}
							}
							else if (PrimaryChamberFire && SecondaryChamberFire)
							{
								self.Fire();
								StartCoroutine(DelayedFiring());
								if (GM.CurrentSceneSettings.IsAmmoInfinite || GM.CurrentPlayerBody.IsInfiniteAmmo)
								{
									self.Chambers[self.CurChamber].IsSpent = false;
									self.Chambers[self.CurChamber].UpdateProxyDisplay();
									self.Chambers[PrevChamberN(NChambersBack)].IsSpent = false;
									self.Chambers[PrevChamberN(NChambersBack)].UpdateProxyDisplay();
								}
								if (self.DoesFiringRecock)
								{
									self.m_shouldRecock = true;
								}
							}

						}
					}
					else if ((self.m_curTriggerFloat <= 0.08f || !self.IsDoubleActionTrigger) && !self.m_isHammerLocked && self.CanManuallyCockHammer)
					{
						bool flag2 = false;
						if (self.DoesFiringRecock && self.m_recockingState != Revolver.RecockingState.Forward)
						{
							flag2 = true;
						}
						if (!self.IsAltHeld && !flag2)
						{
							if (self.m_hand.IsInStreamlinedMode)
							{
								if (self.m_hand.Input.AXButtonDown)
								{
									self.m_isHammerLocked = true;
									self.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
								}
							}
							else if (self.m_hand.Input.TouchpadDown && Vector2.Angle(self.TouchPadAxes, Vector2.down) < 45f)
							{
								self.m_isHammerLocked = true;
								self.PlayAudioEvent(FirearmAudioEventType.Prefire, 1f);
							}
						}
					}
				}
				else if (self.m_hasTriggerCycled && self.m_curRealTriggerFloat <= 0.08f)
				{
					self.m_hasTriggerCycled = false;
					self.PlayAudioEvent(FirearmAudioEventType.TriggerReset, 1f);
				}
				if (!self.isChiappaHammer)
				{
					if (self.m_hasTriggerCycled || !self.IsDoubleActionTrigger)
					{
						if (self.m_isHammerLocked)
						{
							self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerBackwardRot, Time.deltaTime * 10f);
						}
						else
						{
							self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerForwardRot, Time.deltaTime * 30f);
						}
					}
					else if (self.m_isHammerLocked)
					{
						self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerBackwardRot, Time.deltaTime * 10f);
					}
					else
					{
						self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerForwardRot, self.m_hammerBackwardRot, self.m_curTriggerFloat);
					}
				}
				if (self.isChiappaHammer)
				{
					bool flag3 = false;
					if (self.m_hand.IsInStreamlinedMode && self.m_hand.Input.AXButtonPressed)
					{
						flag3 = true;
					}
					else if (Vector2.Angle(self.m_hand.Input.TouchpadAxes, Vector2.down) < 45f && self.m_hand.Input.TouchpadPressed)
					{
						flag3 = true;
					}
					if (self.m_curTriggerFloat <= 0.02f && !self.IsAltHeld && flag3)
					{
						self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerBackwardRot, Time.deltaTime * 15f);
					}
					else
					{
						self.m_hammerCurrentRot = Mathf.Lerp(self.m_hammerCurrentRot, self.m_hammerForwardRot, Time.deltaTime * 6f);
					}
				}
				if (self.Hammer != null)
				{
					self.Hammer.localEulerAngles = new Vector3(self.m_hammerCurrentRot, 0f, 0f);
				}
			}
			else
			{
				orig(self);
			}
		}

		private void FireSecondaryChamber()
		{
			FVRFireArmChamber fvrfireArmChamber = revolver.Chambers[PrevChamberN(NChambersBack)];
			revolver.Fire(fvrfireArmChamber, SecondMuzzle, true, 1f, -1f);
			FireSecondMuzzleSmoke();
			if (fvrfireArmChamber.GetRound().IsHighPressure)
			{
				bool twoHandStabilized = revolver.IsTwoHandStabilized();
				bool foregripStabilized = revolver.AltGrip != null;
				bool shoulderStabilized = revolver.IsShoulderStabilized();
				revolver.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
			}
			revolver.PlayAudioGunShot(fvrfireArmChamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), revolver.ShotLoudnessMult);

			if (fvrfireArmChamber.GetRound().IsCaseless)
			{
				fvrfireArmChamber.SetRound(null, false);
			}
		}

		IEnumerator DelayedFiring()
		{
			yield return new WaitForSeconds(FiringDelay);

			FVRFireArmChamber fvrfireArmChamber = revolver.Chambers[PrevChamberN(NChambersBack)];
			revolver.Fire(fvrfireArmChamber, SecondMuzzle, true, 1f, -1f);
			FireSecondMuzzleSmoke();
			if (fvrfireArmChamber.GetRound().IsHighPressure)
			{
				bool twoHandStabilized = revolver.IsTwoHandStabilized();
				bool foregripStabilized = revolver.AltGrip != null;
				bool shoulderStabilized = revolver.IsShoulderStabilized();
				revolver.Recoil(twoHandStabilized, foregripStabilized, shoulderStabilized, null, 1f);
			}
			revolver.PlayAudioGunShot(fvrfireArmChamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment(), revolver.ShotLoudnessMult);

			if (fvrfireArmChamber.GetRound().IsCaseless)
			{
				fvrfireArmChamber.SetRound(null, false);
			}
		}

		private void FireSecondMuzzleSmoke()
		{
			if (GM.CurrentSceneSettings.IsSceneLowLight)
			{
				if (revolver.IsSuppressed())
				{
					FXM.InitiateMuzzleFlash(SecondMuzzle.position, SecondMuzzle.forward, 0.25f, new Color(1f, 0.9f, 0.77f), 0.5f);
				}
				else
				{
					FXM.InitiateMuzzleFlash(SecondMuzzle.position, SecondMuzzle.forward, 1f, new Color(1f, 0.9f, 0.77f), 1f);
				}
			}
			for (int i = 0; i < revolver.m_muzzleSystems.Count; i++)
			{
				if (this.m_muzzleSystems[i].OverridePoint == null)
				{
					this.m_muzzleSystems[i].PSystem.transform.position = SecondMuzzle.position;
				}
				this.m_muzzleSystems[i].PSystem.Emit(this.m_muzzleSystems[i].NumParticlesPerShot);
			}
		}

		private void RegenerateSecondaryMuzzleEffects()
		{
			for (int i = 0; i < this.m_muzzleSystems.Count; i++)
			{
				Destroy(this.m_muzzleSystems[i].PSystem);
			}
			this.m_muzzleSystems.Clear();
			bool flag = false;
			MuzzleEffect[] muzzleEffects = revolver.MuzzleEffects;
			for (int j = 0; j < muzzleEffects.Length; j++)
			{
				if (muzzleEffects[j].Entry != MuzzleEffectEntry.None)
				{
					MuzzleEffectConfig muzzleConfig = FXM.GetMuzzleConfig(muzzleEffects[j].Entry);
					MuzzleEffectSize muzzleEffectSize = muzzleEffects[j].Size;
					if (flag)
					{
						muzzleEffectSize = revolver.DefaultMuzzleEffectSize;
					}
					GameObject gameObject;
					if (GM.CurrentSceneSettings.IsSceneLowLight)
					{
						gameObject = Instantiate<GameObject>(muzzleConfig.Prefabs_Lowlight[(int)muzzleEffectSize], SecondMuzzle.position, SecondMuzzle.rotation);
					}
					else
					{
						gameObject = Instantiate<GameObject>(muzzleConfig.Prefabs_Highlight[(int)muzzleEffectSize], SecondMuzzle.position, SecondMuzzle.rotation);
					}
					if (muzzleEffects[j].OverridePoint == null)
					{
						gameObject.transform.SetParent(SecondMuzzle.transform);
					}
					else
					{
						gameObject.transform.SetParent(muzzleEffects[j].OverridePoint);
						gameObject.transform.localPosition = Vector3.zero;
						gameObject.transform.localEulerAngles = Vector3.zero;
					}
					MuzzlePSystem muzzlePSystem = new MuzzlePSystem();
					muzzlePSystem.PSystem = gameObject.GetComponent<ParticleSystem>();
					muzzlePSystem.OverridePoint = muzzleEffects[j].OverridePoint;
					int index = (int)muzzleEffectSize;
					if (GM.CurrentSceneSettings.IsSceneLowLight)
					{
						muzzlePSystem.NumParticlesPerShot = muzzleConfig.NumParticles_Lowlight[index];
					}
					else
					{
						muzzlePSystem.NumParticlesPerShot = muzzleConfig.NumParticles_Highlight[index];
					}
					this.m_muzzleSystems.Add(muzzlePSystem);
				}
			}
		}
#endif
	}
}
