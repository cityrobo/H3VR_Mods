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
                bool wasActive = rootWorldObject.activeSelf;

                rootWorldObject.SetActive(false);
                thermalBody = rootWorldObject.AddComponent<ThermalBody>();

                thermalBody.ThermalDistribution = this.ThermalDistribution;
                thermalBody.MaximumTemperature = this.MaximumTemperature;
                thermalBody.MinimumTemperature = this.MinimumTemperature;

                rootWorldObject.SetActive(wasActive);
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
                bool wasActive = rigidbody.gameObject.activeSelf;

                rigidbody.gameObject.SetActive(false);
                thermalBody = rigidbody.AddComponent<ThermalBody>();

                thermalBody.ThermalDistribution = ThermalVision_Hooks.physicalObject_tempDist;
                thermalBody.MaximumTemperature = ThermalVision_Hooks.physicalObject_maxTemp;
                thermalBody.MinimumTemperature = ThermalVision_Hooks.physicalObject_minTemp;

                rigidbody.gameObject.SetActive(wasActive);
            }
            foreach (var sosig in sosigs)
            {
                if (sosig.GetComponent<ThermalBody>() != null) continue;

                bool wasActive = sosig.gameObject.activeSelf;
                sosig.gameObject.SetActive(false);
                thermalBody = sosig.AddComponent<ThermalBody>();

                thermalBody.ThermalDistribution = ThermalVision_Hooks.sosig_tempDist;
                thermalBody.MaximumTemperature = ThermalVision_Hooks.sosig_maxTemp;
                thermalBody.MinimumTemperature = ThermalVision_Hooks.sosig_minTemp;

                sosig.gameObject.SetActive(wasActive);
            }


            if (!ThermalVision_Hooks.IsHooked)
            {
                if (ThermalVision_BepInEx.ThermalPlugin.enableThermalVision.Value) ThermalVision_BepInEx.ThermalHooks.Hook(ThermalVision_BepInEx.ThermalPlugin.TemperatureData);
                else ThermalVision_BepInEx.ThermalHooks.EssentialsHook(ThermalVision_BepInEx.ThermalPlugin.TemperatureData);
            }
        }

        void OnDestroy()
        {
            ThermalVisionsInScene.Remove(this);

            if (ThermalVisionsInScene.Count == 0)
            {
                ThermalVision_BepInEx.ThermalHooks.Unhook();
            }
        }
#endif
    }
}
