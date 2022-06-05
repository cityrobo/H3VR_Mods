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
		
        public virtual void Start()
        {
			if (GM.CurrentPlayerBody != null)
			{
				//RegisterQuickbeltSlot();
			}
        }
		/*
		public virtual void OnEnable()
        {
			if (GM.CurrentPlayerBody != null)
			{
				RegisterQuickbeltSlot();
			}
		}

		public virtual void OnDisable()
        {
			if (GM.CurrentPlayerBody != null)
			{
				DeRegisterQuickbeltSlot();
			}
		}

		public virtual void OnDestroy()
        {
			if (GM.CurrentPlayerBody != null)
			{
				DeRegisterQuickbeltSlot();
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
		*/
#endif
	}
}
