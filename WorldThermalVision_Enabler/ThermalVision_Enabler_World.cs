using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
    public class ThermalVision_Enabler_World : MonoBehaviour
    {
        public float ThermalDistribution = 4f;
        public float MaximumTemperature = 0.15f;
        public float MinimumTemperature = 0.001f;

        [HideInInspector]
        public static List<ThermalVision_Enabler_World> ThermalVisionsInScene;
        private string rootWorldObjectName = "[Level]";

        private GameObject rootWorldObject;
        private ThermalBody thermalBody = null;

#if !DEBUG
        public void Start()
        {
            if (ThermalVisionsInScene == null) ThermalVisionsInScene = new List<ThermalVision_Enabler_World>();

            ThermalVisionsInScene.Add(this);

            rootWorldObject = GameObject.Find(rootWorldObjectName);
            GameObject[] rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            if (rootWorldObject != null && rootWorldObject.GetComponent<ThermalBody>() == null)
            {
                rootWorldObject.SetActive(false);
                thermalBody = rootWorldObject.AddComponent<ThermalBody>();

                thermalBody.ThermalDistribution = this.ThermalDistribution;
                thermalBody.MaximumTemperature = this.MaximumTemperature;
                thermalBody.MinimumTemperature = this.MinimumTemperature;

                rootWorldObject.SetActive(true);
            }
            else if (rootWorldObject == null)
            {
                rootWorldObject = new GameObject(rootWorldObjectName);
                foreach (GameObject gameObject in rootGameObjects)
                {
                    Rigidbody RB = gameObject.GetComponent<Rigidbody>();
                    Sosig sosig = gameObject.GetComponent<Sosig>();

                    if (RB == null && sosig == null) gameObject.transform.parent = rootWorldObject.transform;
                }

                rootWorldObject.SetActive(false);
                thermalBody = rootWorldObject.AddComponent<ThermalBody>();

                thermalBody.ThermalDistribution = this.ThermalDistribution;
                thermalBody.MaximumTemperature = this.MaximumTemperature;
                thermalBody.MinimumTemperature = this.MinimumTemperature;

                rootWorldObject.SetActive(true);
            }

            List<GameObject> sosigs = new List<GameObject>();
            List<GameObject> rigidbodies = new List<GameObject>();
            foreach (GameObject gameObject in rootGameObjects)
            {
                Rigidbody RB = gameObject.GetComponent<Rigidbody>();
                Sosig sosig = gameObject.GetComponent<Sosig>();

                if (RB != null && sosig == null) rigidbodies.Add(RB.gameObject);
                else if (sosig != null) sosigs.Add(sosig.gameObject);
            }

            foreach (var rigidbody in rigidbodies)
            {
                if (rigidbody.GetComponent<ThermalBody>() != null) continue;
                rigidbody.SetActive(false);
                thermalBody = rigidbody.AddComponent<ThermalBody>();

                thermalBody.ThermalDistribution = ThermalBody_Hooks.physicalObject_tempDist;
                thermalBody.MaximumTemperature = ThermalBody_Hooks.physicalObject_maxTemp;
                thermalBody.MinimumTemperature = ThermalBody_Hooks.physicalObject_minTemp;

                rigidbody.SetActive(true);
            }
            foreach (var sosig in sosigs)
            {
                if (sosig.GetComponent<ThermalBody>() != null) continue;
                sosig.SetActive(false);
                thermalBody = sosig.AddComponent<ThermalBody>();

                thermalBody.ThermalDistribution = ThermalBody_Hooks.sosig_tempDist;
                thermalBody.MaximumTemperature = ThermalBody_Hooks.sosig_maxTemp;
                thermalBody.MinimumTemperature = ThermalBody_Hooks.sosig_minTemp;

                sosig.SetActive(true);
            }


            if (!ThermalBody_Hooks.IsHooked)
            {
                if (ThermalBody_BepInEx.ThermalPlugin.enableThermalVision.Value) ThermalBody_BepInEx.ThermalHooks.Hook(ThermalBody_BepInEx.ThermalPlugin.TemperatureData);
                else ThermalBody_BepInEx.ThermalHooks.EssentialsHook(ThermalBody_BepInEx.ThermalPlugin.TemperatureData);
            }
        }

        void OnDestroy()
        {
            ThermalVisionsInScene.Remove(this);

            if (ThermalVisionsInScene.Count == 0)
            {
                ThermalBody_BepInEx.ThermalHooks.Unhook();
            }
        }
#endif
    }
}
