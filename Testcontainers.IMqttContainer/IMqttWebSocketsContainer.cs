// Ignore Spelling: MQTT TLS

namespace Testcontainers;

public interface IMqttWebSocketsContainer : ICommonMqttContainer
{
    /// <summary>
    /// The listening port for MQTT over Web Sockets
    /// </summary>
    public ushort MqttWebSocketsPort { get; }

    Uri ICommonMqttContainer.GetMqttUri(string? user) => GetWebSocketsUri(user);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for connecting to the MQTT server using MQTT over Web Sockets.
    /// </summary>
    /// <param name="user">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for connecting to the MQTT server using MQTT over Web Sockets.</returns>
    public Uri GetWebSocketsUri(string? user = null);
}
