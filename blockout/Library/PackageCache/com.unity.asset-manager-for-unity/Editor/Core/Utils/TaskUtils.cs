using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.AssetManager.Core.Editor
{
    static class TaskUtils
    {
        //number based on half the back-end rate limit and to keep it the UI reactive when running batches of tasks
        const int k_MaxConcurrentTasks = 60;
        const int k_MaxConcurrentTasksInQueue = 30;
        public const int BackgroundRefreshQueueSize = 20;

        public static void TrackException(Task task, Action<Exception> exceptionCallback = null)
        {
            var awaiter = task.GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                if (task.Exception != null)
                {
                    if (exceptionCallback != null)
                    {
                        exceptionCallback.Invoke(task.Exception);
                    }
                    else
                    {
                        Debug.LogException(task.Exception);
                    }
                }
            });

            _ = task;
        }

        public static async Task WaitForTaskWithHandleExceptions(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                if (task.IsFaulted && task.Exception != null)
                {
                    Debug.LogException(task.Exception);
                }
            }
        }

        public static async Task WaitForTasksWithHandleExceptions(IReadOnlyCollection<Task> tasks)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                foreach (var task in tasks)
                {
                    if (task.IsFaulted && task.Exception != null)
                    {
                        Debug.LogException(task.Exception);
                    }
                }
            }
        }

        public static async Task<IReadOnlyCollection<Task>> RunAllTasks<T>(IEnumerable<T> inputs, Func<T, Task> taskCreation)
        {
            var allTasks = inputs.Select(taskCreation.Invoke).ToList();
            await Task.WhenAll(allTasks);
            return allTasks;
        }

        public static async Task<IReadOnlyCollection<Task>> RunAllTasksBatched<T>(IEnumerable<T> inputs, Func<T, Task> taskCreation, int maxConcurrentTasks = k_MaxConcurrentTasks)
        {
            var allTasks = new List<Task>();
            var batch = new List<Task>(maxConcurrentTasks);
            foreach (var input in inputs)
            {
                var task = taskCreation(input);
                allTasks.Add(task);
                batch.Add(task);
                if (batch.Count < maxConcurrentTasks) continue;
                await Task.WhenAll(batch);
                batch.Clear();
            }
            if (batch.Count > 0)
            {
                await Task.WhenAll(batch);
            }
            return allTasks;
        }

        public static async Task<IReadOnlyCollection<Task>> RunAllTasksInQueue<T>(IEnumerable<T> inputs, Func<T, Task> taskCreation, int maxConcurrentTasks = k_MaxConcurrentTasksInQueue)
        {
            var queue = new Queue<T>(inputs);
            var runningTasks = new List<Task>();
            var allTasks = new List<Task>();

            while (queue.Count > 0 || runningTasks.Count > 0)
            {
                // Start new tasks while under concurrency limit
                while (queue.Count > 0 && runningTasks.Count < maxConcurrentTasks)
                {
                    var item = queue.Dequeue();
                    var task = taskCreation(item);
                    runningTasks.Add(task);
                    allTasks.Add(task);
                }

                // Wait for at least one task to complete before continuing
                var completed = await Task.WhenAny(runningTasks);
                runningTasks.Remove(completed);
            }

            return allTasks;
        }

        public static async Task<IEnumerable<Task>> RunAllTasksInQueue(IEnumerable<Func<Task>> taskCreations, int maxConcurrentTasks = k_MaxConcurrentTasksInQueue)
        {
            var queue = new Queue<Func<Task>>(taskCreations);
            var runningTasks = new List<Task>();
            var allTasks = new List<Task>();

            while (queue.Count > 0 || runningTasks.Count > 0)
            {
                // Start new tasks while under concurrency limit
                while (queue.Count > 0 && runningTasks.Count < maxConcurrentTasks)
                {
                    var taskCreation = queue.Dequeue();
                    var task = taskCreation();
                    runningTasks.Add(task);
                    allTasks.Add(task);
                }

                // Wait for at least one task to complete before continuing
                var completed = await Task.WhenAny(runningTasks);
                runningTasks.Remove(completed);
            }

            return allTasks;
        }

        public static async Task<IEnumerable<Task<T>>> RunAllTasksInQueue<T>(IEnumerable<Func<Task<T>>> taskCreations, int maxConcurrentTasks = k_MaxConcurrentTasksInQueue)
        {
            var queue = new Queue<Func<Task<T>>>(taskCreations);
            var runningTasks = new List<Task<T>>();
            var allTasks = new List<Task<T>>();

            while (queue.Count > 0 || runningTasks.Count > 0)
            {
                // Start new tasks while under concurrency limit
                while (queue.Count > 0 && runningTasks.Count < maxConcurrentTasks)
                {
                    var taskCreation = queue.Dequeue();
                    var task = taskCreation();
                    runningTasks.Add(task);
                    allTasks.Add(task);
                }

                // Wait for at least one task to complete before continuing
                var completed = await Task.WhenAny(runningTasks);
                runningTasks.Remove(completed);
            }

            return allTasks;
        }


        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, CancellationToken token)
        {
            var list = new List<T>();
            await foreach (var item in asyncEnumerable.WithCancellation(token))
            {
                list.Add(item);
            }

            return list;
        }
    }
}
