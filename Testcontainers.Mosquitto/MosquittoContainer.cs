// Ignore Spelling: mosquitto MQTT mqtts usr passwd ws
using DotNet.Testcontainers.Containers;

namespace Testcontainers.Mosquitto;

public class MosquittoContainer(MosquittoConfiguration configuration) : DockerContainer(configuration), IMqttContainer, IMqttWebSocketsContainer, IRequiresAuthentication
{
    ushort IMqttContainer.MqttPort => GetMappedPublicPort(MosquittoBuilder.MqttPort);

    ushort IMqttWebSocketsContainer.MqttWebSocketsPort => GetMappedPublicPort(MosquittoBuilder.MqttWebSocketsPort);

    Uri ICommonMqttContainer.GetMqttUri(string? userName) => (this as IMqttContainer).GetMqttUri(userName);

    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => (this as IMqttContainer).GetNetworkMqttUri(userName);

    Uri IMqttContainer.GetMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, Hostname, (this as IMqttContainer).MqttPort), userName).Uri;

    Uri IMqttContainer.GetNetworkMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, configuration.NetworkAliases.First(), MosquittoBuilder.MqttPort), userName).Uri;

    Uri IMqttWebSocketsContainer.GetWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, Hostname, (this as IMqttWebSocketsContainer).MqttWebSocketsPort), userName).Uri;

    Uri IMqttWebSocketsContainer.GetNetworkWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, configuration.NetworkAliases.First(), MosquittoBuilder.MqttWebSocketsPort), userName).Uri;

    async Task IRequiresAuthentication.AddUser(string userName, string password, CancellationToken cancellationToken)
    {
        async Task Exec(string[] command)
        {
            var result = await ExecAsync(command, cancellationToken);

            if (result.ExitCode != 0)
                throw new Exception("TODO throw a better exception");
        }

        await Exec(["/usr/bin/mosquitto_passwd", "-b", "/mosquitto/config/passwd", userName, password]);

        configuration.Users[userName] = password;

        await Exec(["kill", "-HUP", "1"]);
    }
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
            if (configuration.Users.TryGetValue(username, out var password))
                uriBuilder.Password = Uri.EscapeDataString(password);
        }
        return uriBuilder;
    }

}
