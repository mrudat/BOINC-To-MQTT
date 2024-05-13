// Ignore Spelling: MQTT TLS

namespace Testcontainers;

public interface IMqttTlsContainer : ICommonMqttContainer, IGetServerCertificate
{
    /// <summary>
    /// The listening port for MQTT over TLS
    /// </summary>
    public ushort MqttTlsPort { get; }

    Uri ICommonMqttContainer.GetMqttUri(string? userName) => GetMqttTlsUri(userName);

    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => GetNetworkMqttTlsUri(userName);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for connecting to the MQTT server using MQTT with TLS.
    /// </summary>
    /// <param name="userName">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for connecting to the MQTT server.</returns>
    public Uri GetMqttTlsUri(string? userName = null);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for other containers to connect to the MQTT server using MQTT with TLS.
    /// </summary>
    /// <param name="userName">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for other containers to connect to the MQTT server.</returns>
    public Uri GetNetworkMqttTlsUri(string? userName = null);
}
