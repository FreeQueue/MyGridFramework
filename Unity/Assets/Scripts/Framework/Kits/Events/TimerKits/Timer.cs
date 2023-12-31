using System;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.TimerKits
{
	 public class Timer : IReference
        {
            /// <summary>
            /// 自增id
            /// </summary>
            private static int s_serialId;

            static Timer()
            {
                s_serialId = 0;
            }

            /// <summary>
            /// timer 类型
            /// </summary>
            public TimerType TimerType { get; private set; }

            /// <summary>
            /// 计时结束回调函数
            /// </summary>
            public object Callback { get; private set; }

            /// <summary>
            /// 每帧回调函数 (返回剩余时间)
            /// </summary>
            public Action<long> UpdateCallBack { get; private set; }

            /// <summary>
            /// 时间
            /// </summary>
            public long Time { get; set; }

            /// <summary>
            /// 开始时间
            /// </summary>
            public long StartTime { get; set; }

            /// <summary>
            /// 开始时间
            /// </summary>
            public int RepeatCount { get; set; }

            /// <summary>
            /// ID
            /// </summary>
            public int ID { get; private set; }

            /// <summary>
            /// 创建定时器
            /// </summary>
            /// <param name="time">时间</param>
            /// <param name="startTime">开始时间</param>
            /// <param name="timerType">定时器类型</param>
            /// <param name="callback">回调</param>
            /// <param name="repeatCount">调用次数</param>
            /// <param name="updateCallBack">每帧回调</param>
            /// <returns>定时器</returns>
            public static Timer Create(long time, long startTime, TimerType timerType, object callback,
                int repeatCount = 0, Action<long> updateCallBack = null)
            {
                var timer = ReferencePool.Get<Timer>();
                timer.ID = s_serialId++;
                timer.Time = time;
                timer.StartTime = startTime;
                timer.TimerType = timerType;
                timer.Callback = callback;
                timer.RepeatCount = repeatCount;
                timer.UpdateCallBack = updateCallBack;
                return timer;
            }

            public void Clear()
            {
                ID = -1;
                Time = 0;
                StartTime = 0;
                Callback = null;
                UpdateCallBack = null;
                RepeatCount = 0;
                TimerType = TimerType.None;
            }
        }
}