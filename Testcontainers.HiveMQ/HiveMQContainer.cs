// Ignore Spelling: MQTT ws

using DotNet.Testcontainers.Containers;

namespace Testcontainers.HiveMQ;

public class HiveMQContainer(HiveMQConfiguration configuration) : DockerContainer(configuration), IMqttContainer, IMqttWebSocketsContainer
{
    ushort IMqttContainer.MqttPort => GetMappedPublicPort(HiveMQBuilder.MqttPort);

    ushort IMqttWebSocketsContainer.MqttWebSocketsPort => GetMappedPublicPort(HiveMQBuilder.MqttWebSocketsPort);

    Uri ICommonMqttContainer.GetMqttUri(string? userName) => (this as IMqttContainer).GetMqttUri(userName);

    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => (this as IMqttContainer).GetNetworkMqttUri(userName);

    Uri IMqttContainer.GetMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, Hostname, (this as IMqttContainer).MqttPort), userName).Uri;
    Uri IMqttContainer.GetNetworkMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, configuration.NetworkAliases.First(), HiveMQBuilder.MqttPort), userName).Uri;

    Uri IMqttWebSocketsContainer.GetWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, Hostname, (this as IMqttWebSocketsContainer).MqttWebSocketsPort, "mqtt"), userName).Uri;

    Uri IMqttWebSocketsContainer.GetNetworkWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, configuration.NetworkAliases.First(), HiveMQBuilder.MqttWebSocketsPort, "mqtt"), userName).Uri;

    private static UriBuilder SetCredentials(UriBuilder uriBuilder, string? userName)
    {
        uriBuilder.UserName = Uri.EscapeDataString(userName ?? HiveMQBuilder.DefaultUsername);
        return uriBuilder;
    }
}
