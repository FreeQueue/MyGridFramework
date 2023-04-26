using System;

namespace Framework.Kits.RandomKits
{
	public class SystemRandom : IRandom
	{
		private readonly Random _random;

		public SystemRandom()
		{
			_random = new Random();
		}

		public SystemRandom(int seed)
		{
			_random = new Random(seed);
		}

		public int Next()
		{
			return _random.Next();
		}
		public int Next(int maxValue)
		{
			return _random.Next(maxValue);
		}
		public int Next(int minValue, int maxValue)
		{
			return _random.Next(minValue, maxValue);
		}

		public void NextBytes(byte[] b)
		{
			_random.NextBytes(b);
		}

		public double NextDouble()
		{
			return _random.NextDouble();
		}

	}
}