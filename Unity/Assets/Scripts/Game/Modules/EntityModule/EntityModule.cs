using System;
using System.Collections.Generic;
using cfg;
using cfg.entity;
using Cysharp.Threading.Tasks;
using Framework;
using Framework.Kits.EntityKits;
using Helper = S100.Modules.EntityModuleHelper;

namespace S100.Modules
{
	public class EntityModule : IModule
	{
		private EntityManager _entityManager;
		void IModule.Init() {
			Tables tables = GameEntry.Tables;
			_entityManager = new EntityManager(EntityModuleHelper.Instance.instanceRoot, FrameworkEntry.GetModule<ObjectPoolModule>());
			foreach (EntityGroupData data in tables.TbEntityGroup.DataList) {
				_entityManager.AddEntityGroup(data.Id.ToString(), data.AutoReleaseInterval,
					data.Capacity, data.ExpireTime, data.Priority);
			}
			AddEntityInfos(tables.TbEntity.DataList);
		}
		void IModule.Update(float elapseSeconds, float realElapseSeconds) {
			_entityManager.Update(elapseSeconds, realElapseSeconds);
		}
		void IModule.Shutdown() {
			_entityManager.Shutdown();
		}
		
		public Entity GetEntityByRuntimeId(int runtimeId) {
			return _entityManager.GetEntityByRuntimeId(runtimeId);
		}
		
		public async UniTask<Entity> ShowEntityAsync<T>(T id, object userData = null, IProgress<float> progress = null)
			where T : IConvertible {
			string name = GameEntry.Tables.TbEntity[id.ToInt32(null)].Name;
			return await _entityManager.ShowEntityAsync(name, userData, progress);
		}

		public Entity ShowEntitySync<T>(T id, object userData = null) where T : IConvertible {
			string name = GameEntry.Tables.TbEntity[id.ToInt32(null)].Name;
			return _entityManager.ShowEntitySync(name, userData);
		}

		public void HideEntity(Entity entity, object userData = null) {
			_entityManager.HideEntity(entity, userData);
		}
		public void AddEntity(EntityData entityData) {
			_entityManager.AddEntityInfo(entityData.Name, entityData.EntityGroupId.ToString(),
				entityData.AssetName);
		}

		public void AddEntityInfos(IEnumerable<EntityData> entityDatas) {
			foreach (EntityData entityData in entityDatas) {
				AddEntity(entityData);
			}
		}
	}
}