#if DEBUG || MEATKIT
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FistVR;
using Cityrobo;

[CustomEditor(typeof(FirearmHeatingEffect))]
public class FirearmHeatingEffectEditor : Editor 
{
    private FirearmHeatingEffect f;
    private bool _heatFoldOut;
    private bool _emissionFoldOut;
    private bool _detailFoldOut;
    private bool _particleFoldOut;
    private bool _soundFoldOut;
    private bool _accuracyFoldOut;
    private bool _boltFoldOut;
    private bool _explodingFoldOut;
    private bool _cookoffFoldOut;
    private bool _debugFoldOut;

    private SerializedProperty _explosionSound;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        f = target as FirearmHeatingEffect;
        GUIStyle bold = new GUIStyle(EditorStyles.boldLabel);
        bold.fontStyle = FontStyle.Bold;
        bold.fontSize = 15;
        bold.fixedHeight = 30;

        GUIStyle foldout = new GUIStyle(EditorStyles.foldout);
        foldout.fontStyle = FontStyle.Bold;

        EditorGUILayout.LabelField("Firearm Heating Effect", bold);
        HorizontalLine(Color.gray);
        f.FireArm = (FVRFireArm)EditorGUILayout.ObjectField(new GUIContent("Firearm", "Use this if you plan to use the script on a firearm."), f.FireArm, typeof(FVRFireArm), true);

        f.Attachment = (FVRFireArmAttachment)EditorGUILayout.ObjectField(new GUIContent("Attachment", "Use this if you plan to use the script on an attachment."), f.Attachment, typeof(FVRFireArmAttachment), true);
        if ((f.FireArm != null || f.Attachment != null) && !(f.FireArm != null && f.Attachment != null))
        {
            _heatFoldOut = EditorGUILayout.Foldout(_heatFoldOut, "Heat Settings", foldout);
            if (_heatFoldOut)
            {
                f.HeatPerShot = EditorGUILayout.FloatField(new GUIContent("Heat per Shot", "Heat added to the effect per shot fired. Heat caps out at 1."), f.HeatPerShot);
                f.HeatMultiplier = EditorGUILayout.FloatField(new GUIContent("Heat Multiplier", "This multiplier will affect the heat gain of all parts of the gun. It will act multiplicatively with other such multipliers."), f.HeatMultiplier);
                f.CooldownRate = EditorGUILayout.FloatField(new GUIContent("Cooldown Rate", "Heat removed per second."), f.CooldownRate);
                f.ChangesWithCartridgePower = EditorGUILayout.Toggle(new GUIContent("Does Round Power affect Heat?"), f.ChangesWithCartridgePower);
                if (f.ChangesWithCartridgePower)
                {
                    SerializedProperty powerMult = serializedObject.FindProperty("RoundPowerMultipliers");
                    EditorGUILayout.PropertyField(powerMult, true);
                }
            }
            _emissionFoldOut = EditorGUILayout.Foldout(_emissionFoldOut, "Emission Modification", foldout);
            if (_emissionFoldOut)
            {
                f.MeshRenderer = (MeshRenderer)EditorGUILayout.ObjectField(new GUIContent("Mesh Renderer", "Affected Mesh Renderer."), f.MeshRenderer, typeof(MeshRenderer), true);
                if (f.MeshRenderer != null)
                {
                    f.MaterialIndex = EditorGUILayout.IntField(new GUIContent("Material Index", "Index of the material in the MeshRenderer's materials list."), f.MaterialIndex);
                    f.HeatExponent = EditorGUILayout.FloatField(new GUIContent("Heat Exponent", "Anton uses the squared value of the heat to determine the emission weight. If you wanna replicate that behavior, leave the value as is, but feel free to go crazy if you wanna mix things up."), f.HeatExponent);
                    f.HeatAffectsEmissionWeight = EditorGUILayout.Toggle("Does Heat affect Emission Weight?", f.HeatAffectsEmissionWeight);
                    f.HeatAffectsEmissionScrollSpeed = EditorGUILayout.Toggle("Does Heat affect Emission Scroll Speed?", f.HeatAffectsEmissionScrollSpeed);
                    if (f.HeatAffectsEmissionScrollSpeed)
                    {
                        f.MaxEmissionScrollSpeed_X = EditorGUILayout.FloatField(new GUIContent("Max Emission Scolly Speed X", "Maximum left/right emission scroll speed."), f.MaxEmissionScrollSpeed_X);
                        f.MaxEmissionScrollSpeed_Y = EditorGUILayout.FloatField(new GUIContent("Max Emission Scolly Speed Y", "Maximum up/down emission scroll speed."), f.MaxEmissionScrollSpeed_Y);
                    }

                    f.EmissionUsesAdvancedCurve = EditorGUILayout.Toggle(new GUIContent("Does Emission use Advanced Curve?", "Enables emission weight AnimationCurve system."), f.EmissionUsesAdvancedCurve);
                    if (f.EmissionUsesAdvancedCurve)
                    {
                        f.EmissionCurve = EditorGUILayout.CurveField(new GUIContent("Emission Curve", "Advanced emission weight control. Values from 0 to 1 only!"), f.EmissionCurve);
                        if (f.HeatAffectsEmissionScrollSpeed)
                        {
                            f.EmissionScrollSpeedCurve_X = EditorGUILayout.CurveField(new GUIContent("Emission Scroll Speed Curve X", "Advanced left/right emission scroll speed control. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-Axis) represents the actual scroll speed and is uncapped. The max value set above is ignored."), f.EmissionScrollSpeedCurve_X);
                            f.EmissionScrollSpeedCurve_Y = EditorGUILayout.CurveField(new GUIContent("Emission Scroll Speed Curve Y", "Advanced up/down emission scroll speed control. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-Axis) represents the actual scroll speed and is uncapped. The max value set above is ignored."), f.EmissionScrollSpeedCurve_Y);
                        }
                    }
                }
            }
            _detailFoldOut = EditorGUILayout.Foldout(_detailFoldOut, "Detail Weight", foldout);
            if (_detailFoldOut)
            {
                f.HeatAffectsDetailWeight = EditorGUILayout.Toggle("Does Heat affects Detail Weight?", f.HeatAffectsDetailWeight);
                if (f.HeatAffectsDetailWeight)
                {
                    f.DetailExponent = EditorGUILayout.FloatField(new GUIContent("Detail Exponent", "Same as the normal HeatExponent, but for the detail weight."), f.DetailExponent);
                    f.DetailUsesAdvancedCurve = EditorGUILayout.Toggle(new GUIContent("Does Detail use Advanced Curve?", "Enables emission weight AnimationCurve system."), f.DetailUsesAdvancedCurve);
                    if (f.DetailUsesAdvancedCurve)
                    {
                        f.DetailCurve = EditorGUILayout.CurveField(new GUIContent("Detail Curve", "Advanced emission weight control. Values from 0 to 1 only!"), f.DetailCurve);
                    }
                }
            }
            _particleFoldOut = EditorGUILayout.Foldout(_particleFoldOut, "Particle Emission", foldout);
            if (_particleFoldOut)
            {
                f.ParticleSystem = (ParticleSystem)EditorGUILayout.ObjectField(new GUIContent("Particle System"), f.ParticleSystem, typeof(ParticleSystem), true);
                if (f.ParticleSystem != null)
                {
                    f.MaxEmissionRate = EditorGUILayout.FloatField(new GUIContent("Max Emission Rate"), f.MaxEmissionRate);
                    f.ParticleHeatExponent = EditorGUILayout.FloatField(new GUIContent("Particle Heat Exponent", "Same as the normal HeatExponent, but for the particle emission rate."), f.ParticleHeatExponent);
                    f.ParticleHeatThreshold = EditorGUILayout.FloatField(new GUIContent("Particle Heat Threshold", "Heat level at which particles start appearing."), f.ParticleHeatThreshold);
                    f.ParticleEmissionRateStartsAtZero = EditorGUILayout.Toggle(new GUIContent("Particle Emission Rate starts at Zero", "If checked, the particle emission rate starts at 0 when hitting the threshold and hits the max when heat is 1, else it starts emitting using the threshold heat value as a reference, aka the heat level it gets enabled at."), f.ParticleEmissionRateStartsAtZero);
                    f.ParticlesUsesAdvancedCurve = EditorGUILayout.Toggle(new GUIContent("Do Particles use Advanced Curve?", "Enables particle rate AnimationCurve system."), f.ParticlesUsesAdvancedCurve);
                    if (f.ParticlesUsesAdvancedCurve)
                    {
                        f.ParticlesCurve = EditorGUILayout.CurveField(new GUIContent("Particle Rate Curve", "Advanced particle rate control. Values from 0 to 1 only! The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts like a multiplier of the max emission rate, clamped between 0 and 1."), f.ParticlesCurve);
                    }
                }
            }
            _soundFoldOut = EditorGUILayout.Foldout(_soundFoldOut, "Sound Effects", foldout);
            if (_soundFoldOut)
            {
                f.SoundEffectSource = (AudioSource)EditorGUILayout.ObjectField(new GUIContent("Sound Effect Audio Source", "Place a preconfigured AudioSource in here. (Configuring AudioSources at runtime is a pain! This lets you much easier choose the desired settings as well.)"), f.SoundEffectSource, typeof(AudioSource), true);

                if (f.SoundEffectSource != null)
                {
                    f.SoundEffect = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Sound Effect Audio Clip"), f.SoundEffect, typeof(AudioClip), true);
                    if (f.SoundEffect != null)
                    {
                        f.MaxVolume = EditorGUILayout.Slider(new GUIContent("Max Volume"), f.MaxVolume, 0f, 1f);
                        f.SoundHeatExponent = EditorGUILayout.FloatField(new GUIContent("Sound Heat Exponent", "Same as the normal HeatExponent, but for the sound volume."), f.SoundHeatExponent);
                        f.SoundHeatThreshold = EditorGUILayout.FloatField(new GUIContent("Sound Heat Threshold", "Heat level at which audio starts."), f.SoundHeatThreshold);
                        f.SoundVolumeStartsAtZero = EditorGUILayout.Toggle(new GUIContent("Sound Volume Starts at Zero", "If checked, the sound volume starts at 0 when hitting the threshold and hits the max when heat is 1, else the volume is using the threshold heat value as a reference, aka the heat level it gets enabled at."), f.SoundVolumeStartsAtZero);
                        f.SoundUsesAdvancedCurve = EditorGUILayout.Toggle(new GUIContent("Does Sound Volume use Advanced Curve?", "Enables sound volume AnimationCurve system."), f.SoundUsesAdvancedCurve);
                        if (f.SoundUsesAdvancedCurve)
                        {
                            f.VolumeCurve = EditorGUILayout.CurveField(new GUIContent("Volume Curve", "Advanced sound volume control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) is the volume at that heat level."), f.VolumeCurve);
                        }
                    }
                }
            }
            _accuracyFoldOut = EditorGUILayout.Foldout(_accuracyFoldOut, "Accuracy", foldout);
            if (_accuracyFoldOut)
            {
                f.DoesHeatAffectAccuracy = EditorGUILayout.Toggle(new GUIContent("Does Heat affect Accuracy?"), f.DoesHeatAffectAccuracy);
                if (f.DoesHeatAffectAccuracy)
                {
                    f.MaximumMOAMultiplier = EditorGUILayout.FloatField(new GUIContent("Maximum MOA Multiplier"), f.MaximumMOAMultiplier);
                    f.AccuracyHeatExponent = EditorGUILayout.FloatField(new GUIContent("Accuracy Heat Exponent", "Same as the normal HeatExponent, but for the MOA multiplier."), f.AccuracyHeatExponent);
                    f.AccuracyUsesAdvancedCurve = EditorGUILayout.Toggle(new GUIContent("Does Accuracy use Advanced Curve?", "Enables MOA multiplier AnimationCurve system."), f.AccuracyUsesAdvancedCurve);
                    if (f.AccuracyUsesAdvancedCurve)
                    {
                        f.AccuracyCurve = EditorGUILayout.CurveField(new GUIContent("Accuracy Curve", "Advanced MOA multiplier control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts like a multiplier of the max MOA multiplier, clamped between 0 and 1."), f.AccuracyCurve);
                    }
                }
            }
            _boltFoldOut = EditorGUILayout.Foldout(_boltFoldOut, "Bolt Speed", foldout);
            if (_boltFoldOut)
            {
                f.DoesHeatAffectBoltSpeed = EditorGUILayout.Toggle(new GUIContent("Does Heat affect Bolt Speed?"), f.DoesHeatAffectBoltSpeed);
                if (f.DoesHeatAffectBoltSpeed)
                {
                    f.MaxBoltForwardSpeedMultiplier = EditorGUILayout.FloatField(new GUIContent("Maximum Bolt Forward Speed Multiplier"), f.MaxBoltForwardSpeedMultiplier);
                    f.MaxBoltRearwardSpeedMultiplier = EditorGUILayout.FloatField(new GUIContent("Maximum Bolt Rearward Speed Multiplier"), f.MaxBoltRearwardSpeedMultiplier);
                    f.MaxBoltSpringStiffnessesMultiplier = EditorGUILayout.FloatField(new GUIContent("Maximum Bolt Spring Stiffness Multiplier"), f.MaxBoltSpringStiffnessesMultiplier);
                    f.BoltSpeedHeatExponent = EditorGUILayout.FloatField(new GUIContent("Bolt Speed Heat Exponent", "Same as the normal HeatExponent, but for the bolt speed system."), f.BoltSpeedHeatExponent);
                    f.BoltSpeedUsesAdvancedCurve = EditorGUILayout.Toggle(new GUIContent("Does Bolt Speed use Advanced Curves?", "Enables bolt speed AnimationCurve system."), f.BoltSpeedUsesAdvancedCurve);
                    if (f.BoltSpeedUsesAdvancedCurve)
                    {
                        f.BoltForwardSpeedMultiplierCurve = EditorGUILayout.CurveField(new GUIContent("Bolt Forward Speed Multiplier Curve", "Advanced bolt speed control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts as an advanced lerp for the above set maximum values."), f.BoltForwardSpeedMultiplierCurve);
                        f.BoltRearwardSpeedMultiplierCurve = EditorGUILayout.CurveField(new GUIContent("Bolt Rearward Speed Multiplier Curve", "Advanced bolt speed control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts as an advanced lerp for the above set maximum values."), f.BoltRearwardSpeedMultiplierCurve);
                        f.BoltSpringStiffnessMultiplierCurve = EditorGUILayout.CurveField(new GUIContent("Bolt Spring Stiffness Multiplier Curve", "Advanced bolt speed control. Values from 0 to 1 only!. The X-axis is clamped between 0 and 1 and represents the heat level. The value (Y-axis) acts as an advanced lerp for the above set maximum values."), f.BoltSpringStiffnessMultiplierCurve);
                    }
                }
            }
            _explodingFoldOut = EditorGUILayout.Foldout(_explodingFoldOut, "Explosion System", foldout);
            if (_explodingFoldOut)
            {
                f.ExplodingPart = (MeshRenderer)EditorGUILayout.ObjectField(new GUIContent("Exploded Part MeshRenderer"), f.ExplodingPart, typeof(MeshRenderer), true);
                if (f.ExplodingPart != null)
                {
                    f.ExplodingHeatThreshhold = EditorGUILayout.FloatField(new GUIContent("Exploding Heat Threshold", "Heat level at which part explodes. Don't use a value of 1. It is very unlikely the part will actually hit a perfect 1 heat."), f.ExplodingHeatThreshhold);
                    f.MessesUpAccuracy = EditorGUILayout.Toggle(new GUIContent("Does Explosion mess up Accuracy?"), f.MessesUpAccuracy);
                    if (f.MessesUpAccuracy)
                    {
                        f.FailureAccuracyMultiplier = EditorGUILayout.FloatField(new GUIContent("Explosion Accuracy Multiplier", "Choose a high enough number for your accuracy class."), f.FailureAccuracyMultiplier);
                    }
                    f.CausesStoppage = EditorGUILayout.Toggle(new GUIContent("Does Explosion cause Gun to stop cycling?"), f.CausesStoppage);
                    if (f.CausesStoppage)
                    {
                        f.StoppageSpeed = EditorGUILayout.FloatField(new GUIContent("Explosion Bolt Speed Override", "Just some random high number to offset the averaging affect of the different FirearmHeatingEffect components."), f.StoppageSpeed);
                    }
                    _explosionSound = serializedObject.FindProperty("ExplosionSound");
                    BetterPropertyField.DrawSerializedProperty(_explosionSound);

                    f.CanRecoverFromExplosion = EditorGUILayout.Toggle(new GUIContent("DEBUG: Can Explosion revert?", "DEBUG VALUE for in editor testing! Actual value determined by in game config."), f.CanRecoverFromExplosion);
                    if (f.CanRecoverFromExplosion)
                    {
                        f.RevertHeatThreshold = EditorGUILayout.FloatField(new GUIContent("DEBUG: Revert Heat Threshold", "DEBUG VALUE for in editor testing! Actual value determined by in game config. Heat level at which explosion will be reverted."), f.RevertHeatThreshold);
                    }
                }
            }
            _cookoffFoldOut = EditorGUILayout.Foldout(_cookoffFoldOut, "Cookoff", foldout);
            if (_cookoffFoldOut)
            {
                f.DoesHeatCauseCookoff = EditorGUILayout.Toggle(new GUIContent("Does Heat cause Cookoff?", "Cookoff is a random heat caused discharge, aka YOUR GUN GO BOOM without you pulling the trigger. Scary stuff."), f.DoesHeatCauseCookoff);
                if (f.DoesHeatCauseCookoff)
                {
                    f.MaxCookoffChancePerSecond = EditorGUILayout.FloatField(new GUIContent("Max Cookoff chance per Second", "Chance of a cookoff to happen every second."), f.MaxCookoffChancePerSecond);
                    f.CookoffHeatExponent = EditorGUILayout.FloatField(new GUIContent("Cookoff Heat Exponent", "Same as the normal HeatExponent, but for the bolt speed system."), f.CookoffHeatExponent);
                    f.CookoffHeatThreshold = EditorGUILayout.FloatField(new GUIContent("Cookoff Heat Threshold", "Heat level at which cookoff starts happening."), f.CookoffHeatThreshold);
                    f.CookoffChanceStartsAtZero = EditorGUILayout.Toggle(new GUIContent("Cookoff Chance Starts at Zero", "If checked, the cookoff chance starts at 0 when hitting the threshold and hits the max when heat is 1, else the chance is using the threshold heat value as a reference, aka the heat level it gets enabled at."), f.CookoffChanceStartsAtZero);
                }
            }
            _debugFoldOut = EditorGUILayout.Foldout(_debugFoldOut, "Debugging", foldout);
            if (_debugFoldOut)
            {
                f.DebugEnabled = EditorGUILayout.Toggle(new GUIContent("Debugging log messages enabled?"), f.DebugEnabled);
                f.Heat = EditorGUILayout.Slider(new GUIContent("Heat"), f.Heat, 0f, 1f);
            }
        }
        else 
        {
            GUIStyle red = new GUIStyle(EditorStyles.label);
            red.normal.textColor = Color.red;
            EditorGUILayout.LabelField("Please select either a Firearm or Attachment.",red);
            if (f.FireArm != null && f.Attachment != null) EditorGUILayout.LabelField("Not both!",red);
        }

        serializedObject.ApplyModifiedProperties();
    }

    static void HorizontalLine(Color color)
    {
        GUIStyle horizontalLine;
        horizontalLine = new GUIStyle();
        horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
        horizontalLine.margin = new RectOffset(0, 0, 4, 4);
        horizontalLine.fixedHeight = 1;

        var c = GUI.color;
        GUI.color = color;
        GUILayout.Box(GUIContent.none, horizontalLine);
        GUI.color = c;
    }

    public static class BetterPropertyField
    {


        /// <summary>
        /// Draws a serialized property (including children) fully, even if it's an instance of a custom serializable class.
        /// Supersedes EditorGUILayout.PropertyField(serializedProperty, true);
        /// </summary>
        /// <param name="_serializedProperty">Serialized property.</param>
        /// source: https://gist.github.com/tomkail/ba8d49e1cee021b0b89d47fca68b53a2
        public static void DrawSerializedProperty(SerializedProperty _serializedProperty)
        {
            if (_serializedProperty == null)
            {
                EditorGUILayout.HelpBox("SerializedProperty was null!", MessageType.Error);
                return;
            }
            var serializedProperty = _serializedProperty.Copy();
            int startingDepth = serializedProperty.depth;
            EditorGUI.indentLevel = serializedProperty.depth;
            DrawPropertyField(serializedProperty);
            while (serializedProperty.NextVisible(serializedProperty.isExpanded && !PropertyTypeHasDefaultCustomDrawer(serializedProperty.propertyType)) && serializedProperty.depth > startingDepth)
            {
                EditorGUI.indentLevel = serializedProperty.depth;
                DrawPropertyField(serializedProperty);
            }
            EditorGUI.indentLevel = startingDepth;
        }
        public static void DrawSerializedProperty(SerializedProperty _serializedProperty, GUIContent content)
        {
            if (_serializedProperty == null)
            {
                EditorGUILayout.HelpBox("SerializedProperty was null!", MessageType.Error);
                return;
            }
            var serializedProperty = _serializedProperty.Copy();
            int startingDepth = serializedProperty.depth;
            EditorGUI.indentLevel = serializedProperty.depth;
            DrawPropertyField(serializedProperty, content);
            while (serializedProperty.NextVisible(serializedProperty.isExpanded && !PropertyTypeHasDefaultCustomDrawer(serializedProperty.propertyType)) && serializedProperty.depth > startingDepth)
            {
                EditorGUI.indentLevel = serializedProperty.depth;
                DrawPropertyField(serializedProperty);
            }
            EditorGUI.indentLevel = startingDepth;
        }

        static void DrawPropertyField(SerializedProperty serializedProperty)
        {
            if (serializedProperty.propertyType == SerializedPropertyType.Generic)
            {
                serializedProperty.isExpanded = EditorGUILayout.Foldout(serializedProperty.isExpanded, serializedProperty.displayName, true);
            }
            else
            {
                EditorGUILayout.PropertyField(serializedProperty);
            }
        }
        static void DrawPropertyField(SerializedProperty serializedProperty, GUIContent content)
        {
            if (serializedProperty.propertyType == SerializedPropertyType.Generic)
            {
                serializedProperty.isExpanded = EditorGUILayout.Foldout(serializedProperty.isExpanded, serializedProperty.displayName, true);
            }
            else
            {
                EditorGUILayout.PropertyField(serializedProperty, content);
            }
        }

        static bool PropertyTypeHasDefaultCustomDrawer(SerializedPropertyType type)
        {
            return
            type == SerializedPropertyType.AnimationCurve ||
            type == SerializedPropertyType.Bounds ||
            type == SerializedPropertyType.Color ||
            type == SerializedPropertyType.Gradient ||
            type == SerializedPropertyType.LayerMask ||
            type == SerializedPropertyType.ObjectReference ||
            type == SerializedPropertyType.Rect ||
            type == SerializedPropertyType.Vector2 ||
            type == SerializedPropertyType.Vector3;
        }
    }
}
#endif