using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace H3VRUtils
{
	[Obsolete("Use vanilla open bolt burst selector setting instead")]
	public class OpenBoltBurstFire : MonoBehaviour
	{
		public OpenBoltReceiver Receiver = null;

		[Tooltip("Selector setting position that will be burst. Remember, selectors go pos: 0, 1 ,2, not 1, 2, 3")]
		public int SelectorSetting = 0;

		public  int BurstAmt = 1;
		private int BurstSoFar;

		private bool wasLoaded;


		public void Start()
		{
			Receiver.FireSelector_Modes[SelectorSetting].ModeType = OpenBoltReceiver.FireSelectorModeType.FullAuto;
			// Debug.Log("OBB Loaded!");
		}

		public void Update()
		{
			//this script breaks without publicized assembly
			//if it's not the correct selector, just don't do anything
			if (Receiver.m_hand == null) return;
			if (Receiver.m_fireSelectorMode != SelectorSetting)
			{
				BurstSoFar = 0;
				return;
			}

			//add to burst if chamber is shot
			if(wasLoaded && !Receiver.Chamber.IsFull)
			{
				BurstSoFar++;
				// Debug.Log(BurstSoFar);
			}
			wasLoaded = Receiver.Chamber.IsFull;
			//if burst amount hit
			if (BurstSoFar >= BurstAmt)
			{
				lockUp();
				if (Receiver.m_hand.Input.TriggerFloat < Receiver.TriggerFiringThreshold)
				{
					unLock();
				}
			}

			//reset amt if trigger is let go
			if (Receiver.m_hand.Input.TriggerFloat < Receiver.TriggerFiringThreshold)
			{
				// if (BurstSoFar > 0) Debug.Log("OBB Mid Burst Reset");
				BurstSoFar = 0;
			}
		}

		public void lockUp()
		{
			//put to safe
			Receiver.FireSelector_Modes[SelectorSetting].ModeType = OpenBoltReceiver.FireSelectorModeType.Safe;
			//Debug.Log("OBB Lock");
		}

		public void unLock()
		{
			//put to auto; reset
			BurstSoFar = 0;
			Receiver.FireSelector_Modes[SelectorSetting].ModeType = OpenBoltReceiver.FireSelectorModeType.FullAuto;
			// Debug.Log("OBB Unlock");
		}
	}
}
