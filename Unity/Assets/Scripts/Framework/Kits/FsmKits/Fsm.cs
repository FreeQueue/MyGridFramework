//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using Framework.Kits.DataNodeKits;
using Framework.Kits.ReferencePoolKits;
using UnityEngine;

namespace Framework.Kits.FsmKits
{
	/// <summary>
	///     有限状态机。
	/// </summary>
	/// <typeparam name="T">有限状态机持有者类型。</typeparam>
	public sealed class Fsm<T> : FsmBase, IReference where T : class
	{
		private readonly Dictionary<Type, FsmState<T>> _states;
		private float _currentStateTime;
		private Dictionary<string, Var> _datas;
		private bool _isDestroyed;

		/// <summary>
		///     初始化有限状态机的新实例。
		/// </summary>
		public Fsm()
		{
			Owner = null;
			_states = new Dictionary<Type, FsmState<T>>();
			_datas = null;
			CurrentState = null;
			_currentStateTime = 0f;
			_isDestroyed = true;
		}
		/// <summary>
		///     获取有限状态机持有者。
		/// </summary>
		public T Owner { get; private set; }

		/// <summary>
		///     获取有限状态机持有者类型。
		/// </summary>
		public override Type OwnerType => typeof(T);

		/// <summary>
		///     获取有限状态机中状态的数量。
		/// </summary>
		public override int FsmStateCount => _states.Count;

		/// <summary>
		///     获取有限状态机是否正在运行。
		/// </summary>
		public override bool IsRunning => CurrentState != null;

		/// <summary>
		///     获取有限状态机是否被销毁。
		/// </summary>
		public override bool IsDestroyed => _isDestroyed;

		/// <summary>
		///     获取当前有限状态机状态。
		/// </summary>
		public FsmState<T> CurrentState { get; private set; }

		/// <summary>
		///     获取当前有限状态机状态名称。
		/// </summary>
		public override string CurrentStateName => CurrentState?.GetType().FullName;

		/// <summary>
		///     获取当前有限状态机状态持续时间。
		/// </summary>
		public override float CurrentStateTime => _currentStateTime;

		/// <summary>
		///     清理有限状态机。
		/// </summary>
		public void Clear()
		{
			CurrentState?.OnLeave(this, true);
			foreach (KeyValuePair<Type, FsmState<T>> state in _states) {
				state.Value.OnDestroy(this);
			}
			Name = null;
			Owner = null;
			_states.Clear();

			if (_datas != null) {
				foreach (KeyValuePair<string, Var> data in _datas) {
					if (data.Value == null) {
						continue;
					}

					ReferencePool.Release(data.Value);
				}

				_datas.Clear();
			}
			CurrentState = null;
			_currentStateTime = 0f;
			_isDestroyed = true;
		}

		/// <summary>
		///     创建有限状态机。
		/// </summary>
		/// <param name="name">有限状态机名称。</param>
		/// <param name="owner">有限状态机持有者。</param>
		/// <param name="states">有限状态机状态集合。</param>
		/// <returns>创建的有限状态机。</returns>
		public static Fsm<T> Create(string name, T owner, params FsmState<T>[] states)
		{
			Fsm<T> fsm = ReferencePool.Get<Fsm<T>>();
			fsm.Name = name;
			fsm.Owner = owner;
			fsm._isDestroyed = false;
			foreach (FsmState<T> state in states) {
				Type stateType = state.GetType();
				fsm._states.Add(stateType, state);
				state.OnInit(fsm);
			}
			return fsm;
		}

		/// <summary>
		///     创建有限状态机。
		/// </summary>
		/// <param name="name">有限状态机名称。</param>
		/// <param name="owner">有限状态机持有者。</param>
		/// <param name="states">有限状态机状态集合。</param>
		/// <returns>创建的有限状态机。</returns>
		public static Fsm<T> Create(string name, T owner, List<FsmState<T>> states)
		{
			Fsm<T> fsm = ReferencePool.Get<Fsm<T>>();
			fsm.Name = name;
			fsm.Owner = owner;
			fsm._isDestroyed = false;
			foreach (FsmState<T> state in states) {
				Type stateType = state.GetType();
				fsm._states.Add(stateType, state);
				state.OnInit(fsm);
			}
			return fsm;
		}

		/// <summary>
		///     开始有限状态机。
		/// </summary>
		/// <typeparam name="TState">要开始的有限状态机状态类型。</typeparam>
		public void Start<TState>() where TState : FsmState<T>
		{
			Debug.Assert(!IsRunning,"FSM is running, can not start again.");
			FsmState<T> state = GetState<TState>();
			CurrentState = state;
			_currentStateTime = 0f;
			CurrentState.OnEnter(this);
		}

		/// <summary>
		///     开始有限状态机。
		/// </summary>
		/// <param name="stateType">要开始的有限状态机状态类型。</param>
		public void Start(Type stateType)
		{
			Debug.Assert(!IsRunning,"FSM is running, can not start again.");
			Debug.Assert(typeof(FsmState<T>).IsAssignableFrom(stateType),
				$"State type '{stateType.FullName}' is invalid.");

			FsmState<T> state = GetState(stateType);
			CurrentState = state;
			_currentStateTime = 0f;
			CurrentState.OnEnter(this);
		}

		/// <summary>
		///     是否存在有限状态机状态。
		/// </summary>
		/// <typeparam name="TState">要检查的有限状态机状态类型。</typeparam>
		/// <returns>是否存在有限状态机状态。</returns>
		public bool HasState<TState>() where TState : FsmState<T>
		{
			return _states.ContainsKey(typeof(TState));
		}

		/// <summary>
		///     是否存在有限状态机状态。
		/// </summary>
		/// <param name="stateType">要检查的有限状态机状态类型。</param>
		/// <returns>是否存在有限状态机状态。</returns>
		public bool HasState(Type stateType)
		{
			Debug.Assert(typeof(FsmState<T>).IsAssignableFrom(stateType),
				$"State type '{stateType.FullName}' is invalid.");
			return _states.ContainsKey(stateType);
		}

		/// <summary>
		///     获取有限状态机状态。
		/// </summary>
		/// <typeparam name="TState">要获取的有限状态机状态类型。</typeparam>
		/// <returns>要获取的有限状态机状态。</returns>
		public TState GetState<TState>() where TState : FsmState<T>
		{
			if (_states.TryGetValue(typeof(TState), out FsmState<T> state)) {
				return (TState)state;
			}
			return null;
		}

		/// <summary>
		///     获取有限状态机状态。
		/// </summary>
		/// <param name="stateType">要获取的有限状态机状态类型。</param>
		/// <returns>要获取的有限状态机状态。</returns>
		public FsmState<T> GetState(Type stateType)
		{
			if (!typeof(FsmState<T>).IsAssignableFrom(stateType)) {
				throw new InvalidOperationException($"State type '{stateType.FullName}' is invalid.");
			}
			return _states.TryGetValue(stateType, out FsmState<T> state) ? state : null;

		}

		/// <summary>
		///     获取有限状态机的所有状态。
		/// </summary>
		/// <returns>有限状态机的所有状态。</returns>
		public FsmState<T>[] GetAllStates()
		{
			int index = 0;
			FsmState<T>[] results = new FsmState<T>[_states.Count];
			foreach (KeyValuePair<Type, FsmState<T>> state in _states) {
				results[index++] = state.Value;
			}

			return results;
		}

		/// <summary>
		///     获取有限状态机的所有状态。
		/// </summary>
		/// <param name="results">有限状态机的所有状态。</param>
		public void GetAllStates(List<FsmState<T>> results)
		{
			results.Clear();
			foreach (KeyValuePair<Type, FsmState<T>> state in _states) {
				results.Add(state.Value);
			}
		}

		/// <summary>
		///     是否存在有限状态机数据。
		/// </summary>
		/// <param name="name">有限状态机数据名称。</param>
		/// <returns>有限状态机数据是否存在。</returns>
		public bool HasData(string name) => _datas != null && _datas.ContainsKey(name);

		/// <summary>
		///     获取有限状态机数据。
		/// </summary>
		/// <typeparam name="TData">要获取的有限状态机数据的类型。</typeparam>
		/// <param name="name">有限状态机数据名称。</param>
		/// <returns>要获取的有限状态机数据。</returns>
		public TData GetData<TData>(string name) where TData : Var
		{
			return (TData)GetData(name);
		}

		/// <summary>
		///     获取有限状态机数据。
		/// </summary>
		/// <param name="name">有限状态机数据名称。</param>
		/// <returns>要获取的有限状态机数据。</returns>
		public Var GetData(string name)
		{
			if (_datas == null) {
				return null;
			}
			return _datas.TryGetValue(name, out Var data) ? data : null;

		}

		/// <summary>
		///     设置有限状态机数据。
		/// </summary>
		/// <typeparam name="TData">要设置的有限状态机数据的类型。</typeparam>
		/// <param name="name">有限状态机数据名称。</param>
		/// <param name="data">要设置的有限状态机数据。</param>
		public void SetData<TData>(string name, TData data) where TData : Var
		{
			SetData(name, (Var)data);
		}

		/// <summary>
		///     设置有限状态机数据。
		/// </summary>
		/// <param name="name">有限状态机数据名称。</param>
		/// <param name="data">要设置的有限状态机数据。</param>
		public void SetData(string name, Var data)
		{
			_datas ??= new Dictionary<string, Var>(StringComparer.Ordinal);

			Var oldData = GetData(name);
			if (oldData != null) {
				ReferencePool.Release(oldData);
			}
			_datas[name] = data;
		}

		/// <summary>
		///     移除有限状态机数据。
		/// </summary>
		/// <param name="name">有限状态机数据名称。</param>
		/// <returns>是否移除有限状态机数据成功。</returns>
		public bool RemoveData(string name)
		{
			if (_datas == null) {
				return false;
			}
			Var oldData = GetData(name);
			if (oldData != null) {
				ReferencePool.Release(oldData);
			}
			return _datas.Remove(name);
		}

		/// <summary>
		///     有限状态机轮询。
		/// </summary>
		/// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
		/// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
		internal override void Update(float elapseSeconds, float realElapseSeconds)
		{
			if (CurrentState == null) {
				return;
			}
			_currentStateTime += elapseSeconds;
			CurrentState.OnUpdate(this, elapseSeconds, realElapseSeconds);
		}

		/// <summary>
		///     关闭并清理有限状态机。
		/// </summary>
		internal override void Shutdown()
		{
			ReferencePool.Release(this);
		}

		/// <summary>
		///     切换当前有限状态机状态。
		/// </summary>
		/// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
		internal void ChangeState<TState>() where TState : FsmState<T>
		{
			ChangeState(typeof(TState));
		}

		/// <summary>
		///     切换当前有限状态机状态。
		/// </summary>
		/// <param name="stateType">要切换到的有限状态机状态类型。</param>
		internal void ChangeState(Type stateType)
		{
			FsmState<T> state = GetState(stateType);
			CurrentState.OnLeave(this, false);
			_currentStateTime = 0f;
			CurrentState = state;
			CurrentState.OnEnter(this);
		}
	}
}