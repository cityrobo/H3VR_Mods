#if !DEBUG
using BepInEx;
using BepInEx.Configuration;
using Deli.Setup;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.thermalvision", "ThermalVision Enabler", "1.2.0")]
    public class ThermalBody_BepInEx : BaseUnityPlugin
    {
        public static ThermalBody_Hooks ThermalHooks;
        public static ThermalBody_BepInEx ThermalPlugin;

        public ThermalBody_Hooks.TemperatureData TemperatureData;

        public ConfigEntry<bool> enableThermalVision;
        private ConfigEntry<float> sosig_ThermalDistribution;
        private ConfigEntry<float> sosig_MaximumTemperature;
        private ConfigEntry<float> sosig_MinimumTemperature;
        private ConfigEntry<bool> encryption_hot;
        private ConfigEntry<float> encryption_ThermalDistributionHot;
        private ConfigEntry<float> encryption_MaximumTemperatureHot;
        private ConfigEntry<float> encryption_MinimumTemperatureHot;
        private ConfigEntry<float> physicalObject_ThermalDistribution;
        private ConfigEntry<float> physicalObject_MaximumTemperature;
        private ConfigEntry<float> physicalObject_MinimumTemperature;
        private ConfigEntry<float> muzzleDevice_HeatingRate;
        private ConfigEntry<float> muzzleDevice_CoolingRate;

        public ThermalBody_BepInEx()
        {
            Logger.LogInfo("ThermalBody Plugin loaded!");

            ThermalBody_BepInEx.ThermalPlugin = this;

            enableThermalVision = Config.Bind<bool>("Thermal Vision Plugin Settings", "Enable Full Thermal Vision Hooks", true, "This enables most of the heavy code hooks that add features like heating casings and such. Sosigs, Items and Encryptions still get thermals.");
            sosig_ThermalDistribution = Config.Bind<float>("Sosig ThermalBody Settings", "ThermalDistribution", 8f, "Thermal Distribution of Sosig Bodies (between 0 and 8)");
            sosig_MaximumTemperature = Config.Bind<float>("Sosig ThermalBody Settings", "MaximumTemperature", 0.3f, "Maximum Temperature of Sosig Bodies (between 0 and 1)");
            sosig_MinimumTemperature = Config.Bind<float>("Sosig ThermalBody Settings", "MinimumTemperature", 0.6f, "Minimum Temperature of Sosig Bodies (between 0 and 1) (yes, this is higher than the max temp because IDK it looks better in game)");

            encryption_hot = Config.Bind<bool>("Encryption ThermalBody Settings", "Encryptions are Hot", true, "if true, encryptions will appear hot in thermal vision.");
            encryption_ThermalDistributionHot = Config.Bind<float>("Encryption ThermalBody Settings", "ThermalDistribution", 8f, "Thermal Distribution of Encryptions (between 0 and 8)");
            encryption_MaximumTemperatureHot = Config.Bind<float>("Encryption ThermalBody Settings", "MaximumTemperature", 0.3f, "Maximum Temperature of Encryptions (between 0 and 1)");
            encryption_MinimumTemperatureHot = Config.Bind<float>("Encryption ThermalBody Settings", "MinimumTemperature", 0.6f, "Minimum Temperature of Encryptions (between 0 and 1) (yes, this is higher than the max temp because IDK it looks better in game)");


            physicalObject_ThermalDistribution = Config.Bind<float>("PhysicalObject ThermalBody Settings", "ThermalDistribution", 4f, "Thermal Distribution of Objects in game (between 0 and 8)");
            physicalObject_MaximumTemperature = Config.Bind<float>("PhysicalObject ThermalBody Settings", "MaximumTemperature", 0.15f, "Maximum Temperature of Objects in game (between 0 and 1)");
            physicalObject_MinimumTemperature = Config.Bind<float>("PhysicalObject ThermalBody Settings", "MinimumTemperature", 0.001f, "Minimum Temperature of Objects in game (between 0 and 1)");

            
            muzzleDevice_HeatingRate = Config.Bind<float>("Muzzle Device Heating Settings", "Heat per Shot", 0.1f, "Heat gained per Shot");
            muzzleDevice_CoolingRate = Config.Bind<float>("Muzzle Device Heating Settings", "Cooling per Second", 0.1f, "Heat cooled off per Second");

            TemperatureData.sosig_tempDist = sosig_ThermalDistribution.Value;
            TemperatureData.sosig_maxTemp = sosig_MaximumTemperature.Value;
            TemperatureData.sosig_minTemp = sosig_MinimumTemperature.Value;

            TemperatureData.encryption_hot = encryption_hot.Value;
            TemperatureData.encryption_tempDistHot = encryption_ThermalDistributionHot.Value;
            TemperatureData.encryption_maxTempHot = encryption_MaximumTemperatureHot.Value;
            TemperatureData.encryption_minTempHot = encryption_MinimumTemperatureHot.Value;

            TemperatureData.physicalObject_tempDist = physicalObject_ThermalDistribution.Value;
            TemperatureData.physicalObject_maxTemp = physicalObject_MaximumTemperature.Value;
            TemperatureData.physicalObject_minTemp = physicalObject_MinimumTemperature.Value;

            TemperatureData.muzzleDevice_HeatingRate = muzzleDevice_HeatingRate.Value;
            TemperatureData.muzzleDevice_CoolingRate = muzzleDevice_CoolingRate.Value;

            if (enableThermalVision.Value)
            {
                ThermalHooks = new ThermalBody_Hooks();

                //ThermalHooks.Hook(TemperatureData);
            }
            else
            {
                ThermalHooks = new ThermalBody_Hooks();
                //ThermalHooks.EssentialsHook(TemperatureData);
            }
        }

        private void OnDestroy()
        {
            ThermalHooks?.Unhook();
        }

    }
}
#endif
