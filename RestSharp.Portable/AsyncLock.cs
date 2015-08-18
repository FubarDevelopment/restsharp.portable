using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestSharp.Portable.HttpClient
{
    internal sealed class AsyncLock : IDisposable
    {
        private readonly object _syncObject = new object();

#if PCL || SILVERLIGHT || NET40
        private readonly AutoResetEvent _lock = new AutoResetEvent(true);
#else
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
#endif
        private readonly Task<IDisposable> _releaser;

        private bool _isDisposed;

        public AsyncLock()
        {
            var releaserTaskSource = new TaskCompletionSource<IDisposable>();
            releaserTaskSource.SetResult(new Releaser(this));
            _releaser = releaserTaskSource.Task;
        }

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

        public void Dispose()
        {
            lock (_syncObject)
            {
                if (_isDisposed)
                    return;
                _lock.Dispose();
                _isDisposed = true;
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
#if PCL || SILVERLIGHT || NET40
                _toRelease._lock.Set();
#else
                _toRelease._lock.Release();
#endif
            }
        }
    }
}
