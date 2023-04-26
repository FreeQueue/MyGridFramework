using System;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.TimerWheelKits
{
	/// <summary>
	///     循环任务
	/// </summary>
	public class LoopTask : IReference
	{
		/// <summary>
		///     是否循环
		/// </summary>
		public bool IsLoop { get; private set; }

		/// <summary>
		///     回调(起始时间和当前任务)
		/// </summary>
		private Action<long, LoopTask> CallBack { get; set; }

		/// <summary>
		///     循环类型
		/// </summary>
		private LoopType LoopType { get; set; }

		/// <summary>
		///     循环频率
		/// </summary>
		private int RateCount { get; set; }

		/// <summary>
		///     最新次数
		/// </summary>
		private int LastCount { get; set; }

		/// <summary>
		///     开始时间
		/// </summary>
		private long StarTime { get; set; }

		/// <summary>
		///     开始时间
		/// </summary>
		private Action CancelAction { get; set; }

		public void Clear() {
			IsLoop = default;
			CallBack = default;
			LoopType = default;
			RateCount = default;
			StarTime = default;
			if (CancelAction != null) {
				CancelAction -= Stop;
			}
			CancelAction = null;
		}

		public static LoopTask Create(
			long startTime, Action<long, LoopTask> callback, LoopType loopType, int rateCount, Action cancelAction
		) {
			var loopTask = ReferencePool.Get<LoopTask>();
			loopTask.IsLoop = true;
			loopTask.StarTime = startTime;
			loopTask.CallBack = callback;
			loopTask.LoopType = loopType;
			loopTask.RateCount = rateCount;
			loopTask.CancelAction = cancelAction;
			if (cancelAction != null) {
				loopTask.CancelAction += loopTask.Stop;
			}
			return loopTask;
		}

		public void Update(TimingWheelManager timingWheelModule) {
			switch (LoopType) {
				case LoopType.Frame: {
					LastCount++;
					if (LastCount == RateCount) {
						CallBack(StarTime, this);
						LastCount = 0;
					}
				}
					break;
				case LoopType.Millisecond:
					if (LastCount == 0) {
						LastCount = -1;
						timingWheelModule.AddTask(TimeSpan.FromMilliseconds(RateCount), MillisecondCallBack);
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void MillisecondCallBack(bool result) {
			LastCount = 0;
			CallBack(StarTime, this);
		}

		public void Stop() {
			IsLoop = false;
		}
	}
}