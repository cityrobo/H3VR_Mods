using System;
using System.Text;
using FistVR;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ShotTimer
{
	public class ShotReviewMenu : TimerMenu
	{
		public int _cursor = 0;

		[SerializeField]
		private Text _text;

		[SerializeField]
		private int _lineCount = 4;

		[SerializeField]
		private string _lineFormatting = "{0}{1:F2} # {2}{3}  SPL ";
		[SerializeField]
		private string _splitFormatting = "{0}{1:F2}";
		[SerializeField]
		private PaddingSettings _timePadding = new PaddingSettings();
		[SerializeField]
		private PaddingSettings _shotNumberPadding = new PaddingSettings();
		[SerializeField]
		private PaddingSettings _splitPadding = new PaddingSettings();
		[SerializeField]
		private TimerMenu next;

		public override void Render()
		{
			
			
			var builder = new StringBuilder();
			int count = Timer.Shots.Count;
			int capacity = Timer.Shots.Capacity;
			
			int start = _cursor;
			int end = start + _lineCount;
			
			for (int i = start; i < end; ++i)
			{
				int modI = (i % capacity);
				float modI_float = modI;

				if (modI >= count) break;
				ShotLine shot = Timer.Shots[i];
				float time = shot.Time;

				builder.AppendFormat(_lineFormatting, _timePadding.Render(time), time, _shotNumberPadding.Render(modI_float+1f),modI_float+1f);
				float split = shot.Split;
				builder.AppendFormat(_splitFormatting, _splitPadding.Render(split), split);
				
				builder.AppendLine();
			}

			_text.text = builder.ToString();
		}

		public override bool UpdateInputs(Inputs inputs, out TimerMenu menu)
		{
			switch (inputs)
			{
				case Inputs.SetUp:
					ScrollString(-1);
					goto default;
					
				case Inputs.SetDown:
					ScrollString(1);
					goto default;
					
				case Inputs.MenuUp:
					menu = next;
					return true;
				case Inputs.MenuDown:
					_cursor = 0;
					goto default;
					//this resets the count for scrolling back to 0

				default:
					menu = null;
					return false;
			}
		}

		private void ScrollString(int amount)
		{
			_cursor += amount;
			if (_cursor < 0) _cursor = 0;
			//Render();
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