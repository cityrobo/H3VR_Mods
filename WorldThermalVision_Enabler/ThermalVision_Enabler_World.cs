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

        public string rootWorldObjectName = "[Level]";
        private GameObject rootWorldObject;
        private ThermalBody thermalBody = null;

#if !DEBUG
        public void Start()
        {
            rootWorldObject = GameObject.Find(rootWorldObjectName);

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

                List<GameObject> rootGameObjects = new List<GameObject>(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());
                foreach (GameObject gameObject in rootGameObjects)
                {
                    if (gameObject.GetComponent<Rigidbody>() == null && gameObject.GetComponent<Sosig>() == null) gameObject.transform.parent = rootWorldObject.transform;
                }

                rootWorldObject.SetActive(false);
                thermalBody = rootWorldObject.AddComponent<ThermalBody>();

                thermalBody.ThermalDistribution = this.ThermalDistribution;
                thermalBody.MaximumTemperature = this.MaximumTemperature;
                thermalBody.MinimumTemperature = this.MinimumTemperature;

                rootWorldObject.SetActive(true);
            }
        }
#endif
    }
}
