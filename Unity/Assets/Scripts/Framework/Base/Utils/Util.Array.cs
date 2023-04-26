using System;

namespace Framework
{
	partial class Util
	{
		public static class Array
		{
			public static T[] Make<T>(int count, Func<int, T> f)
			{
				var result = new T[count];
				for (var i = 0; i < count; i++)
					result[i] = f(i);

				return result;
			}

			public static T[,] Resize<T>(T[,] array, int width, int height, T defaultValue = default)
			{
				var result = new T[width, height];
				for (var i = 0; i < width; i++) {
					for (var j = 0; j < height; j++) {
						// Workaround for broken ternary operators in certain versions of mono
						// (3.10 and certain versions of the 3.8 series): https://bugzilla.xamarin.com/show_bug.cgi?id=23319
						if (i <= array.GetUpperBound(0) && j <= array.GetUpperBound(1))
							result[i, j] = array[i, j];
						else
							result[i, j] = defaultValue;
					}
				}
				return result;
			}
		}
	}
}