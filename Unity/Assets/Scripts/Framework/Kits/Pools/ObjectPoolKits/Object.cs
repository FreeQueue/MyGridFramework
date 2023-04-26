//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using Framework.Kits.ReferencePoolKits;
using UnityEngine;

namespace Framework.Kits.ObjectPoolKits
{

	public abstract class Object<T> : IReference where T : class
	{
		protected T target;

		/// <summary>
		///     获取对象名称。
		/// </summary>
		public string Name { get; internal set; }

		/// <summary>
		///     获取自定义释放检查标记。
		/// </summary>
		public virtual bool CustomCanReleaseFlag => true;

		/// <summary>
		///     获取对象上次使用时间。
		/// </summary>
		public DateTime LastUseTime { get; internal set; }

		/// <summary>
		/// 获取对象是否正在使用。
		/// </summary>
		public bool IsInUse => SpawnCount > 0;

		/// <summary>
		/// 获取对象的获取计数。
		/// </summary>
		public int SpawnCount { get; internal set; }

		void IReference.Clear() {
			target = null;
			Name = null;
			LastUseTime = default;
			SpawnCount = 0;
			Clear();
		}

		public virtual void Clear() {
		}
		/// <summary>
		///     初始化对象基类。
		/// </summary>
		/// <param name="name">对象名称。</param>
		/// <param name="target">对象。</param>
		/// <param name="spawned">是否已生成</param>
		internal void Initialize(
			T target, string name, bool spawned
		) {
			Debug.Assert(target != null, "Target is invalid.");
			this.target = target;
			Name = name??string.Empty;
			LastUseTime = DateTime.UtcNow;
			SpawnCount = spawned ? 1 : 0;
		}
		/// <summary>
		/// 查看对象。
		/// </summary>
		/// <returns>对象。</returns>
		internal T Peek() {
			return target;
		}

		/// <summary>
		/// 获取对象。
		/// </summary>
		/// <returns>对象。</returns>
		internal T Spawn() {
			SpawnCount++;
			LastUseTime = DateTime.UtcNow;
			OnSpawn();
			return target;
		}

		protected virtual void OnSpawn() {

		}

		/// <summary>
		/// 回收对象。
		/// </summary>
		internal void Unspawn() {
			OnUnspawn();
			LastUseTime = DateTime.UtcNow;
			SpawnCount--;
			Debug.Assert(SpawnCount >= 0, $"Object '{Name}' spawn count is less than 0.");
		}

		protected virtual void OnUnspawn() {

		}

		/// <summary>
		/// 释放对象。
		/// </summary>
		/// <param name="isShutdown">是否是关闭对象池时触发。</param>
		protected internal abstract void Release(bool isShutdown);
	}
}