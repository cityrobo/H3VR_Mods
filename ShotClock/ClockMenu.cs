using System;
using System.Text;
using FistVR;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ShotTimer
{
	public class ClockMenu : TimerMenu
	{

		[Header("Time")]
		[SerializeField]
		private Text _timeText;
		[SerializeField]
		private string _timeFormat = "hh:mm:sstt";

				private void RenderTime()
		{
			_timeText.text = DateTime.Now.ToString(_timeFormat);
		}

		[SerializeField]
		private TimerMenu next;

		public override void Render()
		{
			RenderTime();
		}

		public override bool UpdateInputs(Inputs inputs, out TimerMenu menu)
		{
			switch (inputs)
			{
				
					
				case Inputs.MenuUp:
					menu = next;
					return true;
				case Inputs.Start:
					Timer.Nut();
					menu = null;
					return true;
					
				default:
					menu = null;
					return false;
			}
		}



		public override void Activate(TimerMenu next)
		{
			next.gameObject.SetActive(true);
			this.StopAllCoroutines();
			this.gameObject.SetActive(false);
		}

		public IEnumerator Run()
		{
			do 
			{
				Render();
				yield return null;
			} while (this.enabled);
		}
		private void OnEnable()
		{
			Debug.Log("ShotReviewMenu: Start!");
			this.StartCoroutine("Run");
		}
	}
}