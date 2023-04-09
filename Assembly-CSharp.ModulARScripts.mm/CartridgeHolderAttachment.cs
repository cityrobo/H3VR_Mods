using FistVR;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace MeatyScripts
{
    public class CartridgeHolderAttachment : FVRFireArmAttachment
    {
        public FVRFireArmChamber[] Chambers;


        private const string flagDicChamberRoundClassKey = "ChamberRoundClassConfig";

        private const string flagDicChamberRotationKey = "ChamberRotationConfig";
        public override GameObject DuplicateFromSpawnLock(FVRViveHand hand)
        {
            GameObject ReturnValue = base.DuplicateFromSpawnLock(hand);
            CartridgeHolderAttachment component = ReturnValue.GetComponent<CartridgeHolderAttachment>();
            for (int i = 0; i< this.Chambers.Length; i++)
            {
                component.Chambers[i].SetRound(this.Chambers[i].m_round);
                component.Chambers[i].transform.localRotation = this.Chambers[i].transform.localRotation;
            }
            return ReturnValue;
        }

        public override Dictionary<string, string> GetFlagDic()
        {
            Dictionary<string, string> flagDic = base.GetFlagDic();

            string flagDicValue = string.Empty;
            FVRFireArmRound round;
            for (int i = 0; i < Chambers.Length; i++)
            {
                round = Chambers[i].GetRound();
                if (round != null) flagDicValue += round.RoundClass;
                else flagDicValue += "None";

                if (i != Chambers.Length - 1) flagDicValue += ";";
            }

            flagDic.Add(flagDicChamberRoundClassKey, flagDicValue);

            flagDicValue = string.Empty;

            for (int i = 0; i < Chambers.Length; i++)
            {
                flagDicValue += Chambers[i].transform.localRotation.x + "," + Chambers[i].transform.localRotation.y + "," + Chambers[i].transform.localRotation.z + "," + Chambers[i].transform.localRotation.w;

                if (i != Chambers.Length - 1) flagDicValue += ";";
            }

            flagDic.Add(flagDicChamberRotationKey, flagDicValue);

            return flagDic;
        }

        public override void ConfigureFromFlagDic(Dictionary<string, string> f)
        {
            base.ConfigureFromFlagDic(f);

            string flagDicValue;
            if (f.TryGetValue(flagDicChamberRoundClassKey,out flagDicValue))
            {
                string[] separatedValue = flagDicValue.Split(';');
                List<FireArmRoundClass> roundClasses = new List<FireArmRoundClass>();
                foreach (string value in separatedValue)
                {
                    if (value == "None") roundClasses.Add((FireArmRoundClass) int.MinValue);
                    else roundClasses.Add((FireArmRoundClass) Enum.Parse(typeof(FireArmRoundClass), value));
                }

                for (int i = 0; i < Chambers.Length; i++)
                {
                    if (roundClasses[i] != (FireArmRoundClass)int.MinValue)
                    {
                        GameObject prefab = AM.GetRoundSelfPrefab(Chambers[i].RoundType, roundClasses[i]).GetGameObject();
                        FVRFireArmRound round = prefab.GetComponent<FVRFireArmRound>();
                        Chambers[i].SetRound(round);
                    }
                }
            }

            if (f.TryGetValue(flagDicChamberRotationKey, out flagDicValue))
            {
                string[] separatedValue = flagDicValue.Split(';');

                List<Quaternion> quaternions = new List<Quaternion>();
                foreach (string value in separatedValue)
                {
                    string[] splitAxis = value.Split(',');

                    quaternions.Add(new Quaternion(float.Parse(splitAxis[0]), float.Parse(splitAxis[1]), float.Parse(splitAxis[2]), float.Parse(splitAxis[3])));
                }

                for (int i = 0; i < Chambers.Length; i++)
                { 
                    Chambers[i].transform.localRotation = quaternions[i];
                }
            }
        }

        [ContextMenu("Copy existing Attachment's values")]
        public void CopyAttachment()
        {
            FVRFireArmAttachment[] attachments = GetComponents<FVRFireArmAttachment>();

            FVRFireArmAttachment toCopy = attachments.Single(c => c != this);

            toCopy.AttachmentInterface.Attachment = this;
            toCopy.Sensor.Attachment = this;

            CopyComponent(this, toCopy);
        }

        public T CopyComponent<T>(Component target, T reference) where T : Component
        {
            Type type = reference.GetType();
            //if (type != reference.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos)
            {
                if (pinfo.CanWrite)
                {
                    try
                    {
                        pinfo.SetValue(target, pinfo.GetValue(reference, null), null);
                    }
                    catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos)
            {
                finfo.SetValue(target, finfo.GetValue(reference));
            }
            return target as T;
        }
    }
}