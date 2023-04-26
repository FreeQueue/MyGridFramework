//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;
using Framework.Kits.ReferencePoolKits;

namespace Framework.Kits.TaskPoolKits
{
    /// <summary>
    /// 任务池。
    /// </summary>
    /// <typeparam name="T">任务类型。</typeparam>
    internal sealed class TaskPool<T> where T : Task
    {
        private readonly Stack<ITaskAgent<T>> _freeAgents;
        private readonly LinkedList<ITaskAgent<T>> _workingAgents;
        private readonly LinkedList<T> _waitingTasks;

        /// <summary>
        /// 初始化任务池的新实例。
        /// </summary>
        public TaskPool()
        {
            _freeAgents = new Stack<ITaskAgent<T>>();
            _workingAgents = new LinkedList<ITaskAgent<T>>();
            _waitingTasks = new LinkedList<T>();
            Paused = false;
        }

        /// <summary>
        /// 获取或设置任务池是否被暂停。
        /// </summary>
        public bool Paused { get; set; }

        /// <summary>
        /// 获取任务代理总数量。
        /// </summary>
        public int TotalAgentCount => FreeAgentCount + WorkingAgentCount;

        /// <summary>
        /// 获取可用任务代理数量。
        /// </summary>
        public int FreeAgentCount => _freeAgents.Count;

        /// <summary>
        /// 获取工作中任务代理数量。
        /// </summary>
        public int WorkingAgentCount => _workingAgents.Count;

        /// <summary>
        /// 获取等待任务数量。
        /// </summary>
        public int WaitingTaskCount => _waitingTasks.Count;

        /// <summary>
        /// 任务池轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            if (Paused)
            {
                return;
            }

            ProcessRunningTasks(elapseSeconds, realElapseSeconds);
            ProcessWaitingTasks(elapseSeconds, realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理任务池。
        /// </summary>
        public void Shutdown()
        {
            RemoveAllTasks();

            while (FreeAgentCount > 0)
            {
                _freeAgents.Pop().Shutdown();
            }
        }

        /// <summary>
        /// 增加任务代理。
        /// </summary>
        /// <param name="agent">要增加的任务代理。</param>
        public void AddAgent(ITaskAgent<T> agent)
        {
            agent.Initialize();
            _freeAgents.Push(agent);
        }

        /// <summary>
        /// 增加任务。
        /// </summary>
        /// <param name="task">要增加的任务。</param>
        public void AddTask(T task)
        {
            LinkedListNode<T> current = _waitingTasks.Last;
            while (current != null)
            {
                if (task.Priority <= current.Value.Priority)
                {
                    break;
                }

                current = current.Previous;
            }

            if (current != null)
            {
                _waitingTasks.AddAfter(current, task);
            }
            else
            {
                _waitingTasks.AddFirst(task);
            }
        }

        /// <summary>
        /// 移除任务。
        /// </summary>
        /// <param name="serialId">要移除任务的序列编号。</param>
        /// <returns>是否移除任务成功。</returns>
        public bool RemoveTask(int serialId)
        {
            foreach (T task in _waitingTasks)
            {
                if (task.SerialId == serialId)
                {
                    _waitingTasks.Remove(task);
                    ReferencePool.Release(task);
                    return true;
                }
            }

            foreach (ITaskAgent<T> workingAgent in _workingAgents)
            {
                if (workingAgent.Task.SerialId == serialId)
                {
                    T task = workingAgent.Task;
                    workingAgent.Reset();
                    _freeAgents.Push(workingAgent);
                    _workingAgents.Remove(workingAgent);
                    ReferencePool.Release(task);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 移除所有任务。
        /// </summary>
        public void RemoveAllTasks()
        {
            foreach (T task in _waitingTasks)
            {
                ReferencePool.Release(task);
            }

            _waitingTasks.Clear();

            foreach (ITaskAgent<T> workingAgent in _workingAgents)
            {
                T task = workingAgent.Task;
                workingAgent.Reset();
                _freeAgents.Push(workingAgent);
                ReferencePool.Release(task);
            }

            _workingAgents.Clear();
        }

        public TaskInfo[] GetAllTaskInfos()
        {
            List<TaskInfo> results = new List<TaskInfo>();
            foreach (ITaskAgent<T> workingAgent in _workingAgents)
            {
                T workingTask = workingAgent.Task;
                results.Add(new TaskInfo(workingTask.SerialId, workingTask.Priority, workingTask.Done ? TaskStatus.Done : TaskStatus.Doing, workingTask.Description));
            }

            foreach (T waitingTask in _waitingTasks)
            {
                results.Add(new TaskInfo(waitingTask.SerialId, waitingTask.Priority, TaskStatus.Todo, waitingTask.Description));
            }

            return results.ToArray();
        }

        private void ProcessRunningTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<ITaskAgent<T>> current = _workingAgents.First;
            while (current != null)
            {
                T task = current.Value.Task;
                if (!task.Done)
                {
                    current.Value.Update(elapseSeconds, realElapseSeconds);
                    current = current.Next;
                    continue;
                }

                LinkedListNode<ITaskAgent<T>> next = current.Next;
                current.Value.Reset();
                _freeAgents.Push(current.Value);
                _workingAgents.Remove(current);
                ReferencePool.Release(task);
                current = next;
            }
        }

        private void ProcessWaitingTasks(float elapseSeconds, float realElapseSeconds)
        {
            LinkedListNode<T> current = _waitingTasks.First;
            while (current != null && FreeAgentCount > 0)
            {
                ITaskAgent<T> agent = _freeAgents.Pop();
                LinkedListNode<ITaskAgent<T>> agentNode = _workingAgents.AddLast(agent);
                T task = current.Value;
                LinkedListNode<T> next = current.Next;
                StartTaskStatus status = agent.Start(task);
                if (status == StartTaskStatus.Done || status == StartTaskStatus.HasToWait || status == StartTaskStatus.UnknownError)
                {
                    agent.Reset();
                    _freeAgents.Push(agent);
                    _workingAgents.Remove(agentNode);
                }

                if (status == StartTaskStatus.Done || status == StartTaskStatus.CanResume || status == StartTaskStatus.UnknownError)
                {
                    _waitingTasks.Remove(current);
                }

                if (status == StartTaskStatus.Done || status == StartTaskStatus.UnknownError)
                {
                    ReferencePool.Release(task);
                }

                current = next;
            }
        }
    }
}
