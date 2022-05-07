using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class MovableWeaponPartMk2 : FVRInteractiveObject
    {
        public enum Mode
        {
            Translation,
            Rotation,
            Folding
        }
		public Mode MovementMode = Mode.Translation;

		public enum Axis
        {
			X,
			Y,
			Z
        }
		public Axis MovementAxis;

		public Transform StationaryRoot;
		public Transform ObjectToMove;

		public float LowerLimit = 0f;
		public float UpperLimit = 0f;
		public float LimitWiggleRoom = 0.02f;

		public AudioEvent CloseSound;
		public AudioEvent OpenSound;

		private enum State
		{
			Open,
			Mid,
			Closed
		}

		private State _curState;
		private State _lastState;

		private float _curPos;
		private Vector3 _origPos;
		private Quaternion _origRot;

		private Vector3 _startingHandPos;
		private Quaternion _startingHandRot;
		private Quaternion _lastHandRot;

		public bool DebugMode = false;

#if !(UNITY_EDITOR || UNITY_5)
		public override void Start()
        {
			base.Start();
			_origPos = ObjectToMove.localPosition;
			_origRot = ObjectToMove.localRotation;

            switch (MovementMode)
            {
                case Mode.Translation:
                    switch (MovementAxis)
                    {
                        case Axis.X:
							_curPos = _origPos.x;
							break;
                        case Axis.Y:
							_curPos = _origPos.y;
							break;
                        case Axis.Z:
							_curPos = _origPos.z;
							break;
                        default:
                            break;
                    }
                    break;
                case Mode.Rotation:
					switch (MovementAxis)
					{
						case Axis.X:
							_curPos = _origRot.eulerAngles.x;
							break;
						case Axis.Y:
							_curPos = _origRot.eulerAngles.y;
							break;
						case Axis.Z:
							_curPos = _origRot.eulerAngles.z;
							break;
						default:
							break;
					}
					break;
                case Mode.Folding:
					switch (MovementAxis)
					{
						case Axis.X:
							_curPos = _origRot.eulerAngles.x;
							break;
						case Axis.Y:
							_curPos = _origRot.eulerAngles.y;
							break;
						case Axis.Z:
							_curPos = _origRot.eulerAngles.z;
							break;
						default:
							break;
					}
					break;
                default:
                    break;
            }

            if (MovementMode == Mode.Rotation || MovementMode == Mode.Folding)
            {
				if (UpperLimit >= 180f) UpperLimit = 180f - float.Epsilon;
				if (LowerLimit <= -180f) LowerLimit = -180f + float.Epsilon;
			}
        }
		public override void BeginInteraction(FVRViveHand hand)
		{
			base.BeginInteraction(hand);

			_startingHandPos = hand.transform.position;
			_startingHandRot = hand.transform.rotation;
			_lastHandRot = hand.transform.rotation;
		}

		public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            switch (MovementMode)
            {
                case Mode.Translation:
					TranslationMode(hand);
                    break;
                case Mode.Rotation:
					RotationMode(hand);
                    break;
                case Mode.Folding:
                    FoldingMode(hand);
                    break;
                default:
                    break;
            }
			_lastHandRot = hand.transform.rotation;
        }

		public void TranslationMode(FVRViveHand hand)
        {
			Vector3 posVector;
			switch (MovementAxis)
            {
                case Axis.X:
					posVector = GetClosestValidPoint(new Vector3(LowerLimit,0f,0f), new Vector3(UpperLimit, 0f, 0f), StationaryRoot.InverseTransformPoint(base.m_handPos));
					_curPos = posVector.x;
					ObjectToMove.localPosition = new Vector3(_curPos, _origPos.y, _origPos.z);
					break;
                case Axis.Y:
					posVector = GetClosestValidPoint(new Vector3(0f, LowerLimit, 0f), new Vector3(0f, UpperLimit, 0f), StationaryRoot.InverseTransformPoint(base.m_handPos));
					_curPos = posVector.y;
					ObjectToMove.localPosition = new Vector3(_origPos.x, _curPos, _origPos.z);
					break;
                case Axis.Z:
					posVector = GetClosestValidPoint(new Vector3(0f, 0f, LowerLimit), new Vector3(0f, 0f, UpperLimit), StationaryRoot.InverseTransformPoint(base.m_handPos));
					_curPos = posVector.z;
					ObjectToMove.localPosition = new Vector3(_origPos.x, _origPos.y, _curPos);
					break;
                default:
                    break;
            }

			float lerp = Mathf.InverseLerp(this.LowerLimit, this.UpperLimit, this._curPos);
			CheckSound(lerp);
		}

		public void RotationMode(FVRViveHand hand)
		{
			Quaternion curHandRot = hand.transform.rotation;
			Quaternion handRotDelta = curHandRot * Quaternion.Inverse(_lastHandRot);

			float rotAngle;
			Vector3 rotAxis;
			handRotDelta.ToAngleAxis(out rotAngle, out rotAxis);
			if (rotAxis.magnitude == 0 || rotAngle == 0) return;

			if (rotAngle >= 180f) rotAngle -= 180f;
			else if (rotAngle <= -180f) rotAngle += 180f;
			rotAxis = StationaryRoot.InverseTransformDirection(rotAxis);

			float rotAxisProjected = rotAxis[(int)MovementAxis];


			if (DebugMode)
            {
				Popcron.Gizmos.Line(ObjectToMove.transform.position, ObjectToMove.transform.position + rotAxis * 0.1f * rotAngle, Color.magenta);
			}
			float deltaAngle = rotAxisProjected * rotAngle;

            if (Mathf.Abs(deltaAngle) > 90f)
            {
                Debug.Log("DeltaAngle: " + deltaAngle);

            }
			/*
			if (_curPos + deltaAngle > UpperLimit) deltaAngle = UpperLimit - _curPos;
			else if (_curPos + deltaAngle < LowerLimit) deltaAngle = LowerLimit + _curPos;

			
			switch (MovementAxis)
			{
				case Axis.X:
					ObjectToMove.Rotate(new Vector3(deltaAngle, 0, 0));
					break;
				case Axis.Y:
					ObjectToMove.Rotate(new Vector3(0, deltaAngle, 0));
					break;
				case Axis.Z:
					ObjectToMove.Rotate(new Vector3(0, 0, deltaAngle));
					break;
				default:
					break;
			}
			*/


			if (deltaAngle > 0)
            {
				Quaternion upperLimit;
				switch (MovementAxis)
				{
					case Axis.X:
						upperLimit = Quaternion.Euler(new Vector3(UpperLimit, 0, 0));
						ObjectToMove.localRotation = Quaternion.RotateTowards(ObjectToMove.localRotation, upperLimit, deltaAngle);
						break;
					case Axis.Y:
						upperLimit = Quaternion.Euler(new Vector3(0, UpperLimit, 0));
						ObjectToMove.localRotation = Quaternion.RotateTowards(ObjectToMove.localRotation, upperLimit, deltaAngle);
						break;
					case Axis.Z:
						upperLimit = Quaternion.Euler(new Vector3(0, 0, UpperLimit));
						ObjectToMove.localRotation = Quaternion.RotateTowards(ObjectToMove.localRotation, upperLimit, deltaAngle);
						break;
					default:
						break;
				}
			}
            else
            {
				Quaternion lowerLimit;
				switch (MovementAxis)
				{
					case Axis.X:
						lowerLimit = Quaternion.Euler(new Vector3(LowerLimit, 0, 0));
						ObjectToMove.localRotation = Quaternion.RotateTowards(ObjectToMove.localRotation, lowerLimit, deltaAngle);
						break;
					case Axis.Y:
						lowerLimit = Quaternion.Euler(new Vector3(0, LowerLimit, 0));
						ObjectToMove.localRotation = Quaternion.RotateTowards(ObjectToMove.localRotation, lowerLimit, deltaAngle);
						break;
					case Axis.Z:
						lowerLimit = Quaternion.Euler(new Vector3(0, 0, LowerLimit));
						ObjectToMove.localRotation = Quaternion.RotateTowards(ObjectToMove.localRotation, lowerLimit, deltaAngle);
						break;
					default:
						break;
				}
			}

			if (DebugMode)
			{
				Popcron.Gizmos.Line(hand.transform.position, hand.transform.position + hand.transform.forward * 0.1f, Color.blue);
				Popcron.Gizmos.Line(hand.transform.position, hand.transform.position + hand.transform.up * 0.1f, Color.green);
				Popcron.Gizmos.Line(hand.transform.position, hand.transform.position + hand.transform.right * 0.1f, Color.red);
				/*
				Popcron.Gizmos.Line(hand.transform.position, base.transform.position + lhs * 0.1f, Color.cyan);
				Popcron.Gizmos.Line(hand.transform.position, base.transform.position + rhs * 0.1f, Color.yellow);
				Popcron.Gizmos.Line(hand.transform.position, base.transform.position + Vector3.Cross(lhs, rhs) * 0.1f, Color.magenta);
				*/
				
			}
			_curPos += deltaAngle;

			if (Mathf.Abs(_curPos - UpperLimit) <= LimitWiggleRoom) 
			{
				_curPos = UpperLimit;
				Quaternion limitRot;
                switch (MovementAxis)
                {
                    case Axis.X:
						limitRot = Quaternion.Euler(UpperLimit, 0, 0);
                        break;
                    case Axis.Y:
						limitRot = Quaternion.Euler(0, UpperLimit, 0);
						break;
                    case Axis.Z:
						limitRot = Quaternion.Euler(0, 0, UpperLimit);
						break;
                    default:
						limitRot = Quaternion.identity;
						break;
                }

				//ObjectToMove.localRotation = limitRot;
            }
			else if (Mathf.Abs(_curPos - LowerLimit) <= LimitWiggleRoom)
			{
				_curPos = LowerLimit;
				Quaternion limitRot;
				switch (MovementAxis)
				{
					case Axis.X:
						limitRot = Quaternion.Euler(LowerLimit, 0, 0);
						break;
					case Axis.Y:
						limitRot = Quaternion.Euler(0, LowerLimit, 0);
						break;
					case Axis.Z:
						limitRot = Quaternion.Euler(0, 0, LowerLimit);
						break;
					default:
						limitRot = Quaternion.identity;
						break;
				}

				//ObjectToMove.localRotation = limitRot;
			}
			float lerp = Mathf.InverseLerp(this.LowerLimit, this.UpperLimit, _curPos);
			CheckSound(lerp);
        }

		private void FoldingMode(FVRViveHand hand)
		{
			Vector3 vector = base.m_handPos - this.StationaryRoot.position;
			Vector3 lhs = new Vector3();
			
			switch (MovementAxis)
			{
				case Axis.X:
					lhs = -this.StationaryRoot.transform.forward;
					vector = Vector3.ProjectOnPlane(vector, this.StationaryRoot.right).normalized;
					_curPos = Mathf.Atan2(Vector3.Dot(this.StationaryRoot.right, Vector3.Cross(lhs, vector)), Vector3.Dot(lhs, vector)) * 57.29578f;
					break;
				case Axis.Y:
					lhs = -this.StationaryRoot.transform.forward;
					vector = Vector3.ProjectOnPlane(vector, this.StationaryRoot.up).normalized;
					_curPos = Mathf.Atan2(Vector3.Dot(this.StationaryRoot.up, Vector3.Cross(lhs, vector)), Vector3.Dot(lhs, vector)) * 57.29578f;
					break;
				case Axis.Z:
					lhs = this.StationaryRoot.transform.up;
					vector = Vector3.ProjectOnPlane(vector, this.StationaryRoot.forward).normalized;
					_curPos = Mathf.Atan2(Vector3.Dot(this.StationaryRoot.forward, Vector3.Cross(lhs, vector)), Vector3.Dot(lhs, vector)) * 57.29578f;
					break;
				default:
					break;
			}

            if (DebugMode)
            {
				Popcron.Gizmos.Line(this.StationaryRoot.position, base.m_handPos, Color.magenta);
				Popcron.Gizmos.Line(this.StationaryRoot.position, lhs, Color.green);
				Popcron.Gizmos.Line(this.StationaryRoot.position, vector, Color.red);
				Popcron.Gizmos.Line(this.StationaryRoot.position, Vector3.Cross(lhs, vector), Color.blue);
			}

			if (Mathf.Abs(_curPos - this.LowerLimit) < 5f)
			{
				_curPos = LowerLimit;
			}
			if (Mathf.Abs(_curPos - this.UpperLimit) < 5f)
			{
				_curPos = this.UpperLimit;
			}
			if (_curPos >= this.LowerLimit && _curPos <= this.UpperLimit)
			{
				switch (MovementAxis)
				{
					case Axis.X:
						this.ObjectToMove.localEulerAngles = new Vector3(this._curPos, 0f, 0f);
						break;
					case Axis.Y:
						this.ObjectToMove.localEulerAngles = new Vector3(0f, this._curPos, 0f);
						break;
					case Axis.Z:
						this.ObjectToMove.localEulerAngles = new Vector3(0f, 0f, this._curPos);
						break;
					default:
						break;
				}

				float lerp = Mathf.InverseLerp(this.LowerLimit, this.UpperLimit, _curPos);
				CheckSound(lerp);

			}
		}

		private void CheckSound(float lerp)
        {
			if (lerp < LimitWiggleRoom)
			{
				_curState = State.Open;

			}
			else if (lerp > 1f - LimitWiggleRoom)
			{
				_curState = State.Closed;
			}
			else
			{
				_curState = State.Mid;
			}
			if (_curState == State.Open && _lastState != State.Open)
			{
				SM.PlayGenericSound(OpenSound, StationaryRoot.position);
			}
			if (_curState == State.Closed && _lastState != State.Closed)
			{
				SM.PlayGenericSound(CloseSound, StationaryRoot.position);
			}
			_lastState = _curState;
		}

		public Vector3 ProjectOnPlaneThroughPoint(Vector3 vector, Vector3 point, Vector3 planeNormal)
		{
			return Vector3.ProjectOnPlane(vector, planeNormal) + Vector3.Dot(point, planeNormal) * planeNormal;
		}
#endif
	}
}
