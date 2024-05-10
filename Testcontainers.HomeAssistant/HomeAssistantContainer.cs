// Ignore Spelling: RPC

using DotNet.Testcontainers.Containers;

namespace Testcontainers.HomeAssistant;

public class HomeAssistantContainer(HomeAssistantConfiguration configuration) : DockerContainer(configuration)
{
    /// <summary>
    /// 
    /// </summary>
    public ushort WebUiPort => GetMappedPublicPort(HomeAssistantBuilder.WebUIPort);

    /// <summary>
    /// 
    /// </summary>
    public ushort RpcPort => GetMappedPublicPort(HomeAssistantBuilder.RpcPort);

    /// <summary>
    /// 
    /// </summary>
    public Uri WebUiUri => new UriBuilder("http", Hostname, WebUiPort).Uri;

    /// <summary>
    /// 
    /// </summary>
    public Uri RpcUri => new UriBuilder("http", Hostname, RpcPort).Uri;

    public async Task<ExecResult> CheckConfig(CancellationToken cancellationToken = default)
    {
        return await ExecAsync(["python", "-m", "homeassistant", "--script", "check_config", "--config", "/config"], cancellationToken);
    }
}
