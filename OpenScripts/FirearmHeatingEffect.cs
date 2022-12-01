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
    public class FirearmHeatingEffect : MonoBehaviour
    {
        // Public Variables
        [Header("Firearm Heating Effect")]
        [Tooltip("Use this if you plan to use the script on a firearm.")]
        public FVRFireArm FireArm;
        [Tooltip("Use this if you plan to use the script on an attachment.")]
        public FVRFireArmAttachment Attachment;

        // Heat System
        [Header("Heat Config")]
        [Tooltip("Heat added to the effect per shot fired. Heat caps out at 1.")]
        public float HeatPerShot = 0.01f;
        [Tooltip("Heat removed per second.")]
        public float CooldownRate = 0.01f;
        [Tooltip("This multiplier will affect the heat gain of all parts of the gun. It will act multiplicatively with other such multipliers.")]
        public float HeatMultiplier = 1f;
        [Tooltip("Adds a separate Multiplier for Round power. (None, Tiny, Pistol, Shotgun, Intermediate, FullPower, AntiMaterial, Ordnance, Exotic, Fire)")]
        public bool ChangesWithCartridgePower = false;
        [Tooltip("None, Tiny, Pistol, Shotgun, Intermediate, FullPower, AntiMaterial, Ordnance, Exotic, Fire")]
        public float[] RoundPowerMultipliers =
        {
            0f,
            0.25f,
            0.5f,
            0.75f,
            1f,
            2f,
            4f,
            4f,
            1f,
            8f
        };
        

        // Emission weight system
        [Header("Emission modification config")]
        public MeshRenderer MeshRenderer;
        [Tooltip("Index of the material in the MeshRenderer's materials list.")]
        public int MaterialIndex = 0;
        [Tooltip("Anton uses the squared value of the heat to determine the emission weight. If you wanna replicate that behavior, leave the value as is, but feel free to go crazy if you wanna mix things up.")]
        public float HeatExponent = 2f;
        public bool HeatAffectsEmissionWeight = true;
        public bool HeatAffectsEmissionScrollSpeed = false;
        [Tooltip("Maximum left/right emission scroll speed.")]
        public float MaxEmissionScrollSpeed_X = 0f;
        [Tooltip("Maximum up/down emission scroll speed.")]
        public float MaxEmissionScrollSpeed_Y = 0f;
        [Tooltip("Enables emission weight AnimationCurve system.")]
        public bool EmissionUsesAdvancedCurve = false;
        [Tooltip("Advanced emission weight control. Values from 0 to 1 only!")]
        public AnimationCurve EmissionCurve;
        [Tooltip("Advanced left/right emission scroll speed control. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-Axis) represents the actual scroll speed and is uncapped. The max value set above is ignored.")]
        public AnimationCurve EmissionScrollSpeedCurve_X;
        [Tooltip("Advanced up/down emission scroll speed control. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-Axis) represents the actual scroll speed and is uncapped. The max value set above is ignored.")]
        public AnimationCurve EmissionScrollSpeedCurve_Y;

        // Detail weight system
        [Header("Detail weight config")]
        public bool HeatAffectsDetailWeight = true;
        [Tooltip("Same as the normal HeatExponent, but for the detail weight.")]
        public float DetailExponent = 2f;
        [Tooltip("Enables emission weight AnimationCurve system.")]
        public bool DetailUsesAdvancedCurve = false;
        [Tooltip("Advanced emission weight control. Values from 0 to 1 only!")]
        public AnimationCurve DetailCurve;

        // Particle emission rate system
        [Header("Particle effects config")]
        public ParticleSystem ParticleSystem;
        public float MaxEmissionRate;
        [Tooltip("Same as the normal HeatExponent, but for the particle emission rate.")]
        public float ParticleHeatExponent = 2f;
        [Tooltip("Heat level at which particles start appearing.")]
        public float ParticleHeatThreshold = 0f;
        [Tooltip("If checked, the particle emission rate starts at 0 when hitting the threshold and hits the max when heat is 1, else it starts emitting using the threshold heat value as a reference, aka the heat level it gets enabled at.")]
        public bool ParticleEmissionRateStartsAtZero = false;
        [Tooltip("Enables particle rate AnimationCurve system.")]
        public bool ParticlesUsesAdvancedCurve = false;
        [Tooltip("Advanced particle rate control. Values from 0 to 1 only! The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts like a multiplier of the max emission rate, clamped between 0 and 1.")]
        public AnimationCurve ParticlesCurve;

        // Sound volume system
        [Header("Sound effects config")]
        public AudioClip SoundEffect;
        [Tooltip("Place a preconfigured AudioSource in here. (Configuring AudioSources at runtime is a pain! This lets you much easier choose the desired settings as well.)")]
        public AudioSource SoundEffectSource;
        [Tooltip("Same as the normal HeatExponent, but for the sound volume.")]
        public float SoundHeatExponent = 2f;
        public float MaxVolume = 0.4f;
        [Tooltip("Heat level at which audio starts.")]
        public float SoundHeatThreshold = 0f;
        [Tooltip("If checked, the sound volume starts at 0 when hitting the threshold and hits the max when heat is 1, else the volume is using the threshold heat value as a reference, aka the heat level it gets enabled at.")]
        public bool SoundVolumeStartsAtZero = false;
        [Tooltip("Enables sound volume AnimationCurve system.")]
        public bool SoundUsesAdvancedCurve = false;
        [Tooltip("Advanced sound volume control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) is the volume at that heat level.")]
        public AnimationCurve VolumeCurve;

        // Accuracy MOA multiplier system
        [Header("Accuracy Settings")]
        public bool DoesHeatAffectAccuracy = false;
        public float MaximumMOAMultiplier = 15f;
        [Tooltip("Same as the normal HeatExponent, but for the MOA multiplier.")]
        public float AccuracyHeatExponent = 2f;
        [Tooltip("Enables MOA multiplier AnimationCurve system.")]
        public bool AccuracyUsesAdvancedCurve = false;
        [Tooltip("Advanced MOA multiplier control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts like a multiplier of the max MOA multiplier, clamped between 0 and 1.")]
        public AnimationCurve AccuracyCurve;

        // Bolt Speed Change System
        [Header("Bolt Speed Settings")]
        public bool DoesHeatAffectBoltSpeed = false;
        public float MaxBoltForwardSpeedMultiplier = 0.8f;
        public float MaxBoltRearwardSpeedMultiplier = 0.8f;
        public float MaxBoltSpringStiffnessesMultiplier = 0.8f;
        [Tooltip("Same as the normal HeatExponent, but for the bolt speed system.")]
        public float BoltSpeedHeatExponent = 2f;

        [Tooltip("Enables bolt speed AnimationCurve system.")]
        public bool BoltSpeedUsesAdvancedCurve = false;
        [Tooltip("Advanced bolt speed control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts as an advanced lerp for the above set maximum values.")]
        public AnimationCurve BoltForwardSpeedMultiplierCurve;
        [Tooltip("Advanced bolt speed control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts as an advanced lerp for the above set maximum values.")]
        public AnimationCurve BoltRearwardSpeedMultiplierCurve;
        [Tooltip("Advanced bolt speed control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts as an advanced lerp for the above set maximum values.")]
        public AnimationCurve BoltSpringStiffnessMultiplierCurve;

        // Exploding Part System
        [Header("Explosion System")]
        public MeshRenderer ExplodingPart;
        [Tooltip("Heat level at which part explodes. Don't use a value of 1. It is very unlikely the part will actually hit a perfect 1 heat.")]
        public float ExplodingHeatThreshhold = 0.95f;
        public bool MessesUpAccuracy = true;
        [Tooltip("Choose a high enough number for your accuracy class.")]
        public float FailureAccuracyMultiplier = 1000f;
        [Tooltip("Causes bolt rearwards speed to become a positive number, therefor the firearm won't cycle correctly")]
        public bool CausesStoppage = true;
        [Tooltip("Just some random high negative number to offset the averaging affect of the different FirearmHeatingEffect components and invert the bolt speed.")]
        public float StoppageSpeed = -1000f;
        public AudioEvent ExplosionSound;
        [Tooltip("DEBUG VALUE for in editor testing! Actual value determined by in game config. Can Explosion revert?")]
        public bool CanRecoverFromExplosion = true;
        [Tooltip("DEBUG VALUE for in editor testing! Actual value determined by in game config. Heat level at which explosion will be reverted.")]
        public float RevertHeatThreshold = 0f;

        // Cookoff System
        [Header("Cookoff System")]
        [Tooltip("Cookoff is a random heat caused discharge, aka YOUR GUN GO BOOM without you pulling the trigger. Scary stuff.")]
        public bool DoesHeatCauseCookoff = false;
        [Tooltip("Chance of a cookoff to happen every second.")]
        public float MaxCookoffChancePerSecond = 0.5f;
        [Tooltip("Same as the normal HeatExponent, but for the bolt speed system.")]
        public float CookoffHeatExponent = 2f;
        [Tooltip("Heat level at which cookoff starts happening.")]
        public float CookoffHeatThreshold = 0f;
        [Tooltip("If checked, the cookoff chance starts at 0 when hitting the threshold and hits the max when heat is 1, else the chance is using the threshold heat value as a reference, aka the heat level it gets enabled at.")]
        public bool CookoffChanceStartsAtZero = true;

        // Debugging system
        [Header("Debug Messages")]
        public bool DebugEnabled = false;
        [Range(0f,1f)]
        public float Heat = 0f;

        [HideInInspector]
        public float CurrentBoltForwardSpeedMultiplier;
        [HideInInspector]
        public float CurrentBoltRearwardSpeedMultiplier;
        [HideInInspector]
        public float CurrentBoltSpringMultiplier;

        // Private variables
        private FirearmHeatingEffect_FirearmCore Core;
        private Material _copyMaterial;
        private Material _copyExplodedMaterial;
        

        private bool _isAttached = false;

        private bool _soundEnabled = false;
        private bool _canExplode = true;
        private bool _hasPartExploded = false;

        private bool _canChangeAccuracy = true;
        private bool _canCookOff = true;

        private float _origInternalMechanicalMOA = 0f;

        // Constants
        private const string c_EmissionWeightPropertyString = "_EmissionWeight";
        private const string c_IncandescenceScrollSpeedPropertyString = "_IncandescenceMapVelocity";
        private const string c_DetailWeightPropertyString = "_DetailWeight";


        public void Awake()
        {
			Hook();
#if !DEBUG
#if !MEATKIT
            _canExplode = OpenScripts_BepInEx.FirearmHeatingEffect_CanExplode.Value;
            _canChangeAccuracy = OpenScripts_BepInEx.FirearmHeatingEffect_CanChangeAccuracy.Value;
            _canCookOff = OpenScripts_BepInEx.FirearmHeatingEffect_CanCookOff.Value;
            CanRecoverFromExplosion = OpenScripts_BepInEx.FirearmHeatingEffect_CanRecover.Value;
            RevertHeatThreshold = OpenScripts_BepInEx.FirearmHeatingEffect_RecoverThreshold.Value;
#endif
            if (FireArm != null)
            {
                _origInternalMechanicalMOA = FireArm.m_internalMechanicalMOA;

                Core = FireArm.GetComponent<FirearmHeatingEffect_FirearmCore>();
                if (Core == null)
                {
                    Core = FireArm.gameObject.AddComponent<FirearmHeatingEffect_FirearmCore>(); 
                }
                Core.FirearmHeatingEffects.Add(this);
            }
            else if (Attachment != null && Attachment is MuzzleDevice muzzleDevice) _origInternalMechanicalMOA = muzzleDevice.m_mechanicalAccuracy;
#endif
            if (MeshRenderer != null)
            {
                _copyMaterial = MeshRenderer.materials[MaterialIndex];
            }

            if (ParticleSystem != null) ChangeParticleEmissionRate(0f);

            if (SoundEffectSource != null && SoundEffect != null) 
            {
                SoundEffectSource.loop = true;
                SoundEffectSource.clip = SoundEffect;
                SoundEffectSource.volume = 0f;
                SoundEffectSource.Stop();
            }

            if (ExplodingPart != null)
            {
                _copyExplodedMaterial = ExplodingPart.materials[MaterialIndex];
                ExplodingPart.gameObject.SetActive(false);
            }
            else _canExplode = false;

            if (DoesHeatCauseCookoff)
            {
                StartCoroutine(CookoffSystem());
            }
        }
		public void OnDestroy()
        {
			Unhook();

            if (_copyMaterial != null) Destroy(_copyMaterial);
            if (_copyExplodedMaterial != null) Destroy(_copyExplodedMaterial);
        }

        public void Update()
        {
            // Attachment Stuff
#if !(DEBUG)
            if (Attachment != null)
            {
                if (!_isAttached && Attachment.curMount != null && Attachment.curMount.GetRootMount().MyObject is FVRFireArm fireArm)
                {
                    FireArm = fireArm;
                    if (!(Attachment is MuzzleDevice)) _origInternalMechanicalMOA = FireArm.m_internalMechanicalMOA;
                    AttachToCore();
                    _isAttached = true;
                }
                else if (_isAttached && Attachment.curMount == null)
                {
                    DetachFromCore();
                    if (!(Attachment is MuzzleDevice)) FireArm.m_internalMechanicalMOA = _origInternalMechanicalMOA;
                    FireArm = null;
                    _isAttached = false;
                }
            }
#endif
            // Cooldown
            if (Heat > 0f) Heat -= Time.deltaTime * CooldownRate;
            Heat = Mathf.Clamp(Heat, 0f, 1f);

            // Emission and Detail
            if (MeshRenderer != null)
            {
                if (!EmissionUsesAdvancedCurve)
                {
                    float pow = Mathf.Pow(Heat, HeatExponent);
                    if (!_hasPartExploded)
                    {
                        if (HeatAffectsEmissionWeight) _copyMaterial.SetFloat(c_EmissionWeightPropertyString, pow);
                        if (HeatAffectsEmissionScrollSpeed)
                        {
                            Vector4 ScrollSpeed = Vector4.zero;
                            ScrollSpeed.x = Mathf.Lerp(0f, MaxEmissionScrollSpeed_X, pow);
                            ScrollSpeed.y = Mathf.Lerp(0f, MaxEmissionScrollSpeed_Y, pow);
                            _copyMaterial.SetVector(c_IncandescenceScrollSpeedPropertyString, ScrollSpeed);
                        }
                    }
                    else
                    {
                        if (HeatAffectsEmissionWeight) _copyExplodedMaterial.SetFloat(c_EmissionWeightPropertyString, pow);
                        if (HeatAffectsEmissionScrollSpeed)
                        {
                            Vector4 ScrollSpeed = Vector4.zero;
                            ScrollSpeed.x = Mathf.Lerp(0f, MaxEmissionScrollSpeed_X, pow);
                            ScrollSpeed.y = Mathf.Lerp(0f, MaxEmissionScrollSpeed_Y, pow);
                            _copyExplodedMaterial.SetVector(c_IncandescenceScrollSpeedPropertyString, ScrollSpeed);
                        }
                    }
                }
                else
                {
                    if (!_hasPartExploded)
                    {
                        if (HeatAffectsEmissionWeight) _copyMaterial.SetFloat(c_EmissionWeightPropertyString, EmissionCurve.Evaluate(Heat));
                        if (HeatAffectsEmissionScrollSpeed)
                        {
                            Vector4 ScrollSpeed = Vector4.zero;
                            ScrollSpeed.x = EmissionScrollSpeedCurve_X.Evaluate(Heat);
                            ScrollSpeed.y = EmissionScrollSpeedCurve_Y.Evaluate(Heat);
                            _copyMaterial.SetVector(c_IncandescenceScrollSpeedPropertyString, ScrollSpeed);
                        }
                    }
                    else
                    {
                        if (HeatAffectsEmissionWeight) _copyExplodedMaterial.SetFloat(c_EmissionWeightPropertyString, EmissionCurve.Evaluate(Heat));
                        if (HeatAffectsEmissionScrollSpeed)
                        {
                            Vector4 ScrollSpeed = Vector4.zero;
                            ScrollSpeed.x = EmissionScrollSpeedCurve_X.Evaluate(Heat);
                            ScrollSpeed.y = EmissionScrollSpeedCurve_Y.Evaluate(Heat);
                            _copyExplodedMaterial.SetVector(c_IncandescenceScrollSpeedPropertyString, ScrollSpeed);
                        }
                    }
                }

                Log(MeshRenderer.materials[MaterialIndex].GetFloat(c_EmissionWeightPropertyString));
                if (HeatAffectsDetailWeight)
                {
                    if (!DetailUsesAdvancedCurve) _copyMaterial.SetFloat(c_DetailWeightPropertyString, Mathf.Pow(Heat, DetailExponent));
                    else _copyMaterial.SetFloat(c_DetailWeightPropertyString, DetailCurve.Evaluate(Heat));
                }
            }

            // Particles
            if (ParticleSystem != null)
            {
                if (!ParticlesUsesAdvancedCurve)
                {
                    if (Heat > ParticleHeatThreshold)
                    {
                        float inverseLerp;
                        if (ParticleEmissionRateStartsAtZero) inverseLerp = Mathf.InverseLerp(ParticleHeatThreshold, 1f, Heat);
                        else inverseLerp = Heat;
                        ChangeParticleEmissionRate(inverseLerp);
                    }
                    else
                    {
                        ChangeParticleEmissionRate(0f);
                    }
                }
                else ChangeParticleEmissionRate(ParticlesCurve.Evaluate(Heat) * MaxEmissionRate);
            }

            // Sounds
            if (SoundEffect != null)
            {
                if (!SoundUsesAdvancedCurve)
                {
                    if (Heat > SoundHeatThreshold)
                    {
                        if (!_soundEnabled)
                        {
                            SoundEffectSource.Play();
                            _soundEnabled = true;
                        }
                        float inverseLerp;
                        if (SoundVolumeStartsAtZero) inverseLerp = Mathf.InverseLerp(SoundHeatThreshold, 1f, Heat);
                        else inverseLerp = Heat;
                        SoundEffectSource.volume = Mathf.Lerp(0f, MaxVolume, Mathf.Pow(inverseLerp, SoundHeatExponent));
                    }
                    else if (_soundEnabled)
                    {
                        SoundEffectSource.Stop();
                        _soundEnabled = false;
                    }
                }
                else
                {
                    float volumeEvaluation = VolumeCurve.Evaluate(Heat);
                    if (volumeEvaluation > 0f)
                    {
                        if (!_soundEnabled)
                        {
                            SoundEffectSource.Play();
                            _soundEnabled = true;
                        }
                        SoundEffectSource.volume = volumeEvaluation;
                    }
                    else if (_soundEnabled)
                    {
                        SoundEffectSource.Stop();
                        _soundEnabled = false;
                    }
                }
            }

            // Accuracy
            if (_canChangeAccuracy && DoesHeatAffectAccuracy && FireArm != null && !_hasPartExploded)
            {
#if !DEBUG
                ChangeAccuracy();
#endif
            }

            // Bolt Speed
            if (DoesHeatAffectBoltSpeed && !_hasPartExploded)
            {
                if (!BoltSpeedUsesAdvancedCurve)
                {
                    float pow = Mathf.Pow(Heat, BoltSpeedHeatExponent);
                    CurrentBoltForwardSpeedMultiplier = Mathf.Lerp(1f, MaxBoltForwardSpeedMultiplier, pow);
                    CurrentBoltRearwardSpeedMultiplier = Mathf.Lerp(1f, MaxBoltRearwardSpeedMultiplier, pow);
                    CurrentBoltSpringMultiplier = Mathf.Lerp(1f, MaxBoltSpringStiffnessesMultiplier, pow);
                }
                else
                {
                    CurrentBoltForwardSpeedMultiplier = Mathf.Lerp(1f, MaxBoltForwardSpeedMultiplier, BoltForwardSpeedMultiplierCurve.Evaluate(Heat));
                    CurrentBoltRearwardSpeedMultiplier = Mathf.Lerp(1f, MaxBoltRearwardSpeedMultiplier, BoltRearwardSpeedMultiplierCurve.Evaluate(Heat));
                    CurrentBoltSpringMultiplier = Mathf.Lerp(1f, MaxBoltSpringStiffnessesMultiplier, BoltSpringStiffnessMultiplierCurve.Evaluate(Heat));
                }
            }

            // Part Explosion

            if (!_hasPartExploded && _canExplode && Heat > ExplodingHeatThreshhold)
            {
                _hasPartExploded = true;
                SM.PlayCoreSound(FVRPooledAudioType.Explosion, ExplosionSound, transform.position);
                if (MeshRenderer.gameObject == this.gameObject) MeshRenderer.enabled = false;
                else MeshRenderer.gameObject.SetActive(false);
                ExplodingPart.gameObject.SetActive(true);
#if !DEBUG
                if (Attachment == null) FireArm.m_internalMechanicalMOA = FailureAccuracyMultiplier * _origInternalMechanicalMOA;
                else if (Attachment != null && Attachment is MuzzleDevice muzzleDevice) muzzleDevice.m_mechanicalAccuracy = FailureAccuracyMultiplier * _origInternalMechanicalMOA;
                else if (Attachment != null && !(Attachment is MuzzleDevice)) FireArm.m_internalMechanicalMOA = FailureAccuracyMultiplier * _origInternalMechanicalMOA;
#endif
                CurrentBoltRearwardSpeedMultiplier = StoppageSpeed;
            }
            else if (_hasPartExploded && CanRecoverFromExplosion && Heat <= RevertHeatThreshold)
            {
                _hasPartExploded = false;
                if (MeshRenderer.gameObject == this.gameObject) MeshRenderer.enabled = true;
                else MeshRenderer.gameObject.SetActive(true);
                ExplodingPart.gameObject.SetActive(false);
#if !DEBUG
                if (Attachment == null) FireArm.m_internalMechanicalMOA = _origInternalMechanicalMOA;
                else if (Attachment != null && Attachment is MuzzleDevice muzzleDevice) muzzleDevice.m_mechanicalAccuracy = _origInternalMechanicalMOA;
                else if (Attachment != null && !(Attachment is MuzzleDevice)) FireArm.m_internalMechanicalMOA = _origInternalMechanicalMOA;
#endif
                CurrentBoltRearwardSpeedMultiplier = 1f;
            }
            Log(Heat);
        }
#if !DEBUG
        private void ChangeAccuracy()
        {
            float pow;
            if (!AccuracyUsesAdvancedCurve) pow = Mathf.Pow(Heat, AccuracyHeatExponent);
            else pow = AccuracyCurve.Evaluate(Heat);

            if (Attachment == null) FireArm.m_internalMechanicalMOA = Mathf.Lerp(1f, MaximumMOAMultiplier, pow) * _origInternalMechanicalMOA;
            else if (Attachment != null && Attachment is MuzzleDevice muzzleDevice) muzzleDevice.m_mechanicalAccuracy = Mathf.Lerp(1f, MaximumMOAMultiplier, pow) * _origInternalMechanicalMOA;
            else if (Attachment != null && !(Attachment is MuzzleDevice)) FireArm.m_internalMechanicalMOA = Mathf.Lerp(1f, MaximumMOAMultiplier, pow) * _origInternalMechanicalMOA;
        }
#endif
        private void ChangeParticleEmissionRate(float heat)
        {
            float particleEmissionRate = Mathf.Lerp(0f, MaxEmissionRate, Mathf.Pow(heat, ParticleHeatExponent));
            ParticleSystem.EmissionModule emission = ParticleSystem.emission;
            ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
            rateOverTime.mode = ParticleSystemCurveMode.Constant;
            rateOverTime.constantMax = particleEmissionRate;
            rateOverTime.constantMin = particleEmissionRate;
            emission.rateOverTime = rateOverTime;
        }

        private void AttachToCore()
        {
            Core = FireArm.GetComponent<FirearmHeatingEffect_FirearmCore>();
            if (Core == null)
            {
                Core = FireArm.gameObject.AddComponent<FirearmHeatingEffect_FirearmCore>();
            }
            Core.FirearmHeatingEffects.Add(this);
        }

        private void DetachFromCore()
        {
            Core.FirearmHeatingEffects.Remove(this);
            Core = null;
        }
        void Unhook()
        {
#if !DEBUG
            GM.CurrentSceneSettings.ShotFiredEvent -= OnShotFired;
#if !MEATKIT
            OpenScripts_BepInEx.FirearmHeatingEffect_CanExplode.SettingChanged -= SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_CanRecover.SettingChanged -= SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_RecoverThreshold.SettingChanged -= SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_CanChangeFirerate.SettingChanged -= SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_CanChangeAccuracy.SettingChanged -= SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_CanCookOff.SettingChanged -= SettingsChanged;
#endif
#endif
        }

        void Hook()
        {
#if !DEBUG
            GM.CurrentSceneSettings.ShotFiredEvent += OnShotFired;
#if !MEATKIT
            OpenScripts_BepInEx.FirearmHeatingEffect_CanExplode.SettingChanged += SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_CanRecover.SettingChanged += SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_RecoverThreshold.SettingChanged += SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_CanChangeFirerate.SettingChanged += SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_CanChangeAccuracy.SettingChanged += SettingsChanged;
            OpenScripts_BepInEx.FirearmHeatingEffect_CanCookOff.SettingChanged += SettingsChanged;
#endif
#endif
        }

        private void OnShotFired(FVRFireArm firearm)
        {
            if (FireArm != null && firearm == FireArm)
            {
                float powerMult = 1f;
                if (ChangesWithCartridgePower)
                {
                    FVRObject.OTagFirearmRoundPower power = AM.GetRoundPower(FireArm.RoundType);
                    powerMult = RoundPowerMultipliers[(int)power];
                }

                if (Core != null) Heat += HeatPerShot * Core.CombinedHeatMultiplier * powerMult;
                else Heat += HeatPerShot * HeatMultiplier * powerMult;
            }
        }

        private void SettingsChanged(object sender, EventArgs e)
        {
            if (ExplodingPart != null) _canExplode = OpenScripts_BepInEx.FirearmHeatingEffect_CanExplode.Value;
            else _canExplode = false;
            CanRecoverFromExplosion = OpenScripts_BepInEx.FirearmHeatingEffect_CanRecover.Value;
            RevertHeatThreshold = OpenScripts_BepInEx.FirearmHeatingEffect_RecoverThreshold.Value;
            _canChangeAccuracy = OpenScripts_BepInEx.FirearmHeatingEffect_CanChangeAccuracy.Value;
            _canCookOff = OpenScripts_BepInEx.FirearmHeatingEffect_CanCookOff.Value;
        }

        private IEnumerator CookoffSystem()
        {
            while (DoesHeatCauseCookoff)
            {
                float rand = UnityEngine.Random.Range(0f, 1f);
                if (_canCookOff && Heat > CookoffHeatThreshold)
                {
                    float inverseLerp;
                    if (CookoffChanceStartsAtZero) inverseLerp = Mathf.InverseLerp(CookoffHeatThreshold, 1f, Heat);
                    else inverseLerp = Heat;
                    float chance = Mathf.Lerp(0f, MaxCookoffChancePerSecond, inverseLerp);

                    if (rand < chance && FireArm != null)
                    {
                        switch (FireArm)
                        {
                            case ClosedBoltWeapon w:
#if !DEBUG
                                w.DropHammer();
#endif
                                break;
                            case Handgun w:
#if !DEBUG
                                w.DropHammer(false);
#endif
                                break;
                            default:
                                break;
                        }
                    }
                }
                yield return new WaitForSeconds(1f);
            }
        }
        
        private void Log(object message)
        {
            if (DebugEnabled) Debug.Log(gameObject.name + " " + message);
        }
    }
}
