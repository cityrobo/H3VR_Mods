using System;
using UnityEngine;

namespace FistVR
{
	// Token: 0x020009D9 RID: 2521
	public class ClosedBoltToggleAction : MonoBehaviour
	{
		// Token: 0x060035D9 RID: 13785 RVA: 0x0017BBB4 File Offset: 0x00179FB4
		private void Update()
		{
			float t = 1f - this.Bolt.GetBoltLerpBetweenRearAndFore();
			this.BarrelSlide.localPosition = Vector3.Lerp(this.BarrelSlideForward.localPosition, this.BarrelSlideLockPoint.localPosition, t);
			float x = Mathf.Lerp(this.RotationPiece1.x, this.RotationPiece1.y, t);
			float x2 = Mathf.Lerp(this.RotationPiece2.x, this.RotationPiece2.y, t);
			float z = Mathf.Lerp(this.TranslationPiece3.x, this.TranslationPiece3.y, t);
			Vector3 localEulerAngles = new Vector3(x, 0f, 0f);
			this.RearTogglePiece.localEulerAngles = localEulerAngles;
			Vector3 localEulerAngles2 = new Vector3(x2, 0f, 0f);
			this.CenterTogglePiece.localEulerAngles = localEulerAngles2;
			Vector3 localPosition = new Vector3(0f, this.Height, z);
			this.Breechblock.localPosition = localPosition;
		}

		// Token: 0x04005ACA RID: 23242
		public ClosedBolt Bolt;

		// Token: 0x04005ACB RID: 23243<
		public Transform BarrelSlide;

		// Token: 0x04005ACC RID: 23244
		public Transform BarrelSlideForward;

		// Token: 0x04005ACD RID: 23245
		public Transform BarrelSlideLockPoint;

		// Token: 0x04005ACE RID: 23246
		public Transform RearTogglePiece;

		// Token: 0x04005ACF RID: 23247
		public Transform CenterTogglePiece;

		// Token: 0x04005AD0 RID: 23248
		public Transform Breechblock;

		// Token: 0x04005AD1 RID: 23249
		public Vector2 RotationPiece1 = new Vector2(0f, 0f);

		// Token: 0x04005AD2 RID: 23250
		public Vector2 RotationPiece2 = new Vector2(0f, 0f);

		// Token: 0x04005AD3 RID: 23251
		public Vector2 TranslationPiece3 = new Vector2(0f, 0f);

		// Token: 0x04005AD4 RID: 23252
		public float Height = 0f;
	}
}