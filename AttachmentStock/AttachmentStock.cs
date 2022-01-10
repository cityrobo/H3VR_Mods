using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
    public class AttachmentStock : FVRFireArmAttachment
    {
		[Header("VirtualStock config")]
		public bool HasActiveShoulderStock = true;
		public Transform StockPos;

#if !(UNITY_EDITOR || UNITY_5)
		public override bool HasStockPos()
		{
			return this.HasActiveShoulderStock;
		}

		public override Transform GetStockPos()
		{
			return this.StockPos;
		}
#endif
	}
}
