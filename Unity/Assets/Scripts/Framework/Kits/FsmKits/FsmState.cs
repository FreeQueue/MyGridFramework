//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;

namespace Framework.Kits.FsmKits
{
    /// <summary>
    ///     有限状态机状态基类。
    /// </summary>
    /// <typeparam name="T">有限状态机持有者类型。</typeparam>
    public abstract class FsmState<T> where T : class
	{
        /// <summary>
        ///     初始化有限状态机状态基类的新实例。
        /// </summary>
        public FsmState() {
		}

        /// <summary>
        ///     有限状态机状态初始化时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        protected internal virtual void OnInit(Fsm<T> fsm) {
		}

        /// <summary>
        ///     有限状态机状态进入时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        protected internal virtual void OnEnter(Fsm<T> fsm) {
		}

        /// <summary>
        ///     有限状态机状态轮询时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate(Fsm<T> fsm, float elapseSeconds, float realElapseSeconds) {
		}

        /// <summary>
        ///     有限状态机状态离开时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="isShutdown">是否是关闭有限状态机时触发。</param>
        protected internal virtual void OnLeave(Fsm<T> fsm, bool isShutdown) {
		}

        /// <summary>
        ///     有限状态机状态销毁时调用。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        protected internal virtual void OnDestroy(Fsm<T> fsm) {
		}

        /// <summary>
        ///     切换当前有限状态机状态。
        /// </summary>
        /// <typeparam name="TState">要切换到的有限状态机状态类型。</typeparam>
        /// <param name="fsm">有限状态机引用。</param>
        protected void ChangeState<TState>(Fsm<T> fsm) where TState : FsmState<T> {
			fsm.ChangeState<TState>();
		}

        /// <summary>
        ///     切换当前有限状态机状态。
        /// </summary>
        /// <param name="fsm">有限状态机引用。</param>
        /// <param name="stateType">要切换到的有限状态机状态类型。</param>
        protected void ChangeState(Fsm<T> fsm, Type stateType) {
			fsm.ChangeState(stateType);
		}
	}
}