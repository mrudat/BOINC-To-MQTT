
namespace BOINC_To_MQTT;

internal interface IGPUController : IController
{
    void SetGPUUsageLimit(double cpuUsageLimit);
    Task UpdateThrottle(double throttle, CancellationToken cancellationToken = default);
}