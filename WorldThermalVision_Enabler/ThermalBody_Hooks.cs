using UnityEngine;
namespace Cityrobo
{
    public class ThermalBody_Hooks
    {
        public struct TemperatureData
        {
            public float sosig_tempDist;
            public float sosig_maxTemp;
            public float sosig_minTemp;

            public bool encryption_hot;
            public float encryption_tempDistHot;
            public float encryption_maxTempHot;
            public float encryption_minTempHot;

            public float physicalObject_tempDist;
            public float physicalObject_maxTemp;
            public float physicalObject_minTemp;

            public float muzzleDevice_HeatingRate;
            public float muzzleDevice_CoolingRate;
        }

        private TemperatureData temperatureData;
        private bool essentialsOnly = false;

        public static float sosig_tempDist;
        public static float sosig_maxTemp;
        public static float sosig_minTemp;

        public static float physicalObject_tempDist;
        public static float physicalObject_maxTemp;
        public static float physicalObject_minTemp;

        public static bool IsHooked;

#if !DEBUG
        public void Unhook()
        {
            if (essentialsOnly)
            {
                On.FistVR.Sosig.Start -= Sosig_Start;
                On.FistVR.FVRPhysicalObject.Awake -= FVRPhysicalObject_Awake;
                On.FistVR.TNH_EncryptionTarget.Start -= TNH_EncryptionTarget_Start;
            }
            else
            {
                On.FistVR.Sosig.Start -= Sosig_Start;
                On.FistVR.FVRPhysicalObject.Awake -= FVRPhysicalObject_Awake;
                On.FistVR.TNH_EncryptionTarget.Start -= TNH_EncryptionTarget_Start;
                On.FistVR.MuzzleDevice.Awake -= MuzzleDevice_Awake;
                On.FistVR.FVRFireArmRound.FVRUpdate -= FVRFireArmRound_FVRUpdate;
                On.FistVR.FVRFireArmChamber.UpdateProxyDisplay -= FVRFireArmChamber_UpdateProxyDisplay;
                On.FistVR.FVRFireArmMagazine.UpdateBulletDisplay -= FVRFireArmMagazine_UpdateBulletDisplay;
            }

            IsHooked = false;
        }
        public void Hook(TemperatureData data)
        {
            this.temperatureData = data;

            On.FistVR.Sosig.Start += Sosig_Start;
            On.FistVR.FVRPhysicalObject.Awake += FVRPhysicalObject_Awake;
            On.FistVR.TNH_EncryptionTarget.Start += TNH_EncryptionTarget_Start;
            On.FistVR.MuzzleDevice.Awake += MuzzleDevice_Awake;
            On.FistVR.FVRFireArmRound.FVRUpdate += FVRFireArmRound_FVRUpdate;
            On.FistVR.FVRFireArmChamber.UpdateProxyDisplay += FVRFireArmChamber_UpdateProxyDisplay;
            On.FistVR.FVRFireArmMagazine.UpdateBulletDisplay += FVRFireArmMagazine_UpdateBulletDisplay;

            ApplyStaticValues();
            IsHooked = true;
        }

        public void EssentialsHook(TemperatureData data)
        {
            this.temperatureData = data;

            On.FistVR.Sosig.Start += Sosig_Start;
            On.FistVR.FVRPhysicalObject.Awake += FVRPhysicalObject_Awake;
            On.FistVR.TNH_EncryptionTarget.Start += TNH_EncryptionTarget_Start;

            essentialsOnly = true;

            ApplyStaticValues();
            IsHooked = true;
        }

        void ApplyStaticValues()
        {
            ThermalBody_Hooks.sosig_tempDist = temperatureData.sosig_tempDist;
            ThermalBody_Hooks.sosig_maxTemp = temperatureData.sosig_maxTemp;
            ThermalBody_Hooks.sosig_minTemp = temperatureData.sosig_minTemp;

            ThermalBody_Hooks.physicalObject_tempDist = temperatureData.physicalObject_tempDist;
            ThermalBody_Hooks.physicalObject_maxTemp = temperatureData.physicalObject_maxTemp;
            ThermalBody_Hooks.physicalObject_minTemp = temperatureData.physicalObject_minTemp;
        }


        private void TNH_EncryptionTarget_Start(On.FistVR.TNH_EncryptionTarget.orig_Start orig, FistVR.TNH_EncryptionTarget self)
        {
            orig(self);
            ThermalBody tB = self.gameObject.GetComponent<ThermalBody>();
            if (temperatureData.encryption_hot)
            {
                if (tB == null)
                {
                    tB = self.gameObject.AddComponent<ThermalBody>();
                    tB.enabled = false;
                    tB.ThermalDistribution = temperatureData.encryption_tempDistHot;
                    tB.MaximumTemperature = temperatureData.encryption_maxTempHot;
                    tB.MinimumTemperature = temperatureData.encryption_minTempHot;
                    tB.enabled = true;
                }
                else
                {
                    tB.enabled = false;
                    tB.ThermalDistribution = temperatureData.encryption_tempDistHot;
                    tB.MaximumTemperature = temperatureData.encryption_maxTempHot;
                    tB.MinimumTemperature = temperatureData.encryption_minTempHot;
                    tB.enabled = true;
                }
            }
            else
            {
                if (tB == null)
                {
                    tB = self.gameObject.AddComponent<ThermalBody>();
                    tB.enabled = false;
                    tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                    tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                    tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                    tB.enabled = true;
                }
                else
                {
                    tB.enabled = false;
                    tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                    tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                    tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                    tB.enabled = true;
                }
            }
        }

        private void FVRFireArmMagazine_UpdateBulletDisplay(On.FistVR.FVRFireArmMagazine.orig_UpdateBulletDisplay orig, FistVR.FVRFireArmMagazine self)
        {
            orig(self);
            /*foreach (var displayBullet in self.DisplayBullets)
            {
                if (displayBullet != null && displayBullet.activeSelf)
                {
                    ThermalBody tB = displayBullet.GetComponent<ThermalBody>();
                    if (tB == null)
                    {
                        tB = displayBullet.gameObject.AddComponent<ThermalBody>();
                        tB.enabled = false;
                        tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                        tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                        tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                        tB.enabled = true;
                    }
                    else
                    {
                        tB.enabled = false;
                        tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                        tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                        tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                        tB.enabled = true;
                    }
                }
            }*/

            ThermalBody tB = self.gameObject.GetComponent<ThermalBody>();
            if (tB == null)
            {
                tB = self.gameObject.AddComponent<ThermalBody>();
                tB.enabled = false;
                tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                tB.enabled = true;
            }
            else
            {
                tB.enabled = false;
                tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                tB.enabled = true;
            }
        }

        private void FVRFireArmChamber_UpdateProxyDisplay(On.FistVR.FVRFireArmChamber.orig_UpdateProxyDisplay orig, FistVR.FVRFireArmChamber self)
        {
            orig(self);
            if (self.IsSpent == true)
            {
                ThermalBody tB = self.ProxyMesh.gameObject.GetComponent<ThermalBody>();
                if (tB == null)
                {
                    tB = self.gameObject.AddComponent<ThermalBody>();
                    tB.enabled = false;
                    tB.isVariable = true;
                    tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                    tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                    tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                    tB.enabled = true;
                }
                else
                {
                    tB.enabled = false;
                    tB.isVariable = true;
                    tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                    tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                    tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                    tB.enabled = true;
                }

                CoolingDownPart coolingDownPart = self.ProxyMesh.gameObject.GetComponent<CoolingDownPart>();

                if (coolingDownPart == null)
                {
                    coolingDownPart = self.gameObject.AddComponent<CoolingDownPart>();
                    coolingDownPart.tB = tB;
                    coolingDownPart.enabled = false;
                    coolingDownPart.startingHeat = 3f;
                    coolingDownPart.heatDissipatedPerSecond = 0.4f;
                    coolingDownPart.enabled = true;
                }
            }
        }

        private void FVRFireArmRound_FVRUpdate(On.FistVR.FVRFireArmRound.orig_FVRUpdate orig, FistVR.FVRFireArmRound self)
        {
            orig(self);
            if (self.m_isSpent == true)
            {
                ThermalBody tB = self.gameObject.GetComponent<ThermalBody>();
                if (tB == null)
                {
                    tB = self.gameObject.AddComponent<ThermalBody>();
                    tB.enabled = false;
                    tB.isVariable = true;
                    tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                    tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                    tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                    tB.enabled = true;
                }
                else
                {
                    tB.enabled = false;
                    tB.isVariable = true;
                    tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                    tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                    tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                    tB.enabled = true;
                }

                CoolingDownPart coolingDownPart = self.gameObject.GetComponent<CoolingDownPart>();

                if (coolingDownPart == null)
                {
                    coolingDownPart = self.gameObject.AddComponent<CoolingDownPart>();
                    coolingDownPart.tB = tB;
                    coolingDownPart.enabled = false;
                    coolingDownPart.startingHeat = 3f;
                    coolingDownPart.heatDissipatedPerSecond = 0.4f;
                    coolingDownPart.enabled = true;
                }
            }
        }
        private void MuzzleDevice_Awake(On.FistVR.MuzzleDevice.orig_Awake orig, FistVR.MuzzleDevice self)
        {
            orig(self);

            ThermalBody tB = self.gameObject.GetComponent<ThermalBody>();

            if (tB == null)
            {
                tB = self.gameObject.AddComponent<ThermalBody>();
                tB.enabled = false;
                tB.isVariable = true;
                tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                tB.enabled = true;
            }
            else
            {
                tB.enabled = false;
                tB.isVariable = true;
                tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                tB.enabled = true;
            }
            HeatingUpAttachmentPart heatingUpAttachmentPart = self.gameObject.GetComponent<HeatingUpAttachmentPart>();

            if (heatingUpAttachmentPart == null)
            {
                heatingUpAttachmentPart = self.gameObject.AddComponent<HeatingUpAttachmentPart>();
                heatingUpAttachmentPart.enabled = false;
                heatingUpAttachmentPart.attachment = self;
                heatingUpAttachmentPart.heatPerShot = temperatureData.muzzleDevice_HeatingRate;
                heatingUpAttachmentPart.heatDissipatedPerSecond = temperatureData.muzzleDevice_CoolingRate;
                heatingUpAttachmentPart.enabled = true;
            }
        }
        //Add ThermalBody to all Sosig Links
        private void Sosig_Start(On.FistVR.Sosig.orig_Start orig, global::FistVR.Sosig self)
        {
            orig(self);
            if (self.DeParentOnSpawn.gameObject.GetComponent<ThermalBody>() == null)
            {
                ThermalBody tB = self.DeParentOnSpawn.gameObject.AddComponent<ThermalBody>();
                tB.enabled = false;
                tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                tB.enabled = true;
            }
            else
            {
                ThermalBody tB = self.DeParentOnSpawn.gameObject.GetComponent<ThermalBody>();
                tB.enabled = false;
                tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                tB.enabled = true;
            }
            foreach (var link in self.Renderers)
            {
                ThermalBody tB = link.gameObject.GetComponent<ThermalBody>();
                if (tB == null)
                {
                    tB = link.gameObject.AddComponent<ThermalBody>();
                    tB.enabled = false;
                    tB.ThermalDistribution = temperatureData.sosig_tempDist;
                    tB.MaximumTemperature = temperatureData.sosig_maxTemp;
                    tB.MinimumTemperature = temperatureData.sosig_minTemp;
                    tB.enabled = true;
                }
                else
                {
                    tB.enabled = false;
                    tB.ThermalDistribution = temperatureData.sosig_tempDist;
                    tB.MaximumTemperature = temperatureData.sosig_maxTemp;
                    tB.MinimumTemperature = temperatureData.sosig_minTemp;
                    tB.enabled = true;
                }
            }
            //Debug.Log("Sosig Thermal Body Enabled!");
        }
        //Add ThermalBody to all FVRPhysicalObjects
        private void FVRPhysicalObject_Awake(On.FistVR.FVRPhysicalObject.orig_Awake orig, FistVR.FVRPhysicalObject self)
        {
            orig(self);
            if (self.gameObject.GetComponent<ThermalBody>() == null)
            {
                self.gameObject.SetActive(false);
                ThermalBody tB = self.gameObject.AddComponent<ThermalBody>();
                tB.enabled = false;
                tB.ThermalDistribution = temperatureData.physicalObject_tempDist;
                tB.MaximumTemperature = temperatureData.physicalObject_maxTemp;
                tB.MinimumTemperature = temperatureData.physicalObject_minTemp;
                tB.enabled = true;
                self.gameObject.SetActive(true);
            }
        }
#endif
    }
}
