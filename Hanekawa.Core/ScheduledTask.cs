using System;
using System.Threading.Tasks;

namespace Hanekawa.Core
{
    public class ScheduledTask
    {
        internal ScheduledTask(object obj, DateTimeOffset when, Func<object, Task> task)
        {
            Object = obj;
            ExecutionTime = when;
            Task = task;

            Tcs = new TaskCompletionSource<bool>();
        }

        /// <summary>
        ///     Whether the task has been cancelled or not.
        /// </summary>
        public bool IsCancelled { get; private set; }

        /// <summary>
        ///     Whether the task has been completed or not.
        /// </summary>
        public bool HasCompleted { get; private set; }

        /// <summary>
        ///     The object that will be passed to the tasks callback.
        /// </summary>
        public object Object { get; }

        /// <summary>
        ///     The time at when the task will execute.
        /// </summary>
        public DateTimeOffset ExecutionTime { get; }

        /// <summary>
        ///     Gets how long until this task executes.
        /// </summary>
        public TimeSpan ExecutesIn
        {
            get
            {
                var time = ExecutionTime - DateTimeOffset.UtcNow;

                return time > TimeSpan.Zero ? time : TimeSpan.FromSeconds(-1);
            }
        }

        /// <summary>
        ///     Gets the exception (if thrown) from execution.
        /// </summary>
        public Exception Exception { get; internal set; }

        internal TaskCompletionSource<bool> Tcs { get; }
        internal Func<object, Task> Task { get; }

        /// <summary>
        ///     Cancels this task.
        /// </summary>
        public void Cancel()
        {
            IsCancelled = true;
        }

        /// <summary>
        ///     Waits until this task has been completed.
        /// </summary>
        /// <returns>An awaitable <see cref="System.Threading.Tasks.Task" /></returns>
        public Task WaitForCompletionAsync()
        {
            return Tcs.Task;
        }

        internal void Completed()
        {
            Tcs.SetResult(true);
            HasCompleted = true;
        }
    }
}