using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class WristQBSlot : FVRQuickBeltSlot
    {
        [Header("WristQBSlot Config")]
		public Vector3 wristOffsetPosition;
        public Vector3 wristOffsetRotation;

        public enum Wrist
        {
			leftWrist,
			rightWrist
        }

		[SearchableEnum]
		public Wrist wrist;

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

        public FVRViveHand Hand
        {
            get
            {
                return m_hand;
            }
        }

#if !(UNITY_EDITOR || UNITY_5)
        private FVRViveHand m_hand;

		public void Start()
        {
            if (GM.CurrentPlayerBody != null && GM.CurrentPlayerBody.LeftHand != null && GM.CurrentPlayerBody.RightHand != null)
            {
                switch (wrist)
                {
                    case Wrist.leftWrist:
                        this.transform.SetParent(GM.CurrentPlayerBody.LeftHand);
                        m_hand = GameObject.Find("Controller (left)").GetComponent<FVRViveHand>();
                        break;
                    case Wrist.rightWrist:
                        this.transform.SetParent(GM.CurrentPlayerBody.RightHand);
                        m_hand = GameObject.Find("Controller (right)").GetComponent<FVRViveHand>();
                        break;
                    default:
                        break;
                }
            }

            this.transform.localPosition = wristOffsetPosition;
            this.transform.localRotation = Quaternion.Euler(wristOffsetRotation);
        }

		public void OnDestroy()
        {

		}

#endif
	}
}
