//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using Framework.Kits.FsmKits;
using UnityEngine;

namespace Framework.Kits.ProcedureKits
{
	/// <summary>
	///     流程管理器。
	/// </summary>
	public class ProcedureManager
	{
		private FsmManager _fsmManager;
		private Fsm<ProcedureManager> _procedureFsm;

		/// <summary>
		///     初始化流程管理器的新实例。
		/// </summary>
		public ProcedureManager() {
			_fsmManager = null;
			_procedureFsm = null;
		}

		/// <summary>
		///     获取当前流程。
		/// </summary>
		public ProcedureBase CurrentProcedure => (ProcedureBase)_procedureFsm.CurrentState;

		/// <summary>
		///     获取当前流程持续时间。
		/// </summary>
		public float CurrentProcedureTime => _procedureFsm.CurrentStateTime;

		/// <summary>
		///     关闭并清理流程管理器。
		/// </summary>
		public void Clear() {
			if (_fsmManager == null) return;
			if (_procedureFsm != null) {
				_fsmManager.DestroyFsm(_procedureFsm);
				_procedureFsm = null;
			}

			_fsmManager = null;
		}

		/// <summary>
		///     初始化流程管理器。
		/// </summary>
		/// <param name="fsmManager">有限状态机管理器。</param>
		/// <param name="procedures">流程管理器包含的流程。</param>
		public void Initialize(FsmManager fsmManager, params ProcedureBase[] procedures) {
			Debug.Assert(fsmManager != null, "FSM manager is invalid.");
			_fsmManager = fsmManager;
			_procedureFsm = _fsmManager.CreateFsm(this, procedures);
		}

		/// <summary>
		///     开始流程。
		/// </summary>
		/// <typeparam name="T">要开始的流程类型。</typeparam>
		public void StartProcedure<T>() where T : ProcedureBase {
			_procedureFsm.Start<T>();
		}

		/// <summary>
		///     开始流程。
		/// </summary>
		/// <param name="procedureType">要开始的流程类型。</param>
		public void StartProcedure(Type procedureType) {
			_procedureFsm.Start(procedureType);
		}

		/// <summary>
		///     是否存在流程。
		/// </summary>
		/// <typeparam name="T">要检查的流程类型。</typeparam>
		/// <returns>是否存在流程。</returns>
		public bool HasProcedure<T>() where T : ProcedureBase {
			return _procedureFsm.HasState<T>();
		}

		/// <summary>
		///     是否存在流程。
		/// </summary>
		/// <param name="procedureType">要检查的流程类型。</param>
		/// <returns>是否存在流程。</returns>
		public bool HasProcedure(Type procedureType) {
			return _procedureFsm.HasState(procedureType);
		}

		/// <summary>
		///     获取流程。
		/// </summary>
		/// <typeparam name="T">要获取的流程类型。</typeparam>
		/// <returns>要获取的流程。</returns>
		public ProcedureBase GetProcedure<T>() where T : ProcedureBase {
			return _procedureFsm.GetState<T>();
		}

		/// <summary>
		///     获取流程。
		/// </summary>
		/// <param name="procedureType">要获取的流程类型。</param>
		/// <returns>要获取的流程。</returns>
		public ProcedureBase GetProcedure(Type procedureType) {
			return (ProcedureBase)_procedureFsm.GetState(procedureType);
		}
	}
}