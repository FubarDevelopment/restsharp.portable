using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Portable.WebRequest
{
    /// <summary>
    /// Task helper functions
    /// </summary>
    internal static class TaskExtensions
    {
        /// <summary>
        /// Handle a cancellation when we have an non-cancellable asynchronous operation
        /// </summary>
        /// <typeparam name="TResult">The return type of the asynchronous operation</typeparam>
        /// <param name="asyncTask">The asynchronous operation to handle the cancellation for</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <param name="cancellationAction">The action to perform when a cancellation occurred</param>
        /// <returns>The result of the operation</returns>
        public static async Task<TResult> HandleCancellation<TResult>(
            this Task<TResult> asyncTask,
            CancellationToken cancellationToken,
            Action<Task> cancellationAction = null)
        {
            // Create another task that completes as soon as cancellation is requested.
            // http://stackoverflow.com/a/18672893/1149773
            var tcs = new TaskCompletionSource<TResult>();
            cancellationToken.Register(
                () => tcs.TrySetCanceled(),
                false);
            var cancellationTask = tcs.Task;

            // Create a task that completes when either the async operation completes,
            // or cancellation is requested.
#if USE_TASKEX
            var readyTask = await TaskEx.WhenAny(asyncTask, cancellationTask);
#else
            var readyTask = await Task.WhenAny(asyncTask, cancellationTask);
#endif

            // In case of cancellation, register a continuation to observe any unhandled
            // exceptions from the asynchronous operation (once it completes).
            // In .NET 4.0, unobserved task exceptions would terminate the process.
            if (readyTask == cancellationTask)
            {
#pragma warning disable 4014
                asyncTask.ContinueWith(
#pragma warning restore 4014
                    t => asyncTask.Exception,
                    TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
                cancellationAction?.Invoke(cancellationTask);
            }

            return await readyTask;
        }
    }
}
