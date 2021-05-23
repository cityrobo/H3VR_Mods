using System;
using UnityEngine;

namespace FistVR
{
	public class SchockhammerToggleAction : MonoBehaviour
	{
		private void Update()
		{
			float t = 1f - this.Bolt.GetBoltLerpBetweenRearAndFore();
			this.BarrelSlide.localPosition = Vector3.Lerp(this.BarrelSlideForward.localPosition, this.BarrelSlideLockPoint.localPosition, t);
			float x = Mathf.Lerp(this.RotSet1.x, this.RotSet1.y, t);
			float x2 = Mathf.Lerp(this.RotSet2.x, this.RotSet2.y, t);
			float z = Mathf.Lerp(this.PosSet1.x, this.PosSet1.y, t);
			Vector3 localEulerAngles = new Vector3(x, 0f, 0f);
			this.TogglePiece1.localEulerAngles = localEulerAngles;
			Vector3 localEulerAngles2 = new Vector3(x2, 0f, 0f);
			this.TogglePiece2.localEulerAngles = localEulerAngles2;
			Vector3 localPosition = new Vector3(0f, this.Height, z);
			this.TogglePiece3.localPosition = localPosition;
		}

		public ClosedBolt Bolt;
		public Transform BarrelSlide;
		public Transform BarrelSlideForward;
		public Transform BarrelSlideLockPoint;
		public Transform TogglePiece1;
		public Transform TogglePiece2;
		public Transform TogglePiece3;
		public Vector2 RotSet1 = new Vector2(0f, -86f);
		public Vector2 RotSet2 = new Vector2(0f, 132.864f);
		public Vector2 PosSet1 = new Vector2(0.02199817f, -0.02124f);
		public float Height = 0.03527606f;
	}
}