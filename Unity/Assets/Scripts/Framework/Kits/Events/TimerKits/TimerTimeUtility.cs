using System;

namespace UnityGameFramework.Extension
{
    public static class TimerTimeUtility
    {
        private static readonly long s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
        /// <summary>
        /// 当前时间
        /// </summary>
        /// <returns></returns>
        public static long Now()
        {
            return (DateTime.UtcNow.Ticks - s_epoch) / 10000;
        }
    }
}