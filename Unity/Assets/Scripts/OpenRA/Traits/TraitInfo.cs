using Framework.Kits.YamlSerializeKits;

namespace OpenRA.Traits
{
	public abstract class TraitInfo : ITraitInfoInterface
	{
		// Value is set using reflection during TraitInfo creation
		[FieldLoader.Ignore] public readonly string InstanceName = null;

		public abstract object Create(ActorInitializer init);
	}

	public class TraitInfo<T> : TraitInfo where T : new()
	{
		public override object Create(ActorInitializer init) {
			return new T();
		}
	}
}