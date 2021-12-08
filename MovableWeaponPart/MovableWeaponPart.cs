using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class MovableWeaponPart : FVRInteractiveObject
    {
        public enum Mode
        {
            Translation,
            Rotation,
            Tilt
        }
		public Mode mode;

		public enum Direction
        {
			X,
			Y,
			Z
        }
		public Direction direction;

		public Transform root;
		public Transform objectToMove;

		public float lowerLimit = 0f;
		public float upperLimit = 0f;
		public float limitWiggleRoom = 0.02f;

		public AudioSource audioSource;
		public AudioClip closeSound;
		public AudioClip openSound;

		private enum State
		{
			Open,
			Mid,
			Closed
		}

		private State state;
		private State last_state;

		private float pos;
		private Vector3 orig_pos;

		private Vector3 lastHandPlane;

		private bool debug = false;

#if !(UNITY_EDITOR || UNITY_5)
		public override void Start()
        {
			base.Start();
			orig_pos = objectToMove.localPosition;
        }
		public override void BeginInteraction(FVRViveHand hand)
		{
			base.BeginInteraction(hand);
			switch (direction)
			{
				case Direction.X:
					this.lastHandPlane = Vector3.ProjectOnPlane(this.m_hand.transform.up, root.right);
					break;
				case Direction.Y:
					this.lastHandPlane = Vector3.ProjectOnPlane(this.m_hand.transform.up, root.forward);
					break;
				case Direction.Z:
					this.lastHandPlane = Vector3.ProjectOnPlane(this.m_hand.transform.forward, -root.up);
					break;
				default:
					break;
			}
			
		}

		public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            switch (mode)
            {
                case Mode.Translation:
					TranslationMode(hand);
                    break;
                case Mode.Rotation:
					RotationMode(hand);
                    break;
                case Mode.Tilt:
                    TiltMode(hand);
                    break;
                default:
                    break;
            }
        }

		public void TranslationMode(FVRViveHand hand)
        {
			Vector3 posVector;
			switch (direction)
            {
                case Direction.X:
					posVector = GetClosestValidPoint(new Vector3(lowerLimit,0f,0f), new Vector3(upperLimit, 0f, 0f), root.InverseTransformPoint(base.m_handPos));
					pos = posVector.x;
					objectToMove.localPosition = new Vector3(pos, orig_pos.y, orig_pos.z);
					break;
                case Direction.Y:
					posVector = GetClosestValidPoint(new Vector3(0f, lowerLimit, 0f), new Vector3(0f, upperLimit, 0f), root.InverseTransformPoint(base.m_handPos));
					pos = posVector.y;
					objectToMove.localPosition = new Vector3(orig_pos.x, pos, orig_pos.z);
					break;
                case Direction.Z:
					posVector = GetClosestValidPoint(new Vector3(0f, 0f, lowerLimit), new Vector3(0f, 0f, upperLimit), root.InverseTransformPoint(base.m_handPos));
					pos = posVector.z;
					objectToMove.localPosition = new Vector3(orig_pos.x, orig_pos.y, pos);
					break;
                default:
                    break;
            }
			if (audioSource != null)
			{
				float lerp = Mathf.InverseLerp(this.lowerLimit, this.upperLimit, this.pos);
				CheckSound(lerp);
			}
		}

		public void RotationMode(FVRViveHand hand)
		{
			Vector3 lhs; 
			Vector3 rhs;
			
			switch (direction)
            {
                case Direction.X:
					lhs = Vector3.ProjectOnPlane(this.m_hand.transform.up, -base.transform.right);
					rhs = Vector3.ProjectOnPlane(this.lastHandPlane, -base.transform.right);
					pos = Mathf.Atan2(Vector3.Dot(-base.transform.right, Vector3.Cross(lhs, rhs)), Vector3.Dot(lhs, rhs)) * 57.29578f;

					this.objectToMove.localEulerAngles = new Vector3(this.pos, 0f, 0f);
					break;
                case Direction.Y:
					lhs = Vector3.ProjectOnPlane(this.m_hand.transform.forward, base.transform.up);
					rhs = Vector3.ProjectOnPlane(this.lastHandPlane, -base.transform.up);
					pos = Mathf.Atan2(Vector3.Dot(-base.transform.up, Vector3.Cross(lhs, rhs)), Vector3.Dot(lhs, rhs)) * 57.29578f;

					this.objectToMove.localEulerAngles = new Vector3(0f, this.pos, 0f);
					break;
                case Direction.Z:
					lhs = Vector3.ProjectOnPlane(this.m_hand.transform.up, -base.transform.forward);
					rhs = Vector3.ProjectOnPlane(this.lastHandPlane, -base.transform.forward);
					pos = Mathf.Atan2(Vector3.Dot(-base.transform.forward, Vector3.Cross(lhs, rhs)), Vector3.Dot(lhs, rhs)) * 57.29578f;

					this.objectToMove.localEulerAngles = new Vector3(0f, 0f, this.pos);
					break;
                default:
					lhs = Vector3.ProjectOnPlane(this.m_hand.transform.up, -base.transform.right);
					break;
            }
			this.lastHandPlane = lhs;

			if (audioSource != null)
			{
				float lerp = Mathf.InverseLerp(this.lowerLimit, this.upperLimit, this.pos);
				CheckSound(lerp);
			}
		}

		private void TiltMode(FVRViveHand hand)
		{
			Vector3 vector = base.m_handPos - this.root.position;
			Vector3 lhs = new Vector3();
			
			switch (direction)
			{
				case Direction.X:
					lhs = -this.root.transform.forward;
					vector = Vector3.ProjectOnPlane(vector, this.root.right).normalized;
					pos = Mathf.Atan2(Vector3.Dot(this.root.right, Vector3.Cross(lhs, vector)), Vector3.Dot(lhs, vector)) * 57.29578f;
					break;
				case Direction.Y:
					lhs = -this.root.transform.forward;
					vector = Vector3.ProjectOnPlane(vector, this.root.up).normalized;
					pos = Mathf.Atan2(Vector3.Dot(this.root.up, Vector3.Cross(lhs, vector)), Vector3.Dot(lhs, vector)) * 57.29578f;
					break;
				case Direction.Z:
					lhs = this.root.transform.up;
					vector = Vector3.ProjectOnPlane(vector, this.root.forward).normalized;
					pos = Mathf.Atan2(Vector3.Dot(this.root.forward, Vector3.Cross(lhs, vector)), Vector3.Dot(lhs, vector)) * 57.29578f;
					break;
				default:
					break;
			}

            if (debug)
            {
				Popcron.Gizmos.Line(this.root.position, base.m_handPos, Color.magenta);
				Popcron.Gizmos.Line(this.root.position, lhs, Color.green);
				Popcron.Gizmos.Line(this.root.position, vector, Color.red);
				Popcron.Gizmos.Line(this.root.position, Vector3.Cross(lhs, vector), Color.blue);
			}

			if (Mathf.Abs(this.pos - this.lowerLimit) < 5f)
			{
				this.pos = this.lowerLimit;
			}
			if (Mathf.Abs(this.pos - this.upperLimit) < 5f)
			{
				this.pos = this.upperLimit;
			}
			if (this.pos >= this.lowerLimit && this.pos <= this.upperLimit)
			{
				switch (direction)
				{
					case Direction.X:
						this.objectToMove.localEulerAngles = new Vector3(this.pos, 0f, 0f);
						break;
					case Direction.Y:
						this.objectToMove.localEulerAngles = new Vector3(0f, this.pos, 0f);
						break;
					case Direction.Z:
						this.objectToMove.localEulerAngles = new Vector3(0f, 0f, this.pos);
						break;
					default:
						break;
				}
				if (audioSource != null)
				{
					float lerp = Mathf.InverseLerp(this.lowerLimit, this.upperLimit, this.pos);
					CheckSound(lerp);
				}
			}
		}

		private void CheckSound(float lerp)
        {
			if (lerp < limitWiggleRoom)
			{
				this.state = State.Open;

			}
			else if (lerp > 1f - limitWiggleRoom)
			{
				this.state = State.Closed;
			}
			else
			{
				this.state = State.Mid;
			}
			if (this.state == State.Open && this.last_state != State.Open)
			{
				audioSource.PlayOneShot(openSound);
			}
			if (this.state == State.Closed && this.last_state != State.Closed)
			{
				audioSource.PlayOneShot(closeSound);
			}
			this.last_state = this.state;
		}
#endif
	}
}
