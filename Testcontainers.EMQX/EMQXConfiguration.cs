// Ignore Spelling: MQTT username

using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace Testcontainers.EMQX;

/// <inheritdoc cref="ContainerConfiguration" />
public class EMQXConfiguration : ContainerConfiguration
{
    public string? Username { get; }
    public string? Password { get; }

    public Dictionary<string, string> Users { get; } = [];

    public EMQXConfiguration(
        string? username = null,
        string? password = null)
    {
        Username = username;
        Password = password;
    }

    public EMQXConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) : base(resourceConfiguration) { }

    public EMQXConfiguration(IContainerConfiguration resourceConfiguration) : base(resourceConfiguration) { }

    public EMQXConfiguration(EMQXConfiguration resourceConfiguration) : this(new EMQXConfiguration(), resourceConfiguration) { }

    public EMQXConfiguration(EMQXConfiguration oldValue, EMQXConfiguration newValue) : base(oldValue, newValue)
    {
        Username = BuildConfiguration.Combine(oldValue.Username, newValue.Username);
        Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
    }
}
