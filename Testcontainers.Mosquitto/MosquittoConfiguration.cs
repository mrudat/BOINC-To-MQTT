// Ignore Spelling: BOINC Dockerfile mosquitto openssl username MQTT
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;

namespace Testcontainers.Mosquitto;

/// <inheritdoc cref="ContainerConfiguration" />
public class MosquittoConfiguration : ContainerConfiguration
{
    public string? Username { get; }
    public string? Password { get; }

    public Dictionary<string, string> Users { get; } = [];

    public MosquittoConfiguration(
        string? username = null,
        string? password = null)
    {
        Username = username;
        Password = password;
    }

    public MosquittoConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration) : base(resourceConfiguration) { }

    public MosquittoConfiguration(IContainerConfiguration resourceConfiguration) : base(resourceConfiguration) { }

    public MosquittoConfiguration(MosquittoConfiguration resourceConfiguration) : this(new MosquittoConfiguration(), resourceConfiguration) { }

    public MosquittoConfiguration(MosquittoConfiguration oldValue, MosquittoConfiguration newValue) : base(oldValue, newValue)
    {
        Username = BuildConfiguration.Combine(oldValue.Username, newValue.Username);
        Password = BuildConfiguration.Combine(oldValue.Password, newValue.Password);
    }
}