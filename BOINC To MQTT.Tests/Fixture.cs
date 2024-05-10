// Ignore Spelling: Mosquitto homeassistant offline MQTT username frontend
using DotNet.Testcontainers.Configurations;
using Meziantou.Extensions.Logging.Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using Testcontainers;
using Testcontainers.HomeAssistant;
using Testcontainers.Mosquitto;
using TestContainers.BOINC;
using Xunit.Abstractions;

namespace BOINC_To_MQTT.Tests;

// TODO Use at least one other MQTT server.

public class Fixture<TBuilder, TContainer>(ITestOutputHelper testOutputHelper, ILogger logger) : IAsyncLifetime
{
    private readonly ILogger logger = logger;

    private IMqttContainer? mqttContainer = null;

    private BOINCContainer? boincContainer = null;

    private HomeAssistantContainer? homeassistantContainer = null;

    internal readonly HostApplicationBuilder hostApplicationBuilder = Program.CreateApplicationBuilder([]);

    internal readonly MockFileSystem mockFileSystem = new();

    internal IMqttContainer MqttContainer => mqttContainer!;

    internal BOINCContainer BOINCContainer => boincContainer!;

    internal HomeAssistantContainer HomeAssistantContainer => homeassistantContainer!;

    async Task IAsyncLifetime.InitializeAsync()
    {
        mqttContainer = new MosquittoBuilder()
            .WithLogger(logger)
            .Build();

        boincContainer = new BoincBuilder()
            .WithLogger(logger)
            .Build();

        var boincStarted = boincContainer.StartAsync();

        await mqttContainer.StartAsync();

        await (mqttContainer switch
        {
            IAddUser container => container.AddUser("homeassistant", "homeassistant"),
            _ => Task.FromException(new NotImplementedException($"Can't add a new user to a {typeof(TContainer)}")),
        });


        var configurationYaml = new StringBuilder();

        configurationYaml.Append("""
            # Loads default set of integrations. Do not remove.
            default_config:

            # Load frontend themes from the themes folder
            #frontend:
            #  themes: !include_dir_merge_named themes

            #automation: !include automations.yaml
            #script: !include scripts.yaml
            #scene: !include scenes.yaml
            """);

        configurationYaml.Append($"""
            mqtt:
                broker: {mqttContainer.Hostname}
                port: {mqttContainer.MqttPort}
                discovery: true
                discovery_prefix: "homeassistant"
                username: "homeassistant"
                password: "homeassistant"
                birth_message:
                    topic: 'homeassistant/status'
                    payload: 'online'
                will_message:
                    topic: 'homeassistant/status'
                    payload: 'offline'
            """);

        homeassistantContainer = new HomeAssistantBuilder()
            .WithLogger(logger)
            .WithResourceMapping(Encoding.UTF8.GetBytes(configurationYaml.ToString()), "/config/configuration.yaml", Unix.FileMode644)
            .Build();

        var homeassistantStarted = homeassistantContainer.StartAsync();

        await boincStarted;

        var boincUri = new UriBuilder
        {
            Scheme = "tcp",
            Host = boincContainer.Hostname,
            Port = boincContainer.Port
        }.Uri;

        mockFileSystem
            .AddDirectory(hostApplicationBuilder.Environment.ContentRootPath);

        hostApplicationBuilder.Services
            .AddSingleton<IFileSystem>(mockFileSystem);

        hostApplicationBuilder.Logging
            .AddProvider(new XUnitLoggerProvider(testOutputHelper));

        hostApplicationBuilder.Configuration
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["BOINC2MQTT:MQTT:URI"] = mqttContainer.GetMqttUri().ToString(),
                ["BOINC2MQTT:Remote:0:BoincUri"] = boincUri.ToString(),
                ["BOINC2MQTT:Remote:0:GuiRpcKey"] = await boincContainer.GetGuiRpcKeyAsync()
            });

        await homeassistantStarted;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        List<Task> tasks = new(3);

        if (homeassistantContainer != null)
            tasks.Add(homeassistantContainer.StopAsync());

        if (mqttContainer != null)
            tasks.Add(mqttContainer.StopAsync());

        if (boincContainer != null)
            tasks.Add(boincContainer.StopAsync());

        await Task.WhenAll(tasks);
    }
}

