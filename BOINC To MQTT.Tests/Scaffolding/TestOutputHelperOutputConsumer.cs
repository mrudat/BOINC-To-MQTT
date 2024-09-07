// <copyright file="TestOutputHelperOutputConsumer.cs" company="Martin Rudat">
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

namespace BOINC_To_MQTT.Tests.Scaffolding;

using System.IO.Pipelines;
using DotNet.Testcontainers.Configurations;
using Xunit.Abstractions;

internal class TestOutputHelperOutputConsumer : IOutputConsumer, IAsyncDisposable
{
    private readonly Stream stderr;
    private readonly LoggerWriter stderrLoggerWriter;
    private readonly Pipe stderrPipe;
    private readonly Stream stdout;
    private readonly LoggerWriter stdoutLoggerWriter;
    private readonly Pipe stdoutPipe;
    private bool disposedValue;

    private bool enabled;

    public TestOutputHelperOutputConsumer(ITestOutputHelper testOutputHelper, string prefix = "")
    {
        this.stdoutPipe = new();
        this.stderrPipe = new();

        this.stdout = this.stdoutPipe.Writer.AsStream();
        this.stderr = this.stderrPipe.Writer.AsStream();

        this.stdoutLoggerWriter = new LoggerWriter(testOutputHelper, this.stdoutPipe.Reader, prefix);
        this.stderrLoggerWriter = new LoggerWriter(testOutputHelper, this.stderrPipe.Reader, prefix);
    }

    /// <inheritdoc/>
    bool IOutputConsumer.Enabled
    {
        get
        {
            if (!this.enabled)
            {
                this.StartAsync().Wait();
            }

            return this.enabled;
        }
    }

    /// <inheritdoc/>
    Stream IOutputConsumer.Stderr => this.stderr;

    /// <inheritdoc/>
    Stream IOutputConsumer.Stdout => this.stdout;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!this.disposedValue)
        {
            List<Task> tasks = new(2);
            if (this.stdout != null)
            {
                tasks.Add(this.stdout.DisposeAsync().AsTask());
            }

            if (this.stderr != null)
            {
                tasks.Add(this.stderr.DisposeAsync().AsTask());
            }

            await Task.WhenAll(tasks);
            this.disposedValue = true;
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll(
            this.stdoutLoggerWriter.StartAsync(cancellationToken),
            this.stderrLoggerWriter.StartAsync(cancellationToken));

        this.enabled = true;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.stdout?.Dispose();
                this.stderr?.Dispose();
            }

            this.disposedValue = true;
        }
    }

    private class LoggerWriter(ITestOutputHelper testOutputHelper, PipeReader reader, string prefix = "") : IAsyncDisposable
    {
        private CancellationTokenSource? cancellationTokenSource = null;

        private Task? task = null;

        public async ValueTask DisposeAsync()
        {
            if (this.task != null)
            {
                this.cancellationTokenSource?.Cancel();
                await this.task;
            }

            this.cancellationTokenSource?.Dispose();
        }

        public async Task Run(CancellationToken cancellationToken = default)
        {
            var stream = reader.AsStream();
            using var textStream = new StreamReader(stream);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var line = await textStream.ReadLineAsync(cancellationToken);
                    if (line == null)
                    {
                        return;
                    }

                    testOutputHelper.WriteLine($"{prefix}{line}");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            testOutputHelper.WriteLine($"{prefix}{await textStream.ReadToEndAsync(CancellationToken.None)}");
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (this.task != null)
            {
                throw new Exception("Already started!");
            }

            this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            this.task = this.Run(this.cancellationTokenSource.Token);

            return Task.CompletedTask;
        }
    }
}
