using FistVR;
using HarmonyLib;
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
    public class DirectionalFireArmChamber : FVRFireArmChamber
    {
        private static List<FVRFireArmChamber> _exsitingDirectionalFireArmChambers = new List<FVRFireArmChamber>();

        static DirectionalFireArmChamber()
        {
            Harmony.CreateAndPatchAll(typeof(DirectionalFireArmChamber));
        }

        public override void Awake()
        {
            base.Awake();
            _exsitingDirectionalFireArmChambers.Add(this);
        }
        public override void OnDestroy()
        {
            _exsitingDirectionalFireArmChambers.Remove(this);
            base.OnDestroy();
        }

        [HarmonyPatch(typeof(FVRFireArmRound), "Chamber")]
        [HarmonyPrefix]
        static public void SetRoundPatch(FVRFireArmRound __instance, FVRFireArmChamber c)
        {
            if (_exsitingDirectionalFireArmChambers.Contains(c))
            {
                float angle = Vector3.Angle(__instance.transform.forward, c.transform.forward);

                if (angle > 90f)
                {
                    c.transform.Rotate(180f, 0f, 0f);
                }
            }
        }

        [ContextMenu("Copy existing Chmaber's values")]
        public void CopyChamber()
        {
            FVRFireArmChamber toCopy = GetComponents<FVRFireArmChamber>().Single(c => c != this);

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