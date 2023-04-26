using System;

namespace Framework.Kits.RandomKits
{
	public interface IRandom
	{
		public int Next();
		public int Next(int maxValue)
		{
			return Next(0, maxValue);
		}
		public int Next(int minValue, int maxValue)
		{
			if (maxValue < minValue)
				throw new ArgumentOutOfRangeException(nameof(maxValue), "Maximum value is less than the minimum value.");

			var diff = maxValue - minValue;
			if (diff <= 1)
				return minValue;
			return minValue + Next() % diff;
		}
		
		public float NextSingle()
		{
			return Math.Abs(Next() / (float)int.MaxValue);
		}
		
		public double NextDouble()
		{
			return Math.Abs(Next() / (double)int.MaxValue);
		}
		
		public void NextBytes(byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte)Next();
		}
	}
}