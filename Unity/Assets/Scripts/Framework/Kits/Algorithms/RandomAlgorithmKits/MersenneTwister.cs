#region Copyright & License Information
/*
 * Copyright (c) The OpenRA Developers and Contributors
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using Framework.Kits.RandomKits;

namespace Framework.Kits.RandomAlgorithmKits
{
	// Quick & dirty Mersenne Twister [MT19937] implementation
	public class MersenneTwister:IRandom
	{
		private readonly uint[] _mt = new uint[624];
		private int _index = 0;
		public int Last{ get; private set; }
		public int TotalCount { get; private set; } = 0;
		public MersenneTwister()
			: this(Environment.TickCount) { }

		public MersenneTwister(int seed)
		{
			_mt[0] = (uint)seed;
			for (var i = 1u; i < _mt.Length; i++)
				_mt[i] = 1812433253u * (_mt[i - 1] ^ (_mt[i - 1] >> 30)) + i;
		}

		public int Next()
		{
			if (_index == 0) Generate();

			var y = _mt[_index];
			y ^= y >> 11;
			y ^= (y << 7) & 2636928640;
			y ^= (y << 15) & 4022730752;
			y ^= y >> 18;

			_index = (_index + 1) % 624;
			TotalCount++;
			Last = (int)(y % int.MaxValue);
			return Last;
		}
		
		void Generate()
		{
			unchecked
			{
				for (var i = 0u; i < _mt.Length; i++)
				{
					var y = (_mt[i] & 0x80000000) | (_mt[(i + 1) % 624] & 0x7fffffff);
					_mt[i] = _mt[(i + 397u) % 624u] ^ (y >> 1);
					if ((y & 1) == 1)
						_mt[i] = _mt[i] ^ 2567483615;
				}
			}
		}
	}
}
