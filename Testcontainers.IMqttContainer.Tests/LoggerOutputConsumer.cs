// Ignore Spelling: MQTT Testcontainers Initialize TLS

using DotNet.Testcontainers.Configurations;
using System.IO.Pipelines;
using Xunit.Abstractions;

namespace Testcontainers.Tests;
internal class LoggerOutputConsumer : IOutputConsumer
{
    private readonly Pipe _stdoutPipe = new();
    private readonly Pipe _stderrPipe = new();

    private readonly Stream _stdout;
    private readonly Stream _stderr;
    private readonly LoggerWriter _stdoutLoggerWriter;
    private readonly LoggerWriter _stderrLoggerWriter;
    private bool disposedValue;

    private bool _enabled;

    public LoggerOutputConsumer(ITestOutputHelper testOutputHelper)
    {
        _stdoutPipe = new();
        _stderrPipe = new();

        _stdout = _stdoutPipe.Writer.AsStream();
        _stderr = _stderrPipe.Writer.AsStream();

        _stdoutLoggerWriter = new LoggerWriter(testOutputHelper, _stdoutPipe.Reader);
        _stderrLoggerWriter = new LoggerWriter(testOutputHelper, _stderrPipe.Reader);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await Task.WhenAll([
            _stdoutLoggerWriter.StartAsync(cancellationToken),
            _stderrLoggerWriter.StartAsync(cancellationToken)
            ]);

        _enabled = true;
    }

    private class LoggerWriter(ITestOutputHelper testOutputHelper, PipeReader reader) : IAsyncDisposable
    {
        private CancellationTokenSource? _cancellationTokenSource = null;

        private Task? _task = null;

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (_task != null)
                throw new Exception("Already started!");

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            _task = Run(_cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        public async Task Run(CancellationToken cancellationToken = default)
        {
            var stream = reader.AsStream();
            var textStream = new StreamReader(stream);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var line = await textStream.ReadLineAsync(cancellationToken);
                    if (line == null)
                        return;
                    testOutputHelper.WriteLine(line);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            testOutputHelper.WriteLine(await textStream.ReadToEndAsync(CancellationToken.None));
        }

        public async ValueTask DisposeAsync()
        {
            if (_task != null)
            {
                _cancellationTokenSource?.Cancel();
                await _task;
            }
        }
    }

    bool IOutputConsumer.Enabled => _enabled;

    Stream IOutputConsumer.Stdout => _stdout;

    Stream IOutputConsumer.Stderr => _stderr;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _stdout?.Dispose();
                _stderr?.Dispose();
                // TODO: dispose managed state (managed objects)
            }
            disposedValue = true;
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}