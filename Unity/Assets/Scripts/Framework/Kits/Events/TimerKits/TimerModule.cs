using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Framework.Kits.UGF_CollectionKits;
using Framework.Kits.ReferencePoolKits;
using Framework.Kits.TimerKits;
using UnityEngine;
using UnityGameFramework.Extension;
namespace Framework.Kits.TimerKits
{
    public class TimerManager
    {
        /// <summary>
        /// 存储所有的timer
        /// </summary>
        private readonly Dictionary<int, Timer> _timers = new Dictionary<int, Timer>();

        /// <summary>
        /// 根据timer的到期时间存储 对应的 N个timerId
        /// </summary>
        private readonly SortedMultiDictionary<long, int> _timeIdDic = new SortedMultiDictionary<long, int>();

        /// <summary>
        /// 需要执行的 到期时间
        /// </summary>
        private readonly Queue<long> _timeOutTime = new Queue<long>();

        /// <summary>
        /// 到期的所有 timerId
        /// </summary>
        private readonly Queue<int> _timeOutTimerIds = new Queue<int>();

        /// <summary>
        /// 暂停的计时器
        /// </summary>
        private readonly Dictionary<int, PausedTimer> _pausedTimer = new Dictionary<int, PausedTimer>();

        /// <summary>
        /// 需要每帧回调的计时器
        /// </summary>
        private readonly Dictionary<int, Timer> _updateTimer = new Dictionary<int, Timer>();

        /// <summary>
        /// 记录最小时间，不用每次都去MultiMap取第一个值
        /// </summary>
        private long _minTime;

        public void Update()
        {
            RunUpdateCallBack();
            if (_timeIdDic.Count == 0)
            {
                return;
            }

            long timeNow = TimerTimeUtility.Now();

            if (timeNow < _minTime)
            {
                return;
            }

            foreach (long k in _timeIdDic.Select(kv => kv.Key)) {
                if (k > timeNow)
                {
                    _minTime = k;
                    break;
                }
                _timeOutTime.Enqueue(k);
            }

            while (_timeOutTime.Count > 0)
            {
                long time = _timeOutTime.Dequeue();
                foreach (int timerId in _timeIdDic[time])
                {
                    _timeOutTimerIds.Enqueue(timerId);
                }
                _timeIdDic.Remove(time);
            }

            while (_timeOutTimerIds.Count > 0)
            {
                int timerId = _timeOutTimerIds.Dequeue();

                _timers.TryGetValue(timerId, out Timer timer);
                if (timer == null)
                {
                    continue;
                }

                RunTimer(timer);
            }
        }

        /// <summary>
        /// 执行每帧回调
        /// </summary>
        private void RunUpdateCallBack()
        {
            if (_updateTimer.Count == 0)
            {
                return;
            }

            long timeNow = TimerTimeUtility.Now();
            foreach (Timer timer in _updateTimer.Values)
            {
                timer.UpdateCallBack?.Invoke(timer.Time + timer.StartTime - timeNow);
            }
        }

        /// <summary>
        /// 执行定时器回调
        /// </summary>
        /// <param name="timer">定时器</param>
        private void RunTimer(Timer timer)
        {
            switch (timer.TimerType)
            {
                case TimerType.OnceWait:
                {
                    TaskCompletionSource<bool> tcs = timer.Callback as TaskCompletionSource<bool>;
                    RemoveTimer(timer.ID);
                    tcs?.SetResult(true);
                    break;
                }
                case TimerType.Once:
                {
                    var action = timer.Callback as Action;
                    RemoveTimer(timer.ID);
                    action?.Invoke();
                    break;
                }
                case TimerType.Repeated:
                {
                    var action = timer.Callback as Action;
                    long nowTime = TimerTimeUtility.Now();
                    long tillTime = nowTime + timer.Time;
                    if (timer.RepeatCount == 1)
                    {
                        RemoveTimer(timer.ID);
                    }
                    else
                    {
                        if (timer.RepeatCount > 1)
                        {
                            timer.RepeatCount--;
                        }

                        timer.StartTime = nowTime;
                        AddTimer(tillTime, timer.ID);
                    }

                    action?.Invoke();

                    break;
                }
            }
        }

        /// <summary>
        /// 添加定时器
        /// </summary>
        /// <param name="tillTime">延时时间</param>
        /// <param name="id">定时器ID</param>
        private void AddTimer(long tillTime, int id)
        {
            _timeIdDic.Add(tillTime, id);
            if (tillTime < _minTime)
            {
                _minTime = tillTime;
            }
        }

        /// <summary>
        /// 删除定时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        private void RemoveTimer(int id)
        {
            _timers.TryGetValue(id, out Timer timer);
            if (timer == null)
            {
                Debug.LogError($"删除了不存在的Timer ID:{id}");
                return;
            }

            ReferencePool.Release(timer);
            _timers.Remove(id);
            _updateTimer.Remove(id);
            if (_pausedTimer.ContainsKey(id))
            {
                ReferencePool.Release(_pausedTimer[id]);
                _pausedTimer.Remove(id);
            }
        }

        /// <summary>
        /// 取消计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public void CancelTimer(int id)
        {
            if (_pausedTimer.ContainsKey(id))
            {
                ReferencePool.Release(_pausedTimer[id].Timer);
                ReferencePool.Release(_pausedTimer[id]);
                _pausedTimer.Remove(id);
                return;
            }

            RemoveTimer(id);
        }

        /// <summary>
        /// 查询是否存在计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public bool IsExistTimer(int id)
        {
            return _pausedTimer.ContainsKey(id) || _timers.ContainsKey(id);
        }

        /// <summary>
        /// 暂停计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public void PauseTimer(int id)
        {
            _timers.TryGetValue(id, out Timer oldTimer);
            if (oldTimer == null)
            {
                Debug.LogError($"Timer不存在 ID:{id}");
                return;
            }

            _timeIdDic.Remove(oldTimer.StartTime + oldTimer.Time, oldTimer.ID);
            _timers.Remove(id);
            _updateTimer.Remove(id);
            var timer = PausedTimer.Create(TimerTimeUtility.Now(), oldTimer);
            _pausedTimer.Add(id, timer);
        }

        /// <summary>
        /// 恢复计时器
        /// </summary>
        /// <param name="id">定时器ID</param>
        public void ResumeTimer(int id)
        {
            _pausedTimer.TryGetValue(id, out PausedTimer timer);
            if (timer == null)
            {
                Debug.LogError($"Timer不存在 ID:{id}");
                return;
            }

            _timers.Add(id, timer.Timer);
            if (timer.Timer.UpdateCallBack != null)
            {
                _updateTimer.Add(id, timer.Timer);
            }

            long tillTime = TimerTimeUtility.Now() + timer.GetResidueTime();
            timer.Timer.StartTime += TimerTimeUtility.Now() - timer.PausedTime;
            AddTimer(tillTime, timer.Timer.ID);
            ReferencePool.Release(timer);
            _pausedTimer.Remove(id);
        }

        /// <summary>
        /// 修改定时器时间
        /// </summary>
        /// <param name="id">定时器ID</param>
        /// <param name="time">修改时间</param>
        /// <param name="isChangeRepeat">是否修改如果是RepeatTimer每次运行时间</param>
        public void ChangeTime(int id, long time, bool isChangeRepeat = false)
        {
            _pausedTimer.TryGetValue(id, out PausedTimer pausedTimer);
            if (pausedTimer?.Timer != null)
            {
                pausedTimer.Timer.Time += time;
                return;
            }

            _timers.TryGetValue(id, out Timer oldTimer);
            if (oldTimer == null)
            {
                Debug.LogError($"Timer不存在 ID:{id}");
            }

            _timeIdDic.Remove(oldTimer.StartTime + oldTimer.Time, oldTimer.ID);
            if (oldTimer.TimerType == TimerType.Repeated && !isChangeRepeat)
            {
                oldTimer.StartTime += time;
            }
            else
            {
                oldTimer.Time += time;
            }

            AddTimer(oldTimer.StartTime + oldTimer.Time, oldTimer.ID);
        }

        /// <summary>
        /// 添加执行一次的定时器
        /// </summary>
        /// <param name="time">定时时间</param>
        /// <param name="callback">回调函数</param>
        /// <param name="updateCallBack">每帧回调函数</param>
        /// <returns></returns>
        public int AddOnceTimer(long time, Action callback, Action<long> updateCallBack = null)
        {
            if (time < 0)
            {
                Debug.LogError($"new once time too small: {time}");
            }

            long nowTime = TimerTimeUtility.Now();
            var timer = Timer.Create(time, nowTime, TimerType.Once, callback, 1, updateCallBack);
            _timers.Add(timer.ID, timer);
            if (updateCallBack != null)
            {
                _updateTimer.Add(timer.ID, timer);
            }

            AddTimer(nowTime + time, timer.ID);
            return timer.ID;
        }

        /// <summary>
        /// 可等待执行一次的定时器
        /// </summary>
        /// <param name="time">定时时间</param>
        /// <param name="cancelAction">取消任务函数</param>
        /// <returns></returns>
        public async Task<bool> OnceTimerAsync(long time, Action cancelAction = null)
        {
            long nowTime = TimerTimeUtility.Now();
            if (time <= 0)
            {
                return true;
            }

            var tcs = new TaskCompletionSource<bool>();
            var timer = Timer.Create(time, nowTime, TimerType.OnceWait, tcs);
            _timers.Add(timer.ID, timer);
            int timerId = timer.ID;

            AddTimer(nowTime + time, timerId);

            void CancelAction()
            {
                RemoveTimer(timerId);
                tcs.SetResult(false);
            }

            bool result;
            try
            {
                if (cancelAction != null)
                {
                    cancelAction += CancelAction;
                }
                result = await tcs.Task;
            }
            finally
            {
                if (cancelAction != null)
                {
                    cancelAction -= CancelAction;
                }
            }

            return result;
        }

        /// <summary>
        /// 可等待的帧定时器
        /// </summary>
        /// <returns>定时器 ID</returns>
        public async Task<bool> FrameAsync(Action cancelAction = null)
        {
            return await OnceTimerAsync(1, cancelAction);
        }

        /// <summary>
        /// 添加执行多次的定时器
        /// </summary>
        /// <param name="time">定时时间</param>
        /// <param name="repeatCount">重复次数 (小于等于零 无限次调用） </param>
        /// <param name="callback">回调函数</param>
        /// <param name="updateCallback">每帧回调函数</param>
        /// <returns>定时器 ID</returns>
        /// <exception cref="Exception">定时时间太短 无意义</exception>
        public int AddRepeatedTimer(long time, int repeatCount, Action callback, Action<long> updateCallback = null)
        {
            if (time < 0)
            {
                Debug.LogError($"new once time too small: {time}");
            }

            long nowTime = TimerTimeUtility.Now();
            var timer = Timer.Create(time, nowTime, TimerType.Repeated, callback, repeatCount, updateCallback);
            _timers.Add(timer.ID, timer);
            if (updateCallback != null)
            {
                _updateTimer.Add(timer.ID, timer);
            }

            AddTimer(nowTime + time, timer.ID);
            return timer.ID;
        }

        public void AddRepeatedTimer(out int id, long time, int repeatCount, Action callback,
            Action<long> updateCallback = null)
        {
            if (time < 0)
            {
                Debug.LogError($"new once time too small: {time}");
            }

            long nowTime = TimerTimeUtility.Now();
            var timer = Timer.Create(time, nowTime, TimerType.Repeated, callback, repeatCount, updateCallback);
            _timers.Add(timer.ID, timer);
            if (updateCallback != null)
            {
                _updateTimer.Add(timer.ID, timer);
            }

            id = timer.ID;
            AddTimer(nowTime + time, timer.ID);
        }

        /// <summary>
        /// 添加帧定时器
        /// </summary>
        /// <param name="callback">回调函数</param>
        /// <returns>定时器 ID</returns>
        public int AddFrameTimer(Action callback)
        {
            long nowTime = TimerTimeUtility.Now();
            var timer = Timer.Create(1, nowTime, TimerType.Once, callback);
            _timers.Add(timer.ID, timer);
            AddTimer(nowTime + 1, timer.ID);
            return timer.ID;
        }
    }
}