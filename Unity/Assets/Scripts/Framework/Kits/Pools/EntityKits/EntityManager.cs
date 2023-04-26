using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Framework.Extensions;
using Framework.Kits.ObjectPoolKits;
using Framework.Kits.ResourceKits;
using UnityEngine;

namespace Framework.Kits.EntityKits
{
	public sealed class EntityManager
	{
		private readonly Dictionary<string, EntityGroup> _entityGroups = new Dictionary<string, EntityGroup>();
		private readonly Dictionary<string, EntityInfo> _entityInfos = new Dictionary<string, EntityInfo>();
		private readonly Dictionary<int, Entity> _entities = new Dictionary<int, Entity>();
		private readonly ObjectPoolManager _objectPoolManager;
		private bool _isShutdown;
		private int _idMaker;
		public Transform EntityGroupRoot { get; }

		public EntityManager(
			Transform entityGroupRoot, ObjectPoolManager objectPoolManager
		)
		{
			EntityGroupRoot = entityGroupRoot;
			_objectPoolManager = objectPoolManager;
		}

		public void Update(float elapseSeconds, float realElapseSeconds)
		{
			foreach (EntityGroup entityGroup in _entityGroups.Values.Where(entityGroup => entityGroup.Active)) {
				entityGroup.Update(elapseSeconds, realElapseSeconds);
			}
		}
		public void Shutdown()
		{
			_isShutdown = true;
			List<Entity> entities = new List<Entity>();
			GetAllEntities(entities);
			foreach (Entity entity in entities) {
				HideEntity(entity);
			}
			foreach (EntityGroup entityGroup in _entityGroups.Values) {
				entityGroup.Clear();
			}
		}

		#region EntityGroup
		public bool HasEntityGroup(string entityGroupName)
		{
			return _entityGroups.ContainsKey(entityGroupName);
		}

		public EntityGroup GetEntityGroup(string entityGroupName)
		{
			return _entityGroups.TryGetValue(entityGroupName, out EntityGroup entityGroup) ? entityGroup : null;
		}

		public void SetEntityGroupActive(string entityGroupName, bool active)
		{
			if (_entityGroups.TryGetValue(entityGroupName, out EntityGroup entityGroup)) {
				entityGroup.Active = active;
			}
		}
		/// <summary>
		/// 增加实体组。
		/// </summary>
		/// <param name="entityGroupName">实体组名称。</param>
		/// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
		/// <param name="instanceCapacity">实体实例对象池容量。</param>
		/// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
		/// <param name="instancePriority">实体实例对象池的优先级。</param>
		/// <returns>是否增加实体组成功。</returns>
		public bool AddEntityGroup(
			string entityGroupName,
			float? instanceAutoReleaseInterval = null,
			int? instanceCapacity = null,
			float? instanceExpireTime = null,
			int? instancePriority = null
		)
		{
			if (HasEntityGroup(entityGroupName)) {
				return false;
			}
			_entityGroups.Add(entityGroupName,
				new EntityGroup(entityGroupName, this, _objectPoolManager, instanceAutoReleaseInterval,
					instanceCapacity, instanceExpireTime, instancePriority));
			return true;
		}
		#endregion

		#region EntityInfo
		public void AddEntityInfo(string entityName, string entityGroupName, string entityAssetName)
		{
			_entityInfos.Add(entityName,
				EntityInfo.Create(entityName, GetEntityGroup(entityGroupName), entityAssetName));
		}
		public EntityInfo GetEntityInfo(string entityName)
		{
			return _entityInfos[entityName];
		}
		#endregion

		public Entity GetEntityByRuntimeId(int id)
		{
			return _entities.TryGetValue(id, out Entity entity) ? entity : null;
		}
		public Entity GetEntity(string entityName)
		{
			return GetEntityInfo(entityName).EntityGroup.GetEntity(entityName);
		}

		public Entity[] GetEntities(string entityName)
		{
			return GetEntityInfo(entityName).EntityGroup.GetEntities(entityName);
		}

		public void GetEntities(string entityName, List<Entity> results)
		{
			GetEntityInfo(entityName).EntityGroup.GetEntities(entityName, results);
		}

		public void GetAllEntities(List<Entity> result)
		{
			foreach (EntityGroup entityGroup in _entityGroups.Values) {
				entityGroup.GetAllEntities(result);
			}
		}
		public async UniTask<Entity> ShowEntityAsync(
			string entityName, object userData = null,
			IProgress<float> progress = null
		)
		{
			EntityInfo entityInfo = GetEntityInfo(entityName);
			EntityGroup entityGroup = entityInfo.EntityGroup;
			Entity entity = entityGroup.Spawn(entityName);
			if (entity == null) {
				GameObject gameObject = await ResourceManager.InstantiateGameObjectAsync(entityName, progress);
				gameObject.SetActive(true);
				entity = gameObject.GetOrAddComponent<Entity>();
				entityGroup.Register(entityName, entity, true);
				entity.Init(++_idMaker, entityInfo);
			}
			_entities.Add(entity.Id, entity);
			entity.OnShow(userData);
			return entity;
		}

		public Entity ShowEntitySync(
			string entityName, object userData = null
		)
		{
			EntityInfo entityInfo = GetEntityInfo(entityName);
			EntityGroup entityGroup = entityInfo.EntityGroup;
			Entity entity = entityGroup.Spawn(entityName);
			if (entity == null) {
				GameObject gameObject = ResourceManager.InstantiateGameObjectSync(entityName);
				gameObject.SetActive(true);
				entity = gameObject.GetOrAddComponent<Entity>();
				entityGroup.Register(entityName, entity, true);
				entity.Init(++_idMaker, entityInfo);
			}
			_entities.Add(entity.Id, entity);
			entity.OnShow(userData);
			return entity;
		}

		public void HideEntity(Entity entity, object userData = null)
		{
			entity.OnHide(_isShutdown, userData);
			EntityGroup entityGroup = entity.Info.EntityGroup;
			entityGroup.Unspawn(entity);
			_entities.Remove(entity.Id);
		}
	}
}