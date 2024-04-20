// Ignore Spelling: BOINC

using BoincRpc;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace BOINC_To_MQTT.Tests;

public class BOINCConnectionTests
{
    private class TestBOINCConnection(
        IOptions<BOINC2MQTTWorkerOptions> options,
        IFileSystem fileSystem,
        Mock<HostInfo>? hostInfo = null) : BOINCConnection(options, fileSystem)
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#pragma warning disable IDE0060 // Remove unused parameter
        public new async Task<HostInfo> GetHostInfoAsync(CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (hostInfo == null)
                throw new InvalidOperationException("Shouldn't request HostInfo");
            return hostInfo.Object;
        }
    }

    [Fact(Skip = "FIXME Can't mock HostInfo")]
    public async Task TestGetClientIdentifierFromBOINC()
    {
        var mockOptions = new Mock<IOptions<BOINC2MQTTWorkerOptions>>();
        var fileSystem = new MockFileSystem();
        var mockHostInfo = new Mock<HostInfo>();

        mockHostInfo.Setup(hostInfo => hostInfo.HostCPID).Returns("abc");

        var foo = new TestBOINCConnection(mockOptions.Object, fileSystem, mockHostInfo);

        var clientId = await foo.GetClientIdentifierAsync();

        clientId.Should().Be("abc");
    }

    [Fact]
    public async Task TestGetClientIdentifierFromConfig()
    {
        var mockOptions = new Mock<IOptions<BOINC2MQTTWorkerOptions>>();
        var fileSystem = new MockFileSystem();

        var options = new BOINC2MQTTWorkerOptions()
        {
            BOINC = new()
            {
                DataPath = "",
                BinaryPath = ""
            },
            MQTT = new()
            {
                ClientIdentifier = "abc"
            }
        };

        mockOptions.Setup((options) => options.Value).Returns(options);

        var boincConnection = new BOINCConnection(mockOptions.Object, fileSystem);

        var clientIdentifier = await boincConnection.GetClientIdentifierAsync();

        clientIdentifier.Should().Be("abc");
    }

    [Fact]
    public async Task TestGetRPCKey()
    {
        var mockOptions = new Mock<IOptions<BOINC2MQTTWorkerOptions>>();
        var fileSystem = new MockFileSystem();
        fileSystem.AddFile("C:\\Program Data\\BOINC\\gui_rpc_auth.cfg", "abc");

        var options = new BOINC2MQTTWorkerOptions()
        {
            BOINC = new()
            {
                DataPath = "C:\\Program Data\\BOINC",
                BinaryPath = "C:\\Program Files\\BOINC"
            },
            MQTT = new()
        };

        mockOptions.Setup((options) => options.Value).Returns(options);

        var boincConnection = new BOINCConnection(mockOptions.Object, fileSystem);

        var rpcKey = await boincConnection.GetRPCKey();

        rpcKey.Should().Be("abc");

    }

    [Fact]
    public async Task TestGetRPCKeyMissing()
    {
        var mockOptions = new Mock<IOptions<BOINC2MQTTWorkerOptions>>();
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("C:\\Program Data\\BOINC");

        var options = new BOINC2MQTTWorkerOptions()
        {
            BOINC = new()
            {
                DataPath = "C:\\Program Data\\BOINC",
                BinaryPath = "C:\\Program Files\\BOINC"
            },
            MQTT = new()
        };

        mockOptions.Setup((options) => options.Value).Returns(options);

        var boincConnection = new BOINCConnection(mockOptions.Object, fileSystem);

        var getRpcKey = async () => await boincConnection.GetRPCKey();

        await getRpcKey.Should().ThrowAsync<FileNotFoundException>();
    }
}