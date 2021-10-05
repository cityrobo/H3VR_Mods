using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class QBArmorPiece_Backpack : PlayerBackPack
    {
#if !(UNITY_EDITOR || UNITY_5)
		public override void SetQuickBeltSlot(FVRQuickBeltSlot slot)
		{
			if (this.m_quickbeltSlot != null && slot != this.m_quickbeltSlot)
			{
				this.m_quickbeltSlot.HeldObject = null;
				this.m_quickbeltSlot.CurObject = null;
				this.m_quickbeltSlot.IsKeepingTrackWithHead = false;
			}
			if (slot != null && !base.IsHeld)
			{
				slot.HeldObject = this;
				slot.CurObject = this;
				slot.IsKeepingTrackWithHead = this.DoesQuickbeltSlotFollowHead;
			}
			this.m_quickbeltSlot = slot;
		}
#endif
	}
}