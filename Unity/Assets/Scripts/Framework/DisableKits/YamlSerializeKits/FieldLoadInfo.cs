using System;
using System.Reflection;
using Framework.Kits.MiniYamlKits;

namespace Framework.Kits.YamlSerializeKits
{
	public sealed class FieldLoadInfo
	{
		public readonly FieldLoader.SerializeAttribute Attribute;
		public readonly FieldInfo Field;
		public readonly Func<MiniYaml, object> Loader;
		public readonly string YamlName;

		internal FieldLoadInfo(
			FieldInfo field, FieldLoader.SerializeAttribute attr, string yamlName, Func<MiniYaml, object> loader = null
		) {
			Field = field;
			Attribute = attr;
			YamlName = yamlName;
			Loader = loader;
		}
	}
}