// Ignore Spelling: MQTT TLS

namespace Testcontainers;

public interface IMqttContainer : ICommonMqttContainer
{
    /// <summary>
    /// The listening port for unencrypted MQTT
    /// </summary>
    public ushort MqttPort { get; }

    Uri ICommonMqttContainer.GetMqttUri(string? userName) => GetMqttUri(userName);

    Uri ICommonMqttContainer.GetNetworkMqttUri(string? userName) => GetNetworkMqttUri(userName);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for connecting to the MQTT server using MQTT.
    /// </summary>
    /// <param name="userName">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for connecting to the MQTT server using MQTT.</returns>
    public new Uri GetMqttUri(string? userName = null);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for other containers to connect to the MQTT server using MQTT.
    /// </summary>
    /// <param name="userName">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for other containers to connect to the MQTT server using MQTT.</returns>
    public new Uri GetNetworkMqttUri(string? userName = null);
}
