// Ignore Spelling: MQTT TLS

namespace Testcontainers;

public interface IMqttContainer : ICommonMqttContainer
{
    /// <summary>
    /// The listening port for unencrypted MQTT
    /// </summary>
    public ushort MqttPort { get; }

    Uri ICommonMqttContainer.GetMqttUri(string? user) => GetMqttUri(user);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for connecting to the MQTT server using MQTT.
    /// </summary>
    /// <param name="user">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for connecting to the MQTT server using MQTT.</returns>
    public new Uri GetMqttUri(string? user = null);
}
