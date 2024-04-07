using Microsoft.Extensions.Options;

namespace BOINCWorker.Tests;

public class OptionsWrapper(BOINCWorkerOptions theOptions) : IOptions<BOINCWorkerOptions>
{
    public BOINCWorkerOptions Value => theOptions;
}
