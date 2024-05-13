// Ignore Spelling: BOINC MQTT TCP

using BOINC_To_MQTT.Boinc;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Xunit.Abstractions;

namespace BOINC_To_MQTT.Tests;

public class BoincIntegrationTests(BoincFixture boincFixture, ITestOutputHelper testOutputHelper) : IClassFixture<BoincFixture>
{
    [Fact]
    public void TestBoincAuthenticationFailure()
    {
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
                ["BOINC2MQTT:Remote:0:BoincUri"] = new UriBuilder("tcp", boincFixture.Container.Hostname, boincFixture.Container.Port).ToString(),
                ["BOINC2MQTT:Remote:0:GuiRpcKey"] = "Some Invalid Value"
            });

        using var host = builder.Build();

        CancellationTokenSource cts = new();

        cts.CancelAfter(TimeSpan.FromSeconds(5));

        host.Invoking(c => c!.RunAsync(cts.Token))
            .Should()
            .ThrowAsync<AuthorisationFailedException>();
    }
}