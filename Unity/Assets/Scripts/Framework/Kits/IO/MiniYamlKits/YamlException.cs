using System;

namespace Framework.Kits.MiniYamlKits
{
	[Serializable]
	public class YamlException : Exception
	{
		public YamlException(string s)
			: base(s) { }
	}
}