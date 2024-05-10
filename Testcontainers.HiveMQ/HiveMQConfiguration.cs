// Ignore Spelling: MQTT username

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace Testcontainers.HiveMQ;

/// <inheritdoc cref="ContainerConfiguration" />
public class HiveMQConfiguration : ContainerConfiguration
{
    public string? Username { get; }
    public string? Password { get; }

    public Dictionary<string, string> Users { get; } = [];

    public HiveMQConfiguration(
        string? username = null,
        string? password = null)
    {
        Username = username;
        Password = password;
    }

    public HiveMQConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) : base(resourceConfiguration) { }

    public HiveMQConfiguration(IContainerConfiguration resourceConfiguration) : base(resourceConfiguration) { }

    public HiveMQConfiguration(HiveMQConfiguration resourceConfiguration) : this(new HiveMQConfiguration(), resourceConfiguration) { }

    public HiveMQConfiguration(HiveMQConfiguration oldValue, HiveMQConfiguration newValue) : base(oldValue, newValue)
    {
        Username = BuildConfiguration.Combine(oldValue.Username, newValue.Username);
        Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
    }
}
