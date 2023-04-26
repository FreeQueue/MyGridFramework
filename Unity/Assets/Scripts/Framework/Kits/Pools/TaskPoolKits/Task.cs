//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.TaskPoolKits
{
    /// <summary>
    /// 任务基类。
    /// </summary>
    internal abstract class Task : IReference
    {
        /// <summary>
        /// 任务默认优先级。
        /// </summary>
        public const int DEFAULT_PRIORITY = 0;

        /// <summary>
        /// 初始化任务基类的新实例。
        /// </summary>
        public Task()
        {
            SerialId = 0;
            Priority = DEFAULT_PRIORITY;
            Done = false;
        }

        /// <summary>
        /// 获取任务的序列编号。
        /// </summary>
        public int SerialId { get; private set; }

        /// <summary>
        /// 获取任务的优先级。
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// 获取或设置任务是否完成。
        /// </summary>
        public bool Done { get; set; }

        /// <summary>
        /// 获取任务描述。
        /// </summary>
        public virtual string Description => null;

        /// <summary>
        /// 初始化任务基类。
        /// </summary>
        /// <param name="serialId">任务的序列编号。</param>
        /// <param name="priority">任务的优先级。</param>
        internal void Initialize(int serialId, int priority)
        {
            SerialId = serialId;
            Priority = priority;
            Done = false;
        }

        /// <summary>
        /// 清理任务基类。
        /// </summary>
        public virtual void Clear()
        {
            SerialId = 0;
            Priority = DEFAULT_PRIORITY;
            Done = false;
        }
    }
}
