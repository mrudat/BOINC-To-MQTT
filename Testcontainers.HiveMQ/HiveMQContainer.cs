// Ignore Spelling: MQTT username ws

using DotNet.Testcontainers.Containers;

namespace Testcontainers.HiveMQ;

public class HiveMQContainer(HiveMQConfiguration configuration) : DockerContainer(configuration), IMqttContainer, IMqttWebSocketsContainer
{
    ushort IMqttContainer.MqttPort => GetMappedPublicPort(HiveMQBuilder.MqttPort);

    ushort IMqttWebSocketsContainer.MqttWebSocketsPort => GetMappedPublicPort(HiveMQBuilder.WebSocketsPort);

    Uri ICommonMqttContainer.GetMqttUri(string? user) => (this as IMqttContainer).GetMqttUri(user);

    Uri IMqttContainer.GetMqttUri(string? user) => SetCredentials(new UriBuilder("mqtt", Hostname, (this as IMqttContainer).MqttPort), user).Uri;

    Uri IMqttWebSocketsContainer.GetWebSocketsUri(string? user) => SetCredentials(new UriBuilder("ws", Hostname, (this as IMqttWebSocketsContainer).MqttWebSocketsPort, "mqtt"), user).Uri;

    private UriBuilder SetCredentials(UriBuilder uriBuilder, string? username)
    {
        if (username == null)
        {
            uriBuilder.UserName = Uri.EscapeDataString(configuration.Username!);
            uriBuilder.Password = Uri.EscapeDataString(configuration.Password!);
        }
        else
        {
            uriBuilder.UserName = Uri.EscapeDataString(username);
            uriBuilder.Password = Uri.EscapeDataString(configuration.Users[username]);
        }
        return uriBuilder;
    }

    public Task AddUser(string username, string password, CancellationToken cancellationToken = default)
    {
        // TODO enable authentication.
        // HiveMQ's default configuration allows unauthenticated connections.
        configuration.Users[username] = password;
        return Task.CompletedTask;
    }
}
