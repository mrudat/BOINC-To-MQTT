// Ignore Spelling: MQTT TLS

using DotNet.Testcontainers.Containers;

namespace Testcontainers;

public interface ICommonMqttContainer : IContainer
{
    /// <summary>
    /// Returns a <seealso cref="Uri"/> for connecting to the MQTT server.
    /// </summary>
    /// <param name="user">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for connecting to the MQTT server.</returns>
    public Uri GetMqttUri(string? user = null);

    /// <summary>
    /// Returns a <seealso cref="Uri"/> for other containers to connect to the MQTT server.
    /// </summary>
    /// <param name="user">The user to authenticate to the MQTT server.</param>
    /// <returns>The <seealso cref="Uri"/> for other containers to connect to the MQTT server.</returns>
    public Uri GetNetworkMqttUri(string? user = null);
}
