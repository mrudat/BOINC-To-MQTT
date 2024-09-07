// <copyright file="AsyncTaskCompletionSource.cs" company="Martin Rudat">
// BOINC To MQTT - Exposes some BOINC controls via MQTT for integration with Home Assistant.
// Copyright (C) 2024  Martin Rudat
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
// </copyright>

namespace BOINC_To_MQTT;

using STask = System.Threading.Tasks.Task;

/// <inheritdoc cref="TaskCompletionSource" />
internal class AsyncTaskCompletionSource
{
    private readonly TaskCompletionSource taskCompletionSource;

    /// <inheritdoc cref="TaskCompletionSource()" />
    public AsyncTaskCompletionSource()
    {
        this.taskCompletionSource = new();

        async STask Await()
        {
            await this.taskCompletionSource.Task.ConfigureAwait(false);
            await STask.Yield();
        }

        this.Task = Await();
    }

    /// <inheritdoc cref="TaskCompletionSource.Task" />
    public STask Task { get; init; }

    /// <inheritdoc cref="TaskCompletionSource.SetResult"/>
    public async STask SetResult()
    {
        this.taskCompletionSource.SetResult();
        await STask.Yield();
    }

    /// <inheritdoc cref="TaskCompletionSource.TrySetException(Exception)"/>
    internal async STask TrySetException(Exception exception)
    {
        this.taskCompletionSource.TrySetException(exception);
        await STask.Yield();
    }

    /// <inheritdoc cref="TaskCompletionSource.TrySetException(IEnumerable{Exception})"/>
    internal async STask TrySetException(IEnumerable<Exception> exception)
    {
        this.taskCompletionSource.TrySetException(exception);
        await STask.Yield();
    }

    /// <inheritdoc cref="TaskCompletionSource.TrySetResult"/>
    internal async STask TrySetResult()
    {
        this.taskCompletionSource.TrySetResult();
        await STask.Yield();
    }
}

/// <inheritdoc cref="TaskCompletionSource{TResult}"/>
internal class AsyncTaskCompletionSource<TResult>
{
    private readonly TaskCompletionSource<TResult> taskCompletionSource;

    /// <inheritdoc cref="TaskCompletionSource{TResult}()" />
    public AsyncTaskCompletionSource()
    {
        this.taskCompletionSource = new();

        async Task<TResult> Await()
        {
            var result = await this.taskCompletionSource.Task.ConfigureAwait(false);
            await STask.Yield();
            return result;
        }

        this.Task = Await();
    }

    /// <inheritdoc cref="TaskCompletionSource{TResult}.Task" />
    public Task<TResult> Task { get; init; }

    /// <inheritdoc cref="TaskCompletionSource{TResult}.SetResult(TResult)"/>
    public async STask SetResult(TResult result)
    {
        this.taskCompletionSource.SetResult(result);
        await STask.Yield();
    }
}
