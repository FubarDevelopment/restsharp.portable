using System;
using System.Collections.Concurrent;
using System.Linq;
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
            var taskStartMutex = new AutoResetEvent(false);
            var guard = new AsyncLock();
            var results = new ConcurrentQueue<int>();
            var t1 = Task.Run(
                async () =>
                    {
                        using (await guard.LockAsync(CancellationToken.None))
                        {
                            taskStartMutex.Set();
                            Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        }
                        results.Enqueue(1);
                    });
            taskStartMutex.WaitOne();
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

        [Fact]
        public async void TestGuardLockMultipleDispose()
        {
            var guard = new AsyncLock();
            var task = Task.Run(
                async () =>
                {
                    using (await guard.LockAsync(CancellationToken.None))
                    {
                        await Task.Delay(100);
                        return Int32.MaxValue;
                    }
                });

            await Task.Delay(50);
            await Task.WhenAll(Enumerable.Repeat(Task.Run((Action)guard.Dispose), 100));
            Assert.Equal(Int32.MaxValue, await task);
        }

        [Fact]
        public async void TestGuardLockEarlyDispose()
        {
            var cts = new CancellationTokenSource();
            var guard = new AsyncLock();
            var task = Task.Run(
                async () =>
                {
                    using (await guard.LockAsync(cts.Token))
                    {
                        await Task.Delay(200);
                        cts.Token.ThrowIfCancellationRequested();
                    }
                },
                cts.Token);
            await Task.Delay(100);
            cts.Cancel();
            guard.Dispose();

            await Task.Delay(200);
            Assert.Equal(TaskStatus.Canceled, task.Status);
        }

        private static void DoNothing()
        {
        }
    }
}
