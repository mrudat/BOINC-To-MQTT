// Ignore Spelling: BOINC MQTT Awaiter

using STask = System.Threading.Tasks.Task;

namespace BOINC_To_MQTT;

/// <inheritdoc cref="TaskCompletionSource">
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

    /// <inheritdoc cref="TaskCompletionSource.SetResult"/>
    public async STask SetResult()
    {
        taskCompletionSource.SetResult();
        await STask.Yield();
    }

    /// <inheritdoc cref="TaskCompletionSource.TrySetResult"/>
    internal async STask TrySetResult()
    {
        taskCompletionSource.TrySetResult();
        await STask.Yield();
    }

    /// <inheritdoc cref="TaskCompletionSource.TrySetException(Exception)"/>
    internal async STask TrySetException(Exception exception)
    {
        taskCompletionSource.TrySetException(exception);
        await STask.Yield();
    }

    /// <inheritdoc cref="TaskCompletionSource.TrySetException(IEnumerable{Exception})"/>
    internal async STask TrySetException(IEnumerable<Exception> exception)
    {
        taskCompletionSource.TrySetException(exception);
        await STask.Yield();
    }
}

/// <inheritdoc cref="TaskCompletionSource{TResult}">
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


    /// <inheritdoc cref="TaskCompletionSource{TResult}.SetResult(TResult)"/>
    public async STask SetResult(TResult result)
    {
        taskCompletionSource.SetResult(result);
        await STask.Yield();
    }

}
