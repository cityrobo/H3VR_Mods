using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ShotTimer
{
	public class SingleTimeOnlyMenu : TimerMenu
	{
		[SerializeField]
		private Text _display;

		[SerializeField]
		private string _format = "{0}{1:F2}";

		[SerializeField]
		private PaddingSettings _padding = new PaddingSettings();

		[SerializeField]
		private TimerMenu next;

		public override void Render()
		{
			ShotLine shot;
			if (Timer.Shots.TryGetLast(out shot))
			{
				float time = shot.Time;

				_display.text = string.Format(_format, _padding.Render(time), time);
			}
			else
			{
				_display.text = "";
			}
		}

		public override bool UpdateInputs(Inputs inputs, out TimerMenu menu)
		{
			switch (inputs)
			{
				case Inputs.MenuUp:
					menu = next;
					return true;

				case Inputs.SetDown:
					Timer.NextDelayMode();
					
					goto default;
				case Inputs.SetUp:
					Timer.PreviousDelayMode();
					
					goto default;
				case Inputs.Start:
					Timer.StartCoroutine("Run");
					menu = null;
					((ShotReviewMenu)next)._cursor = 0;
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
			Debug.Log("SingleTimeOnlyMenu: Start!");
			this.StartCoroutine("Run");
		}
	}
}