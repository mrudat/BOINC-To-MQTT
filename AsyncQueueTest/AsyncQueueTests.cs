global using AsyncQueue;
global using Xunit;
global using Xunit.Abstractions;
using FluentAssertions;

namespace AsyncQueueTest
{
    public class AsyncQueueTests(ITestOutputHelper output)
    {
        private readonly AsyncQueue<int> q = new();

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task TestQueue(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Enqueue(i);
            }

            for (int i = 0; i < count; i++)
            {
                await AssertDequeue(i);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task TestAsyncQueue(int count)
        {
            async Task EnqueueMany()
            {
                for (int i = 0; i < count; i++)
                {
                    Enqueue(i);
                    await Task.Yield();
                }
            }

            async Task DequeueMany()
            {
                for (int i = 0; i < count; i++)
                {
                    await AssertDequeue(i);
                }
            }

            await Task.WhenAll([
                EnqueueMany(),
                DequeueMany()
            ]);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task TestQueueAfterDequeue(int count)
        {
            async Task DequeueMany()
            {
                for (int i = 0; i < count; i++)
                {
                    await AssertDequeue(i);
                }
            }

            async Task EnqueueMany()
            {
                for (int i = 0; i < count; i++)
                {
                    await Task.Delay(1);
                    Enqueue(i);
                }
            }

            await Task.WhenAll([
                DequeueMany(),
                EnqueueMany()
            ]);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task TestCancellation(int count)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;

            async Task DequeueMany()
            {
                for (int i = 0; i < count; i++)
                {
                    await AssertDequeue(i, cancellationToken);
                }
            }

            async Task EnqueueMany()
            {
                for (int i = 0; i < count; i++)
                {
                    Enqueue(i);
                    if (i >= (count / 2))
                    {
                        output.WriteLine("Cancelled {0}", i);
                        cts.Cancel();
                    }
                    await Task.Yield();
                }
            }

            async Task DoTheThing()
            {
                await Task.WhenAll([
                    DequeueMany(),
                    EnqueueMany()
                ]);
            }

            Func<Task> act = DoTheThing;

            await act.Should().ThrowAsync<TaskCanceledException>();
        }

        private void Enqueue(int val)
        {
            output.WriteLine("Enqueue {0}", val);
            q.Enqueue(val);
        }

        private async Task AssertDequeue(int expected, CancellationToken cancellationToken = default)
        {
            output.WriteLine("Dequeue {0}", expected);

            var actual = await q.DequeueAsync(cancellationToken);

            actual.Should().Be(expected);
        }
    }
}