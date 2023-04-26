using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Framework.Kits.ReferencePoolKits;
using Framework.Kits.TimerWheelKits;
using UnityEngine;

namespace Framework.Kits.TimerWheelKits
{
	public class TimingWheelManager
	{
		private readonly List<FrameTask> _frameTasks = new List<FrameTask>();
		private readonly List<LoopTask> _loopTasks = new List<LoopTask>();

		private ITimer _timer;

		/// <summary>
		/// 初始化
		/// </summary>
		/// <param name="slotCount">时间槽数量</param>
		/// <param name="tickSpan">时间槽大小</param>
		public void Init(int slotCount=100,int tickSpan=100) {
			_timer = TimingWheelTimer.Build(TimeSpan.FromMilliseconds(tickSpan), slotCount);
		}

		public void Update() {
			for (int i = _loopTasks.Count - 1; i >= 0; i--) {
				if (_loopTasks[i] == null || !_loopTasks[i].IsLoop) {
					ReferencePool.Release(_loopTasks[i]);
					_loopTasks.RemoveAt(i);
					continue;
				}

				_loopTasks[i].Update(this);
			}

			int currentFrameCount = Time.frameCount;
			for (int i = _frameTasks.Count - 1; i >= 0; i--) {
				if (_frameTasks.Count > currentFrameCount) continue;
				_frameTasks[i].CallBack.Invoke();
				ReferencePool.Release(_frameTasks[i]);
				_frameTasks.RemoveAt(i);
			}
		}

		public void Clear() {

		}
		/// <summary>
		///     添加任务
		/// </summary>
		/// <param name="timeout">过期时间，相对时间</param>
		/// <param name="cancelAction">任务取消令牌</param>
		/// <returns></returns>
		public Task<bool> AddTaskAsync(TimeSpan timeout, Action cancelAction = default) {
			return _timer.AddTask(timeout, cancelAction);
		}

		/// <summary>
		///     添加任务
		/// </summary>
		/// <param name="timeout">过期时间，相对时间</param>
		/// <param name="callback">任务回调(true 成功执行  false 取消执行)</param>
		/// <returns></returns>
		public ITimeTask AddTask(TimeSpan timeout, Action<bool> callback) {
			return _timer.AddTask(timeout, callback);
		}


		/// <summary>
		///     添加任务
		/// </summary>
		/// <param name="timeoutMs">过期时间戳，绝对时间</param>
		/// <param name="cancelAction">任务取消令牌</param>
		/// <returns></returns>
		public Task<bool> AddTaskAsync(long timeoutMs, Action cancelAction = default) {
			return _timer.AddTask(timeoutMs, cancelAction);
		}

		/// <summary>
		///     添加任务
		/// </summary>
		/// <param name="timeoutMs">过期时间戳，绝对时间</param>
		/// <param name="callback">任务回调(true 成功执行  false 取消执行)</param>
		/// <returns></returns>
		public ITimeTask AddTask(long timeoutMs, Action<bool> callback) {
			return _timer.AddTask(timeoutMs, callback);
		}

		/// <summary>
		///     启动
		/// </summary>
		public void StartTimer() {
			_timer.Start();
		}

		/// <summary>
		///     停止
		/// </summary>
		public void StopTimer() {
			_timer.Stop();
		}

		/// <summary>
		///     暂停
		/// </summary>
		public void PauseTimer() {
			_timer.Pause();
		}

		/// <summary>
		///     恢复
		/// </summary>
		public void ResumeTimer() {
			_timer.Start();
		}
		
		/// <summary>
		///     添加帧定时任务
		/// </summary>
		/// <param name="callback">回调函数</param>
		/// <param name="count">延迟帧数</param>
		/// <returns></returns>
		public void AddFrameTask(Action callback, int count = 1) {
			_frameTasks.Add(FrameTask.Create(Time.frameCount + count, callback));
		}

		/// <summary>
		///     添加帧定时任务(默认1帧后执行)
		/// </summary>
		/// <param name="count">延迟帧数</param>
		/// <returns></returns>
		public async Task AddFrameTaskAsync(int count = 1) {
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

			void CallBack() {
				tcs.SetResult(true);
			}

			AddFrameTask(CallBack, count);
			await tcs.Task;
		}

		/// <summary>
		///     添加循环调用任务
		/// </summary>
		/// <param name="callback">回调</param>
		/// <param name="loopType">循环类型 0帧  1 毫秒 </param>
		/// <param name="rateCount"></param>
		/// <param name="cancelAction"></param>
		/// <returns></returns>
		public LoopTask AddLoopTask(
			Action<long, LoopTask> callback, LoopType loopType, int rateCount, Action cancelAction = default
		) {
			var task = LoopTask.Create(DateTimeHelper.GetTimestamp(), callback, loopType, rateCount, cancelAction);
			_loopTasks.Add(task);
			return task;
		}
	}

}