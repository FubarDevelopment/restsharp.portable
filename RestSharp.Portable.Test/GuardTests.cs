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
            var guard = new RequestGuard();
            var results = new ConcurrentBag<int>();
            var t1 = Task.Run(
                () =>
                    {
                        using (guard.Guard(CancellationToken.None))
                            Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        results.Add(1);
                    });
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            var t2 = Task.Run(
                () =>
                {
                    using (guard.Guard(CancellationToken.None))
                        DoNothing();
                    results.Add(2);
                });
            Task.WaitAll(t1, t2);
            var resultsData = results.ToArray();
            Assert.Equal(1, resultsData[0]);
            Assert.Equal(2, resultsData[1]);
        }

        [Fact]
        public void TestGuardLock2()
        {
            var guard = new RequestGuard();
            var results = new ConcurrentBag<int>();
            var t2 = Task.Run(
                () =>
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                    using (guard.Guard(CancellationToken.None))
                        DoNothing();
                    results.Add(2);
                });
            var t1 = Task.Run(
                () =>
                {
                    using (guard.Guard(CancellationToken.None))
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    results.Add(1);
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
