namespace Framework
{
	partial class Util
	{
		public static class String
		{
			public static string TrimToNull(string s)
			{
				if (s == null)
					return null;
				s = s.Trim();
				return s.Length == 0 ? null : s;
			}
			
		}
	}
}