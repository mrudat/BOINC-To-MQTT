// Ignore Spelling: RPC BOINC

using DotNet.Testcontainers.Containers;
using System.Text;

namespace TestContainers.BOINC;

public class BOINCContainer(BOINCConfiguration configuration) : DockerContainer(configuration)
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetGuiRpcKeyAsync(CancellationToken cancellationToken = default)
    {
        var contents = await ReadFileAsync("/var/lib/boinc/gui_rpc_auth.cfg", cancellationToken);
        using var ms = new MemoryStream(contents);
        using var sr = new StreamReader(ms, encoding: Encoding.ASCII);
        return sr.ReadLine()!;
    }

    /// <summary>
    /// 
    /// </summary>
    public ushort Port => GetMappedPublicPort(BoincBuilder.GuiRpcPort);
}
