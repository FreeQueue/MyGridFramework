using System;

namespace Framework.Extensions
{
	public static class SystemEnumExts
	{
		public static bool HasFlag<T>(this T @this, T flag) where T : Enum, IConvertible
		{
			ulong num = @this.ToUInt64(null);
			return (@this.ToUInt64(null) & num) == num;
		}
	}
}