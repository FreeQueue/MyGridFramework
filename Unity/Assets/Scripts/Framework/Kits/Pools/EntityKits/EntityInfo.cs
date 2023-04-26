using System;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.EntityKits
{
	public class EntityInfo : IReference
	{
		public string EntityName { get; private set; }
		public EntityGroup EntityGroup { get; private set; }
		public string EntityAssetName { get; private set; }

		public static EntityInfo Create(string entityName, EntityGroup entityGroup, string entityAssetName) {
			var info = ReferencePool.Get<EntityInfo>();
			info.EntityName = entityName;
			info.EntityGroup = entityGroup;
			info.EntityAssetName = entityAssetName;
			return info;
		}
		public void Clear() {
			EntityGroup = null;
		}
	}
}