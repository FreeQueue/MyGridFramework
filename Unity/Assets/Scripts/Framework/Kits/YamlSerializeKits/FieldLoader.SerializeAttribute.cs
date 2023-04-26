using System;
using System.Reflection;
using Framework.Kits.MiniYamlKits;

namespace Framework.Kits.YamlSerializeKits
{
	public static partial class FieldLoader
	{
		[AttributeUsage(AttributeTargets.Field)]
		public class SerializeAttribute : Attribute
		{
			public static readonly SerializeAttribute Default = new SerializeAttribute();

			public readonly bool Serialize;
			public bool Required;
			public bool AllowEmptyEntries;
			
			public bool DictionaryFromYamlKey;
			public bool FromYamlKey;
			public string Loader;
			public string FieldName;

			public SerializeAttribute(bool serialize = true, bool required = false, bool allowEmptyEntries = false) {
				Serialize = serialize;
				Required = required;
				AllowEmptyEntries = allowEmptyEntries;
			}

			public bool IsDefault => this == Default;

			//根据Loader获取字段的加载函数
			internal Func<MiniYaml, object> GetLoader(Type type) {
				const BindingFlags FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
											BindingFlags.FlattenHierarchy;

				if (string.IsNullOrEmpty(Loader)) return null;
				MethodInfo method = type.GetMethod(Loader, FLAGS);
				if (method == null) {
					throw new InvalidOperationException(
						$"{type.Name} does not specify a loader function '{Loader}'");
				}

				return (Func<MiniYaml, object>)Delegate.CreateDelegate(typeof(Func<MiniYaml, object>), method);
			}
		}
	}
}