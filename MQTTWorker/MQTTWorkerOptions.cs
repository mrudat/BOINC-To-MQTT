using System.ComponentModel.DataAnnotations;

namespace MQTTWorker;

public sealed class MQTTWorkerOptions
{
    public const string ConfigurationSectionName = "MQTT";

    /// <summary>
    /// The host name of the MQTT server to connect to.
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// The port number of the MQTT server to connect to.
    /// </summary>
    [Range(1, 65535)]
    public int? Port { get; set; }

    /// <summary>
    /// A unique identifier for this host, defaults to host_cpid read from client_state.xml.
    /// </summary>
    public string? ClientIdentifier;

    /// <summary>
    /// The user to use to authenticate to the MQTT server.
    /// </summary>
    [StringLength(1)]
    public string? User { get; set; }

    /// <summary>
    /// The password to use to authenticate to the MQTT server.
    /// </summary>
    [StringLength(1)]
    public string? Password { get; set; }

    public bool UserAndPasswordValid => User != null && Password != null;

    [Range(1, int.MaxValue, ErrorMessage = "Must supply at least one of (User name and Password), ... or ...")]
    public int AuthenticationCount => UserAndPasswordValid ? 1 : 0;
}

