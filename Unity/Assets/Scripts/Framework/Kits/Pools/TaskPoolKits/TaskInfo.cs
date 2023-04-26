//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace Framework.Kits.TaskPoolKits
{
    /// <summary>
    /// 任务信息。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct TaskInfo
    {

        /// <summary>
        /// 初始化任务信息的新实例。
        /// </summary>
        /// <param name="serialId">任务的序列编号。</param>
        /// <param name="priority">任务的优先级。</param>
        /// <param name="status">任务状态。</param>
        /// <param name="description">任务描述。</param>
        public TaskInfo(int serialId, int priority, TaskStatus status, string description)
        {
            SerialId = serialId;
            Priority = priority;
            Status = status;
            Description = description;
        }

        /// <summary>
        /// 获取任务的序列编号。
        /// </summary>
        public int SerialId { get; }

        /// <summary>
        /// 获取任务的优先级。
        /// </summary>
        public int Priority { get; }

        /// <summary>
        /// 获取任务状态。
        /// </summary>
        public TaskStatus Status { get; }

        /// <summary>
        /// 获取任务描述。
        /// </summary>
        public string Description { get; }
    }
}
