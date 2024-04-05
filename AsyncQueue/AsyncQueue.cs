namespace AsyncQueue
{
    public class AsyncQueue<T> {
        private readonly Queue<T> theQueue = new();
        private readonly Queue<TaskCompletionSource<T>> waitForItemsQueue = new();

        public AsyncQueue() {
        }

        public async Task EnqueueAsync(T item, CancellationToken cancellationToken = default) {
            if (waitForItemsQueue.TryDequeue(out var tcs))
                tcs.SetResult(item);
            else
                theQueue.Enqueue(item);
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default) {
            if (theQueue.TryDequeue(out var item))
                return item;

            return await WaitForNextItem(cancellationToken);
        }

        private Task<T> WaitForNextItem(CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<T>();
            var ctr = cancellationToken.Register(() =>
            {
                tcs.TrySetCanceled();
            });
            tcs.Task.ContinueWith(async (t) =>
            {
                await ctr.DisposeAsync();
                return await t;
            });
            waitForItemsQueue.Enqueue(tcs);
            return tcs.Task; 
        }
    }
}
