// Ignore Spelling: BOINC MQTT homeassistant RPC

using System.ComponentModel.DataAnnotations;

namespace BOINC_To_MQTT.Mqtt;

public sealed class MqttOptions
{
    public const ushort MaximumTopicLength = ushort.MaxValue;

    // TODO v3.1 clientId max length is 23, for V5 it's 256.
    //public const ushort MaximumClientIdentifierLength = 23;
    public const ushort MaximumClientIdentifierLength = 256;

    /// <summary>
    /// The host name of the MQTT server to connect to.
    /// </summary>
    [Required]
    public /*required*/ Uri Uri { get; set; } = new Uri("mqtt://localhost");

    /// <summary>
    /// A unique identifier for this host, defaults to host_cpid read from client_state.xml.
    /// </summary>
    [StringLength(maximumLength: MaximumClientIdentifierLength, MinimumLength = 1)]
    [MqttIdentifier]
    public string? ClientIdentifier { get; set; }

    /// <summary>
    /// The discovery prefix for Home Assistant, needs to be set to a non-default value if the discovery prefix is changed in home assistant
    /// </summary>
    [StringLength(maximumLength: MaximumTopicLength, MinimumLength = 1)]
    public string DiscoveryPrefix { get; set; } = "homeassistant";
}
