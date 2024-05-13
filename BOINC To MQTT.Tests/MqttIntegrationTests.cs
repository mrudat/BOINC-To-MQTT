// Ignore Spelling: BOINC MQTT TCP

using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Adapter;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Testcontainers;
using Testcontainers.EMQX;
using Testcontainers.HiveMQ;
using Testcontainers.Mosquitto;
using Xunit.Abstractions;

namespace BOINC_To_MQTT.Tests;

[Collection(nameof(MqttCollection))]
public class MqttIntegrationTests(
    MqttFixture<EmqxContainer, EmqxBuilder> emqxFixture,
    MqttFixture<HiveMQContainer, HiveMQBuilder> hiveMqFixture,
    MqttFixture<MosquittoContainer, MosquittoBuilder> mosquittoFixture,
    ITestOutputHelper testOutputHelper)
{
    private readonly Dictionary<string, IMqttFixture> fixtures = new()
    {
        [nameof(EmqxContainer)] = emqxFixture,
        [nameof(HiveMQContainer)] = hiveMqFixture,
        [nameof(MosquittoContainer)] = mosquittoFixture,
    };

    public static readonly TheoryData<string> RequiresAuthentication = TheoryDataStuff.Implementing<IRequiresAuthentication>([
            typeof(EmqxContainer),
            typeof(HiveMQContainer),
            typeof(MosquittoContainer),
        ]);

    [Theory]
    [MemberData(nameof(RequiresAuthentication))]
    public void TestMqttAuthenticationFailure(string containerTypeName)
    {
        var fixture = fixtures[containerTypeName];

        var builder = Program.CreateApplicationBuilder([]);

        MockFileSystem mockFileSystem = new();

        mockFileSystem
            .AddDirectory(builder.Environment.ContentRootPath);

        builder.Services
            .AddSingleton<IFileSystem>(mockFileSystem);

        builder.Logging
            .AddProvider(new XUnitLoggerProvider(testOutputHelper));

        builder.Configuration
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["BOINC2MQTT:MQTT:URI"] = new UriBuilder(fixture.MqttContainer.GetMqttUri())
                {
                    Password = "Some Invalid Value"
                }.ToString()
            });

        using var host = builder.Build();

        CancellationTokenSource cts = new();

        cts.CancelAfter(TimeSpan.FromSeconds(5));

        host.Invoking(c => c!.RunAsync(cts.Token))
            .Should()
            .ThrowAsync<MqttConnectingFailedException>();
    }
}