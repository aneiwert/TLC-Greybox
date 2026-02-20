using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.AssetManager.Core.Editor
{
    /// <summary>
    /// Will keep a task alive for a specified amount of time after it has completed.
    /// </summary>
    class CachedTask
    {
        readonly Func<CancellationToken, Task> m_TaskGetter;

        Task m_Task;
        float m_LastCompletedTime;

        public CachedTask(Func<CancellationToken, Task> taskGetter)
        {
            m_TaskGetter = taskGetter;
        }

        /// <summary>
        /// Runs the task.
        /// If the task is already running, it will await its completion.
        /// If the task has completed and the keep alive time has not expired, it will return immediately.
        /// If the task has completed and the keep alive time has expired, it will start a new task.
        /// </summary>
        /// <param name="cancellationToken">A token that can be used to cancel the task. </param>
        /// <param name="keepAliveTime">Time to keep alive in seconds. </param>
        public async Task RunAsync(CancellationToken cancellationToken, float keepAliveTime = 0f)
        {
            var isInvalidatedTask = m_Task == null || m_Task.IsCanceled || m_Task.IsFaulted;
            if (isInvalidatedTask || m_LastCompletedTime + keepAliveTime < (float) DateTime.Now.TimeOfDay.TotalSeconds)
            {
                m_Task = m_TaskGetter(cancellationToken);
            }

            var wasCompleted = m_Task.IsCompleted;

            await m_Task;

            // If the task was already completed, we don't update the last completed time.
            if (!wasCompleted)
            {
                m_LastCompletedTime = (float) DateTime.Now.TimeOfDay.TotalSeconds;
            }
        }
    }
}
