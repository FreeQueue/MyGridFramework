/****************************************************************************
 * Copyright (c) 2015 - 2022 liangxiegame UNDER MIT License
 * 
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/


using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Framework.Extensions
{

	public static class SystemStringExtension
	{
		/// <summary>
		///     缓存
		/// </summary>
		private static readonly char[] _mCachedSplitCharArray = { '.' };

		public static bool IsNullOrEmpty(this string selfStr)
		{
			return string.IsNullOrEmpty(selfStr);
		}

		public static bool IsNotNullAndEmpty(this string selfStr)
		{
			return !string.IsNullOrEmpty(selfStr);
		}

		public static bool IsTrimNullOrEmpty(this string selfStr)
		{
			return selfStr == null || string.IsNullOrEmpty(selfStr.Trim());
		}

		/// <summary>
		///     Check Whether string trim is null or empty
		/// </summary>
		/// <param name="selfStr"></param>
		/// <returns></returns>
		public static bool IsTrimNotNullAndEmpty(this string selfStr)
		{
			return selfStr != null && !string.IsNullOrEmpty(selfStr.Trim());
		}

		public static string[] Split(this string selfStr, char splitSymbol)
		{
			_mCachedSplitCharArray[0] = splitSymbol;
			return selfStr.Split(_mCachedSplitCharArray);
		}

		public static string FillFormat(this string selfStr, params object[] args)
		{
			return string.Format(selfStr, args);
		}

		public static StringBuilder Builder(this string selfStr)
		{
			return new StringBuilder(selfStr);
		}

		public static StringBuilder AddPrefix(this StringBuilder self, string prefixString)
		{
			self.Insert(0, prefixString);
			return self;
		}

		public static int ToInt(this string selfStr, int defaulValue = 0)
		{
			int retValue = defaulValue;
			return int.TryParse(selfStr, out retValue) ? retValue : defaulValue;
		}

		public static DateTime ToDateTime(this string selfStr, DateTime defaultValue = default)
		{
			return DateTime.TryParse(selfStr, out DateTime retValue) ? retValue : defaultValue;
		}

		public static float ToFloat(this string selfStr, float defaultValue = 0)
		{
			return float.TryParse(selfStr, out float retValue) ? retValue : defaultValue;
		}

		public static bool HasChinese(this string input)
		{
			return Regex.IsMatch(input, @"[\u4e00-\u9fa5]");
		}

		public static bool HasSpace(this string input)
		{
			return input.Contains(" ");
		}

		public static string RemoveString(this string str, params string[] targets)
		{
			return targets.Aggregate(str, (current, t) => current.Replace(t, string.Empty));
		}
	}
}