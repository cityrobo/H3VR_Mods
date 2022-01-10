using System;

namespace ShotTimer
{
	[Flags]
	public enum Inputs
	{
		MenuUp = 1 << 0,
		MenuDown = 1 << 1,
		SetUp = 1 << 2,
		SetDown = 1 << 3,
		Start = 1 << 4
	}

	public static class ExtInputs
	{
		public static Inputs Encode(this Inputs @this, bool v)
		{
			return v ? @this : 0;
		}

		public static bool Decode(this Inputs @this, Inputs flag)
		{
			return (@this & flag) == flag;
		}
	}
}