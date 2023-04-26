//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Kits.FsmKits
{
	/// <summary>
	///     有限状态机管理器。
	/// </summary>
	public class FsmManager
	{
		private readonly Dictionary<TypeNamePair, FsmBase> _fsms;
		private readonly List<FsmBase> _tempFsms;

		/// <summary>
		///     获取有限状态机数量。
		/// </summary>
		public int Count => _fsms.Count;

		/// <summary>
		///     初始化有限状态机管理器的新实例。
		/// </summary>
		public FsmManager() {
			_fsms = new Dictionary<TypeNamePair, FsmBase>();
			_tempFsms = new List<FsmBase>();
		}

		/// <summary>
		///     有限状态机管理器轮询。
		/// </summary>
		/// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
		/// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
		public void Update(float elapseSeconds, float realElapseSeconds) {
			_tempFsms.Clear();
			if (_fsms.Count <= 0) {
				return;
			}

			foreach (KeyValuePair<TypeNamePair, FsmBase> fsm in _fsms) {
				_tempFsms.Add(fsm.Value);
			}

			foreach (FsmBase fsm in _tempFsms.Where(fsm => !fsm.IsDestroyed)) {
				fsm.Update(elapseSeconds, realElapseSeconds);
			}
		}

		/// <summary>
		///     关闭并清理有限状态机管理器。
		/// </summary>
		public void Shutdown() {
			foreach (FsmBase fsm in _fsms.Values) {
				fsm.Shutdown();
			}
			_fsms.Clear();
			_tempFsms.Clear();
		}

		/// <summary>
		///     检查是否存在有限状态机。
		/// </summary>
		/// <typeparam name="T">有限状态机持有者类型。</typeparam>
		/// <param name="name">有限状态机名称。</param>
		/// <returns>是否存在有限状态机。</returns>
		public bool HasFsm<T>(string name = null) where T : class {
			return InternalHasFsm(new TypeNamePair(typeof(T), name));
		}

		/// <summary>
		///     检查是否存在有限状态机。
		/// </summary>
		/// <param name="ownerType">有限状态机持有者类型。</param>
		/// <param name="name">有限状态机名称。</param>
		/// <returns>是否存在有限状态机。</returns>
		public bool HasFsm(Type ownerType, string name = null) {
			return InternalHasFsm(new TypeNamePair(ownerType, name));
		}


		/// <summary>
		///     获取有限状态机。
		/// </summary>
		/// <typeparam name="T">有限状态机持有者类型。</typeparam>
		/// <param name="name">有限状态机名称。</param>
		/// <returns>要获取的有限状态机。</returns>
		public Fsm<T> GetFsm<T>(string name = null) where T : class {
			return (Fsm<T>)InternalGetFsm(new TypeNamePair(typeof(T), name));
		}

		/// <summary>
		///     获取有限状态机。
		/// </summary>
		/// <param name="ownerType">有限状态机持有者类型。</param>
		/// <param name="name">有限状态机名称。</param>
		/// <returns>要获取的有限状态机。</returns>
		public FsmBase GetFsm(Type ownerType, string name = null) {
			return InternalGetFsm(new TypeNamePair(ownerType, name));
		}

		/// <summary>
		///     获取所有有限状态机。
		/// </summary>
		/// <returns>所有有限状态机。</returns>
		public FsmBase[] GetAllFsms() {
			int index = 0;
			FsmBase[] results = new FsmBase[_fsms.Count];
			foreach (FsmBase fsm in _fsms.Values) {
				results[index++] = fsm;
			}
			return results;
		}

		/// <summary>
		///     获取所有有限状态机。
		/// </summary>
		/// <param name="results">所有有限状态机。</param>
		public void GetAllFsms(List<FsmBase> results) {
			results.Clear();
			results.AddRange(_fsms.Values);
		}

		/// <summary>
		///     创建有限状态机。
		/// </summary>
		/// <typeparam name="T">有限状态机持有者类型。</typeparam>
		/// <param name="owner">有限状态机持有者。</param>
		/// <param name="states">有限状态机状态集合。</param>
		/// <returns>要创建的有限状态机。</returns>
		public Fsm<T> CreateFsm<T>(T owner, params FsmState<T>[] states) where T : class {
			return CreateFsm(string.Empty, owner, states);
		}

		/// <summary>
		///     创建有限状态机。
		/// </summary>
		/// <typeparam name="T">有限状态机持有者类型。</typeparam>
		/// <param name="name">有限状态机名称。</param>
		/// <param name="owner">有限状态机持有者。</param>
		/// <param name="states">有限状态机状态集合。</param>
		/// <returns>要创建的有限状态机。</returns>
		public Fsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states) where T : class {
			var typeNamePair = new TypeNamePair(typeof(T), name);
			Fsm<T> fsm = Fsm<T>.Create(name, owner, states);
			_fsms.Add(typeNamePair, fsm);
			return fsm;
		}

		/// <summary>
		///     创建有限状态机。
		/// </summary>
		/// <typeparam name="T">有限状态机持有者类型。</typeparam>
		/// <param name="owner">有限状态机持有者。</param>
		/// <param name="states">有限状态机状态集合。</param>
		/// <returns>要创建的有限状态机。</returns>
		public Fsm<T> CreateFsm<T>(T owner, List<FsmState<T>> states) where T : class {
			return CreateFsm(string.Empty, owner, states);
		}

		/// <summary>
		///     创建有限状态机。
		/// </summary>
		/// <typeparam name="T">有限状态机持有者类型。</typeparam>
		/// <param name="name">有限状态机名称。</param>
		/// <param name="owner">有限状态机持有者。</param>
		/// <param name="states">有限状态机状态集合。</param>
		/// <returns>要创建的有限状态机。</returns>
		public Fsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states) where T : class {
			var typeNamePair = new TypeNamePair(typeof(T), name);
			Fsm<T> fsm = Fsm<T>.Create(name, owner, states);
			_fsms.Add(typeNamePair, fsm);
			return fsm;
		}

		/// <summary>
		///     销毁有限状态机。
		/// </summary>
		/// <typeparam name="T">有限状态机持有者类型。</typeparam>
		/// <param name="name">要销毁的有限状态机名称。</param>
		/// <returns>是否销毁有限状态机成功。</returns>
		public bool DestroyFsm<T>(string name = null) where T : class {
			return InternalDestroyFsm(new TypeNamePair(typeof(T), name));
		}

		/// <summary>
		///     销毁有限状态机。
		/// </summary>
		/// <param name="ownerType">有限状态机持有者类型。</param>
		/// <param name="name">要销毁的有限状态机名称。</param>
		/// <returns>是否销毁有限状态机成功。</returns>
		public bool DestroyFsm(Type ownerType, string name = null) {
			return InternalDestroyFsm(new TypeNamePair(ownerType, name));
		}

		/// <summary>
		///     销毁有限状态机。
		/// </summary>
		/// <typeparam name="T">有限状态机持有者类型。</typeparam>
		/// <param name="fsm">要销毁的有限状态机。</param>
		/// <returns>是否销毁有限状态机成功。</returns>
		public bool DestroyFsm<T>(Fsm<T> fsm) where T : class {
			return InternalDestroyFsm(new TypeNamePair(typeof(T), fsm.Name));
		}

		/// <summary>
		///     销毁有限状态机。
		/// </summary>
		/// <param name="fsm">要销毁的有限状态机。</param>
		/// <returns>是否销毁有限状态机成功。</returns>
		public bool DestroyFsm(FsmBase fsm) {
			return InternalDestroyFsm(new TypeNamePair(fsm.OwnerType, fsm.Name));
		}

		private bool InternalHasFsm(TypeNamePair typeNamePair) {
			return _fsms.ContainsKey(typeNamePair);
		}

		private FsmBase InternalGetFsm(TypeNamePair typeNamePair) {
			return _fsms.TryGetValue(typeNamePair, out FsmBase fsm) ? fsm : null;
		}

		private bool InternalDestroyFsm(TypeNamePair typeNamePair) {
			if (!_fsms.TryGetValue(typeNamePair, out FsmBase fsm)) return false;
			fsm.Shutdown();
			return _fsms.Remove(typeNamePair);
		}
	}
}