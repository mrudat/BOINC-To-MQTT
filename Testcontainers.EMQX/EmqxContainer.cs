// Ignore Spelling: MQTT username TLS mqtts ws wss

using DotNet.Testcontainers.Containers;
using System.Security.Cryptography.X509Certificates;

namespace Testcontainers.EMQX;

public class EmqxContainer(EMQXConfiguration configuration) : DockerContainer(configuration), IMqttContainer, IMqttTlsContainer, IMqttWebSocketsContainer, IMqttWebSocketsTlsContainer
{
    ushort IMqttContainer.MqttPort => GetMappedPublicPort(EmqxBuilder.MqttPort);

    ushort IMqttTlsContainer.MqttTlsPort => GetMappedPublicPort(EmqxBuilder.MqttTlsPort);

    ushort IMqttWebSocketsContainer.MqttWebSocketsPort => GetMappedPublicPort(EmqxBuilder.WebSocketsPort);

    ushort IMqttWebSocketsTlsContainer.MqttWebSocketsTlsPort => GetMappedPublicPort(EmqxBuilder.WebSocketsTlsPort);

    public ushort DashboardPort => GetMappedPublicPort(EmqxBuilder.DashboardPort);

    Uri ICommonMqttContainer.GetMqttUri(string? user) => (this as IMqttTlsContainer).GetMqttTlsUri(user);

    Uri IMqttContainer.GetMqttUri(string? user) => SetCredentials(new UriBuilder("mqtt", Hostname, (this as IMqttContainer).MqttPort), user).Uri;

    Uri IMqttTlsContainer.GetMqttTlsUri(string? user) => SetCredentials(new UriBuilder("mqtts", Hostname, (this as IMqttTlsContainer).MqttTlsPort), user).Uri;

    Uri IMqttWebSocketsContainer.GetWebSocketsUri(string? user) => SetCredentials(new UriBuilder("ws", Hostname, (this as IMqttWebSocketsContainer).MqttWebSocketsPort, "mqtt"), user).Uri;

    Uri IMqttWebSocketsTlsContainer.GetWebSocketsTlsUri(string? user) => SetCredentials(new UriBuilder("wss", Hostname, (this as IMqttWebSocketsTlsContainer).MqttWebSocketsTlsPort, "mqtt"), user).Uri;

    public Uri GetDashboardUri() => new UriBuilder("http", Hostname, DashboardPort).Uri;

    Task<X509Certificate2> IGetServerCertificate.GetServerCertificateAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
            uriBuilder.Password = Uri.EscapeDataString(configuration.Users[username]);
        }
        return uriBuilder;
    }

    public async Task AddUser(string username, string password, CancellationToken cancellationToken = default)
    {
        // TODO actually add a user.
        configuration.Users[username] = password;
    }

}
