using System;

namespace Framework
{
	partial class Util
	{
		public static class Math
		{
			public static int NextPowerOf2(int v)
			{
				--v;
				v |= v >> 1;
				v |= v >> 2;
				v |= v >> 4;
				v |= v >> 8;
				++v;
				return v;
			}

			public static bool IsPowerOf2(int v)
			{
				return (v & (v - 1)) == 0;
			}

			//开平方根取整模式
			public enum SqrtRoundMode
			{
				Floor,
				Nearest,
				Ceiling
			}

			public static int Sqrt(int number, SqrtRoundMode round = SqrtRoundMode.Floor)
			{
				if (number < 0)
					throw new InvalidOperationException(
						$"Attempted to calculate the square root of a negative integer: {number}");

				return (int)Sqrt((uint)number, round);
			}

			public static uint Sqrt(uint number, SqrtRoundMode round = SqrtRoundMode.Floor)
			{
				var divisor = 1U << 30;

				var root = 0U;
				var remainder = number;

				// Find the highest term in the divisor
				while (divisor > number)
					divisor >>= 2;

				// Evaluate the root, two bits at a time
				while (divisor != 0) {
					if (root + divisor <= remainder) {
						remainder -= root + divisor;
						root += 2 * divisor;
					}

					root >>= 1;
					divisor >>= 2;
				}

				switch (round) {
					// Adjust for other rounding modes
					case SqrtRoundMode.Nearest when remainder > root:
					case SqrtRoundMode.Ceiling when root * root < number: root += 1;
						break;
				}

				return root;
			}

			public static long Sqrt(long number, SqrtRoundMode round = SqrtRoundMode.Floor)
			{
				if (number < 0)
					throw new InvalidOperationException(
						$"Attempted to calculate the square root of a negative integer: {number}");

				return (long)Sqrt((ulong)number, round);
			}

			public static ulong Sqrt(ulong number, SqrtRoundMode round = SqrtRoundMode.Floor)
			{
				var divisor = 1UL << 62;

				var root = 0UL;
				var remainder = number;

				// Find the highest term in the divisor
				while (divisor > number)
					divisor >>= 2;

				// Evaluate the root, two bits at a time
				while (divisor != 0) {
					if (root + divisor <= remainder) {
						remainder -= root + divisor;
						root += 2 * divisor;
					}

					root >>= 1;
					divisor >>= 2;
				}

				switch (round) {
					// Adjust for other rounding modes
					case SqrtRoundMode.Nearest when remainder > root:
					case SqrtRoundMode.Ceiling when root * root < number: root += 1;
						break;
				}

				return root;
			}

			public static int MultiplyBySqrtTwo(short number)
			{
				return number * 46341 / 32768;
			}

			public static int IntegerDivisionRoundingAwayFromZero(int dividend, int divisor)
			{
				var quotient = System.Math.DivRem(dividend, divisor, out var remainder);
				if (remainder == 0)
					return quotient;
				return quotient + (System.Math.Sign(dividend) == System.Math.Sign(divisor) ? 1 : -1);
			}
		}
	}
}