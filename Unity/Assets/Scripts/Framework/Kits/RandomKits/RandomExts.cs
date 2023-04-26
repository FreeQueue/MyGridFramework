using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Kits.RandomAlgorithmKits;

namespace Framework.Kits.RandomKits
{
	public static class RandomExts
	{
		public static T Random<T>(this IEnumerable<T> @this, IRandom random)
		{
			return Random(@this, random, true);
		}

		public static T RandomOrDefault<T>(this IEnumerable<T> @this, IRandom random)
		{
			return Random(@this, random, false);
		}

		private static T Random<T>(IEnumerable<T> ts, IRandom random, bool throws)
		{
			var xs = ts as ICollection<T>;
			xs ??= ts.ToList();
			if (xs.Count != 0) return xs.ElementAt(random.Next(xs.Count));
			if (throws)
				throw new ArgumentException("Collection must not be empty.", nameof(ts));
			return default;
		}
	}
}