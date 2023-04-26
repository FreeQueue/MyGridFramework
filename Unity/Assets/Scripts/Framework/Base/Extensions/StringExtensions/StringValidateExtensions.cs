using System;

namespace Framework.Extensions
{
	public static class StringValidateExtensions
	{
		public static bool IsUppercase(this string str)
		{
			return string.CompareOrdinal(str.ToUpperInvariant(), str) == 0;
		}

		public static string Format(this string fmt, params object[] args)
		{
			return string.Format(fmt, args);
		}
	}
}