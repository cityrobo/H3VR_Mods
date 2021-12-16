using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class StandaloneQBSlot : FVRQuickBeltSlot
    {
#if !(UNITY_EDITOR || UNITY_5)
        public void Start()
        {
			if (GM.CurrentPlayerBody != null)
			{
				this.RegisterQuickbeltSlot();
			}
        }

		public void OnDestroy()
        {
			if (GM.CurrentPlayerBody != null)
			{
				this.DeRegisterQuickbeltSlot();
			}
		}

		public void RegisterQuickbeltSlot()
		{
			if (!GM.CurrentPlayerBody.QuickbeltSlots.Contains(this))
			{
				GM.CurrentPlayerBody.QuickbeltSlots.Add(this);
			}
		}

		public void DeRegisterQuickbeltSlot()
		{
			if (GM.CurrentPlayerBody.QuickbeltSlots.Contains(this))
			{
				GM.CurrentPlayerBody.QuickbeltSlots.Remove(this);
			}
		}
#endif
	}
}
