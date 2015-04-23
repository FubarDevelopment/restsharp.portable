using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace RestSharp.Portable.Test
{
    public class GuardTests
    {
        [Fact]
        public void TestGuardLock1()
        {
            var guard = new AsyncLock();
            var results = new ConcurrentQueue<int>();
            var t1 = Task.Run(
                async () =>
                    {
                        using (await guard.LockAsync(CancellationToken.None))
                            Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        results.Enqueue(1);
                    });
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            var t2 = Task.Run(
                async () =>
                {
                    using (await guard.LockAsync(CancellationToken.None))
                        DoNothing();
                    results.Enqueue(2);
                });
            Task.WaitAll(t1, t2);
            var resultsData = results.ToArray();
            Assert.Equal(1, resultsData[0]);
            Assert.Equal(2, resultsData[1]);
        }

        [Fact]
        public void TestGuardLock2()
        {
            var guard = new AsyncLock();
            var results = new ConcurrentQueue<int>();
            var t2 = Task.Run(
                async () =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    using (await guard.LockAsync(CancellationToken.None))
                        DoNothing();
                    results.Enqueue(2);
                });
            var t1 = Task.Run(
                async () =>
                {
                    using (await guard.LockAsync(CancellationToken.None))
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    results.Enqueue(1);
                });
            Task.WaitAll(t1, t2);
            var resultsData = results.ToArray();
            Assert.Equal(1, resultsData[0]);
            Assert.Equal(2, resultsData[1]);
        }

        private static void DoNothing()
        {
        }
    }
}
