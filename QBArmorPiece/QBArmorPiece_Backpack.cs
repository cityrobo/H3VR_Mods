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
		[Header("QBArmorPiece_Backpack Config")]
		public string layerName = "Default";
#if !(UNITY_EDITOR || UNITY_5)
		public override void SetQuickBeltSlot(FVRQuickBeltSlot slot)
		{
			if (slot != null && !base.IsHeld)
			{
				if (this.AttachmentsList.Count > 0)
				{
					for (int i = 0; i < this.AttachmentsList.Count; i++)
					{
						if (this.AttachmentsList[i] != null)
						{
							this.AttachmentsList[i].SetAllCollidersToLayer(false, layerName);
						}
					}
				}
			}
			else if (this.AttachmentsList.Count > 0)
			{
				for (int j = 0; j < this.AttachmentsList.Count; j++)
				{
					if (this.AttachmentsList[j] != null)
					{
						this.AttachmentsList[j].SetAllCollidersToLayer(false, "Default");
					}
				}
			}
			if (this.m_quickbeltSlot != null && slot != this.m_quickbeltSlot)
			{
				this.m_quickbeltSlot.HeldObject = null;
				this.m_quickbeltSlot.CurObject = null;
				this.m_quickbeltSlot.IsKeepingTrackWithHead = false;
			}
			if (slot != null && !base.IsHeld)
			{
				base.SetAllCollidersToLayer(false, layerName);
				slot.HeldObject = this;
				slot.CurObject = this;
				slot.IsKeepingTrackWithHead = this.DoesQuickbeltSlotFollowHead;
			}
			else
			{
				base.SetAllCollidersToLayer(false, "Default");
			}
			this.m_quickbeltSlot = slot;
		}
#endif
	}
}