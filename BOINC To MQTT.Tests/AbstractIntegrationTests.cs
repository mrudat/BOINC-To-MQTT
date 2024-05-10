// Ignore Spelling: BOINC MQTT

using BOINC_To_MQTT.Boinc;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Adapter;
using Testcontainers;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace BOINC_To_MQTT.Tests;

public abstract class AbstractIntegrationTests<TBuilder, TContainer>(ITestOutputHelper testOutputHelper, ILogger logger) : IAsyncLifetime
{
    private readonly Fixture<TBuilder, TContainer> fixture = new(testOutputHelper, logger);

    Task IAsyncLifetime.InitializeAsync()
    {
        return (fixture as IAsyncLifetime).InitializeAsync();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return (fixture as IAsyncLifetime).DisposeAsync();
    }

    [Fact]
    public async Task Test1()
    {
        static async Task TheTest(CancellationTokenSource cancellationTokenSource)
        {
            // TODO interact with home assistant, and check BOINC

            bool forever = false;

            if (!forever)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                await Task.Yield();
            }
            else
            {
                bool done = false;

                while (!done)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30));
                    await Task.Yield();
                }
            }

            cancellationTokenSource.Cancel();
        }

        using var host = fixture.hostApplicationBuilder.Build();

        var cancellationTokenSource = new CancellationTokenSource();

        await Task.WhenAll([
            host.RunAsync(cancellationTokenSource.Token),
            TheTest(cancellationTokenSource)
        ]);
    }

    [Fact]
    public async Task TestBoincAuthenticationFailure()
    {
        fixture.hostApplicationBuilder.Configuration
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["BOINC2MQTT:Remote:0:GuiRpcKey"] = "Some Invalid Value"
            });

        using var host = fixture.hostApplicationBuilder.Build();

        CancellationTokenSource cancellationTokenSource = new();

        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));

        var action = async () => await host.RunAsync(cancellationTokenSource.Token);

        await action.Should().ThrowAsync<AuthorisationFailedException>();
    }

    [Fact]
    public async Task TestMqttAuthenticationFailure()
    {
        fixture.hostApplicationBuilder.Configuration
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["BOINC2MQTT:MQTT:URI"] = new UriBuilder((fixture.MqttContainer as ICommonMqttContainer).GetMqttUri())
                {
                    Password = "Some Invalid Value"
                }.ToString()
            });

        using var host = fixture.hostApplicationBuilder.Build();

        CancellationTokenSource cancellationTokenSource = new();

        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(5));

        var action = async () => await host.RunAsync(cancellationTokenSource.Token);

        await action.Should().ThrowAsync<MqttConnectingFailedException>();
    }

}

