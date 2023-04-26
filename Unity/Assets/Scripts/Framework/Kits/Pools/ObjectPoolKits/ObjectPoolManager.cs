//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using Framework.Kits.ReferencePoolKits;
using System.Linq;
using UnityEngine;

namespace Framework.Kits.ObjectPoolKits
{

	/// <summary>
	///     对象池管理器。
	/// </summary>
	public class ObjectPoolManager : IReference
	{
		private readonly List<ObjectPoolBase> _cachedAllObjectPools;
		private readonly Dictionary<TypeNamePair, ObjectPoolBase> _objectPools;

		/// <summary>
		///     获取对象池数量。
		/// </summary>
		public int Count => _objectPools.Count;

		public ObjectPoolManager()
		{
			_objectPools = new Dictionary<TypeNamePair, ObjectPoolBase>();
			_cachedAllObjectPools = new List<ObjectPoolBase>();
		}

		public void Update(float realElapseSeconds)
		{
			foreach (KeyValuePair<TypeNamePair, ObjectPoolBase> objectPool in _objectPools) {
				objectPool.Value.Update(realElapseSeconds);
			}
		}

		/// <summary>
		///     关闭并清理对象池管理器。
		/// </summary>
		public void Clear()
		{
			foreach (var objectPool in _objectPools) {
				objectPool.Value.Clear();
			}
			_objectPools.Clear();
			_cachedAllObjectPools.Clear();
		}

		/// <summary>
		///     检查是否存在对象池。
		/// </summary>
		/// <typeparam name="TObject">对象类型。</typeparam>
		/// <param name="name">对象池名称。</param>
		/// <returns>是否存在对象池。</returns>
		public bool HasObjectPool<TObject>(string name = null)
		{
			CheckObjectType(typeof(TObject));
			return InternalHasObjectPool(new TypeNamePair(typeof(TObject), name));
		}

		public static void CheckObjectType(Type objectType)
		{
			Debug.Assert(typeof(Object<>).IsAssignableFrom(objectType),
				$"Object type '{objectType.FullName}' is invalid.");
		}

		/// <summary>
		///     检查是否存在对象池。
		/// </summary>
		/// <param name="objectType">对象类型。</param>
		/// <param name="name">对象池名称。</param>
		/// <returns>是否存在对象池。</returns>
		public bool HasObjectPool(Type objectType, string name = null)
		{
			CheckObjectType(objectType);
			return InternalHasObjectPool(new TypeNamePair(objectType, name));
		}


		/// <summary>
		///     检查是否存在对象池。
		/// </summary>
		/// <param name="condition">要检查的条件。</param>
		/// <returns>是否存在对象池。</returns>
		public bool HasObjectPool(Predicate<ObjectPoolBase> condition)
		{
			return _objectPools.Any(objectPool => condition(objectPool.Value));
		}

		/// <summary>
		///     获取对象池。
		/// </summary>
		/// <typeparam name="T">实例类型。</typeparam>
		/// <typeparam name="TObject">对象类型</typeparam>
		/// <param name="name">对象池名称。</param>
		/// <returns>要获取的对象池。</returns>
		public ObjectPool<T, TObject> GetObjectPool<T, TObject>(string name = null)
			where TObject : Object<T>, new() where T : class
		{
			return (ObjectPool<T, TObject>)InternalGetObjectPool(new TypeNamePair(typeof(TObject), name));
		}

		/// <summary>
		///     获取对象池。
		/// </summary>
		/// <param name="objectType">对象类型。</param>
		/// <param name="name">对象池名称。</param>
		/// <returns>要获取的对象池。</returns>
		public ObjectPoolBase GetObjectPool(Type objectType, string name = null)
		{
			CheckObjectType(objectType);
			return InternalGetObjectPool(new TypeNamePair(objectType, name));
		}

		/// <summary>
		///     获取对象池。
		/// </summary>
		/// <param name="condition">要检查的条件。</param>
		/// <returns>要获取的对象池。</returns>
		public ObjectPoolBase GetObjectPool(Predicate<ObjectPoolBase> condition)
		{
			return (from objectPool in _objectPools
				where condition(objectPool.Value)
				select objectPool.Value).FirstOrDefault();
		}

		/// <summary>
		///     获取对象池。
		/// </summary>
		/// <param name="condition">要检查的条件。</param>
		/// <param name="results">要获取的对象池。</param>
		public void GetObjectPools(Predicate<ObjectPoolBase> condition, List<ObjectPoolBase> results)
		{
			results.Clear();
			results.AddRange(from objectPool in _objectPools
				where condition(objectPool.Value)
				select objectPool.Value);
		}

		/// <summary>
		///     获取所有对象池。
		/// </summary>
		/// <param name="sort">是否根据对象池的优先级排序。</param>
		/// <param name="results">所有对象池。</param>
		public void GetAllObjectPools(List<ObjectPoolBase> results, bool sort = false)
		{
			results.Clear();
			results.AddRange(_objectPools.Select(objectPool => objectPool.Value));
			if (sort) {
				results.Sort(ObjectPoolComparer);
			}
		}

		/// <summary>
		///     创建允许单次获取的对象池。
		/// </summary>
		/// <typeparam name="T">实例类型。</typeparam>
		/// <typeparam name="TObject">对象类型</typeparam>
		/// <param name="name">对象池名称。</param>
		/// <param name="autoReleaseInterval">对象池自动释放可释放对象的间隔秒数。</param>
		/// <param name="capacity">对象池的容量。</param>
		/// <param name="expireTime">对象池对象过期秒数。</param>
		/// <param name="priority">对象池的优先级。</param>
		/// <returns>要创建的允许单次获取的对象池。</returns>
		public ObjectPool<T, TObject> CreateSingleSpawnObjectPool<T, TObject>(
			string name = null,
			float? autoReleaseInterval = null,
			int? capacity = null,
			float? expireTime = null,
			int? priority = null
		) where T : class where TObject : Object<T>, new()
		{
			return InternalCreateObjectPool<T, TObject>(
				name, false, autoReleaseInterval, capacity, expireTime, priority);
		}

		/// <summary>
		///     创建允许多次获取的对象池。
		/// </summary>
		/// <typeparam name="T">实例类型。</typeparam>
		/// <typeparam name="TObject">对象类型</typeparam>
		/// <param name="name">对象池名称。</param>
		/// <param name="autoReleaseInterval">对象池自动释放可释放对象的间隔秒数。</param>
		/// <param name="capacity">对象池的容量。</param>
		/// <param name="expireTime">对象池对象过期秒数。</param>
		/// <param name="priority">对象池的优先级。</param>
		/// <returns>要创建的允许多次获取的对象池。</returns>
		public ObjectPool<T, TObject> CreateMultiSpawnObjectPool<T, TObject>(
			string name = null,
			float? autoReleaseInterval = null,
			int? capacity = null,
			float? expireTime = null,
			int? priority = null
		) where T : class where TObject : Object<T>, new()
		{
			return InternalCreateObjectPool<T, TObject>(name, true, autoReleaseInterval, capacity, expireTime,
				priority);
		}

		public bool DestroyObjectPool<T, TObject>(string name = null) where TObject : Object<T> where T : class
		{
			return InternalDestroyObjectPool(new TypeNamePair(typeof(TObject), name));
		}

		public bool DestroyObjectPool(Type objectType, string name = null)
		{
			CheckObjectType(objectType);
			return InternalDestroyObjectPool(new TypeNamePair(objectType, name));
		}

		public bool DestroyObjectPool(ObjectPoolBase objectPool)
		{
			return DestroyObjectPool(objectPool.ObjectType, objectPool.Name);
		}

		/// <summary>
		///     释放对象池中的可释放对象。
		/// </summary>
		public void Release()
		{
			GetAllObjectPools(_cachedAllObjectPools, true);
			foreach (ObjectPoolBase objectPool in _cachedAllObjectPools) {
				objectPool.Release();
			}
		}

		/// <summary>
		///     释放对象池中的所有未使用对象。
		/// </summary>
		public void ReleaseAllUnused()
		{
			GetAllObjectPools(_cachedAllObjectPools, true);
			foreach (ObjectPoolBase objectPool in _cachedAllObjectPools) {
				objectPool.ReleaseAllUnused();
			}
		}

		private bool InternalHasObjectPool(TypeNamePair typeNamePair)
		{
			return _objectPools.ContainsKey(typeNamePair);
		}

		private ObjectPoolBase InternalGetObjectPool(TypeNamePair typeNamePair)
		{
			return _objectPools.TryGetValue(typeNamePair, out ObjectPoolBase objectPool) ? objectPool : null;
		}

		private ObjectPool<T, TObject> InternalCreateObjectPool<T, TObject>(
			string name, bool allowMultiSpawn,
			float? autoReleaseInterval = null,
			int? capacity = null,
			float? expireTime = null,
			int? priority = null
		) where T : class where TObject : Object<T>, new()
		{
			var typeNamePair = new TypeNamePair(typeof(T), name);
			Debug.Assert(!InternalHasObjectPool(typeNamePair),
				$"Already exist object pool '{typeNamePair.ToString()}'.");
			autoReleaseInterval ??= ObjectPoolConstant.DEFAULT_EXPIRE_TIME;
			capacity ??= ObjectPoolConstant.DEFAULT_CAPACITY;
			expireTime ??= ObjectPoolConstant.DEFAULT_EXPIRE_TIME;
			priority ??= ObjectPoolConstant.DEFAULT_PRIORITY;
			ObjectPool<T, TObject> objectPool =
				new ObjectPool<T, TObject>(name, allowMultiSpawn,
					autoReleaseInterval.Value, capacity.Value,
					expireTime.Value, priority.Value);
			_objectPools.Add(typeNamePair, objectPool);
			return objectPool;
		}

		private bool InternalDestroyObjectPool(TypeNamePair typeNamePair)
		{
			if (!_objectPools.TryGetValue(typeNamePair, out ObjectPoolBase objectPool)) return false;
			objectPool.Clear();
			return _objectPools.Remove(typeNamePair);
		}

		private static int ObjectPoolComparer(ObjectPoolBase a, ObjectPoolBase b)
		{
			return a.Priority.CompareTo(b.Priority);
		}
	}
}