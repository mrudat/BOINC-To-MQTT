// Ignore Spelling: mosquitto MQTT mqtts usr passwd ws
using DotNet.Testcontainers.Containers;

namespace Testcontainers.Mosquitto;

public class MosquittoContainer(MosquittoConfiguration configuration) : DockerContainer(configuration), IMqttContainer, IMqttWebSocketsContainer, IAddUser
{
    ushort IMqttContainer.MqttPort => GetMappedPublicPort(MosquittoBuilder.MqttPort);

    ushort IMqttWebSocketsContainer.MqttWebSocketsPort => GetMappedPublicPort(MosquittoBuilder.WebSocketsPort);

    Uri ICommonMqttContainer.GetMqttUri(string? user) => (this as IMqttContainer).GetMqttUri(user);

    Uri IMqttContainer.GetMqttUri(string? user) => SetCredentials(new UriBuilder("mqtt", Hostname, (int)(this as IMqttContainer).MqttPort), user).Uri;

    Uri IMqttWebSocketsContainer.GetWebSocketsUri(string? user) => SetCredentials(new UriBuilder("ws", Hostname, (int)(this as IMqttWebSocketsContainer).MqttWebSocketsPort), user).Uri;

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

    async Task IAddUser.AddUser(string user, string password, CancellationToken cancellationToken)
    {
        async Task Exec(string[] command)
        {
            var result = await ExecAsync(command, cancellationToken);

            if (result.ExitCode != 0)
                throw new Exception("TODO throw a better exception");
        }

        await Exec(["/usr/bin/mosquitto_passwd", "-b", "/mosquitto/config/passwd", user, password]);

        configuration.Users[user] = password;

        await Exec(["kill", "-HUP", "1"]);
    }
}
