using System;
using System.Linq;
using System.Runtime.Serialization;
using Framework.Kits.MiniYamlKits;

namespace Framework.Kits.YamlSerializeKits
{
	public static partial class FieldLoader
	{
		[Serializable]
		public class MissingFieldsException : YamlException
		{
			public readonly string Header;
			public readonly string[] Missing;

			public MissingFieldsException(string[] missing, string header = null, string headerSingle = null)
				: base(null) {
				Header = missing.Length > 1 ? header : headerSingle ?? header;
				Missing = missing;
			}
			public override string Message =>
				(string.IsNullOrEmpty(Header) ? "" : Header + ": ") + Missing[0] +
				string.Concat(Missing.Skip(1).Select(m => ", " + m));

			public override void GetObjectData(SerializationInfo info, StreamingContext context) {
				base.GetObjectData(info, context);
				info.AddValue("Missing", Missing);
				info.AddValue("Header", Header);
			}
		}
	}
}