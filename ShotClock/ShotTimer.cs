using System;
using System.Collections;
using FistVR;
using UnityEngine;
using UnityEngineInternal.Input;
using Random = UnityEngine.Random;

namespace ShotTimer
{
	public class ShotTimer : FVRPhysicalObject
	{
		private ShotLineFactory _factory;
		public OverflowArray<ShotLine> Shots;

		System.Random random = new System.Random();

		[Header("Gameplay")]
		private float _timeout;

		[Header("Display")]
		[SerializeField]
		private TimerMenu _menu;

		[Header("Audio")]
		[SerializeField]
		public AudioSource _speaker;
		
		[Header("Audio/Status")]
		[SerializeField]
		private AudioClip[] _shooterReadys;
		[SerializeField]
		private AudioClip[] _beeps;
		[SerializeField]
		private AudioClip[] _delays;
		[SerializeField]
		private AudioClip[] nut;

		public enum delaymode { none, ipsc, idpa,issc, onefive, fiveten }
		public delaymode currentmode;

		private int _currentShooterReady;
		private int _currentBeep;
		private int _currentNut;

		private float _minDelay;
		private float _maxDelay;




		/*
		if a player presses left, it delays the beep for 5-10 seconds
		If a player presses right, it delays the beep for 1 second

	*/

		public override void Awake()
		{
			base.Awake();
			currentmode = delaymode.none;
			_factory = new ShotLineFactory();
			Shots = new OverflowArray<ShotLine>(999);
			Shots.Clear();
			_factory.Reset();
		}

		public void NextDelayMode()
		{
			switch (currentmode)
			{
				case delaymode.fiveten:
					_speaker.Stop();
					currentmode = delaymode.none;
					_speaker.PlayOneShot(_delays[0]);
					break;
				case delaymode.none:
					_speaker.Stop();
					currentmode = delaymode.ipsc;
					_speaker.PlayOneShot(_delays[1]);
					break;
				case delaymode.ipsc:
					_speaker.Stop();
					currentmode = delaymode.idpa;
					_speaker.PlayOneShot(_delays[2]);
					break;
				case delaymode.idpa:
					_speaker.Stop();
					currentmode = delaymode.issc;
					_speaker.PlayOneShot(_delays[3]);
					break;
				case delaymode.issc:
					_speaker.Stop();
					currentmode = delaymode.onefive;
					_speaker.PlayOneShot(_delays[4]);
					break;
				case delaymode.onefive:
					_speaker.Stop();
					currentmode = delaymode.fiveten;
					_speaker.PlayOneShot(_delays[5]);
					break;
				default:
					currentmode = delaymode.none;
					break;
			}

		}
		public void PreviousDelayMode()
		{
			switch (currentmode)
			{
				case delaymode.none:
					_speaker.Stop();
					currentmode = delaymode.fiveten;
					_speaker.PlayOneShot(_delays[5]);
					break;
				case delaymode.fiveten:
					_speaker.Stop();
					currentmode = delaymode.onefive;
					_speaker.PlayOneShot(_delays[4]);
					break;
				case delaymode.onefive:
					_speaker.Stop();
					currentmode = delaymode.issc;
					_speaker.PlayOneShot(_delays[3]);
					break;
				case delaymode.issc:
					_speaker.Stop();
					currentmode = delaymode.idpa;
					_speaker.PlayOneShot(_delays[2]);
					break;
				case delaymode.idpa:
					_speaker.Stop();
					currentmode = delaymode.ipsc;
					_speaker.PlayOneShot(_delays[1]);
					break;
				case delaymode.ipsc:
					_speaker.Stop();
					currentmode = delaymode.none;
					_speaker.PlayOneShot(_delays[0]);
					break;
				default:
					currentmode = delaymode.none;
					break;
			}
		}

		
		public override void UpdateInteraction(FVRViveHand hand)
		{
			base.UpdateInteraction(hand);

			var input = hand.Input;
			var inputs =
				Inputs.MenuUp.Encode(Vector2.Angle(input.TouchpadAxes, Vector2.up) <= 45f && input.TouchpadAxes.magnitude > 0.4f && input.TouchpadDown) |
				Inputs.MenuDown.Encode(Vector2.Angle(input.TouchpadAxes, -Vector2.up) <= 45f && input.TouchpadAxes.magnitude > 0.4f && input.TouchpadDown) |
				Inputs.SetUp.Encode(Vector2.Angle(input.TouchpadAxes, -Vector2.right) <= 45f && input.TouchpadAxes.magnitude > 0.4f && input.TouchpadDown) |
				Inputs.SetDown.Encode(Vector2.Angle(input.TouchpadAxes, Vector2.right) <= 45f && input.TouchpadAxes.magnitude > 0.4f && input.TouchpadDown) |
				Inputs.Start.Encode(input.TriggerDown);

			TimerMenu next;
			if (_menu.UpdateInputs(inputs, out next))
			{
				if (next == null)
				{
					//next.deac;
				}
				else
				{
                    _menu.Activate(next);
					_menu = next;
				}
			}
		}



		public void Nut()
		{
			_speaker.Stop();
			_currentNut = random.Next(nut.Length - 1);
			_speaker.PlayOneShot(nut[_currentNut]);

		}

		private IEnumerator Run()
		{
			Shots.Clear();
			
			_currentShooterReady = random.Next(_shooterReadys.Length - 1);
			_currentBeep = random.Next(_beeps.Length - 1);

			if (_shooterReadys.Length != 0)
            {
				_speaker.Stop();
				AudioClip ready = _shooterReadys[_currentShooterReady];
				_speaker.PlayOneShot(ready);
				yield return new WaitForSeconds(ready.length);
				float delay = 1;
				switch (currentmode)	
				{
					case delaymode.none:
						delay = 1;
						break;
					case delaymode.ipsc:
						delay = Random.Range(1, 4);
						break;
					case delaymode.idpa:
						delay = Random.Range(1, 4);
						break;
					case delaymode.issc:
						delay = Random.Range(2, 6);
						break;
					case delaymode.onefive:
						delay = Random.Range(1, 5);
						break;
					case delaymode.fiveten:
						delay = Random.Range(5, 10);
						break;
					default:
						delay = 1;
						break;
				}

				
				yield return new WaitForSeconds(delay);
			}

			if (_beeps.Length != 0)
            {
					AudioClip beep = _beeps[_currentBeep];
					_speaker.PlayOneShot(beep);
					yield return new WaitForSeconds(beep.length);
			}

			_factory.Reset();
		}

		private void OnFire(FVRFireArm firearm)
		{
			Shots.Add(_factory.Create(Shots));
		}

#if !DEBUG


		private void OnEnable()
		{
			GM.CurrentSceneSettings.ShotFiredEvent += this.OnFire;
		}

		private void OnDisable()
		{
			GM.CurrentSceneSettings.ShotFiredEvent -= this.OnFire;
		}


#endif
	}
}