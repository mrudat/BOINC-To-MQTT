// Ignore Spelling: MQTT TLS

namespace Testcontainers;

public interface IRequiresAuthentication
{
    public Task AddUser(string userName, string password, CancellationToken cancellationToken = default);
}