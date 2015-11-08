using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Portable.HttpClient
{
    /// <summary>
    /// Asynchronous locking
    /// </summary>
    internal sealed class AsyncLock : IDisposable
    {
#if PCL || SILVERLIGHT || NET40
        private readonly AutoResetEvent _lock = new AutoResetEvent(true);
#else
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
#endif
        private readonly Task<IDisposable> _releaser;

        private int _isDisposed = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLock"/> class.
        /// </summary>
        public AsyncLock()
        {
            var releaserTaskSource = new TaskCompletionSource<IDisposable>();
            releaserTaskSource.SetResult(new Releaser(this));
            _releaser = releaserTaskSource.Task;
        }

        /// <summary>
        /// Acquires an asynchronous lock
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The asynchronously acquired lock</returns>
        public Task<IDisposable> LockAsync(CancellationToken cancellationToken)
        {
#if PCL || SILVERLIGHT || NET40
            var wait = Task.Factory.StartNew(() =>
            {
                WaitHandle.WaitAny(new[] { _lock, cancellationToken.WaitHandle });
                cancellationToken.ThrowIfCancellationRequested();
            });
#else
            var wait = _lock.WaitAsync(cancellationToken);
#endif

            return wait.IsCompleted
                       ? _releaser
                       : wait.ContinueWith(
                           prevTask => _releaser.Result,
                           cancellationToken,
                           TaskContinuationOptions.ExecuteSynchronously,
                           TaskScheduler.Default);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                _lock.Dispose();
            }
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;

            internal Releaser(AsyncLock toRelease)
            {
                _toRelease = toRelease;
            }

            public void Dispose()
            {
                if (_toRelease._isDisposed == 0)
                {
#if PCL || SILVERLIGHT || NET40
                    _toRelease._lock.Set();
#else
                    _toRelease._lock.Release();
#endif
                }
            }
        }
    }
}
