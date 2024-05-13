// Ignore Spelling: MQTT username TLS mqtts ws wss EMQX

using DotNet.Testcontainers.Containers;
using System.Security.Cryptography.X509Certificates;

namespace Testcontainers.EMQX;

public class EmqxContainer(EmqxConfiguration configuration) : DockerContainer(configuration), IMqttContainer, IMqttTlsContainer, IMqttWebSocketsContainer, IMqttWebSocketsTlsContainer
{
    ushort IMqttContainer.MqttPort => GetMappedPublicPort(EmqxBuilder.MqttPort);

    ushort IMqttTlsContainer.MqttTlsPort => GetMappedPublicPort(EmqxBuilder.MqttTlsPort);

    ushort IMqttWebSocketsContainer.MqttWebSocketsPort => GetMappedPublicPort(EmqxBuilder.MqttWebSocketsPort);

    ushort IMqttWebSocketsTlsContainer.MqttWebSocketsTlsPort => GetMappedPublicPort(EmqxBuilder.MqttWebSocketsTlsPort);

    public ushort DashboardPort => GetMappedPublicPort(EmqxBuilder.DashboardPort);

    Uri ICommonMqttContainer.GetMqttUri(string? userName) => (this as IMqttTlsContainer).GetMqttTlsUri(userName);

    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => (this as IMqttTlsContainer).GetNetworkMqttTlsUri(userName);

    Uri IMqttContainer.GetMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, Hostname, (this as IMqttContainer).MqttPort), userName).Uri;

    Uri IMqttContainer.GetNetworkMqttUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtt, configuration.NetworkAliases.First(), EmqxBuilder.MqttPort), userName).Uri;

    Uri IMqttTlsContainer.GetMqttTlsUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtts, Hostname, (this as IMqttTlsContainer).MqttTlsPort), userName).Uri;

    Uri IMqttTlsContainer.GetNetworkMqttTlsUri(string? userName) => SetCredentials(new UriBuilder(MqttConstants.UriSchemeMqtts, configuration.NetworkAliases.First(), EmqxBuilder.MqttTlsPort), userName).Uri;

    Uri IMqttWebSocketsContainer.GetWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, Hostname, (this as IMqttWebSocketsContainer).MqttWebSocketsPort, "mqtt"), userName).Uri;

    Uri IMqttWebSocketsContainer.GetNetworkWebSocketsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWs, configuration.NetworkAliases.First(), EmqxBuilder.MqttWebSocketsPort, "mqtt"), userName).Uri;

    Uri IMqttWebSocketsTlsContainer.GetWebSocketsTlsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWss, Hostname, (this as IMqttWebSocketsTlsContainer).MqttWebSocketsTlsPort, "mqtt"), userName).Uri;

    Uri IMqttWebSocketsTlsContainer.GetNetworkWebSocketsTlsUri(string? userName) => SetCredentials(new UriBuilder(Uri.UriSchemeWss, configuration.NetworkAliases.First(), EmqxBuilder.MqttWebSocketsTlsPort, "mqtt"), userName).Uri;

    public Uri GetDashboardUri() => new UriBuilder(Uri.UriSchemeHttp, Hostname, DashboardPort).Uri;

    Task<X509Certificate2> IGetServerCertificate.GetServerCertificateAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    private static UriBuilder SetCredentials(UriBuilder uriBuilder, string? userName)
    {
        uriBuilder.UserName = Uri.EscapeDataString(userName ?? EmqxBuilder.DefaultUsername);
        return uriBuilder;
    }
}
