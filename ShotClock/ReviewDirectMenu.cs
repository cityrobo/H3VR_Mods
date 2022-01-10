using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using FistVR;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ShotTimer
{
	public class ReviewDirectMenu : TimerMenu
	{ 
	
		[Header("Last Shot")]
		[SerializeField]
		private Text _lastShotText;
		[SerializeField]
		private string _lastShotFormat = "{0}{1:F2}";
		[SerializeField]
		private PaddingSettings _lastShotPadding = new PaddingSettings();
		
		[Header("Details")]
		[SerializeField]
		private Text _detailsText;



		[SerializeField]
		private string _detailsFormat = "SPL {0}{1:F2}\n" + "1st {2}{3:F2}\n" + "DELAY UNKN";

		
		

		string delaynone = "SPL {0}{1:F2}\n" + "1st {2}{3:F2}\n" + "DELAY NONE";
		string delayipsc = "SPL {0}{1:F2}\n" + "1st {2}{3:F2}\n" + "DELAY IPSC";
		string delayidpa = "SPL {0}{1:F2}\n" + "1st {2}{3:F2}\n" + "DELAY IDPA";
		string delayissc = "SPL {0}{1:F2}\n" + "1st {2}{3:F2}\n" + "DELAY ISSC";
		string delayonefive = "SPL {0}{1:F2}\n" + "1st {2}{3:F2}\n" + "DELAY 1-5";
		string delayfiveten = "SPL {0}{1:F2}\n" + "1st {2}{3:F2}\n" + "DELAY 5-10";

		[SerializeField]
		private PaddingSettings _splitPadding = new PaddingSettings();
		[SerializeField]
		private PaddingSettings _firstShotPadding = new PaddingSettings();
		
		[Header("Shot Count")]
		[SerializeField]
		private Text _shotCountText;
		[SerializeField]
		private string _shotCountFormatting = "SHOT # {0}{1}";
		[SerializeField]
		private PaddingSettings _shotCountPadding = new PaddingSettings();

		[Header("Time")]
		[SerializeField]
		private Text _timeText;
		[SerializeField]
		private string _timeFormat = "hh:mm:sstt";

		[SerializeField]
		private TimerMenu next;

		[SerializeField]
		public AudioClip[] _Delayvoice;

		

		private void RenderTime()
		{
			_timeText.text = DateTime.Now.ToString(_timeFormat);
		}
		
		public override void Render()
		{

			switch (Timer.currentmode)
			{
				case ShotTimer.delaymode.none:
					_detailsFormat = delaynone;
					break;
				case ShotTimer.delaymode.ipsc:
					_detailsFormat = delayipsc;
					break;
				case ShotTimer.delaymode.idpa:
					_detailsFormat = delayidpa;
					break;
				case ShotTimer.delaymode.issc:
					_detailsFormat = delayissc;
					break;
				case ShotTimer.delaymode.onefive:
					_detailsFormat = delayonefive;
					break;
				case ShotTimer.delaymode.fiveten:
					_detailsFormat = delayfiveten;
					break;
				default:
					break;
			}
			
			var shots = Timer.Shots;
			int count = shots.Count;

			if (count > 0)
			{
				ShotLine first = shots[0];
				ShotLine last = shots[count - 1];
				_lastShotText.text = string.Format(_lastShotFormat, _lastShotPadding.Render(last.Time), last.Time);

				float split = last.Split;
				if (split != 0)
				{
					_detailsText.text = string.Format(_detailsFormat, _splitPadding.Render(split), split,_firstShotPadding.Render(first.Time), first.Time);
				}
				else
				{
					_detailsText.text = string.Format(_detailsFormat, "", "",_firstShotPadding.Render(first.Time), first.Time);
				}
				_shotCountText.text = string.Format(_shotCountFormatting, _shotCountPadding.Render(last.Time), count);
			}
			else
			{
				_shotCountText.text = string.Format(_shotCountFormatting, _shotCountPadding.Render(0), count);
				_lastShotText.text = "";
				_detailsText.text = "";
			}
			RenderTime();
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
			Debug.Log("ReviewDirectMenu: Start!");
			this.StartCoroutine("Run");
        }
	}
}