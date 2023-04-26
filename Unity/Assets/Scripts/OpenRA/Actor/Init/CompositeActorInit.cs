using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Kits.MiniYamlKits;
using Framework.Kits.YamlSerializeKits;
using OpenRA.Traits;

namespace OpenRA
{
	//
	public abstract class CompositeActorInit : ActorInit
	{
		protected CompositeActorInit(TraitInfo info)
			: base(info.InstanceName) {
		}

		protected CompositeActorInit() {
		}

		public virtual void Initialize(MiniYaml yaml) {
			FieldLoader.Load(this, yaml);
		}

		public virtual void Initialize(Dictionary<string, object> values) {
			foreach (FieldInfo field in GetType()
						.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				FieldLoader.SerializeAttribute sa = field.GetCustomAttributes<FieldLoader.SerializeAttribute>(false)
					.DefaultIfEmpty(FieldLoader.SerializeAttribute.Default).First();
				if (!sa.Serialize) continue;

				if (values.TryGetValue(field.Name, out object value)) {
					field.SetValue(this, value);
				}
			}
		}

		public virtual Dictionary<string, Type> InitializeArgs() {
			Dictionary<string, Type> dict = new Dictionary<string, Type>();
			foreach (FieldInfo field in GetType()
						.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)) {
				FieldLoader.SerializeAttribute sa = field.GetCustomAttributes<FieldLoader.SerializeAttribute>(false)
					.DefaultIfEmpty(FieldLoader.SerializeAttribute.Default).First();
				if (!sa.Serialize) continue;

				dict[field.Name] = field.FieldType;
			}

			return dict;
		}

		public override MiniYaml Save() {
			return FieldSaver.Save(this);
		}
	}
}