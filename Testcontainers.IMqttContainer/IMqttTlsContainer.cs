// Ignore Spelling: MQTT TLS

namespace Testcontainers;

public interface IMqttTlsContainer : ICommonMqttContainer, IGetServerCertificate
{
    /// <summary>
    /// The listening port for MQTT over TLS
    /// </summary>
    public ushort MqttTlsPort { get; }

    Uri ICommonMqttContainer.GetMqttUri(string? user) => GetMqttTlsUri(user);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for connecting to the MQTT server using MQTT with TLS.
    /// </summary>
    /// <param name="user">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for connecting to the MQTT server.</returns>
    public Uri GetMqttTlsUri(string? user = null);
}
