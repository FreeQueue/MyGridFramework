using System.Collections.Generic;
using System.IO;
using System.Linq;
using Framework.Kits.ObjectPoolKits;
using UnityEngine;

namespace Framework.Kits.EntityKits
{

	public sealed partial class EntityGroup
	{
		private readonly Transform _entityPoolRoot;
		private readonly ObjectPool<Entity, EntityObject> _hideEntityPool;
		private readonly Queue<(bool, Entity)> _addOrRemoveEntities;
		private readonly Dictionary<int, Entity> _showEntities;
		/// <summary>
		/// 获取实体组名称。
		/// </summary>
		public string Name { get; }
		public bool Active { get; internal set; }
		/// <summary>
		/// 获取实体组中实体数量。
		/// </summary>
		public int EntityCount => _showEntities.Count;

		/// <summary>
		/// 初始化实体组的新实例。
		/// </summary>
		/// <param name="name">实体组名称。</param>
		/// <param name="entityManager">实体管理器</param>
		/// <param name="objectPoolManager">对象池管理器。</param>
		/// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
		/// <param name="instanceCapacity">实体实例对象池容量。</param>
		/// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
		/// <param name="instancePriority">实体实例对象池的优先级。</param>
		public EntityGroup(
			string name, EntityManager entityManager, ObjectPoolManager objectPoolManager,
			float? instanceAutoReleaseInterval = null,
			int? instanceCapacity = null,
			float? instanceExpireTime = null,
			int? instancePriority = null
		) {
			if (string.IsNullOrEmpty(name)) throw new InvalidDataException("Entity group name is invalid.");
			
			Name = name;
			_entityPoolRoot = new GameObject($"Entity Group ({Name})").transform;
			_entityPoolRoot.SetParent(entityManager.EntityGroupRoot);
			_hideEntityPool = objectPoolManager.CreateSingleSpawnObjectPool<Entity, EntityObject>(
				$"Entity Pool ({name})", instanceAutoReleaseInterval,
				instanceCapacity, instanceExpireTime, instancePriority);
			_showEntities = new Dictionary<int, Entity>();
			_addOrRemoveEntities = new Queue<(bool, Entity)>();
			Active = true;
		}

		public void Update(float elapseSeconds, float realElapseSeconds) {
			foreach (Entity entity in _showEntities.Values) {
				entity.OnUpdate(elapseSeconds, realElapseSeconds);
			}
			while (_addOrRemoveEntities.Count > 0) {
				(bool, Entity) operation = _addOrRemoveEntities.Dequeue();
				if (operation.Item1) {
					_showEntities.Add(operation.Item2.Id, operation.Item2);
				}
				else {
					_showEntities.Remove(operation.Item2.Id);
				}
			}
		}

		public void Clear() {
			_hideEntityPool.ReleaseAllUnused();
		}
		public bool HasEntityByRuntimeId(int id) => _showEntities.ContainsKey(id);
		public Entity GetEntityByRuntimeId(int id) {
			_showEntities.TryGetValue(id, out Entity entity);
			return entity;
		}

		public bool HasEntity(string name) => _showEntities.Values.Any(entity => entity.Info.EntityName == name);
		public Entity GetEntity(string name) =>
			_showEntities.Values.FirstOrDefault(entity => entity.Info.EntityName == name);
		public Entity[] GetEntities(string name) =>
			_showEntities.Values.Where(entity => entity.Info.EntityName == name).ToArray();
		public void GetEntities(string name, List<Entity> results) {
			results.AddRange(_showEntities.Values.Where(entity => entity.Info.EntityName == name));
		}

		/// <summary>
		/// 从实体组中获取所有实体。
		/// </summary>
		/// <returns>实体组中的所有实体。</returns>
		public Entity[] GetAllEntities() {
			return _showEntities.Values.ToArray();
		}

		/// <summary>
		/// 从实体组中获取所有实体。
		/// </summary>
		/// <param name="results">实体组中的所有实体。</param>
		public void GetAllEntities(List<Entity> results) {
			results.AddRange(_showEntities.Values);
		}

		/// <summary>
		/// 往实体组增加实体。
		/// </summary>
		/// <param name="entity">要增加的实体。</param>
		private void AddEntity(Entity entity) {
			_addOrRemoveEntities.Enqueue((true, entity));
		}

		/// <summary>
		/// 从实体组移除实体。
		/// </summary>
		/// <param name="entity">要移除的实体。</param>
		private void RemoveEntity(Entity entity) {
			_addOrRemoveEntities.Enqueue((false, entity));
		}

		public void Register(string name, Entity entity, bool spawned) {
			_hideEntityPool.Register(name, entity, spawned);
			if (spawned) AddEntity(entity);
		}

		public bool CanSpawn(string name) {
			return _hideEntityPool.CanSpawn(name);
		}

		public Entity Spawn(string name) {
			Entity entity = _hideEntityPool.Spawn(name);
			if (entity != null) AddEntity(entity);
			return entity;
		}

		public void Unspawn(Entity entity) {
			_hideEntityPool.Unspawn(entity);
			RemoveEntity(entity);
		}
	}
}