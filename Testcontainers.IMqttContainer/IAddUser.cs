// Ignore Spelling: MQTT TLS

namespace Testcontainers;

public interface IAddUser
{
    public Task AddUser(string user, string password, CancellationToken cancellationToken = default);
}