using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CVaS.Shared.Helpers
{
    public static class TaskExtensions
    {
        public static async Task<TaskWithTimeout<TResult>> WithTimeout<TResult>(this Task<TResult> task, TimeSpan softTimeout) where TResult : class
        {
            var softTokenSource = new CancellationTokenSource();

            Task winner = await (Task.WhenAny(task, Task.Delay(softTimeout, softTokenSource.Token)));
            if (winner != task)
            {
                return new TaskWithTimeout<TResult>();
            }
            else
            {
                softTokenSource.Cancel();
                // Task completed within timeout.
                // Consider that the task may have faulted or been canceled.
                // We re-await the task so that any exceptions/cancellation is rethrown.
                return new TaskWithTimeout<TResult>(await task);
            }
        }

        public struct TaskWithTimeout<T> where T : class
        {
            public TaskWithTimeout(T value)
            {
                Timeouted = false;
                Value = value;
            }

            public T Value { get; private set; }
            public bool Timeouted { get; private set; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SupressError(this Task task)
        {

        }


    }
}