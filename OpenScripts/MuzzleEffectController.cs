using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
    public class MuzzleEffectController : MonoBehaviour
    {
        public FVRFireArm FireArm;
		public MuzzleEffect[] MuzzleEffects;
		public bool SupressorChangesSize = false;
		public MuzzleEffectSize[] SuppressedMuzzleEffectSizes;
#if !(DEBUG)
		private List<MuzzlePSystem> m_muzzleSystems;
		private bool _wasSuppressed = false;


		public void Awake()
        {
			m_muzzleSystems = new List<MuzzlePSystem>();
			if (SupressorChangesSize && MuzzleEffects.Length != SuppressedMuzzleEffectSizes.Length)
			{
				Debug.LogError($"{gameObject.name} MuzzleEffectController: MuzzleEffects list length ({MuzzleEffects.Length}) not equal to SuppressedMuzzleEffectSizes list length ({SuppressedMuzzleEffectSizes.Length})! ABORTING!!!");
				Destroy(this);
			}

			GM.CurrentSceneSettings.ShotFiredEvent += OnFire;
			RegenerateMuzzleEffects(false);
        }

		public void OnDestroy()
        {
			GM.CurrentSceneSettings.ShotFiredEvent -= OnFire;
		}

		public void Update()
        {
			if (SupressorChangesSize)
            {
				if (!_wasSuppressed && FireArm.IsSuppressed())
                {
					RegenerateMuzzleEffects(true);
					_wasSuppressed = true;
				}
                else if (_wasSuppressed && !FireArm.IsSuppressed())
                {
					RegenerateMuzzleEffects(false);
					_wasSuppressed = false;
				}
            }
        }

		private void FireMuzzleSmoke()
		{
			for (int i = 0; i < m_muzzleSystems.Count; i++)
			{
				if (m_muzzleSystems[i].OverridePoint == null)
				{
					m_muzzleSystems[i].PSystem.transform.position = FireArm.GetMuzzle().position;
				}
				m_muzzleSystems[i].PSystem.Emit(m_muzzleSystems[i].NumParticlesPerShot);
			}
		}

		private void RegenerateMuzzleEffects(bool Suppressed)
		{
			for (int i = 0; i < m_muzzleSystems.Count; i++)
			{
				Destroy(m_muzzleSystems[i].PSystem);
			}
			m_muzzleSystems.Clear();
			MuzzleEffect[] muzzleEffects = MuzzleEffects;
			for (int j = 0; j < muzzleEffects.Length; j++)
			{
				if (muzzleEffects[j].Entry != MuzzleEffectEntry.None)
				{
					MuzzleEffectConfig muzzleConfig = FXM.GetMuzzleConfig(muzzleEffects[j].Entry);
					MuzzleEffectSize muzzleEffectSize;
					if (Suppressed)
                    {
						muzzleEffectSize = SuppressedMuzzleEffectSizes[j];
					}
                    else
                    {
						muzzleEffectSize = muzzleEffects[j].Size;
					}
					GameObject newMuzzleEffect;
					if (GM.CurrentSceneSettings.IsSceneLowLight)
					{
						newMuzzleEffect = Instantiate(muzzleConfig.Prefabs_Lowlight[(int)muzzleEffectSize], FireArm.MuzzlePos.position, FireArm.MuzzlePos.rotation);
					}
					else
					{
						newMuzzleEffect = Instantiate(muzzleConfig.Prefabs_Highlight[(int)muzzleEffectSize], FireArm.MuzzlePos.position, FireArm.MuzzlePos.rotation);
					}
					if (muzzleEffects[j].OverridePoint == null)
					{
						newMuzzleEffect.transform.SetParent(FireArm.MuzzlePos.transform);
					}
					else
					{
						newMuzzleEffect.transform.SetParent(muzzleEffects[j].OverridePoint,false);
						newMuzzleEffect.transform.localPosition = Vector3.zero;
						newMuzzleEffect.transform.localRotation = Quaternion.identity;
					}
					MuzzlePSystem muzzlePSystem = new MuzzlePSystem();
					muzzlePSystem.PSystem = newMuzzleEffect.GetComponent<ParticleSystem>();
					muzzlePSystem.OverridePoint = muzzleEffects[j].OverridePoint;
					ParticleSystem.MainModule main = muzzlePSystem.PSystem.main;
					main.scalingMode = ParticleSystemScalingMode.Hierarchy;

					int index = (int)muzzleEffectSize;
					if (GM.CurrentSceneSettings.IsSceneLowLight)
					{
						muzzlePSystem.NumParticlesPerShot = muzzleConfig.NumParticles_Lowlight[index];
					}
					else
					{
						muzzlePSystem.NumParticlesPerShot = muzzleConfig.NumParticles_Highlight[index];
					}
					m_muzzleSystems.Add(muzzlePSystem);
				}
			}
		}

		public void OnFire(FVRFireArm firearm)
        {
			if (firearm == FireArm)
            {
				FireMuzzleSmoke();
			}
        }
#endif
	}
}
