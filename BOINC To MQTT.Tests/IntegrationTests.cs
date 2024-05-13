// Ignore Spelling: BOINC MQTT TCP

using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Testcontainers.EMQX;
using Testcontainers.HiveMQ;
using Testcontainers.Mosquitto;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace BOINC_To_MQTT.Tests;

[Collection(nameof(MqttCollection))]
public class IntegrationTests(
    BoincFixture boincFixture,
    HomeAssistantFixture<EmqxContainer, EmqxBuilder> homeAssistantEmqxFixture,
    HomeAssistantFixture<HiveMQContainer, HiveMQBuilder> homeAssistantHiveMQFixture,
    HomeAssistantFixture<MosquittoContainer, MosquittoBuilder> homeAssistantMosquittoFixture,
    ITestOutputHelper testOutputHelper) : IClassFixture<BoincFixture>, IClassFixture<HomeAssistantFixture<MosquittoContainer, MosquittoBuilder>>, IClassFixture<HomeAssistantFixture<EmqxContainer, EmqxBuilder>>, IClassFixture<HomeAssistantFixture<HiveMQContainer, HiveMQBuilder>>
{
    private readonly Dictionary<string, IHomeAssistantFixture> homeAssistantFixtures = new()
    {
        [nameof(EmqxContainer)] = homeAssistantEmqxFixture,
        [nameof(HiveMQContainer)] = homeAssistantHiveMQFixture,
        [nameof(MosquittoContainer)] = homeAssistantMosquittoFixture,
    };

    public static readonly TheoryData<string> Fixtures = new([
        nameof(EmqxContainer),
        nameof(HiveMQContainer),
        nameof(MosquittoContainer),
    ]);

    [Theory]
    [MemberData(nameof(Fixtures))]
    public async Task Test1Async(string containerTypeName)
    {
        var fixture = homeAssistantFixtures[containerTypeName];

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

        var builder = await GetHostApplicationBuilderAsync(fixture, testOutputHelper);

        using var host = builder.Build();

        var cancellationTokenSource = new CancellationTokenSource();

        await Task.WhenAll([
            host.RunAsync(cancellationTokenSource.Token),
            TheTest(cancellationTokenSource)
        ]);

    }

    internal async Task<HostApplicationBuilder> GetHostApplicationBuilderAsync(IHomeAssistantFixture fixture, ITestOutputHelper testOutputHelper)
    {
        var builder = Program.CreateApplicationBuilder([]);

        MockFileSystem mockFileSystem = new();

        mockFileSystem
            .AddDirectory(builder.Environment.ContentRootPath);

        builder.Services
            .AddSingleton<IFileSystem>(mockFileSystem);

        builder.Logging
            .AddProvider(new XUnitLoggerProvider(testOutputHelper));

        var configuration = new Dictionary<string, string?>() {
            { "BOINC2MQTT:MQTT:URI", fixture.MqttContainer.GetMqttUri().ToString() },
            { "BOINC2MQTT:Remote:0:BoincUri", new UriBuilder("tcp", boincFixture.Container.Hostname, boincFixture.Container.Port).ToString() },
            { "BOINC2MQTT:Remote:0:GuiRpcKey", await boincFixture.Container.GetGuiRpcKeyAsync() }
        };

        builder.Configuration
            .AddInMemoryCollection(configuration);

        return builder;
    }

}
