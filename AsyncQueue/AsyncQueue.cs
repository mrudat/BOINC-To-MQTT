namespace AsyncQueue
{
    public class AsyncQueue<T> {
        private readonly Queue<T> theQueue = new();
        private readonly Queue<TaskCompletionSource<T>> waitForItemsQueue = new();

        public void Enqueue(T item, CancellationToken cancellationToken = default)
        {
            if (waitForItemsQueue.TryDequeue(out var taskCompletionSource))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (taskCompletionSource.Task.Status == TaskStatus.Canceled) { 
                    throw new TaskCanceledException();
                }
                taskCompletionSource.SetResult(item);
            }
            else
                theQueue.Enqueue(item);
        }

        public async Task<T> DequeueAsync(CancellationToken cancellationToken = default) {
            if (theQueue.TryDequeue(out var item))
                return item;

            var taskCompletionSource = new TaskCompletionSource<T>();

            using var ctr = cancellationToken.Register(taskCompletionSource.SetCanceled);
            
            waitForItemsQueue.Enqueue(taskCompletionSource);

            return await taskCompletionSource.Task;
        }
    }
}
