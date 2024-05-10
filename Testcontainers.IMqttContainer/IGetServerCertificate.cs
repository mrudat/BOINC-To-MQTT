// Ignore Spelling: MQTT TLS

using System.Security.Cryptography.X509Certificates;

namespace Testcontainers;

public interface IGetServerCertificate
{
    public Task<X509Certificate2> GetServerCertificateAsync(CancellationToken cancellationToken = default);
}
