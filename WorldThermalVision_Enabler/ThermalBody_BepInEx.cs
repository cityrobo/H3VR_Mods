#if !DEBUG
using BepInEx;
using BepInEx.Configuration;
using Deli.Setup;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.openscripts.thermalvision", "ThermalVision Enabler", "1.2.0")]
    public class ThermalBody_BepInEx : BaseUnityPlugin
    {
        private readonly ThermalBody_Hooks _hooks;

        private ThermalBody_Hooks.TemperatureData temperatureData;

        private ConfigEntry<float> sosig_ThermalDistribution;
        private ConfigEntry<float> sosig_MaximumTemperature;
        private ConfigEntry<float> sosig_MinimumTemperature;
        private ConfigEntry<float> physicalObject_ThermalDistribution;
        private ConfigEntry<float> physicalObject_MaximumTemperature;
        private ConfigEntry<float> physicalObject_MinimumTemperature;
        private ConfigEntry<float> muzzleDevice_HeatingRate;
        private ConfigEntry<float> muzzleDevice_CoolingRate;

        public ThermalBody_BepInEx()
        {
            Logger.LogInfo("ThermalBody Plugin loaded!");

            sosig_ThermalDistribution = Config.Bind<float>("Sosig ThemalBody Settings", "ThermalDistribution", 8f, "Thermal Distribution of Sosig Bodies (between 0 and 8)");
            sosig_MaximumTemperature = Config.Bind<float>("Sosig ThemalBody Settings", "MaximumTemperature", 0.3f, "Maximum Temperature of Sosig Bodies (between 0 and 1)");
            sosig_MinimumTemperature = Config.Bind<float>("Sosig ThemalBody Settings", "MinimumTemperature", 0.6f, "Minimum Temperature of Sosig Bodies (between 0 and 1)");

            physicalObject_ThermalDistribution = Config.Bind<float>("PhysicalObject ThemalBody Settings", "ThermalDistribution", 4f, "Thermal Distribution of Objects in game (between 0 and 8)");
            physicalObject_MaximumTemperature = Config.Bind<float>("PhysicalObject ThemalBody Settings", "MaximumTemperature", 0.15f, "Maximum Temperature of Objects in game (between 0 and 1)");
            physicalObject_MinimumTemperature = Config.Bind<float>("PhysicalObject ThemalBody Settings", "MinimumTemperature", 0.001f, "Minimum Temperature of Objects in game (between 0 and 1)");

            muzzleDevice_HeatingRate = Config.Bind<float>("Muzzle Device Heating Settings", "Heat per Shot", 0.1f, "Heat gained per Shot");
            muzzleDevice_CoolingRate = Config.Bind<float>("Muzzle Device Heating Settings", "Cooling per Second", 0.1f, "Heat cooled off per Second");

            temperatureData.sosig_tempDist = sosig_ThermalDistribution.Value;
            temperatureData.sosig_maxTemp = sosig_MaximumTemperature.Value;
            temperatureData.sosig_minTemp = sosig_MinimumTemperature.Value;

            temperatureData.physicalObject_tempDist = physicalObject_ThermalDistribution.Value;
            temperatureData.physicalObject_maxTemp = physicalObject_MaximumTemperature.Value;
            temperatureData.physicalObject_minTemp = physicalObject_MinimumTemperature.Value;

            temperatureData.muzzleDevice_HeatingRate = muzzleDevice_HeatingRate.Value;
            temperatureData.muzzleDevice_CoolingRate = muzzleDevice_CoolingRate.Value;

            _hooks = new ThermalBody_Hooks();

            _hooks.Hook(temperatureData);
        }

        private void OnDestroy()
        {
            _hooks?.Unhook();
        }

    }
}
#endif
