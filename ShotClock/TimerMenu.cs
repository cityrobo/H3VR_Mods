using System;
using System.Collections.Generic;
using FistVR;
using UnityEngine;

namespace ShotTimer
{
	public abstract class TimerMenu : MonoBehaviour
	{
		[SerializeField]
		private ShotTimer _timer;

		protected ShotTimer Timer
		{
			get { return _timer; }
		}

		public abstract void Render();

		public abstract bool UpdateInputs(Inputs inputs, out TimerMenu next);

		public abstract void Activate(TimerMenu next);
	}
}