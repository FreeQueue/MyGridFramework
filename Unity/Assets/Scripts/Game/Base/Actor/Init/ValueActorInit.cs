using System.Reflection;
using Framework.Kits.MiniYamlKits;
using Framework.Kits.YamlSerializeKits;

namespace S100
{
	public abstract class ValueActorInit<TValue> : ActorInit
	{
		private readonly TValue _value;

		protected ValueActorInit(TraitInfo info, TValue value)
			: base(info.InstanceName) {
			_value = value;
		}

		protected ValueActorInit(string instanceName, TValue value)
			: base(instanceName) {
			_value = value;
		}

		protected ValueActorInit(TValue value) {
			_value = value;
		}

		public virtual TValue Value => _value;

		public virtual void Initialize(MiniYaml yaml) {
			Initialize((TValue)FieldLoader.GetValue(nameof(_value), typeof(TValue), yaml.Value));
		}

		public virtual void Initialize(TValue value) {
			typeof(ValueActorInit<TValue>)
				.GetField(nameof(value), BindingFlags.NonPublic | BindingFlags.Instance)
				?.SetValue(this, value);
		}

		public override MiniYaml Save() {
			return new MiniYaml(FieldSaver.FormatValue(_value));
		}
	}
}