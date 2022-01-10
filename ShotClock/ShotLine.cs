namespace ShotTimer
{
	public struct ShotLine
	{
		public readonly float Time;
		public readonly float Split;
		
		public ShotLine(float time, float split)
		{
			Time = time;
			Split = split;
		}
	}
}