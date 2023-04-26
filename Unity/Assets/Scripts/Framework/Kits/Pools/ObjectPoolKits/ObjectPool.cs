//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Kits.UGF_CollectionKits;
using Framework.Kits.ReferencePoolKits;
using UnityEngine;

namespace Framework.Kits.ObjectPoolKits
{
	/// <summary>
	///     对象池。
	/// </summary>
	/// <typeparam name="T">实例类型</typeparam>
	/// <typeparam name="TObject">对象类型。</typeparam>
	public sealed class ObjectPool<T, TObject> : ObjectPoolBase, IObjectPool<T>
		where TObject : Object<T>, new() where T : class
	{
		private readonly List<TObject> _cachedCanReleaseObjects;
		private readonly List<TObject> _cachedToReleaseObjects;
		private readonly Dictionary<T, TObject> _objectMap;
		private readonly MultiDictionary<string, TObject> _objects;
		private float _autoReleaseTime;
		private int _capacity;
		private float _expireTime;
		/// <summary>
		///     获取对象池对象类型。
		/// </summary>
		public override Type ObjectType => typeof(TObject);

		/// <summary>
		///     获取对象池中对象的数量。
		/// </summary>
		public override int Count => _objectMap.Count;

		/// <summary>
		///     获取对象池中能被释放的对象的数量。
		/// </summary>
		public override int CanReleaseCount {
			get {
				GetCanReleaseObjects(_cachedCanReleaseObjects);
				return _cachedCanReleaseObjects.Count;
			}
		}

		/// <summary>
		///     获取是否允许对象被多次获取。
		/// </summary>
		public override bool AllowMultiSpawn { get; }

		/// <summary>
		///     获取或设置对象池自动释放可释放对象的间隔秒数。
		/// </summary>
		public override float AutoReleaseInterval { get; set; }

		/// <summary>
		///     获取或设置对象池的容量。
		/// </summary>
		public override int Capacity {
			get => _capacity;
			set {
				Debug.Assert(value >= 0, "Capacity is invalid.");
				if (_capacity == value) {
					return;
				}
				_capacity = value;
				Release();
			}
		}

		/// <summary>
		///     获取或设置对象池对象过期秒数。
		/// </summary>
		public override float ExpireTime {
			get => _expireTime;
			set {
				Debug.Assert(value >= 0, "Capacity is invalid.");
				if (Math.Abs(ExpireTime - value) < 0.1f) {
					return;
				}
				_expireTime = value;
				Release();
			}
		}

		/// <summary>
		///     获取或设置对象池的优先级。
		/// </summary>
		public override int Priority { get; set; }

		/// <summary>
		///     初始化对象池的新实例。
		/// </summary>
		/// <param name="name">对象池名称。</param>
		/// <param name="allowMultiSpawn">是否允许对象被多次获取。</param>
		/// <param name="autoReleaseInterval">对象池自动释放可释放对象的间隔秒数。</param>
		/// <param name="capacity">对象池的容量。</param>
		/// <param name="expireTime">对象池对象过期秒数。</param>
		/// <param name="priority">对象池的优先级。</param>
		public ObjectPool(
			string name, bool allowMultiSpawn, float autoReleaseInterval, int capacity, float expireTime, int priority
		)
			: base(name)
		{
			_objects = new MultiDictionary<string, TObject>();
			_objectMap = new Dictionary<T, TObject>();
			_cachedCanReleaseObjects = new List<TObject>();
			_cachedToReleaseObjects = new List<TObject>();
			AllowMultiSpawn = allowMultiSpawn;
			AutoReleaseInterval = autoReleaseInterval;
			Capacity = capacity;
			ExpireTime = expireTime;
			Priority = priority;
			_autoReleaseTime = 0f;
		}

		public override void Update(float realElapseSeconds)
		{
			_autoReleaseTime += realElapseSeconds;
			if (_autoReleaseTime < AutoReleaseInterval) {
				return;
			}
			Release();
		}

		public override void Clear()
		{
			foreach (TObject obj in _objectMap.Values) {
				obj.Release(true);
				ReferencePool.Release(obj);
			}
			_objects.Clear();
			_objectMap.Clear();
			_cachedCanReleaseObjects.Clear();
			_cachedToReleaseObjects.Clear();
		}

		private TObject GetObject(T target)
		{
			return _objectMap.TryGetValue(target, out TObject obj) ? obj : null;
		}

		private void GetCanReleaseObjects(List<TObject> results)
		{
			results.Clear();
			results.AddRange(from obj in _objectMap.Values
				where !obj.IsInUse && obj.CustomCanReleaseFlag
				select obj);
		}
		public override ObjectInfo[] GetAllObjectInfos()
		{
			var list = from objectRanges in _objects
				from obj in objectRanges.Value
				select new ObjectInfo(obj.Name, obj.CustomCanReleaseFlag, obj.LastUseTime, obj.SpawnCount);

			return list.ToArray();
		}

		public void Register(string name, T target, bool spawned)
		{
			var obj = ReferencePool.Get<TObject>();
			obj.Initialize(target, name, spawned);
			_objects.Add(obj.Name, obj);
			_objectMap.Add(obj.Peek(), obj);
			if (Count > _capacity) {
				Release();
			}
		}

		public bool CanSpawn(string name)
		{
			return _objects.TryGetValue(name, out LinkedListRange<TObject> objectRange) &&
					objectRange.Any(obj => AllowMultiSpawn || !obj.IsInUse);
		}

		public T Spawn(string name)
		{
			name ??= string.Empty;

			if (_objects.TryGetValue(name, out LinkedListRange<TObject> objectRange)) {
				return (from obj in objectRange
					where AllowMultiSpawn || !obj.IsInUse
					select obj.Spawn()).FirstOrDefault();
			}
			return null;
		}

		public void Unspawn(T target)
		{
			TObject obj = GetObject(target);
			Debug.Assert(obj != null,
				$"Can not find target in object pool '{new TypeNamePair(typeof(TObject), Name)}', target type is '{target.GetType().FullName}', target value is '{target}'.");
			obj.Unspawn();
			if (Count > _capacity && obj.SpawnCount <= 0) {
				Release();
			}
		}

		/// <summary>
		///     释放对象。
		/// </summary>
		/// <param name="target">要释放的对象。</param>
		/// <returns>释放对象是否成功。</returns>
		public bool Release(T target)
		{
			TObject obj = GetObject(target);
			return ReleaseObject(obj);
		}
		private bool ReleaseObject(TObject obj)
		{
			if (obj == null) {
				return false;
			}
			if (obj.IsInUse || !obj.CustomCanReleaseFlag) {
				return false;
			}

			_objects.Remove(obj.Name, obj);
			_objectMap.Remove(obj.Peek());

			obj.Release(false);
			ReferencePool.Release(obj);
			return true;
		}


		/// <summary>
		///     释放对象池中的可释放对象。
		/// </summary>
		/// <param name="toReleaseCount">尝试释放对象数量。</param>
		public override void Release(int? toReleaseCount = null)
		{
			toReleaseCount ??= Count - _capacity;
			if (toReleaseCount < 0) {
				toReleaseCount = 0;
			}

			var expireTime = DateTime.MinValue;
			if (_expireTime < float.MaxValue) {
				expireTime = DateTime.UtcNow.AddSeconds(-_expireTime);
			}
			_autoReleaseTime = 0f;
			GetCanReleaseObjects(_cachedCanReleaseObjects);
			List<TObject> toReleaseObjects
				= FilterReleaseObject(_cachedCanReleaseObjects, toReleaseCount.Value, expireTime);
			foreach (TObject toReleaseObject in toReleaseObjects) {
				ReleaseObject(toReleaseObject);
			}
		}

		/// <summary>
		///     释放对象池中的所有未使用对象。
		/// </summary>
		public override void ReleaseAllUnused()
		{
			_autoReleaseTime = 0f;
			GetCanReleaseObjects(_cachedCanReleaseObjects);
			foreach (TObject toReleaseObject in _cachedCanReleaseObjects) {
				ReleaseObject(toReleaseObject);
			}
		}

		private List<TObject> FilterReleaseObject(
			List<TObject> candidateObjects, int toReleaseCount, DateTime expireTime
		)
		{
			_cachedToReleaseObjects.Clear();

			if (expireTime > DateTime.MinValue) {
				for (int i = candidateObjects.Count - 1; i >= 0; i--) {
					if (candidateObjects[i].LastUseTime > expireTime) continue;
					_cachedToReleaseObjects.Add(candidateObjects[i]);
					candidateObjects.RemoveAt(i);
				}

				toReleaseCount -= _cachedToReleaseObjects.Count;
			}
			_cachedToReleaseObjects.AddRange(
				candidateObjects.GetRange(0, Math.Min(toReleaseCount, candidateObjects.Count)));
			return _cachedToReleaseObjects;
		}
	}
}