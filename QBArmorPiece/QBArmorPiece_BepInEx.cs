#if!(MEATKIT || DEBUG)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;

namespace Cityrobo
{

[BepInPlugin("h3vr.cityrobo.openscripts.qb_armor_piece", "OpenScripts: QBArmorPiece Scripts", "1.0.0")]
    public class QBArmorPiece_BepInEx : BaseUnityPlugin
    {
        public QBArmorPiece_BepInEx()
        {
            Logger.LogInfo("OpenScripts: QBArmorPiece Scripts loaded!");

            Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }

        void Unhook()
        {
            //On.FistVR.PlayerSosigBody.Start -= PlayerSosigBody_Start;
        }

        void Hook()
        {
            //On.FistVR.PlayerSosigBody.Start += PlayerSosigBody_Start;
        }

        private void PlayerSosigBody_Start(On.FistVR.PlayerSosigBody.orig_Start orig, FistVR.PlayerSosigBody self)
        {
            orig(self);

            GameObject newTorsoTarget = new GameObject("NewTorsoTarget");
            newTorsoTarget.transform.SetParent(self.torso);
            newTorsoTarget.transform.localRotation = Quaternion.identity;
            newTorsoTarget.transform.localPosition = new Vector3(0, 0.09f, -0.1f);

            self.torso = newTorsoTarget.transform;
        }
    }
}
#endif