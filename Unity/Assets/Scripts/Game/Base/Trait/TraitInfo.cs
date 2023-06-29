using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace S100
{
	[Serializable]
	public abstract class TraitInfo : ITraitInfo
	{
		public readonly string InstanceName = null;
		public abstract object Create(ActorInitializer init);
	}

	public class TraitInfo<T> : TraitInfo where T : new()
	{
		public override object Create(ActorInitializer init) {
			return new T();
		}
	}

	public static class TraitInfoExtensions
	{
		public static List<Type> GetPrerequisites(this TraitInfo @this) {
			var types = CustomAttributeExtensions.GetCustomAttributes<RequireTraitAttribute>(@this.GetType())
				.SelectMany(attribute => attribute.Types)
				.Distinct()
				.Where(type => {
					if (!typeof(TraitInfo).IsAssignableFrom(type)) {
						throw new ArgumentException(
							$"PrerequisitesOf({@this.GetType()}) contains {type} which is not a TraitInfo.");
					}
					return true;
				});
			return types.ToList();
		}

		public static List<Type> OptionalPrerequisitesOf(this TraitInfo @this) {
			var types = CustomAttributeExtensions.GetCustomAttributes<OptionTraitAttribute>(@this.GetType())
				.SelectMany(attribute => attribute.Types)
				.Distinct()
				.Where(type => {
					if (!typeof(TraitInfo).IsAssignableFrom(type)) {
						throw new ArgumentException(
							$"OptionalPrerequisitesOf({@this.GetType()}) contains {type} which is not a TraitInfo.");
					}
					return true;
				});
			return types.ToList();
		}
	}
}