using AsyncQueue;
using NUnit.Framework.Internal;

namespace AsyncQueueTest
{
    public class Tests
    {
        private AsyncQueue<int> q;

        [SetUp]
        public void Setup()
        {
            q = new AsyncQueue<int>();

        }

        [Test]
        public async Task TestQueue1()
        {
            await Enqueue(1);

            await Dequeue(1);
        }

        [Test]
        public async Task TestQueue2()
        {
            await Enqueue(1);
            await Enqueue(2);

            await Dequeue(1);
            await Dequeue(2);
        }


        [Test]
        public async Task TestQueueAfterDequeue()
        {
            async Task DequeueMany()
            {
                await Dequeue(1);
                await Dequeue(2);
            }

            async Task EnqueueMany()
            {
                await Task.Delay(1);
                await Enqueue(1);
                await Task.Delay(1);
                await Enqueue(2);
            }

            await Task.WhenAll([
                DequeueMany(),
                EnqueueMany()
            ]);
        }

        private async Task Enqueue(int val)
        {
            TestContext.WriteLine($"Enqueue {val}");
            await q.EnqueueAsync(val);
        }

        private async Task Dequeue(int val)
        {
            TestContext.WriteLine($"Dequeue {val}");
            Assert.That(await q.DequeueAsync(), Is.EqualTo(val));
        }

    }
}