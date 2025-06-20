using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using BepInEx.Configuration;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.StockPosConfigurator", "StockPosition Configurator", "1.1.0")]
    public class StockPosConfigurator : BaseUnityPlugin
    {
        private static ConfigEntry<float> s_stockPosX;
        private static ConfigEntry<float> s_stockPosY;
        private static ConfigEntry<float> s_stockPosZ;

        private static ConfigEntry<List<string>> s_firearmBlackList;

        private static Vector3 s_currentPosChange = Vector3.zero;

        static StockPosConfigurator()
        {
            On.FistVR.FVRPhysicalObject.Awake += FVRPhysicalObject_Awake;
        }

        public StockPosConfigurator()
        {
            s_stockPosX = Config.Bind("StockPosition Configurator", "X Stock Position Modification", 0f, "Left and right stock position adjustment. Positive numbers mean that the stock point will be shifted to the right. You should leave this value at 0, unless you like an asymetrical stock. Value is in meters.");
            s_stockPosY = Config.Bind("StockPosition Configurator", "Y Stock Position Modification", 0f, "Up and down stock position adjustment. Positive numbers mean that the stock point will be shifted upwards. This will make the stock feel taller. Value is in meters.");
            s_stockPosZ = Config.Bind("StockPosition Configurator", "Z Stock Position Modification", 0f, "Forward and back stock position adjustment. Positive numbers mean that the stock point will be shifted forwards, away from the shooter. This will make the stock feel shorter. Value is in meters.");

            s_firearmBlackList = Config.Bind("StockPosition Configurator", "Firearm blacklist", new List<string>(), "Any ItemID put in here will be excluded from the stock position change.");

            s_currentPosChange = new Vector3(s_stockPosX.Value, s_stockPosY.Value, s_stockPosZ.Value);

            s_stockPosX.SettingChanged += SettingsChanged;
            s_stockPosY.SettingChanged += SettingsChanged;
            s_stockPosZ.SettingChanged += SettingsChanged;
        }

        public void OnDestroy()
        {
            s_stockPosX.SettingChanged -= SettingsChanged;
            s_stockPosY.SettingChanged -= SettingsChanged;
            s_stockPosZ.SettingChanged -= SettingsChanged;
        }

        private static void FVRPhysicalObject_Awake(On.FistVR.FVRPhysicalObject.orig_Awake orig, FVRPhysicalObject self)
        {
            orig(self);

            if (self.ObjectWrapper != null && s_firearmBlackList.Value.Contains(self.ObjectWrapper.ItemID)) return;
            Transform stockPos = self.GetStockPos();
            if (stockPos != null) stockPos.localPosition += s_currentPosChange;
        }

        private static void SettingsChanged(object sender, EventArgs e)
        {
            FVRPhysicalObject[] physicalObjectsInScene = FindObjectsOfType<FVRPhysicalObject>();

            foreach (var physicalObject in physicalObjectsInScene)
            {
                if (physicalObject.ObjectWrapper != null && s_firearmBlackList.Value.Contains(physicalObject.ObjectWrapper.ItemID)) continue;
                Transform stockPos = physicalObject.GetStockPos();
                if (stockPos != null)
                {
                    Vector3 lossyScale = stockPos.parent.lossyScale;
                    stockPos.localPosition -= ComponentWiseDiv(s_currentPosChange,lossyScale);
                    s_currentPosChange = new Vector3(s_stockPosX.Value, s_stockPosY.Value, s_stockPosZ.Value);
                    stockPos.localPosition += ComponentWiseDiv(s_currentPosChange,lossyScale);
                }
            }
        }

        private static Vector3 ComponentWiseMult(Vector3 a, Vector3 b)
        {
            Vector3 output = Vector3.zero;

            output.x = a.x * b.x;
            output.y = a.y * b.y;
            output.z = a.z * b.z;

            return output;
        }

        private static Vector3 ComponentWiseDiv(Vector3 a, Vector3 b)
        {
            Vector3 output = Vector3.zero;

            output.x = a.x / b.x;
            output.y = a.y / b.y;
            output.z = a.z / b.z;

            return output;
        }
    }
}