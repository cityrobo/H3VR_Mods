using BepInEx;
using BepInEx.Configuration;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.WindSystem", "Wind System", "1.0.0")]
    public class WindSystem : BaseUnityPlugin
    {
        public static Vector3 WindVelocityVector = Vector3.zero;
        public static Vector3 WindDirection => WindVelocityVector.normalized;
        public static float WindSpeed => WindVelocityVector.magnitude;

        public static ConfigEntry<float> WindChangeFrequency;
        public static ConfigEntry<float> WindChangeVariationStrength;
        public static ConfigEntry<float> MaximumWindSpeed;

        private float xRandom = UnityEngine.Random.value;
        private float zRandom = UnityEngine.Random.value;

        public void Awake()
        {
            WindChangeFrequency = Config.Bind("Wind System", "Wind Change Frequency", 0.001f, "Defines how quickly the wind changes as a whole, so both wind speed and direction.");
            WindChangeVariationStrength = Config.Bind("Wind System", "Wind Change Variation Strength", 1f, "Defines how much the wind direction varies.");
            MaximumWindSpeed = Config.Bind("Wind System", "Maximum Wind Speed", 30f);

            // Randomization at start doesn't really work cause the current value depends on the PerlinNoise and Time.time value entirely.
            //WindForce = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f));

            //WindForce = WindDirection * UnityEngine.Random.Range(0f, MaximumWindSpeed.Value);

            Harmony.CreateAndPatchAll(typeof(WindSystem));

            StartCoroutine(WindDebugReadout());
        }

        public void Update()
        {
            // Generate change over time in wind direction using Perlin noise. The average value of the Perlin noise method isn't actually 0.5 but closer to 0.4666. this is important because else the wind direction would be biased towards one quadrant.
            float biasAdjustment = 0.4666f;
            float xChange = (Mathf.PerlinNoise(Time.time * WindChangeFrequency.Value + xRandom, 0f) - biasAdjustment) * WindChangeVariationStrength.Value;
            float zChange = (Mathf.PerlinNoise(0f, Time.time * WindChangeFrequency.Value + zRandom) - biasAdjustment) * WindChangeVariationStrength.Value;
            Vector3 totalChange = new (xChange, 0f, zChange);

            // Calculate final wind direction with current change over time.
            Vector3 newWindDirection = WindDirection + totalChange;

            // Clamp wind direction to XZ plane, just to be sure
            newWindDirection.y = 0;

            // Randomize wind speed using Perlin noise
            float windSpeed = Mathf.PerlinNoise(Time.time * WindChangeFrequency.Value - xRandom, Time.time * WindChangeFrequency.Value - zRandom) * MaximumWindSpeed.Value;

            // Calculate new wind force vector
            WindVelocityVector = newWindDirection.normalized * windSpeed;
        }

        private Vector3 WindForce(Vector3 velocityVector, float airDensity, float deltaTime, float frontArea, float Mass)
        {
            // get scalar value for the velocity. basic stuff. v = sqrt(vector_v_x^2 + vector_v_y^2 + vector_v_z^2)
            float velocity = velocityVector.magnitude;
            // normalized velocity vector, meaning length = 1. basic stuff, again. vector_v_dir = vector_v / v
            Vector3 velocityDirection = velocityVector.normalized;
            float currentDragCoefficient = GetCurrentDragCoefficient(velocityVector.magnitude);

            // Vector_F_Drag = 1/2 * C_D * rho * A * v * vector_v;
            // C_D is current drag coefficient
            // rho is density of the fluid (air in this case)
            // A is cross-sectional area of the object
            // v is the velocity value (magnitude of velocity vector)
            // vector_v is the velocity vector
            // Drag force is always opposing the velocity vector: F_Drag = 1/2 * C_D * rho * A * v^2 -> Vector_F_Drag = - F_Drag * vector_v_normalized = - F_Drag * vector_v / v
            // the division of the front area through the mass of the bullet is likely done to differentiate the impact of drag on different bullet types with the same cross-sectional area, like .50BMG and .50AE

            Vector3 windForceVector = -(0.5f * currentDragCoefficient * airDensity * (frontArea / Mass) * velocity) * velocityVector;

            // IDK what Anton thought when he did this, really weird way to do it and not very efficient either, turning the dragForceVector back into a scalar just to turn everything back into a vector
            return velocityDirection * Mathf.Clamp(velocity - windForceVector.magnitude * deltaTime, 0f, velocity);

            // this should work just as well and doesn't require square roots for the magnitude calculation (again) either. (the normalized velocityDirection does though)
            // float dragForce = 0.5f * currentDragCoefficient * materialDensity * (frontArea / Mass) * velocity * velocity;
            // float velocityChange = dragForce * deltaTime;
            // return velocityDirection * Mathf.Clamp(velocity - velocityChange, 0f, velocity);
        }

        private float GetCurrentDragCoefficient(float velocityInMS)
        {
            // Convert m/s to speed in Mach number (fraction of speed of sound)
            float velocityInMach = velocityInMS * 0.00291545f;
            return AM.BDCC.Evaluate(velocityInMach);
        }

        private IEnumerator WindDebugReadout()
        {
            while (true)
            {
                Logger.LogInfo($"Wind Speed: {WindSpeed}m/s");
                Vector2 windDirection = new Vector2(WindDirection.x, WindDirection.z);
                Vector2 north = Vector2.up;

                float angle = Mathf.Atan2(north.y * windDirection.x - north.x * windDirection.y, north.x * windDirection.x + north.y * windDirection.y) * Mathf.Rad2Deg;
                if (angle < 0f) angle += 360f;
                Logger.LogInfo($"Wind Direction: {angle}°");
                yield return new WaitForSeconds(2f);
            }
        }

        [HarmonyPatch(typeof(BallisticProjectile), nameof(BallisticProjectile.UpdateVelocity))]
        [HarmonyPostfix]
        public static void BallisticProjectile_UpdateVelocity_Patch(BallisticProjectile __instance, float t)
        {
            __instance.m_velocity += WindVelocityVector / __instance.Mass * t;
        }

#if !DEBUG

#endif
    }
}
