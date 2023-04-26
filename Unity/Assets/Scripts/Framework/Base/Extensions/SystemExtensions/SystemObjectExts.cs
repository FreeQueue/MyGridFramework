/****************************************************************************
 * Copyright (c) 2015 - 2022 liangxiegame UNDER MIT License
 * 
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/


using System;

namespace Framework.Extensions
{
	public static class SystemObjectExts
	{
		public static T Self<T>(this T self, Action<T> onDo)
		{
			onDo.Invoke(self);
			return self;
		}

		public static T Self<T>(this T self, Func<T, T> onDo)
		{
			return onDo.Invoke(self);
		}

		public static bool IsNull<T>(this T selfObj) where T : class
		{
			return selfObj is null;
		}

		public static bool IsNotNull<T>(this T selfObj) where T : class
		{
			return selfObj is not null;
		}

		public static T As<T>(this object selfObj) where T : class
		{
			return selfObj as T;
		}

		public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
		{
			if (val.CompareTo(min) < 0) {
				return min;
			}
			if (val.CompareTo(max) > 0) {
				return max;
			}
			return val;
		}
	}
}