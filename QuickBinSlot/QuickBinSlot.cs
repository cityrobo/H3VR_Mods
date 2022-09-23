using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class QuickBinSlot : FVRQuickBeltSlot
    {
        [Header("QuickBinSlot settings")]
        public Color hoverColor;

        public AudioEvent deleteSound;
        public AudioEvent deleteFailureSound;

        [ContextMenu("CopyQBSlot")]
        public void CopyQBSlot()
        {
            FVRQuickBeltSlot QBS = GetComponent<FVRQuickBeltSlot>();

            this.QuickbeltRoot = QBS.QuickbeltRoot;
            this.PoseOverride = QBS.PoseOverride;
            this.SizeLimit = QBS.SizeLimit;
            this.Shape = QBS.Shape;
            this.Type = QBS.Type;
            this.HoverGeo = QBS.HoverGeo;
            this.RectBounds = QBS.RectBounds;
            this.CurObject = QBS.CurObject;
            this.IsSelectable = QBS.IsSelectable;
            this.IsPlayer = QBS.IsPlayer;
            this.UseStraightAxisAlignment = QBS.UseStraightAxisAlignment;
            this.HeldObject = QBS.HeldObject;
        }

#if !(UNITY_EDITOR || UNITY_5)
        public void Start()
        {
            Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }

        void Unhook()
        {
#if !(DEBUG || MEATKIT)
            On.FistVR.FVRQuickBeltSlot.Update -= FVRQuickBeltSlot_Update;
#endif
        }

        void Hook()
        {
#if !(DEBUG || MEATKIT)
            On.FistVR.FVRQuickBeltSlot.Update += FVRQuickBeltSlot_Update;
#endif
        }
#if !(DEBUG || MEATKIT)
        private void FVRQuickBeltSlot_Update(On.FistVR.FVRQuickBeltSlot.orig_Update orig, FVRQuickBeltSlot self)
        {
            if (this == self)
            {
                if (!GM.CurrentSceneSettings.IsSpawnLockingEnabled && this.HeldObject != null && (this.HeldObject as FVRPhysicalObject).m_isSpawnLock)
                {
                    (this.HeldObject as FVRPhysicalObject).m_isSpawnLock = false;
                }
                if (this.HeldObject != null)
                {
                    if ((this.HeldObject as FVRPhysicalObject).m_isSpawnLock)
                    {
                        if (!this.HoverGeo.activeSelf)
                        {
                            this.HoverGeo.SetActive(true);
                        }
                        this.m_hoverGeoRend.material.SetColor("_RimColor", new Color(0.3f, 0.3f, 1f, 1f));
                    }
                    else if ((this.HeldObject as FVRPhysicalObject).m_isHardnessed)
                    {
                        if (!this.HoverGeo.activeSelf)
                        {
                            this.HoverGeo.SetActive(true);
                        }
                        this.m_hoverGeoRend.material.SetColor("_RimColor", new Color(0.3f, 1f, 0.3f, 1f));
                    }
                    else
                    {
                        if (this.HoverGeo.activeSelf != this.IsHovered)
                        {
                            this.HoverGeo.SetActive(this.IsHovered);
                        }
                        this.m_hoverGeoRend.material.SetColor("_RimColor", hoverColor);
                    }
                }
                else
                {
                    if (this.HoverGeo.activeSelf != this.IsHovered)
                    {
                        this.HoverGeo.SetActive(this.IsHovered);
                    }
                    this.m_hoverGeoRend.material.SetColor("_RimColor", hoverColor);
                }

                if (CurObject != null && CurObject is FVRFireArmMagazine)
                {
                    Destroy(CurObject.gameObject);
                    CurObject = null;
                    HeldObject = null;
                    IsHovered = false;
                    SM.PlayGenericSound(deleteSound, this.transform.position);
                }

                if (CurObject != null && !(CurObject is FVRFireArmMagazine))
                {
                    CurObject.SetQuickBeltSlot(null);
                    CurObject = null;
                    HeldObject = null;
                    IsHovered = false;
                    SM.PlayGenericSound(deleteFailureSound, this.transform.position);
                }
            }
            else orig(self);
        }
#endif
#endif
    }
}
