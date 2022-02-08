using System.Collections;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
    [RequireComponent(typeof(ThermalBody))]
    public class CoolingDownPart : MonoBehaviour
    {
        public float heatDissipatedPerSecond = 0.1f;
        public float startingHeat;

        private float heat;

        public ThermalBody tB;
        private float tbMaxTemp;
        private float tbMinTemp;

        private bool wasEnabled = false;
#if!DEBUG
        public void OnEnable()
        {
            if (tB == null) tB = this.gameObject.GetComponent<ThermalBody>();
            tB.enabled = false;
            tB.isVariable = true;
            tB.enabled = true;
            tbMaxTemp = tB.MaximumTemperature;
            tbMinTemp = tB.MinimumTemperature;
            heat = startingHeat;
            wasEnabled = true;
        }

        public void OnDisable()
        {
            heat = tbMaxTemp;
            if (tB != null)
            {
                tB.MaximumTemperature = heat;
                tB.UpdateSubMaterialProperties();
            }
            wasEnabled = false;
        }

        void Update()
        {
            if (wasEnabled && heat > tbMaxTemp)
            {
                tB.MaximumTemperature = heat;
                tB.UpdateSubMaterialProperties();
                heat -= Time.deltaTime * heatDissipatedPerSecond;
                if (heat < tbMaxTemp) heat = tbMaxTemp;
                if (heat > 1f)
                {
                    tB.MinimumTemperature = heat - 1f + tbMinTemp;
                }
                else tB.MinimumTemperature = tbMinTemp;
            }
        }
#endif
    }
}