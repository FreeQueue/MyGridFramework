using System;

namespace Framework.Kits.YamlSerializeKits
{
	public static partial class FieldLoader
	{
		[AttributeUsage(AttributeTargets.Field)]
		public sealed class IgnoreAttribute : SerializeAttribute
		{
			public IgnoreAttribute()
				: base(false) {
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		public sealed class RequireAttribute : SerializeAttribute
		{
			public RequireAttribute()
				: base(true, true) {
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		public sealed class AllowEmptyEntriesAttribute : SerializeAttribute
		{
			public AllowEmptyEntriesAttribute()
				: base(allowEmptyEntries: true) {
			}
		}

		[AttributeUsage(AttributeTargets.Field)]
		public sealed class LoadUsingAttribute : SerializeAttribute
		{
			public LoadUsingAttribute(string loader, bool required = false) {
				Loader = loader;
				Required = required;
			}
		}
	}
}