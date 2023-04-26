//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Framework.Kits.FsmKits;
using ProcedureOwner = Framework.Kits.FsmKits.Fsm<Framework.Kits.ProcedureKits.ProcedureManager>;

namespace Framework.Kits.ProcedureKits
{
    /// <summary>
    ///     流程基类。
    /// </summary>
    public abstract class ProcedureBase : FsmState<ProcedureManager>
	{
        /// <summary>
        ///     状态初始化时调用。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        protected internal override void OnInit(ProcedureOwner procedureOwner) {
		}

        /// <summary>
        ///     进入状态时调用。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        protected internal override void OnEnter(ProcedureOwner procedureOwner) {
		}

        /// <summary>
        ///     状态轮询时调用。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected internal override void OnUpdate(
			ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds
		) {
		}

        /// <summary>
        ///     离开状态时调用。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        /// <param name="isShutdown">是否是关闭状态机时触发。</param>
        protected internal override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown) {
		}

        /// <summary>
        ///     状态销毁时调用。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        protected internal override void OnDestroy(ProcedureOwner procedureOwner) {
		}
	}
}