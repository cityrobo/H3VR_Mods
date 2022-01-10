using System;
using System.Text;
using UnityEngine;

namespace ShotTimer
{
	[Serializable]
	public class PaddingSettings
	{
		public char Character;
		public int Size;

		public PaddingSettings() : this('_', 2) { }

		public PaddingSettings(char character, int size)
		{
			Character = character;
			Size = size;
		}
		
		public void Apply(StringBuilder builder, int used)
		{
			builder.Append(Character, Size - used);
		}
		
		public void Apply(StringBuilder builder, float n)
		{
			Apply(builder, (int) Mathf.Log10(n));
		}

		public string Render(int used)
		{
			return new string(Character, Size - used);
		}

		public string Render(float n)
		{
			return Render((int) Mathf.Log10(n));
		}
	}
}