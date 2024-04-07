namespace CommonStuff;

public interface IGetHostCrossProjectIdentifier
{
    public Task<string> GetHostCrossProjectIdentifierAsync(CancellationToken cantellationToken = default);
}
