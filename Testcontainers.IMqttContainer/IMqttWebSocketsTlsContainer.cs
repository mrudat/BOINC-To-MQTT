// Ignore Spelling: MQTT TLS

namespace Testcontainers;

public interface IMqttWebSocketsTlsContainer : ICommonMqttContainer, IGetServerCertificate
{
    /// <summary>
    /// The listening port for MQTT over Web Sockets over TLS
    /// </summary>
    public ushort MqttWebSocketsTlsPort { get; }

    Uri ICommonMqttContainer.GetMqttUri(string? user) => GetWebSocketsTlsUri(user);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for connecting to the MQTT server using Web Sockets with TLS.
    /// </summary>
    /// <param name="user">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for connecting to the MQTT server.</returns>
    public Uri GetWebSocketsTlsUri(string? user = null);
}
