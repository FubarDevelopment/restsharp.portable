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
            var evt1 = new AutoResetEvent(false);
            var evt2 = new AutoResetEvent(false);
            var guard = new RequestGuard();
            var results = new ConcurrentBag<int>();
            var t1 = Task.Run(
                () =>
                {
                    using (guard.Guard(CancellationToken.None))
                    {
                        evt1.Set();
                        evt2.WaitOne();
                        evt1.WaitOne();
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                    }

                    results.Add(1);
                });
            evt1.WaitOne();

            var t2 = Task.Run(
                () =>
                {
                    evt2.Set();
                    evt1.Set();
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
