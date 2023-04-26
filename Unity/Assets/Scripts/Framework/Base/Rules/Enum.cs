using System;
using System.Linq;

namespace Framework
{
	public static class Enum<T>
	{
		public static T Parse(string s) {
			return (T)Enum.Parse(typeof(T), s);
		}
		public static T[] GetValues() {
			return (T[])Enum.GetValues(typeof(T));
		}

		public static bool TryParse(string s, bool ignoreCase, out T value) {
			// 字符串可以是逗号分隔的值列表
			var names = ignoreCase
				? Enum.GetNames(typeof(T)).Select(x => x.ToLowerInvariant())
				: Enum.GetNames(typeof(T));
			var values = ignoreCase
				? s.Split(',').Select(x => x.Trim().ToLowerInvariant())
				: s.Split(',').Select(x => x.Trim());

			if (values.Any(x => !names.Contains(x))) {
				value = default;
				return false;
			}

			value = (T)Enum.Parse(typeof(T), s, ignoreCase);

			return true;
		}
	}
}