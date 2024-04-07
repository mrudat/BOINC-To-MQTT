using CommonStuff;
using Microsoft.Extensions.Options;
using System.IO.Abstractions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace BOINCWorker;

public class GetHostCrossProjectIdentifier(
    IOptions<BOINCWorkerOptions> options,
    IFileSystem filesystem
    ) : IGetHostCrossProjectIdentifier
{
    public async Task<string> GetHostCrossProjectIdentifierAsync(CancellationToken cancellationToken = default)
    {
        var dataPath = options.Value.DataPath;

        var clientStatePath = filesystem.Path.Combine(dataPath, "client_state.xml");

        await using var clientStateFh = await Task.Run(() => filesystem.File.OpenRead(clientStatePath), cancellationToken);

        var clientState = await XDocument.LoadAsync(clientStateFh, LoadOptions.None, cancellationToken);

        var host_cpid_element = clientState.XPathSelectElement("/client_state/host_info/host_cpid") ?? throw new InvalidOperationException("host_cpid not present in client_state.xml");

        return host_cpid_element.Value;
    }
}
