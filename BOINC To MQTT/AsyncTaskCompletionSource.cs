// Ignore Spelling: BOINC MQTT Awaiter

using STask = System.Threading.Tasks.Task;

namespace BOINC_To_MQTT;

internal class AsyncTaskCompletionSource
{
    private readonly TaskCompletionSource taskCompletionSource;

    internal readonly STask Task;

    public AsyncTaskCompletionSource()
    {
        taskCompletionSource = new();

        async STask Await()
        {
            await taskCompletionSource.Task;
            await STask.Yield();
        }

        Task = Await();
    }

    public async STask SetResult()
    {
        taskCompletionSource.SetResult();
        await STask.Yield();
    }

    internal async STask TrySetResult()
    {
        taskCompletionSource.TrySetResult();
        await STask.Yield();
    }
}

internal class AsyncTaskCompletionSource<TResult>
{
    private readonly TaskCompletionSource<TResult> taskCompletionSource;

    internal Task<TResult> Task;

    public AsyncTaskCompletionSource()
    {
        taskCompletionSource = new();

        async Task<TResult> Await()
        {
            var result = await taskCompletionSource.Task;
            await STask.Yield();
            return result;
        }

        Task = Await();
    }


    public async STask SetResult(TResult result)
    {
        taskCompletionSource.SetResult(result);
        await STask.Yield();
    }

}
