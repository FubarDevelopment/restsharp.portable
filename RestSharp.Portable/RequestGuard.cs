using System;
using System.Threading;

namespace RestSharp.Portable
{
    internal class RequestGuard : IDisposable
    {
        private readonly object _syncObject = new object();

#if PCL || SILVERLIGHT
        private readonly AutoResetEvent _lock = new AutoResetEvent(true);
#else
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
#endif

        private bool _isDisposed;

        public IDisposable Guard(CancellationToken cancellationToken)
        {
            return new RequestLock(this, cancellationToken);
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

        private class RequestLock : IDisposable
        {
            private readonly RequestGuard _guard;

            public RequestLock(RequestGuard guard, CancellationToken cancellationToken)
            {
                _guard = guard;
#if PCL || SILVERLIGHT
                WaitHandle.WaitAny(new[] { guard._lock, cancellationToken.WaitHandle });
                cancellationToken.ThrowIfCancellationRequested();
#else
                _guard._lock.Wait(cancellationToken);
#endif
            }

            public void Dispose()
            {
#if PCL || SILVERLIGHT
                _guard._lock.Set();
#else
                _guard._lock.Release();
#endif
            }
        }
    }
}
